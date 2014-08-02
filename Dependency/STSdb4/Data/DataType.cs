using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Iveely.STSdb4.Data
{
    /*  examples:

            DataType type1 = DataType.Boolean;
            DataType type2 = DataType.Int32;
            DataType type3 = DataType.String;

            DataType type4 = DataType.Slotes(
                DataType.String,
                DataType.DateTime,
                DataType.Double,
                DataType.Double,
                DataType.Int64
                );

            DataType type5 = DataType.Array(DataType.String);
            DataType type6 = DataType.List(DataType.String);
            DataType type7 = DataType.Dictionary(DataType.Int32, DataType.String);

            DataType type8 = DataType.Slotes(
                DataType.String,
                DataType.DateTime,
                DataType.Double,
                DataType.Double,
                DataType.Slotes(
                    DataType.String,
                    DataType.String),
                    DataType.Array(DataType.Boolean),
                DataType.Int32,
                DataType.Dictionary(DataType.Int32, DataType.String),
                DataType.Boolean,
                DataType.List(DataType.Array(DataType.DateTime)),
                DataType.Dictionary(DataType.String, DataType.List(DataType.DateTime))
                );
    */

    public sealed class DataType : IEquatable<DataType>, IEnumerable<DataType>
    {
        private static readonly Type[] primitiveTypes;
        private static readonly Dictionary<Type, DataType> primitiveMap;

        public static readonly DataType Boolean = new DataType(Code.Boolean, null);
        public static readonly DataType Char = new DataType(Code.Char, null);
        public static readonly DataType SByte = new DataType(Code.SByte, null);
        public static readonly DataType Byte = new DataType(Code.Byte, null);
        public static readonly DataType Int16 = new DataType(Code.Int16, null);
        public static readonly DataType UInt16 = new DataType(Code.UInt16, null);
        public static readonly DataType Int32 = new DataType(Code.Int32, null);
        public static readonly DataType UInt32 = new DataType(Code.UInt32, null);
        public static readonly DataType Int64 = new DataType(Code.Int64, null);
        public static readonly DataType UInt64 = new DataType(Code.UInt64, null);
        public static readonly DataType Single = new DataType(Code.Single, null);
        public static readonly DataType Double = new DataType(Code.Double, null);
        public static readonly DataType Decimal = new DataType(Code.Decimal, null);
        public static readonly DataType DateTime = new DataType(Code.DateTime, null);
        public static readonly DataType String = new DataType(Code.String, null);
        public static readonly DataType ByteArray = new DataType(Code.ByteArray, null);

        private int? cachedHashCode;
        private string cachedToString;
        private byte[] cachedSerialize;

        private readonly Code code;
        private readonly DataType[] types;

        static DataType()
        {
            primitiveTypes = new Type[(int)Code.ByteArray + 1];

            primitiveTypes[(int)Code.Boolean] = typeof(Boolean);
            primitiveTypes[(int)Code.Char] = typeof(Char);
            primitiveTypes[(int)Code.SByte] = typeof(SByte);
            primitiveTypes[(int)Code.Byte] = typeof(Byte);
            primitiveTypes[(int)Code.Int16] = typeof(Int16);
            primitiveTypes[(int)Code.Int32] = typeof(Int32);
            primitiveTypes[(int)Code.UInt32] = typeof(UInt32);
            primitiveTypes[(int)Code.UInt16] = typeof(UInt16);
            primitiveTypes[(int)Code.Int64] = typeof(Int64);
            primitiveTypes[(int)Code.UInt64] = typeof(UInt64);
            primitiveTypes[(int)Code.Single] = typeof(Single);
            primitiveTypes[(int)Code.Double] = typeof(Double);
            primitiveTypes[(int)Code.Decimal] = typeof(Decimal);
            primitiveTypes[(int)Code.DateTime] = typeof(DateTime);
            primitiveTypes[(int)Code.String] = typeof(String);
            primitiveTypes[(int)Code.ByteArray] = typeof(byte[]);

            primitiveMap = new Dictionary<Type, DataType>();

            primitiveMap[typeof(Boolean)] = DataType.Boolean;
            primitiveMap[typeof(Char)] = DataType.Char;
            primitiveMap[typeof(SByte)] = DataType.SByte;
            primitiveMap[typeof(Byte)] = DataType.Byte;
            primitiveMap[typeof(Int16)] = DataType.Int16;
            primitiveMap[typeof(Int32)] = DataType.Int32;
            primitiveMap[typeof(UInt32)] = DataType.UInt32;
            primitiveMap[typeof(UInt16)] = DataType.UInt16;
            primitiveMap[typeof(Int64)] = DataType.Int64;
            primitiveMap[typeof(UInt64)] = DataType.UInt64;
            primitiveMap[typeof(Single)] = DataType.Single;
            primitiveMap[typeof(Double)] = DataType.Double;
            primitiveMap[typeof(Decimal)] = DataType.Decimal;
            primitiveMap[typeof(DateTime)] = DataType.DateTime;
            primitiveMap[typeof(String)] = DataType.String;
            primitiveMap[typeof(byte[])] = DataType.ByteArray;
        }

        private DataType(Code code, params DataType[] types)
        {
            this.code = code;
            this.types = types;
        }

        private int InternalGetHashCode()
        {
            if (IsPrimitive)
                return (int)code;

            int hashcode = 37;
            hashcode = 17 * hashcode + (int)code;

            for (int i = 0; i < types.Length; i++)
                hashcode = 17 * hashcode + types[i].InternalGetHashCode();

            return hashcode;
        }

        private string InternalToString()
        {
            if (IsPrimitive)
                return code.ToString();

            var s = System.String.Join(", ", types.Select(x => x.InternalToString()));

            if (IsSlots)
                return System.String.Format("({0})", s);
            else
                return System.String.Format("{0}<{1}>", code, s);
        }

        private void InternalSerialize(BinaryWriter writer)
        {
            writer.Write((byte)code);
            if (IsPrimitive)
                return;

            writer.Write(checked((byte)types.Length));
            for (int i = 0; i < types.Length; i++)
                types[i].InternalSerialize(writer);
        }

        public bool IsPrimitive
        {
            get { return code < Code.Slots; }
        }

        public bool IsSlots
        {
            get { return code == Code.Slots; }
        }

        public bool IsArray
        {
            get { return code == Code.Array; }
        }

        public bool IsList
        {
            get { return code == Code.List; }
        }

        public bool IsDictionary
        {
            get { return code == Code.Dictionary; }
        }

        public bool AreAllTypesPrimitive
        {
            get
            {
                if (IsPrimitive)
                    throw new InvalidOperationException(System.String.Format("The type {0} is primitive", this));

                return types.All(x => x.IsPrimitive);
            }
        }

        public DataType this[int index]
        {
            get
            {
                if (IsPrimitive)
                    throw new InvalidOperationException(System.String.Format("The type {0} is primitive", this));

                return types[index];
            }
        }

        public int TypesCount
        {
            get
            {
                if (IsPrimitive)
                    throw new InvalidOperationException(System.String.Format("The type {0} is primitive", this));

                return types.Length;
            }
        }

        public Type PrimitiveType
        {
            get
            {
                if (!IsPrimitive)
                    throw new InvalidOperationException(System.String.Format("The type {0} is not primitive", this));

                return DataType.primitiveTypes[(int)code];
            }
        }

        public bool Equals(DataType other)
        {
            if (Object.ReferenceEquals(this, other))
                return true;

            if (Object.ReferenceEquals(null, other))
                return false;

            if (this.code != other.code)
                return false;

            if (IsPrimitive)
                return true;

            DataType[] types1 = this.types;
            DataType[] types2 = other.types;

            if (types1.Length != types2.Length)
                return false;

            for (int i = 0; i < types1.Length; i++)
            {
                if (!types1[i].Equals(types2[i]))
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DataType))
                return false;

            return this.Equals((DataType)obj);
        }

        public override int GetHashCode()
        {
            if (!cachedHashCode.HasValue)
                cachedHashCode = InternalGetHashCode();

            return cachedHashCode.Value;
        }

        public override string ToString()
        {
            if (cachedToString == null)
                cachedToString = InternalToString();

            return cachedToString;
        }

        public IEnumerator<DataType> GetEnumerator()
        {
            for (int i = 0; i < types.Length; i++)
                yield return types[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Serialize(BinaryWriter writer)
        {
            if (cachedSerialize == null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    InternalSerialize(new BinaryWriter(ms));
                    cachedSerialize = ms.ToArray();
                }
            }

            writer.Write(cachedSerialize);
        }

        public static DataType Deserialize(BinaryReader reader)
        {
            Code code = (Code)reader.ReadByte();
            if (code < Code.Slots)
                return new DataType(code, null);

            DataType[] types = new DataType[reader.ReadByte()];
            for (int i = 0; i < types.Length; i++)
                types[i] = DataType.Deserialize(reader);

            return new DataType(code, types);
        }

        public static DataType Slots(params DataType[] slots)
        {
            if (slots.Length == 0)
                throw new ArgumentException("slots.Length == 0");

            if (slots.Length > byte.MaxValue)
                throw new ArgumentException("slots.Length > 255");

            return new DataType(Code.Slots, slots);
        }

        public static DataType Array(DataType T)
        {
            return new DataType(Code.Array, T);
        }

        public static DataType List(DataType T)
        {
            return new DataType(Code.List, T);
        }

        public static DataType Dictionary(DataType TKey, DataType TValue)
        {
            return new DataType(Code.Dictionary, TKey, TValue);
        }

        public static DataType FromPrimitiveType(Type type)
        {
            return primitiveMap[type];
        }

        public static bool IsPrimitiveType(Type type)
        {
            return primitiveMap.ContainsKey(type);
        }

        public static bool operator ==(DataType type1, DataType type2)
        {
            if (Object.ReferenceEquals(type1, type2))
                return true;

            if (Object.ReferenceEquals(type1, null))
                return false;

            return type1.Equals(type2);
        }

        public static bool operator !=(DataType type1, DataType type2)
        {
            return !(type1 == type2);
        }

        private enum Code : byte
        {
            Boolean = TypeCode.Boolean,     // 3  
            Char = TypeCode.Char,           // 4
            SByte = TypeCode.SByte,         // 5
            Byte = TypeCode.Byte,           // 6
            Int16 = TypeCode.Int16,         // 7
            UInt16 = TypeCode.UInt16,       // 8
            Int32 = TypeCode.Int32,         // 9
            UInt32 = TypeCode.UInt32,       // 10
            Int64 = TypeCode.Int64,         // 11
            UInt64 = TypeCode.UInt64,       // 12
            Single = TypeCode.Single,       // 13
            Double = TypeCode.Double,       // 14
            Decimal = TypeCode.Decimal,     // 15
            DateTime = TypeCode.DateTime,   // 16
            String = TypeCode.String,       // 18
            ByteArray = 19,

            Slots = 20,
            Array = 21,
            List = 22,
            Dictionary = 23
        }
    }
}