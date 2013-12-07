using System;
using System.Text;
using NDatabase.Api;
using NDatabase.Meta;
using NDatabase.Oid;

namespace NDatabase.Core.Engine
{
    /// <summary>
    ///   Converts array of bytes into native objects and native objects into array of bytes
    /// </summary>
    internal static class ByteArrayConverter
    {
        private static readonly byte[] BytesForTrue = new byte[] {1};

        private static readonly byte[] BytesForFalse = new byte[] {0};

        private static readonly int IntSize = OdbType.Integer.Size;

        private static readonly int IntSizeX2 = OdbType.Integer.Size * 2;

        internal static byte[] BooleanToByteArray(bool b)
        {
            return b
                       ? BytesForTrue
                       : BytesForFalse;
        }

        internal static bool ByteArrayToBoolean(byte[] bytes, int offset = 0)
        {
            return bytes[offset] != 0;
        }

        internal static byte[] ShortToByteArray(short s)
        {
            return BitConverter.GetBytes(s);
        }

        internal static byte[] UShortToByteArray(ushort s)
        {
            return BitConverter.GetBytes(s);
        }

        internal static short ByteArrayToShort(byte[] bytes)
        {
            return BitConverter.ToInt16(bytes, 0);
        }

        internal static ushort ByteArrayToUShort(byte[] bytes)
        {
            return BitConverter.ToUInt16(bytes, 0);
        }

        internal static byte[] CharToByteArray(char c)
        {
            return BitConverter.GetBytes(c);
        }

        internal static char ByteArrayToChar(byte[] bytes)
        {
            return BitConverter.ToChar(bytes, 0);
        }

        /// <param name="s">Input</param>
        /// <param name="totalSpace"> The total space of the string (can be bigger that the real string size - to support later in place update) </param>
        /// <returns> The byte array that represent the string </returns>
        internal static byte[] StringToByteArray(String s, int totalSpace)
        {
            var stringBytes = Encoding.UTF8.GetBytes(s);

            var size = stringBytes.Length + IntSizeX2;

            var totalSize = totalSpace < size
                                ? size
                                : totalSpace;

            var totalSizeBytes = IntToByteArray(totalSize);
            var stringRealSize = IntToByteArray(stringBytes.Length);

            var bytes2 = new byte[totalSize + IntSizeX2];

            Buffer.BlockCopy(totalSizeBytes, 0, bytes2, 0, 4);
            Buffer.BlockCopy(stringRealSize, 0, bytes2, 4, 4);
            Buffer.BlockCopy(stringBytes, 0, bytes2, 8, stringBytes.Length);

            return bytes2;
        }

        /// <returns> The String represented by the byte array </returns>
        internal static String ByteArrayToString(byte[] bytes)
        {
            var realSize = ByteArrayToInt(bytes, IntSize);
            return Encoding.UTF8.GetString(bytes, IntSizeX2, realSize);
        }

        internal static byte[] DecimalToByteArray(Decimal bigDecimal)
        {
            var bits = Decimal.GetBits(bigDecimal);

            return GetBytes(bits[0], bits[1], bits[2], bits[3]);
        }

        private static byte[] GetBytes(int lo, int mid, int hi, int flags)
        {
            var buffer = new byte[16];
            buffer[0] = (byte) lo;
            buffer[1] = (byte) (lo >> 8);
            buffer[2] = (byte) (lo >> 16);
            buffer[3] = (byte) (lo >> 24);

            buffer[4] = (byte) mid;
            buffer[5] = (byte) (mid >> 8);
            buffer[6] = (byte) (mid >> 16);
            buffer[7] = (byte) (mid >> 24);

            buffer[8] = (byte) hi;
            buffer[9] = (byte) (hi >> 8);
            buffer[10] = (byte) (hi >> 16);
            buffer[11] = (byte) (hi >> 24);

            buffer[12] = (byte) flags;
            buffer[13] = (byte) (flags >> 8);
            buffer[14] = (byte) (flags >> 16);
            buffer[15] = (byte) (flags >> 24);

            return buffer;
        }

        internal static Decimal ByteArrayToDecimal(byte[] buffer)
        {
            var lo = (buffer[0]) | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24);
            var mid = (buffer[4]) | (buffer[5] << 8) | (buffer[6] << 16) | (buffer[7] << 24);
            var hi = (buffer[8]) | (buffer[9] << 8) | (buffer[10] << 16) | (buffer[11] << 24);
            var flags = (buffer[12]) | (buffer[13] << 8) | (buffer[14] << 16) | (buffer[15] << 24);

            return new Decimal(new[] {lo, mid, hi, flags});
        }

        internal static byte[] IntToByteArray(int l)
        {
            return BitConverter.GetBytes(l);
        }

        internal static byte[] UIntToByteArray(uint l)
        {
            return BitConverter.GetBytes(l);
        }

        internal static int ByteArrayToInt(byte[] bytes, int offset = 0)
        {
            return BitConverter.ToInt32(bytes, offset);
        }

        internal static uint ByteArrayToUInt(byte[] bytes)
        {
            return ByteArrayToUInt(bytes, 0);
        }

        private static uint ByteArrayToUInt(byte[] bytes, int offset)
        {
            return BitConverter.ToUInt32(bytes, offset);
        }

        internal static byte[] LongToByteArray(long l)
        {
            return BitConverter.GetBytes(l);
        }

        internal static byte[] ULongToByteArray(ulong l)
        {
            return BitConverter.GetBytes(l);
        }

        internal static ulong ByteArrayToULong(byte[] bytes)
        {
            return ByteArrayToULong(bytes, 0);
        }

        internal static byte[] DateToByteArray(DateTime date)
        {
            return LongToByteArray(date.Ticks);
        }

        internal static DateTime ByteArrayToDate(byte[] bytes)
        {
            var ticks = ByteArrayToLong(bytes);
            return new DateTime(ticks);
        }

        internal static byte[] FloatToByteArray(float f)
        {
            return BitConverter.GetBytes(f);
        }

        internal static float ByteArrayToFloat(byte[] bytes)
        {
            return BitConverter.ToSingle(bytes, 0);
        }

        internal static byte[] DoubleToByteArray(double d)
        {
            return BitConverter.GetBytes(d);
        }

        internal static double ByteArrayToDouble(byte[] bytes)
        {
            return BitConverter.ToDouble(bytes, 0);
        }

        internal static long ByteArrayToLong(byte[] bytes, int offset = 0)
        {
            return BitConverter.ToInt64(bytes, offset);
        }

        private static ulong ByteArrayToULong(byte[] bytes, int offset)
        {
            return BitConverter.ToUInt64(bytes, offset);
        }

        internal static OID DecodeOid(byte[] bytes, int offset)
        {
            var oid = ByteArrayToLong(bytes, offset);
            
            return oid == -1 ? null : OIDFactory.BuildObjectOID(oid);
        }
    }
}
