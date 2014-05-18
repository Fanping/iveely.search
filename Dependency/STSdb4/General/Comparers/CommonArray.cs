using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Iveely.General.Comparers
{
    [StructLayout(LayoutKind.Explicit)]
    public struct CommonArray
    {
        [FieldOffset(0)]
        public byte[] ByteArray;

        [FieldOffset(0)]
        public int[] Int32Array;

        [FieldOffset(0)]
        public uint[] UInt32Array;

        [FieldOffset(0)]
        public long[] Int64Array;

        [FieldOffset(0)]
        public ulong[] UInt64Array;
    }
}
