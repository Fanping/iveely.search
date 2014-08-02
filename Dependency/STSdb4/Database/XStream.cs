using System;
using System.IO;
using Iveely.STSdb4.General.Extensions;
using Iveely.STSdb4.Data;
using Iveely.STSdb4.WaterfallTree;

namespace Iveely.STSdb4.Database
{
    public class XStream : Stream
    {
        internal const int BLOCK_SIZE = 2 * 1024;

        private object SyncRoot;

        private long position;
        private bool isModified;
        private long cachedLength;

        public ITable<IData, IData> Table { get; private set; }

        internal XStream(ITable<IData, IData> table)
        {
            Table = table;
            SyncRoot = new object();
        }

        public IDescriptor Description
        {
            get { return Table.Descriptor; }
        }

        #region Stream Members

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (SyncRoot)
            {
                while (count > 0)
                {
                    int chunk = Math.Min(BLOCK_SIZE - (int)(position % BLOCK_SIZE), count);

                    IData key = new Data<long>(position);
                    IData record = new Data<byte[]>(buffer.Middle(offset, chunk));
                    Table[key] = record;

                    position += chunk;
                    offset += chunk;
                    count -= chunk;
                }

                isModified = true;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (offset < 0)
                throw new ArgumentException("offset < 0");

            if (count < 0)
                throw new ArgumentException("count < 0");

            if (offset + count > buffer.Length)
                throw new ArgumentException("offset + count > buffer.Length");

            lock (SyncRoot)
            {
                int result = (int)(Length - position);

                if (result > count)
                    result = count;
                if (result <= 0)
                    return 0;

                var fromKey = new Data<long>(position - position % BLOCK_SIZE);
                var toKey = new Data<long>(position + count - 1);

                long currentKey = -1;

                int sourceOffset = 0;
                int readCount = 0;
                int bufferOffset = offset;

                foreach (var kv in Table.Forward(fromKey, true, toKey, true))
                {
                    long key = ((Data<long>)kv.Key).Value;
                    byte[] source = ((Data<byte[]>)kv.Value).Value;

                    if (currentKey < 0)
                    {
                        if (position >= key)
                            sourceOffset = (int)(position - key);
                        else
                        {
                            Array.Clear(buffer, bufferOffset, (int)(key - position));
                            bufferOffset += (int)(key - position);
                        }
                    }
                    else if (currentKey != key)
                    {
                        int difference = (int)(key - currentKey);
                        Array.Clear(buffer, bufferOffset, difference);
                        bufferOffset += difference;
                    }

                    if (sourceOffset < source.Length)
                        readCount = source.Length - sourceOffset;

                    if (bufferOffset + readCount > buffer.Length)
                        readCount = buffer.Length - bufferOffset;

                    if (readCount > 0)
                        Buffer.BlockCopy(source, sourceOffset, buffer, bufferOffset, readCount);
                    bufferOffset += readCount;

                    int clearCount = BLOCK_SIZE - (sourceOffset + readCount);
                    int bufferRemainder = (result + offset) - bufferOffset;
                    if (clearCount > bufferRemainder)
                        clearCount = bufferRemainder;

                    Array.Clear(buffer, bufferOffset, clearCount);
                    bufferOffset += clearCount;

                    currentKey = key + BLOCK_SIZE;

                    sourceOffset = 0;
                    readCount = 0;
                }

                if (bufferOffset < result + offset)
                {
                    int clearCount = result;
                    if (bufferOffset + clearCount > buffer.Length)
                        clearCount = buffer.Length - bufferOffset;

                    Array.Clear(buffer, bufferOffset, clearCount);
                }

                position += result;

                return result;
            }
        }

        public override void Flush()
        {
            //do nothing
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get
            {
                lock (SyncRoot)
                {
                    if (!isModified)
                        return cachedLength;
                    else
                    {
                        foreach (var row in Table.Backward())
                        {
                            var key = (Data<long>)row.Key;
                            var rec = (Data<byte[]>)row.Value;

                            isModified = false;

                            return cachedLength = key.Value + rec.Value.Length;
                        }
                    }

                    return 0;
                }
            }
        }

        public override long Position
        {
            get
            {
                lock (SyncRoot)
                    return position;
            }
            set
            {
                lock (SyncRoot)
                    position = value;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            lock (SyncRoot)
            {
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        position = offset;
                        break;
                    case SeekOrigin.Current:
                        position += offset;
                        break;
                    case SeekOrigin.End:
                        position = Length - 1 - offset;
                        break;
                }

                return position;
            }
        }

        public override void SetLength(long value)
        {
            lock (SyncRoot)
            {
                var length = Length;
                if (value == length)
                    return;

                var oldPosition = this.position;
                try
                {
                    if (value > length)
                    {
                        Seek(value - 1, SeekOrigin.Begin);
                        Write(new byte[1] { 0 }, 0, 1);
                    }
                    else //if (value < length)
                    {
                        Seek(value, SeekOrigin.Begin);
                        Zero(length - value);
                    }
                }
                finally
                {
                    Seek(oldPosition, SeekOrigin.Begin);

                    isModified = true;
                }
            }
        }

        #endregion

        public void Zero(long count)
        {
            lock (SyncRoot)
            {
                var fromKey = new Data<long>(position);
                var toKey = new Data<long>(position + count - 1);
                Table.Delete(fromKey, toKey);

                position += count;

                isModified = true;
            }
        }
    }
}