using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iveely.General.Collections;
using Iveely.General.Comparers;
using Iveely.General.Extensions;
using System.IO;
using System.Reflection;

namespace Iveely.Data
{
    public class CompareOption : IEquatable<CompareOption>
    {
        public SortOrder SortOrder { get; private set; }
        public ByteOrder ByteOrder { get; private set; }
        public bool IgnoreCase { get; private set; }

        private CompareOption(SortOrder sortOrder, ByteOrder byteOrder, bool ignoreCase)
        {
            this.SortOrder = sortOrder;
            this.ByteOrder = byteOrder;
            this.IgnoreCase = ignoreCase;
        }

        public CompareOption(SortOrder sortOrder)
            : this(sortOrder, ByteOrder.Unspecified, false)
        {
        }

        public CompareOption(SortOrder sortOrder, ByteOrder byteOrder)
            : this(sortOrder, byteOrder, false)
        {
        }

        public CompareOption(ByteOrder byteOrder)
            : this(SortOrder.Ascending, byteOrder)
        {
        }

        public CompareOption(SortOrder sortOrder, bool ignoreCase)
            : this(sortOrder, ByteOrder.Unspecified, ignoreCase)
        {
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((byte)SortOrder);
            writer.Write((byte)ByteOrder);
            writer.Write(IgnoreCase);
        }

        public static CompareOption Deserialize(BinaryReader reader)
        {
            var sortOrder = (SortOrder)reader.ReadByte();
            var byteOrder = (ByteOrder)reader.ReadByte();
            var ignoreCase = reader.ReadBoolean();

            return new CompareOption(sortOrder, byteOrder, ignoreCase);
        }

        public bool Equals(CompareOption other)
        {
            return this.SortOrder == other.SortOrder && this.ByteOrder == other.ByteOrder && this.IgnoreCase == other.IgnoreCase;
        }

        #region Utils

        public static CompareOption GetDefaultCompareOption(Type type)
        {
            if (type == typeof(byte[]))
                return new CompareOption(SortOrder.Ascending, ByteOrder.BigEndian);

            if (type == typeof(String))
                return new CompareOption(SortOrder.Ascending, false);

            return new CompareOption(SortOrder.Ascending);
        }

        public static CompareOption[] GetDefaultCompareOptions(Type type, Func<Type, MemberInfo, int> memberOrder = null)
        {
            if (DataType.IsPrimitiveType(type))
                return new CompareOption[] { GetDefaultCompareOption(type) };

            if (type == typeof(Guid))
                return new CompareOption[] { GetDefaultCompareOption(type) };

            if (type.IsClass || type.IsStruct())
                return DataTypeUtils.GetPublicMembers(type, memberOrder).Select(x => GetDefaultCompareOption(x.GetPropertyOrFieldType())).ToArray();

            throw new NotSupportedException(type.ToString());
        }

        public static void CheckCompareOption(Type type, CompareOption option)
        {
            if (!DataType.IsPrimitiveType(type) && type != typeof(Guid))
                throw new NotSupportedException(String.Format("The type '{0}' is not primitive.", type));

            if (type == typeof(string))
            {
                if (option.ByteOrder != ByteOrder.Unspecified)
                    throw new ArgumentException("String can't have ByteOrder option.");
            }
            else if (type == typeof(byte[]))
            {
                if (option.ByteOrder == ByteOrder.Unspecified)
                    throw new ArgumentException("byte[] must have ByteOrder option.");
            }
            else
            {
                if (option.ByteOrder != ByteOrder.Unspecified)
                    throw new ArgumentException(String.Format("{0} does not support ByteOrder option.", type));
            }
        }

        public static void CheckCompareOptions(Type type, CompareOption[] compareOptions, Func<Type, MemberInfo, int> memberOrder = null)
        {
            if (type.IsClass || type.IsStruct())
            {
                int i = 0;
                foreach (var member in DataTypeUtils.GetPublicMembers(type, memberOrder).Select(x => x.GetPropertyOrFieldType()).ToArray())
                    CheckCompareOption(member, compareOptions[i++]);
            }
            else
                CheckCompareOption(type, compareOptions[0]);
        }

        #endregion
    }
}
