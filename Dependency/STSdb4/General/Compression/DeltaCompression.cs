using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Iveely.STSdb4.General.Buffers;
using Iveely.STSdb4.General.Comparers;

namespace Iveely.STSdb4.General.Compression
{
    public static class DeltaCompression
    {
        private const byte VERSION = 40;

        #region UIn64 Tools

        private static void SetFlag(ulong[] data, int bitIndex/*, bool value*/)
        {
            //if (value)
            data[bitIndex >> 6] |= 1UL << (bitIndex & 63);
        }

        private static bool GetFlag(ulong[] data, int bitIndex)
        {
            return (data[bitIndex >> 6] & 1UL << (bitIndex & 63)) != 0;
        }

        private static void SetValue(ulong[] data, int bitIndex, ulong value)
        {
            int index = bitIndex >> 6;
            int idx = bitIndex & 63;

            if (idx > 0)
            {
                data[index] |= value << idx;
                data[index + 1] |= value >> (64 - idx);
            }
            else
                data[index] = value;
        }

        private static ulong GetValue(ulong[] data, int bitIndex)
        {
            int index = bitIndex >> 6;
            int idx = bitIndex & 63;

            if (idx > 0)
                return (data[index] >> (idx)) | (data[index + 1] << (64 - idx));
            else
                return data[index];
        }

        private static void SetBits(ulong[] data, int bitIndex, ulong value, int bitCount)
        {
            Debug.Assert(bitCount > 0 && bitCount <= 64);

            int index = bitIndex >> 6;
            int idx = bitIndex & 63;

            data[index] |= value << idx;
            if (idx + bitCount > 64)
                data[index + 1] |= value >> (64 - idx);
        }

        private static ulong GetBits(ulong[] data, int bitIndex, int bitCount)
        {
            Debug.Assert(bitCount > 0 && bitCount <= 64);

            int index = bitIndex >> 6;
            int idx = bitIndex & 63;

            if (idx + bitCount <= 64)
                return (data[index] >> idx) & (UInt64.MaxValue >> (64 - bitCount));
            else
                return ((data[index + 1] << (64 - idx)) | (data[index] >> (idx))) & (UInt64.MaxValue >> (64 - bitCount));
        }

        #endregion

        public static void Compress(BinaryWriter writer, long[] values, int index, int count, Helper helper)
        {
            writer.Write(VERSION);

            if (count == 0)
                return;

            long value = values[index];
            CountCompression.Serialize(writer, (ulong)value);
            if (count == 1)
                return;

            Debug.Assert(count == helper.Count);
            if (!helper.IsReady)
                helper.Prepare();

            index++;
            count--;
            int maxIndex = index + count - 1;

            helper.Serialize(writer);

            if (helper.Type == HelperType.Raw)
            {
                while (index <= maxIndex)
                    writer.Write(values[index++]);

                return;
            }

            if (helper.Type == HelperType.OneStep)
                return;

            ulong maxDelta = helper.Delta;
            bool alwaysUseDelta = helper.AlwaysUseDelta;
            int bitCount = BitUtils.GetBitBounds(maxDelta);

            bool writeSign = helper.Type == HelperType.Delta;

            CommonArray common = new CommonArray();
            int sizeBits = helper.SizeBits > 0 ? helper.SizeBits : (1 + 1 + 64) * (count - 1);
            common.ByteArray = new byte[(int)Math.Ceiling(sizeBits / 8.0)];
            ulong[] data = common.UInt64Array;
            int bitIndex = 0;

            ulong delta;
            bool sign; //false - positive, true - negative

            for (; index <= maxIndex; index++)
            {
                long newValue = values[index];
                if (newValue >= value)
                {
                    sign = false;
                    delta = (ulong)(newValue - value);
                }
                else
                {
                    sign = true;
                    delta = (ulong)(value - newValue);
                }

                if (alwaysUseDelta || delta <= maxDelta)
                {
                    if (!alwaysUseDelta)
                    {
                        SetFlag(data, bitIndex/*, true*/); //use delta
                        bitIndex++;
                    }

                    if (writeSign)
                    {
                        if (sign)
                            SetFlag(data, bitIndex/*, sign*/);
                        bitIndex++;
                    }

                    SetBits(data, bitIndex, delta, bitCount);
                    bitIndex += bitCount;
                }
                else
                {
                    //SetFlag(data, bitIndex, false); //don't use delta
                    bitIndex++;

                    SetValue(data, bitIndex, (ulong)newValue);
                    bitIndex += 64;
                }

                value = newValue;
            }

            int bytesCount = (int)Math.Ceiling(bitIndex / 8.0);
            CountCompression.Serialize(writer, (ulong)bytesCount);
            writer.Write(common.ByteArray, 0, bytesCount);
        }

