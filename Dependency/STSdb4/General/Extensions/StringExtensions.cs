using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iveely.STSdb4.General.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Convert string to byte array
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] ParseHex(this string hex)
        {
            if ((hex.Length & 1) != 0)
                throw new ArgumentException("Input must have even number of characters");

            byte[] result = new byte[hex.Length / 2];

            for (int i = 0; i < result.Length; i++)
            {
                int high = hex[i * 2];
                int low = hex[i * 2 + 1];
                high = (high & 0xf) + ((high & 0x40) >> 6) * 9;
                low = (low & 0xf) + ((low & 0x40) >> 6) * 9;

                result[i] = (byte)((high << 4) | low);
            }

            return result;
        }
    }
}
