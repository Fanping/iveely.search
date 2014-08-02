using Iveely.STSdb4.General.Comparers;
using Iveely.STSdb4.General.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Iveely.STSdb4.Data
{
    public class EqualityComparer<T> : IEqualityComparer<T>
    {
        public readonly Func<T, T, bool> equals;
        public readonly Func<T, int> getHashCode;

        public Expression<Func<T, T, bool>> LambdaEquals { get; private set; }
        public Expression<Func<T, int>> LambdaGetHashCode { get; private set; }

        public Func<Type, MemberInfo, int> MembersOrder { get; private set; }

        public Type Type { get; private set; }
        public CompareOption[] CompareOptions { get; private set; }

        public EqualityComparer(CompareOption[] compareOptions, Func<Type, MemberInfo, int> membersOrder = null)
        {
            Type = typeof(T);
            CompareOptions = compareOptions;
            CompareOption.CheckCompareOptions(Type, compareOptions, membersOrder);

            MembersOrder = membersOrder;

            LambdaEquals = CreateEquals();
            equals = LambdaEquals.Compile();

            LambdaGetHashCode = CreateGetHashCode();
            getHashCode = LambdaGetHashCode.Compile();
        }

        public EqualityComparer(Func<Type, MemberInfo, int> membersOrder = null)
            : this(CompareOption.GetDefaultCompareOptions(typeof(T), membersOrder), membersOrder)
        {
        }

        private Expression<Func<T, T, bool>> CreateEquals()
        {
            var x = Expression.Parameter(Type);
            var y = Expression.Parameter(Type);

            List<Expression> list = new List<Expression>();
            var exitPoint = Expression.Label(typeof(bool));

            var body = EqualityComparerHelper.CreateEqualsBody(x, y, CompareOptions, MembersOrder);
            var lambda = Expression.Lambda<Func<T, T, bool>>(body, x, y);

            return lambda;
        }

        private Expression<Func<T, int>> CreateGetHashCode()
        {
            var obj = Expression.Parameter(Type);

            var body = EqualityComparerHelper.CreateGetHashCodeBody(obj, MembersOrder);
            var lambda = Expression.Lambda<Func<T, int>>(body, obj);

            return lambda;
        }

        public bool Equals(T x, T y)
        {
            return equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return getHashCode(obj);
        }
    }

    public static class EqualityComparerHelper
    {
        public static Expression CreateEqualsBody(Expression x, Expression y, CompareOption[] compareOptions, Func<Type, MemberInfo, int> membersOrder)
        {
            var type = x.Type;
            var exitPoint = Expression.Label(typeof(bool));

            if (DataType.IsPrimitiveType(type) || type == typeof(Guid))
                return EqualityComparerHelper.GetEqualsCommand(x, y, compareOptions[0], exitPoint, true);
            else
            {
                List<Expression> list = new List<Expression>();

                int i = 0;
                int count = DataTypeUtils.GetPublicMembers(type, membersOrder).Count();

                foreach (var member in DataTypeUtils.GetPublicMembers(type, membersOrder))
                    list.Add(GetEqualsCommand(Expression.PropertyOrField(x, member.Name), Expression.PropertyOrField(y, member.Name), compareOptions[i++], exitPoint, i == count));

                return Expression.Block(typeof(bool), list);
            }
        }

        public static Expression CreateGetHashCodeBody(Expression obj, Func<Type, MemberInfo, int> membersOrder)
        {
            var type = obj.Type;

            if (DataType.IsPrimitiveType(type) || type == typeof(Guid))
                return GetHashCodeCommand(obj);
            else
            {
                List<Expression> list = new List<Expression>();

                foreach (var member in DataTypeUtils.GetPublicMembers(type, membersOrder))
                    list.Add(GetHashCodeCommand(Expression.PropertyOrField(obj, member.Name)));

                var xor = list[0];
                for (int i = 1; i < list.Count; i++)
                    xor = Expression.ExclusiveOr(list[i], xor);

                return Expression.Block(typeof(int), Expression.Label(Expression.Label(typeof(int)), xor));
            }
        }

        private static Expression GetEqualsCommand(Expression x, Expression y, CompareOption compareOption, LabelTarget exitPoint, bool isLast)
        {
            var type = x.Type;

            if (type == typeof(Boolean) || type == typeof(Char) || type == typeof(SByte) || type == typeof(Byte) ||
                    type == typeof(Int16) || type == typeof(UInt16) || type == typeof(Int32) || type == typeof(UInt32) || type == typeof(Int64) || type == typeof(UInt64) ||
                    type == typeof(Single) || type == typeof(Double) || type == typeof(DateTime) || type == typeof(Decimal))
            {
                if (isLast)
                    return Expression.Label(exitPoint, Expression.Equal(x, y));

                return Expression.IfThen(Expression.NotEqual(x, y),
                    Expression.Return(exitPoint, Expression.Constant(false)));
            }

            if (type == typeof(string))
            {
                if (compareOption.IgnoreCase)
                {
                    var call = Expression.Call(type.GetMethod("Compare", new Type[] { typeof(string), typeof(string), typeof(bool) }), x, y, Expression.Constant(true));

                    if (isLast)
                        return Expression.Label(exitPoint, Expression.Equal(call, Expression.Constant(0, typeof(int))));

                    return Expression.IfThen(Expression.NotEqual(call, Expression.Constant(0, typeof(int))),
                        Expression.Return(exitPoint, Expression.Constant(false)));
                }
                else
                {
                    var call = Expression.Call(type.GetMethod("Equals", new Type[] { typeof(string), typeof(string) }), x, y);

                    if (isLast)
                        return Expression.Label(exitPoint, call);

                    return Expression.IfThen(Expression.Not(call), Expression.Return(exitPoint, Expression.Constant(false)));
                }
            }

            if (type == typeof(Guid))
            {
                var equalityComparerType = (compareOption.ByteOrder == ByteOrder.BigEndian) ? typeof(BigEndianByteArrayEqualityComparer) : typeof(LittleEndianByteArrayEqualityComparer);
                var call = Expression.Call(x, typeof(Guid).GetMethod("Equals", new Type[] { typeof(Guid) }), y);

                if (isLast)
                    return Expression.Label(exitPoint, call);

                return Expression.IfThen(Expression.Not(call),
                    Expression.Return(exitPoint, Expression.Constant(false)));
            }

            if (type == typeof(byte[]))
            {
                Debug.Assert(compareOption.ByteOrder != ByteOrder.Unspecified);

                var equalityComparerType = (compareOption.ByteOrder == ByteOrder.BigEndian) ? typeof(BigEndianByteArrayEqualityComparer) : typeof(LittleEndianByteArrayEqualityComparer);
                var call = Expression.Call(Expression.Field(null, equalityComparerType, "Instance"), equalityComparerType.GetMethod("Equals", new Type[] { typeof(byte[]), typeof(byte[]) }), x, y);

                if (isLast)
                    return Expression.Label(exitPoint, call);

                return Expression.IfThen(Expression.Not(call),
                    Expression.Return(exitPoint, Expression.Constant(false)));
            }

            throw new NotSupportedException(type.ToString());
        }

        private static Expression GetHashCodeCommand(Expression value)
        {
            var type = value.Type;

            if (DataType.IsPrimitiveType(type))
            {
                //return (int)value;
                if (type == typeof(ushort))
                    return Expression.Label(Expression.Label(typeof(int)), Expression.Convert(value, typeof(int)));

                //return (int)((ushort)value) | (int)value << 16;
                if (type == typeof(short))
                    return Expression.Label(Expression.Label(typeof(int)), Expression.Or(Expression.Convert(Expression.Convert(value, typeof(ushort)), typeof(int)), Expression.LeftShift(Expression.Convert(value, typeof(int)), Expression.Constant(16, typeof(int)))));

                //return value;
                if (type == typeof(int))
                    return Expression.Label(Expression.Label(typeof(int)), value);

                //return value;
                if (type == typeof(uint))
                    return Expression.Label(Expression.Label(typeof(int)), Expression.Convert(value, typeof(int)));

                //return (int)value ^ (int)(value >> 32)
                if (type == typeof(long) || type == typeof(ulong))
                    return Expression.Label(Expression.Label(typeof(int)), Expression.ExclusiveOr(Expression.Convert(value, typeof(int)), Expression.Convert(Expression.RightShift(value, Expression.Constant(32)), typeof(int))));

                if (type == typeof(byte[]))
                    return Expression.Call(typeof(ByteArrayExtensions).GetMethod("GetHashCodeEx", new Type[] { typeof(byte[]) }), value);

                return Expression.Call(value, type.GetMethod("GetHashCode"));
            }

            if (type == typeof(Guid))
                return Expression.Call(value, typeof(Guid).GetMethod("GetHashCode"));

            throw new NotSupportedException(type.ToString());
        }
    }

    #region Examples

    //public class Bar
    //{
    //    public string Name;
    //    public int Value;
    //    public long LongValue;
    //    public double Percents;
    //}

    //public class Expamples : IEqualityComparer<Bar>
    //{
    //    public bool Equals(Bar x, Bar y)
    //    {
    //        if (string.Equals(x.Name, y.Name))
    //            return false;

    //        if (x.Value != y.Value)
    //            return false;

    //        if (x.LongValue != y.LongValue)
    //            return false;

    //        return x.Percents == y.Percents;
    //    }

    //    public int GetHashCode(Bar obj)
    //    {
    //        return obj.Name.GetHashCode() ^ obj.Value ^ (int)obj.LongValue ^ ((int)obj.LongValue >> 32) ^ obj.Percents.GetHashCode();
    //    }
    //}

    #endregion
}