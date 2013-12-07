using System;

namespace NDatabase.IO
{
    /// <summary>
    ///   The interface for buffered IO
    /// </summary>
    internal interface IMultiBufferedFileIO : IDisposable
    {
        long Length { get; }
        
        long CurrentPosition { get; }

        void SetCurrentWritePosition(long currentPosition);

        void SetCurrentReadPosition(long currentPosition);

        void WriteByte(byte b);

        byte ReadByte();

        void WriteBytes(byte[] bytes);

        byte[] ReadBytes(int size);

        void FlushAll();

        void Close();
    }
}