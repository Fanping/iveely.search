using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Iveely.Storage
{
    /// <summary>
    /// Strategies for free space allocation.
    /// </summary>
    public enum AllocationStrategy : byte
    {
        /// <summary>
        /// Searches for free space from the current block (default behaviour).
        /// </summary>
        FromTheCurrentBlock,

        /// <summary>
        /// Always searches for free space from the beginning (reduces the space, but affects the read/write speed).
        /// </summary>
        FromTheBeginning
    }

    public class Space
    {
        private int activeChunkIndex = -1;
        private List<Ptr> free = new List<Ptr>(); //free chunks are always: ordered by position, not overlapped & not contiguous

        public AllocationStrategy Strategy;

        public long FreeBytes { get; private set; }

        public Space()
        {
            Strategy = AllocationStrategy.FromTheCurrentBlock;
        }

        public void Add(Ptr freeChunk)
        {
            if (free.Count == 0)
                free.Add(freeChunk);
            else
            {
                var last = free[free.Count - 1];
                if (freeChunk.Position > last.PositionPlusSize)
                    free.Add(freeChunk);
                else if (freeChunk.Position == last.PositionPlusSize)
                {
                    last.Size += freeChunk.Size;
                    free[free.Count - 1] = last;
                }
                else
                    throw new ArgumentException("Invalid ptr order.");
            }

            FreeBytes += freeChunk.Size;
        }

        public Ptr Alloc(long size)
        {
            if (activeChunkIndex < 0 || activeChunkIndex == free.Count - 1 || free[activeChunkIndex].Size < size)
            {
                int idx = 0;
                switch (Strategy)
                {
                    case AllocationStrategy.FromTheCurrentBlock : idx = activeChunkIndex >= 0 && activeChunkIndex + 1 < free.Count - 1 ? activeChunkIndex + 1 : 0; break;
                    case AllocationStrategy.FromTheBeginning: idx = 0; break;
                    default:
                        throw new NotSupportedException(Strategy.ToString());
                }

                for (int i = idx; i < free.Count; i++)
                {
                    if (free[i].Size >= size)
                    {
                        activeChunkIndex = i;
                        break;
                    }
                }
            }

            Ptr ptr = free[activeChunkIndex];

            if (ptr.Size < size)
                throw new Exception("Not enough space.");

            long pos = ptr.Position;
            ptr.Position += size;
            ptr.Size -= size;

            if (ptr.Size > 0)
                free[activeChunkIndex] = ptr;
            else //if (ptr.Size == 0)
            {
                free.RemoveAt(activeChunkIndex);
                activeChunkIndex = -1; //search for active chunk at next alloc
            }

            FreeBytes -= size;

            return new Ptr(pos, size);
        }

        public void Free(Ptr ptr)
        {
            int idx = free.BinarySearch(ptr);
            if (idx >= 0)
                throw new ArgumentException("Space already freed.");

            idx = ~idx;
            if ((idx < free.Count && ptr.PositionPlusSize > free[idx].Position) || (idx > 0 && ptr.Position < free[idx - 1].PositionPlusSize))
                throw new ArgumentException("Can't free overlapped space.");

            bool merged = false;

            if (idx < free.Count) //try merge with right chunk
            {
                var p = free[idx];
                if (ptr.PositionPlusSize == p.Position)
                {
                    p.Position -= ptr.Size;
                    p.Size += ptr.Size;
                    free[idx] = p;
                    merged = true;
                }
            }

            if (idx > 0) //try merge with left chunk
            {
                var p = free[idx - 1];
                if (ptr.Position == p.PositionPlusSize)
                {
                    if (merged)
                    {
                        p.Size += free[idx].Size;
                        free[idx - 1] = p;
                        free.RemoveAt(idx);
                        if (activeChunkIndex >= idx)
                            activeChunkIndex--;
                    }
                    else
                    {
                        p.Size += ptr.Size;
                        free[idx - 1] = p;
                        merged = true;
                    }
                }
            }

            if (!merged)
                free.Insert(idx, ptr);

            FreeBytes += ptr.Size;
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((byte)Strategy);
            writer.Write(activeChunkIndex);
            writer.Write(free.Count);

            for (int i = 0; i < free.Count; i++)
                free[i].Serialize(writer);
        }

        public void Deserealize(BinaryReader reader)
        {
            Strategy = (AllocationStrategy)reader.ReadByte();
            activeChunkIndex = reader.ReadInt32();
            int count = reader.ReadInt32();

            free.Clear();
            FreeBytes = 0;

            for (int i = 0; i < count; i++)
            {
                var ptr = Ptr.Deserialize(reader);
                free.Add(ptr);
                FreeBytes += ptr.Size;
            }
        }
    }
}