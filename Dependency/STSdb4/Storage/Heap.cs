using Iveely.STSdb4.General.IO;
using Iveely.STSdb4.WaterfallTree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Iveely.STSdb4.Storage
{
    public class Heap : IHeap
    {
        private readonly object SyncRoot = new object();
        private AtomicHeader header;
        private readonly Space space;

        //updated every time after Serialize() invocation.
        private long maxPositionPlusSize;

        //handle -> pointer
        private readonly Dictionary<long, Pointer> used;
        private readonly Dictionary<long, Pointer> reserved;

        private long currentVersion;
        private long maxHandle;

        public Stream Stream { get; private set; }

        public AllocationStrategy Strategy
        {
            get
            {
                lock (SyncRoot)
                    return space.Strategy;
            }
            set
            {
                lock (SyncRoot)
                    space.Strategy = value;
            }
        }

        public Heap(Stream stream, bool useCompression = false, AllocationStrategy strategy = AllocationStrategy.FromTheCurrentBlock)
        {
            stream.Seek(0, SeekOrigin.Begin); //support Seek?

            Stream = stream;

            space = new Space();

            used = new Dictionary<long, Pointer>();
            reserved = new Dictionary<long, Pointer>();

            if (stream.Length < AtomicHeader.SIZE) //create new
            {
                header = new AtomicHeader();
                header.UseCompression = useCompression;
                space.Add(new Ptr(AtomicHeader.SIZE, long.MaxValue - AtomicHeader.SIZE));
            }
            else //open exist (ignore the useCompression flag)
            {
                header = AtomicHeader.Deserialize(Stream);
                stream.Seek(header.SystemData.Position, SeekOrigin.Begin);
                Deserialize(new BinaryReader(stream));

                //manual alloc header.SystemData
                var ptr = space.Alloc(header.SystemData.Size);
                if (ptr.Position != header.SystemData.Position)
                    throw new Exception("Logical error.");
            }

            Strategy = strategy;

            currentVersion++;
        }

        public Heap(string fileName, bool useCompression = false, AllocationStrategy strategy = AllocationStrategy.FromTheCurrentBlock)
            :this(new OptimizedFileStream(fileName, FileMode.OpenOrCreate), useCompression, strategy)
        {
        }

        private void FreeOldVersions()
        {
            List<long> forRemove = new List<long>();

            foreach (var kv in reserved)
            {
                var handle = kv.Key;
                var pointer = kv.Value;
                if (pointer.RefCount > 0)
                    continue;

                space.Free(pointer.Ptr);
                forRemove.Add(handle);
            }

            foreach (var handle in forRemove)
                reserved.Remove(handle);
        }

        private void InternalWrite(long position, int originalCount, byte[] buffer, int index, int count)
        {
            BinaryWriter writer = new BinaryWriter(Stream);
            Stream.Seek(position, SeekOrigin.Begin);

            if (UseCompression)
                writer.Write(originalCount);

            writer.Write(buffer, index, count);
        }

        private byte[] InternalRead(long position, long size)
        {
            BinaryReader reader = new BinaryReader(Stream);
            Stream.Seek(position, SeekOrigin.Begin);

            byte[] buffer;

            if (!UseCompression)
                buffer = reader.ReadBytes((int)size);
            else
            {
                byte[] raw = new byte[reader.ReadInt32()];
                buffer = reader.ReadBytes((int)size - sizeof(int));

                using (MemoryStream stream = new MemoryStream(buffer))
                {
                    using (DeflateStream decompress = new DeflateStream(stream, CompressionMode.Decompress))
                        decompress.Read(raw, 0, raw.Length);
                }

                buffer = raw;
            }

            return buffer;
        }

        private void Serialize(BinaryWriter writer)
        {
            maxPositionPlusSize = AtomicHeader.SIZE;

            writer.Write(maxHandle);
            writer.Write(currentVersion);

            //write free
            space.Serialize(writer);

            //write used
            writer.Write(used.Count);
            foreach (var kv in used)
            {
                writer.Write(kv.Key);
                kv.Value.Serialize(writer);

                long posPlusSize = kv.Value.Ptr.PositionPlusSize;
                if (posPlusSize > maxPositionPlusSize)
                    maxPositionPlusSize = posPlusSize;
            }

            //write reserved
            writer.Write(reserved.Count);
            foreach (var kv in reserved)
            {
                writer.Write(kv.Key);
                kv.Value.Serialize(writer);

                long posPlusSize = kv.Value.Ptr.PositionPlusSize;
                if (posPlusSize > maxPositionPlusSize)
                    maxPositionPlusSize = posPlusSize;
            }
        }

        private void Deserialize(BinaryReader reader)
        {
            maxHandle = reader.ReadInt64();
            currentVersion = reader.ReadInt64();

            //read free
            space.Deserealize(reader);

            //read used
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var handle = reader.ReadInt64();
                var pointer = Pointer.Deserialize(reader);
                used.Add(handle, pointer);
            }

            //read reserved
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var handle = reader.ReadInt64();
                var pointer = Pointer.Deserialize(reader);
                reserved.Add(handle, pointer);
            }
        }

        public byte[] Tag
        {
            get
            {
                lock (SyncRoot)
                    return header.Tag;
            }
            set
            {
                lock (SyncRoot)
                    header.Tag = value;
            }
        }

        public long ObtainNewHandle()
        {
            lock (SyncRoot)
                return maxHandle++;
        }

        public void Release(long handle)
        {
            lock (SyncRoot)
            {
                Pointer pointer;
                if (!used.TryGetValue(handle, out pointer))
                    return; //throw new ArgumentException("handle");

                if (pointer.Version == currentVersion)
                    space.Free(pointer.Ptr);
                else
                {
                    pointer.IsReserved = true;
                    reserved.Add(handle, pointer);
                }

                used.Remove(handle);
            }
        }

        public bool Exists(long handle)
        {
            lock (SyncRoot)
                return used.ContainsKey(handle);
        }

        /// <summary>
        /// Before writting, handle must be obtained (registered).
        /// New block will be written always with version = CurrentVersion
        /// If new block is written to handle and the last block of this handle have same version with the new one, occupied space by the last block will be freed.
        /// </summary>
        public void Write(long handle, byte[] buffer, int index, int count)
        {
            int originalCount = count;

            if (UseCompression)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (DeflateStream compress = new DeflateStream(stream, CompressionMode.Compress, true))
                        compress.Write(buffer, index, count);

                    buffer = stream.GetBuffer();
                    index = 0;
                    count = (int)stream.Length;
                }
            }

            lock (SyncRoot)
            {
                if (handle >= maxHandle)
                    throw new ArgumentException("Invalid handle.");

                Pointer pointer;
                if (used.TryGetValue(handle, out pointer))
                {
                    if (pointer.Version == currentVersion)
                        space.Free(pointer.Ptr);
                    else
                    {
                        pointer.IsReserved = true;
                        reserved.Add(handle, pointer);
                    }
                }

                long size = UseCompression ? sizeof(int) + count : count;
                Ptr ptr = space.Alloc(size);
                used[handle] = pointer = new Pointer(currentVersion, ptr);

                InternalWrite(ptr.Position, originalCount, buffer, index, count);
            }
        }

        public byte[] Read(long handle)
        {
            lock (SyncRoot)
            {
                Pointer pointer;
                if (!used.TryGetValue(handle, out pointer))
                    throw new ArgumentException("No such handle or data exists.");

                Ptr ptr = pointer.Ptr;
                Debug.Assert(ptr != Ptr.NULL);

                return InternalRead(ptr.Position, ptr.Size);
            }
        }

        public void Commit()
        {
            lock (SyncRoot)
            {
                Stream.Flush();

                FreeOldVersions();

                using (MemoryStream ms = new MemoryStream())
                {
                    if (header.SystemData != Ptr.NULL)
                        space.Free(header.SystemData);

                    Serialize(new BinaryWriter(ms));

                    Ptr ptr = space.Alloc(ms.Length);
                    Stream.Seek(ptr.Position, SeekOrigin.Begin);
                    Stream.Write(ms.GetBuffer(), 0, (int)ms.Length);

                    header.SystemData = ptr;

                    //atomic write
                    header.Serialize(Stream);

                    if (ptr.PositionPlusSize > maxPositionPlusSize)
                        maxPositionPlusSize = ptr.PositionPlusSize;
                }

                Stream.Flush();

                //try to truncate the stream
                if (Stream.Length > maxPositionPlusSize)
                    Stream.SetLength(maxPositionPlusSize);

                currentVersion++;
            }
        }

        public long DataSize
        {
            get
            {
                lock (SyncRoot)
                    return used.Sum(kv => kv.Value.Ptr.Size);
            }
        }

        public long Size
        {
            get
            {
                lock (SyncRoot)
                    return Stream.Length;
            }
        }

        public bool UseCompression
        {
            get
            {
                lock (SyncRoot)
                    return header.UseCompression;
            }
        }

        public void Close()
        {
            lock (SyncRoot)
                Stream.Close();
        }

        public IEnumerable<KeyValuePair<long, byte[]>> GetLatest(long atVersion)
        {
            List<KeyValuePair<long, Pointer>> list = new List<KeyValuePair<long, Pointer>>();

            lock (SyncRoot)
            {
                foreach (var kv in used.Union(reserved))
                {
                    var handle = kv.Key;
                    var pointer = kv.Value;

                    if (pointer.Version >= atVersion && pointer.Version < currentVersion)
                    {
                        list.Add(new KeyValuePair<long, Pointer>(handle, pointer));
                        pointer.RefCount++;
                    }
                }
            }

            foreach (var kv in list)
            {
                var handle = kv.Key;
                var pointer = kv.Value;

                byte[] buffer;
                lock (SyncRoot)
                {
                    buffer = InternalRead(pointer.Ptr.Position, pointer.Ptr.Size);
                    pointer.RefCount--;
                    if (pointer.IsReserved && pointer.RefCount <= 0)
                    {
                        space.Free(pointer.Ptr);
                        reserved.Remove(handle);
                    }
                }

                yield return new KeyValuePair<long, byte[]>(handle, buffer);
            }
        }

        public KeyValuePair<long, Ptr>[] GetUsedSpace()
        {
            lock (SyncRoot)
            {
                KeyValuePair<long, Ptr>[] array = new KeyValuePair<long, Ptr>[used.Count + reserved.Count];

                int idx = 0;
                foreach (var kv in used.Union(reserved))
                    array[idx++] = new KeyValuePair<long, Ptr>(kv.Value.Version, kv.Value.Ptr);

                return array;
            }
        }

        public long CurrentVersion
        {
            get
            {
                lock (SyncRoot)
                    return currentVersion;
            }
        }
    }
}
