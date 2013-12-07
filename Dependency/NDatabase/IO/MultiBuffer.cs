using System;

namespace NDatabase.IO
{
    internal sealed class MultiBuffer : IMultiBuffer
    {
        ///<summary>
        ///  The number of buffers.
        ///</summary>
        internal const int NumberOfBuffers = 5;

        internal const int DefaultBufferSizeForData = 4096 * 4;

        internal const int DefaultBufferSizeForTransaction = 4096 * 4;

        ///<summary>
        ///  The buffer size.
        ///</summary>
        private readonly int _bufferSize;

        private readonly long[] _creations;

        ///<summary>
        ///  To know if buffer has been used for write - to speedup flush.
        ///</summary>
        private bool[] _bufferHasBeenUsedForWrite;

        internal MultiBuffer(int bufferSize)
        {
            _bufferSize = bufferSize;
            Buffers = new byte[NumberOfBuffers][];

            for (var x = 0; x < NumberOfBuffers; x++)
                Buffers[x] = new byte[bufferSize];

            BufferPositions = new BufferPosition[NumberOfBuffers];
            MaxPositionInBuffer = new int[NumberOfBuffers];
            _creations = new long[NumberOfBuffers];
            _bufferHasBeenUsedForWrite = new bool[NumberOfBuffers];
        }

        #region IMultiBuffer Members

        public BufferPosition[] BufferPositions { get; private set; }

        public byte[][] Buffers { get; private set; }

        ///<summary>
        ///  The max position in the buffer, used to optimize the flush - to flush only new data and not all the buffer
        ///</summary>
        public int[] MaxPositionInBuffer { get; private set; }

        ///<summary>
        ///  The buffer size.
        ///</summary>
        public int Size
        {
            get { return _bufferSize; }
        }

        public void ClearBuffer(int bufferIndex)
        {
            var buffer = Buffers[bufferIndex];
            var maxPosition = MaxPositionInBuffer[bufferIndex];

            Array.Clear(buffer, 0, maxPosition);

            BufferPositions[bufferIndex] = new BufferPosition();
            MaxPositionInBuffer[bufferIndex] = 0;
            _bufferHasBeenUsedForWrite[bufferIndex] = false;
        }

        public void SetByte(int bufferIndex, int positionInBuffer, byte value)
        {
            if (Buffers[bufferIndex] == null)
                Buffers[bufferIndex] = new byte[Size];

            Buffers[bufferIndex][positionInBuffer] = value;
            _bufferHasBeenUsedForWrite[bufferIndex] = true;

            if (positionInBuffer > MaxPositionInBuffer[bufferIndex])
                MaxPositionInBuffer[bufferIndex] = positionInBuffer;
        }

        public int GetBufferIndexForPosition(long position, int size)
        {
            var max = position + size;

            for (var i = 0; i < NumberOfBuffers; i++)
            {
                // Check if new position is in buffer
                if (max <= BufferPositions[i].End && position >= BufferPositions[i].Start)
                    return i;
            }

            return -1;
        }

        public void SetCreationDate(int bufferIndex, long currentTimeInMs)
        {
            _creations[bufferIndex] = currentTimeInMs;
        }

        public void SetPositions(int bufferIndex, long startPosition, long endPosition)
        {
            BufferPositions[bufferIndex] = new BufferPosition(startPosition, endPosition);
            MaxPositionInBuffer[bufferIndex] = 0;
        }

        public void WriteBytes(int bufferIndex, byte[] bytes, int startIndex, int offsetWhereToCopy, int lengthToCopy)
        {
            Buffer.BlockCopy(bytes, startIndex, Buffers[bufferIndex], offsetWhereToCopy, lengthToCopy);

            _bufferHasBeenUsedForWrite[bufferIndex] = true;

            var positionInBuffer = offsetWhereToCopy + lengthToCopy - 1;
            if (positionInBuffer > MaxPositionInBuffer[bufferIndex])
                MaxPositionInBuffer[bufferIndex] = positionInBuffer;
        }

        public bool HasBeenUsedForWrite(int bufferIndex)
        {
            return _bufferHasBeenUsedForWrite[bufferIndex];
        }

        public void Clear()
        {
            Buffers = null;
            BufferPositions = null;
            MaxPositionInBuffer = null;
            _bufferHasBeenUsedForWrite = null;
        }

        public long GetCreationDate(int bufferIndex)
        {
            return _creations[bufferIndex];
        }

        #endregion
    }

    internal struct BufferPosition
    {
        private readonly long _start;
        private readonly long _end;

        public BufferPosition(long start, long end)
            : this()
        {
            _start = start;
            _end = end;
        }

        ///<summary>
        ///  The current start position of the buffer
        ///</summary>
        public long Start
        {
            get { return _start; }
        }

        ///<summary>
        ///  The current end position of the buffer
        ///</summary>
        public long End
        {
            get { return _end; }
        }
    }
}
