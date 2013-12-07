namespace NDatabase.IO
{
    internal interface IMultiBuffer
    {
        byte[][] Buffers { get; }

        ///<summary>
        ///  The max position in the buffer, used to optimize the flush - to flush only new data and not all the buffer
        ///</summary>
        int[] MaxPositionInBuffer { get; }

        BufferPosition[] BufferPositions { get; }

        ///<summary>
        ///  The buffer size.
        ///</summary>
        int Size { get; }

        void ClearBuffer(int bufferIndex);
        void SetByte(int bufferIndex, int positionInBuffer, byte value);
        int GetBufferIndexForPosition(long position, int size);
        void SetCreationDate(int bufferIndex, long currentTimeInMs);
        void SetPositions(int bufferIndex, long startPosition, long endPosition);
        void WriteBytes(int bufferIndex, byte[] bytes, int startIndex, int offsetWhereToCopy, int lengthToCopy);
        bool HasBeenUsedForWrite(int bufferIndex);
        void Clear();
        long GetCreationDate(int bufferIndex);
    }
}