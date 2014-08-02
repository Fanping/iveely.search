using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iveely.STSdb4.General.Extensions;

namespace Iveely.STSdb4.General.Comparers
{
    public class BigEndianByteArrayComparer : IComparer<byte[]>
    {
        public static readonly BigEndianByteArrayComparer Instance = new BigEndianByteArrayComparer();
        
        public int Compare(byte[] x, byte[] y, int length)
        {
            CommonArray common = new CommonArray();
            common.ByteArray = x;
            ulong[] array1 = common.UInt64Array;
            common.ByteArray = y;
            ulong[] array2 = common.UInt64Array;

            int len = length >> 3;

            for (int i = 0; i < len; i++)
            {
                var v1 = array1[i];
                var v2 = array2[i];

                if (v1 != v2)
                {
                    for (int j = i << 3; ; j++)
                    {
                        byte b1 = x[j];
                        byte b2 = y[j];
                        if (b1 < b2)
                            return -1;
                        if (b1 > b2)
                            return 1;
                    }
                }
            }

            int index = len << 3; 

            switch (length & 7)
            {
                case 7:
                    {
                        var b1 = x[index];
                        var b2 = y[index];
                        if (b1 < b2)
                            return -1;
                        if (b1 > b2)
                            return 1;
                        index++;
                        goto case 6;
                    }
                case 6:
                    {
                        var b1 = x[index];
                        var b2 = y[index];
                        if (b1 < b2)
                            return -1;
                        if (b1 > b2)
                            return 1;
                        index++;
                        goto case 5;
                    }
                case 5:
                    {
                        var b1 = x[index];
                        var b2 = y[index];
                        if (b1 < b2)
                            return -1;
                        if (b1 > b2)
                            return 1;
                        index++;
                        goto case 4;
                    }
                case 4:
                    {
                        var b1 = x[index];
                        var b2 = y[index];
                        if (b1 < b2)
                            return -1;
                        if (b1 > b2)
                            return 1;
                        index++;
                        goto case 3;
                    }
                case 3:
                    {
                        var b1 = x[index];
                        var b2 = y[index];
                        if (b1 < b2)
                            return -1;
                        if (b1 > b2)
                            return 1;
                        index++;
                        goto case 2;
                    }
                case 2:
                    {
                        var b1 = x[index];
                        var b2 = y[index];
                        if (b1 < b2)
                            return -1;
                        if (b1 > b2)
                            return 1;
                        index++;
                        goto case 1;
                    }
                case 1:
                    {
                        var b1 = x[index];
                        var b2 = y[index];
                        if (b1 < b2)
                            return -1;
                        if (b1 > b2)
                            return 1;

                        break;
                    }
            }

            return 0;
        }

        public int Compare(byte[] x, byte[] y)
        {
            int cmp = Compare(x, y, Math.Min(x.Length, y.Length));
            if (cmp != 0)
                return cmp;

            if (x.Length < y.Length)
                return -1;
            if (x.Length > y.Length)
                return 1;

            return 0;
        }
    }
}
