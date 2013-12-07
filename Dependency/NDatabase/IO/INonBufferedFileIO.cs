using System;

namespace NDatabase.IO
{
    internal interface INonBufferedFileIO : IDisposable
    {
        long Length { get; }

        long CurrentPositionForDirectWrite { get; }

        void SetCurrentPosition(long currentPosition);

        void WriteByte(byte b);

        byte[] ReadBytes(int size);

        byte ReadByte();

        void WriteBytes(byte[] bytes, int length);

        long Read(long position, byte[] buffer, int size);
    }
}
