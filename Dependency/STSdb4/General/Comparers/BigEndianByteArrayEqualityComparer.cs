using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iveely.STSdb4.General.Extensions;

namespace Iveely.STSdb4.General.Comparers
{
    public class BigEndianByteArrayEqualityComparer : IEqualityComparer<byte[]>
    {
        public static readonly BigEndianByteArrayEqualityComparer Instance = new BigEndianByteArrayEqualityComparer();
        
        public bool Equals(byte[] x, byte[] y)
        {
            if (x.Length != y.Length)
                return false;

            CommonArray common = new CommonArray();
            common.ByteArray = x;
            ulong[] array1 = common.UInt64Array;
            common.ByteArray = y;
            ulong[] array2 = common.UInt64Array;

            int length = x.Length;
            int len = length >> 3;
            int remainder = length & 7;

            int i = len;

            if (remainder > 0)
            {
                int shift = sizeof(ulong) - remainder;
                if ((array1[i] << shift) >> shift != (array2[i] << shift) >> shift)
                    return false;
            }

            i--;

            while (i >= 7)
            {
                if (array1[i] != array2[i] ||
                    array1[i - 1] != array2[i - 1] ||
                    array1[i - 2] != array2[i - 2] ||
                    array1[i - 3] != array2[i - 3] ||
                    array1[i - 4] != array2[i - 4] ||
                    array1[i - 5] != array2[i - 5] ||
                    array1[i - 6] != array2[i - 6] ||
                    array1[i - 7] != array2[i - 7])
                    return false;

                i -= 8;
            }

            if (i >= 3)
            {
                if (array1[i] != array2[i] ||
                    array1[i - 1] != array2[i - 1] ||
                    array1[i - 2] != array2[i - 2] ||
                    array1[i - 3] != array2[i - 3])
                    return false;

                i -= 4;
            }

            if (i >= 1)
            {
                if (array1[i] != array2[i] ||
                    array1[i - 1] != array2[i - 1])
                    return false;

                i -= 2;
            }

            if (i >= 0)
            {
                if (array1[i] != array2[i])
                    return false;

                //i -= 1;
            }

            return true;
        }

        public int GetHashCode(byte[] obj)
        {
            return obj.GetHashCodeEx();
        }
    }
}
