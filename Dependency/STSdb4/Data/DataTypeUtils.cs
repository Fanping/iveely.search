using Iveely.STSdb4.General.Comparers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Iveely.STSdb4.General.Extensions;
using System.Text;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Iveely.STSdb4.Data
{
    public static class DataTypeUtils
    {
        //dataType -> anonymous type
        private static readonly ConcurrentDictionary<DataType, Type> cacheAnonymousTypes = new ConcurrentDictionary<DataType, Type>();

        //Type -> true/false
        private static readonly ConcurrentDictionary<Type, bool> cacheIsAnonymousTypes = new ConcurrentDictionary<Type, bool>();

        public static IEnumerable<MemberInfo> GetPublicMembers(Type type, Func<Type, MemberInfo, int> membersOrder = null)
        {
            var members = type.GetPublicReadWritePropertiesAndFields();
            if (membersOrder == null)
                return members;

            return members.Where(x => membersOrder(type, x) >= 0).OrderBy(x => membersOrder(type, x));
        }

        public static bool IsAllPrimitive(Type type, Func<Type, MemberInfo, int> membersOrder = null)
        {
            if (DataType.IsPrimitiveType(type))
                return true;

            if (type.IsArray || type.IsList() || type.IsDictionary() || type.IsKeyValuePair() || type.IsNullable() || type == typeof(Guid))
                return false;

            foreach (var member in GetPublicMembers(type, membersOrder))
            {
                if (!DataType.IsPrimitiveType(member.GetPropertyOrFieldType()))
                    return false;
            }

            return true;
        }

        private static bool InternalIsAnonymousType(Type type, Func<Type, MemberInfo, int> membersOrder = null)
        {
            if (DataType.IsPrimitiveType(type))
                return true;

            if (type.IsEnum || type == typeof(Guid))
                return false;

            if (type.IsNullable())
                return false;

            if (type.IsKeyValuePair())
                return false;

            if (type.IsArray)
                return InternalIsAnonymousType(type.GetElementType(), membersOrder);

            if (type.IsList())
                return InternalIsAnonymousType(type.GetGenericArguments()[0], membersOrder);

            if (type.IsDictionary())
                return InternalIsAnonymousType(type.GetGenericArguments()[0], membersOrder) && InternalIsAnonymousType(type.GetGenericArguments()[1], membersOrder);

            if (type.IsInheritInterface(typeof(ISlots)))
            {
                foreach (var slotType in GetPublicMembers(type, membersOrder))
                {
                    if (!InternalIsAnonymousType(slotType.GetPropertyOrFieldType(), membersOrder))
                        return false;
                }
            }

            if ((type.IsClass || type.IsStruct()) && !type.IsInheritInterface(typeof(ISlots)))
                return false;

            return true;
        }

        public static bool IsAnonymousType(Type type, Func<Type, MemberInfo, int> membersOrder = null)
        {
            if (membersOrder != null)
                return InternalIsAnonymousType(type, membersOrder);

            return cacheIsAnonymousTypes.GetOrAdd(type, (x) => InternalIsAnonymousType(x, null));
        }

        public static Type Anonymous(Type type, Func<Type, MemberInfo, int> membersOrder = null)
        {
            DataType dataType = BuildDataType(type, membersOrder);

            return BuildType(dataType);
        }

        private static DataType BuildDataType(Type type, Func<Type, MemberInfo, int> membersOrder, HashSet<Type> cycleCheck)
        {
            if (DataType.IsPrimitiveType(type))
                return DataType.FromPrimitiveType(type);

            if (type.IsEnum)
                return DataType.FromPrimitiveType(type.GetEnumUnderlyingType());

            if (type == typeof(Guid))
                return DataType.ByteArray;

            if (type.IsKeyValuePair())
            {
                return DataType.Slots(
                    BuildDataType(type.GetGenericArguments()[0], membersOrder, cycleCheck),
                    BuildDataType(type.GetGenericArguments()[1], membersOrder, cycleCheck));
            }

            if (type.IsArray)
                return DataType.Array(BuildDataType(type.GetElementType(), membersOrder, cycleCheck));

            if (type.IsList())
                return DataType.List(BuildDataType(type.GetGenericArguments()[0], membersOrder, cycleCheck));

            if (type.IsDictionary())
            {
                return DataType.Dictionary(
                    BuildDataType(type.GetGenericArguments()[0], membersOrder, cycleCheck),
                    BuildDataType(type.GetGenericArguments()[1], membersOrder, cycleCheck));
            }

            if (type.IsNullable())
                return DataType.Slots(BuildDataType(type.GetGenericArguments()[0], membersOrder, cycleCheck));

            List<DataType> slots = new List<DataType>();
            foreach (var member in GetPublicMembers(type, membersOrder))
            {
                var memberType = member.GetPropertyOrFieldType();

                if (cycleCheck.Contains(memberType))
                    throw new NotSupportedException(String.Format("Type {0} has cycle declaration.", memberType));

                cycleCheck.Add(memberType);
                DataType slot = BuildDataType(memberType, membersOrder, cycleCheck);
                cycleCheck.Remove(memberType);
                slots.Add(slot);
            }

            if (slots.Count == 0)
                throw new NotSupportedException(String.Format("{0} do not contains public read/writer properties and fields", type));

            return DataType.Slots(slots.ToArray());
        }

        public static DataType BuildDataType(Type type, Func<Type, MemberInfo, int> membersOrder = null)
        {
            if (DataType.IsPrimitiveType(type) || type.IsEnum || type == typeof(Guid) || type.IsKeyValuePair() || type.IsArray || type.IsList() || type.IsDictionary() || type.IsNullable())
                return BuildDataType(type, membersOrder, new HashSet<Type>());

            List<DataType> slots = new List<DataType>();
            foreach (var member in GetPublicMembers(type, membersOrder))
                slots.Add(BuildDataType(member.GetPropertyOrFieldType(), membersOrder, new HashSet<Type>()));

            return DataType.Slots(slots.ToArray());
        }

        private static Type InternalBuildType(DataType dataType)
        {
            if (dataType.IsPrimitive)
                return dataType.PrimitiveType;

            if (dataType.IsArray)
                return InternalBuildType(dataType[0]).MakeArrayType();

            if (dataType.IsList)
                return typeof(List<>).MakeGenericType(InternalBuildType(dataType[0]));

            if (dataType.IsDictionary)
                return typeof(Dictionary<,>).MakeGenericType(InternalBuildType(dataType[0]), InternalBuildType(dataType[1]));

            if (dataType.IsSlots)
                return SlotsBuilder.BuildType(dataType.Select(x => InternalBuildType(x)).ToArray());

            throw new NotSupportedException();
        }

        public static Type BuildType(DataType dataType)
        {
            if (dataType.IsPrimitive)
                return dataType.PrimitiveType;

            return cacheAnonymousTypes.GetOrAdd(dataType, (x) => InternalBuildType(x));
        }
    }
}
