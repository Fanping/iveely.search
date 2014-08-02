using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace Iveely.STSdb4.General.Compression
{
    public static class CountCompression
    {
        private const ulong CACHE_SIZE = 1024 + 1;

        private static byte[][] cache;

        static CountCompression()
        {
            cache = new byte[CACHE_SIZE][];

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);

                for (int i = 0; i < cache.Length; i++)
                {
                    writer.Seek(0, SeekOrigin.Begin);
                    InternalSerialize(writer, (ulong)i);
                    cache[i] = ms.ToArray();
                }
            }
        }

        private static void InternalSerialize(BinaryWriter writer, ulong number)
        {
            byte[] buffer = new byte[10];
            int index = 0;

            while (number >= 0x80)
            {
                buffer[index] = (byte)(number | 0x80);
                number = number >> 7;
                index++;
            }

            buffer[index] = (byte)number;
            index++;

            writer.Write(buffer, 0, index);
        }

        /// <summary>
        /// Compress value of count by CountCompression, and stores result in BinaryWriter
        /// </summary>
        /// <param name="count">Value for compression.</param>
        public static void Serialize(BinaryWriter writer, ulong number)
        {
            if (number < CACHE_SIZE)
                writer.Write(cache[number]);
            else
                InternalSerialize(writer, number);
        }

        /// <summary>
        /// Decompress a value compressed with CountCompression by successively reading bytes from BinaryReader.
        /// </summary>       
        public static ulong Deserialize(BinaryReader reader)
        {
            ulong value = 0;
            int shift = 0;
            byte b;

            do
            {
                b = reader.ReadByte();
                var temp = (ulong)(b & 0x7F);
                temp <<= shift;
                value |= temp;
                shift += 7;
            }
            while (b > 0x7F);

            return value;
        }
    }
}
