using System;

namespace NDatabase.Reflection
{
    internal sealed class ByteBuffer
    {
        public ByteBuffer(byte[] buffer)
        {
            Buffer = buffer;
        }

        public byte[] Buffer { get; private set; }

        public int Position { get; private set; }

        private void CheckCanRead(int count)
        {
            if ((Position + count) > Buffer.Length)
                throw new ArgumentOutOfRangeException();
        }

        public byte ReadByte()
        {
            CheckCanRead(1);
            return Buffer[Position++];
        }

        private byte[] ReadBytes(int length)
        {
            CheckCanRead(length);
            var dst = new byte[length];
            System.Buffer.BlockCopy(Buffer, Position, dst, 0, length);
            Position += length;
            return dst;
        }

        public double ReadDouble()
        {
            if (!BitConverter.IsLittleEndian)
            {
                var array = ReadBytes(8);
                Array.Reverse(array);
                return BitConverter.ToDouble(array, 0);
            }
            CheckCanRead(8);
            var num = BitConverter.ToDouble(Buffer, Position);
            Position += 8;
            return num;
        }

        public short ReadInt16()
        {
            CheckCanRead(2);
            var num = (short) (Buffer[Position] | (Buffer[Position + 1] << 8));
            Position += 2;
            return num;
        }

        public int ReadInt32()
        {
            CheckCanRead(4);
            var num = ((Buffer[Position] | (Buffer[Position + 1] << 8)) | (Buffer[Position + 2] << 0x10)) |
                      (Buffer[Position + 3] << 0x18);
            Position += 4;
            return num;
        }

        public long ReadInt64()
        {
            CheckCanRead(8);
            var num =
                (uint)
                (((Buffer[Position] | (Buffer[Position + 1] << 8)) | (Buffer[Position + 2] << 0x10)) |
                 (Buffer[Position + 3] << 0x18));
            var num2 =
                (uint)
                (((Buffer[Position + 4] | (Buffer[Position + 5] << 8)) | (Buffer[Position + 6] << 0x10)) |
                 (Buffer[Position + 7] << 0x18));
            long num3 = (num2 << 0x20) | num;
            Position += 8;
            return num3;
        }

        public float ReadSingle()
        {
            if (!BitConverter.IsLittleEndian)
            {
                var array = ReadBytes(4);
                Array.Reverse(array);
                return BitConverter.ToSingle(array, 0);
            }
            CheckCanRead(4);
            var num = BitConverter.ToSingle(Buffer, Position);
            Position += 4;
            return num;
        }
    }
}