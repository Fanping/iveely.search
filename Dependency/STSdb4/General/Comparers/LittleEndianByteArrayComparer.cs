using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iveely.General.Extensions;

namespace Iveely.General.Comparers
{
    public class LittleEndianByteArrayComparer : IComparer<byte[]>
    {
        public static readonly LittleEndianByteArrayComparer Instance = new LittleEndianByteArrayComparer();
        
        public int Compare(byte[] x, byte[] y, int length)
        {
            CommonArray common = new CommonArray();
            common.ByteArray = x;
            ulong[] array1 = common.UInt64Array;
            common.ByteArray = y;
            ulong[] array2 = common.UInt64Array;

            int len = length >> 3;
            int remainder = length & 7;

            int i = len;

            if (remainder > 0)
            {
                int shift = sizeof(ulong) - remainder;
                var v1 = (array1[i] << shift) >> shift;
                var v2 = (array2[i] << shift) >> shift;
                if (v1 < v2)
                    return -1;
                if (v1 > v2)
                    return 1;
            }

            i--;

            while (i >= 0)
            {
                var v1 = array1[i];
                var v2 = array2[i];
                if (v1 < v2)
                    return -1;
                if (v1 > v2)
                    return 1;

                i--;
            }

            return 0;
        }

        public int Compare(byte[] x, byte[] y)
        {
            if (x.Length == y.Length)
                return Compare(x, y, x.Length);

            for (int i = x.Length - 1, j = y.Length - 1, len = Math.Min(x.Length, y.Length); len > 0; i--, j--, len--)
            {
                if (x[i] < y[j])
                    return -1;
                if (x[i] > y[j])
                    return 1;
            }

            if (x.Length < y.Length)
                return -1;
            if (y.Length > y.Length)
                return 1;

            return 0;
        }
    }
}