        public static void Decompress(BinaryReader reader, Action<int, long> values, int count)
        {
            if (reader.ReadByte() != VERSION)
                throw new Exception("Invalid delta compression version.");

            if (count == 0)
                return;

            int index = 0;

            long value = (long)CountCompression.Deserialize(reader);
            values(index,value);
            if (count == 1)
                return;

            var helper = Helper.Deserialize(reader);

            index++;
            count--;
            int maxIndex = index + count - 1;

            if (helper.Type == HelperType.Raw)
            {
                while (index <= maxIndex)
                    values(index++, reader.ReadInt64());

                return;
            }

            if (helper.Type == HelperType.OneStep)
            {
                long step = (long)helper.Delta;

                if (helper.Sign == false)
                    while (index <= maxIndex)
                        values(index++, (value += step));
                else
                    while (index <= maxIndex)
                        values(index++, (value -= step));

                return;
            }

            bool readSign = helper.Type == HelperType.Delta;
            ulong maxDelta = helper.Delta;
            bool alwaysUseDelta = helper.AlwaysUseDelta;
            bool sign = helper.Sign;
            int bitCount = helper.DeltaBits;

            CommonArray common = new CommonArray();
            int bytesCount = (int)CountCompression.Deserialize(reader);
            common.ByteArray = reader.ReadBytes(bytesCount);
            ulong[] data = common.UInt64Array;
            int bitIndex = 0;

            for (; index <= maxIndex; index++)
            {
                long newValue;
                bool useDelta = alwaysUseDelta || GetFlag(data, bitIndex++);

                if (useDelta)
                {
                    if (readSign)
                    {
                        sign = GetFlag(data, bitIndex);
                        bitIndex++;
                    }

                    long delta = (long)GetBits(data, bitIndex, bitCount);
                    bitIndex += bitCount;

                    newValue = sign ? value - delta : value + delta;
                }
                else
                {
                    newValue = (long)GetValue(data, bitIndex);
                    bitIndex += 64;
                }

                values(index, newValue);
                value = newValue;
            }
        }

        public enum HelperType : byte
        {
            Raw = 0,
            Delta = 1,
            DeltaMonotone = 2,
            OneStep = 3
        }

        public class Helper
        {
            private long oldValue;

            private int[] map = new int[1 + 64];
            private int maxIndex;

            public HelperType Type = HelperType.OneStep;
            public byte DeltaBits;
            public bool AlwaysUseDelta;
            public bool Sign;

            public ulong Delta;
            public int SizeBits;

            public int Count;
            public bool IsReady;

            public void Serialize(BinaryWriter writer)
            {
                byte b = 0;

                if (Sign)
                    b |= (byte)0x80;

                if (IsReady)
                    b |= (byte)0x40;

                if (AlwaysUseDelta)
                    b |= (byte)0x20;

                b |= (byte)Type;

                writer.Write(b);

                if (Type == HelperType.Raw)
                    return;

                if (Type == HelperType.OneStep)
                {
                    CountCompression.Serialize(writer, Delta);
                    return;
                }

                writer.Write(DeltaBits);
            }

            public static Helper Deserialize(BinaryReader reader)
            {
                Helper helper = new Helper();

                byte b = reader.ReadByte();
                helper.Sign = (b & 0x80) != 0;
                helper.IsReady = (b & 0x40) != 0;
                helper.AlwaysUseDelta = (b & 0x20) != 0;
                helper.Type = (HelperType)(b & 0x1f);

                if (helper.Type == HelperType.Raw)
                    return helper;

                if (helper.Type == HelperType.OneStep)
                {
                    helper.Delta = CountCompression.Deserialize(reader);
                    return helper;
                }

                helper.DeltaBits = reader.ReadByte();
                helper.Delta = (1UL << helper.DeltaBits) - 1;

                return helper;
            }

            public void Add(long value)
            {
                if (IsReady)
                    return;

                if (Count > 1)
                {
                    bool sign;
                    ulong delta;

                    if (value >= oldValue)
                    {
                        sign = false;
                        delta = (ulong)(value - oldValue);
                    }
                    else
                    {
                        sign = true;
                        delta = (ulong)(oldValue - value);
                    }

                    int bits = BitUtils.GetBitBounds(delta);
                    map[bits]++;
                    if (bits > maxIndex)
                        maxIndex = bits;

                    if (Type > HelperType.Delta)
                    {
                        if (Sign != sign)
                            Type = HelperType.Delta;
                        else if (delta != Delta)
                            Type = HelperType.DeltaMonotone;
                    }
                }
                else if (Count == 1)
                    SecondAdd(value);

                oldValue = value;
                Count++;
            }

            private void SecondAdd(long value)
            {
                if (value >= oldValue)
                {
                    Sign = false;
                    Delta = (ulong)(value - oldValue);
                }
                else
                {
                    Sign = true;
                    Delta = (ulong)(oldValue - value);
                }

                maxIndex = BitUtils.GetBitBounds(Delta);
                map[maxIndex]++;
            }

            public void Prepare()
            {
                if (IsReady)
                    return;
                
                IsReady = true;

                if (Count <= 1 || Type == HelperType.OneStep)
                    return;

                SizeBits = (Count - 1) * (/*1 +*/ 64);
                DeltaBits = 64;

                int hasSign = Type == HelperType.DeltaMonotone ? 0 : 1;

                for (int i = 1, c = 0; i <= maxIndex; i++)
                {
                    c += map[i];
                    int size = c * (1 + hasSign + i) + (Count - 1 - c) * (1 + 64);

                    if (size < SizeBits)
                    {
                        SizeBits = size;
                        DeltaBits = (byte)i;
                    }
                }

                if (DeltaBits < 64)
                {
                    Delta = (1UL << DeltaBits) - 1; //optimal delta
                    AlwaysUseDelta = DeltaBits == maxIndex;
                }
                else
                    Type = HelperType.Raw;
            }
        }
    }
}
