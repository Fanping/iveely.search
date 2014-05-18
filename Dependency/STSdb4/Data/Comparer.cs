using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Iveely.General.Extensions;
using System.Reflection;
using Iveely.General.Comparers;
using System.Diagnostics;

namespace Iveely.Data
{
    public class Comparer<T> : IComparer<T>
    {
        public CompareOption[] CompareOptions { get; private set; }
        public Type Type { get; private set; }
        public Func<Type, MemberInfo, int> MembersOrder { get; private set; }

        public readonly Func<T, T, int> compare;
        public Expression<Func<T, T, int>> LambdaCompare { get; private set; }

        public Comparer(CompareOption[] compareOptions, Func<Type, MemberInfo, int> membersOrder = null)
        {
            Type = typeof(T);
            CompareOptions = compareOptions;
            MembersOrder = membersOrder;

            CompareOption.CheckCompareOptions(Type, compareOptions, membersOrder);

            LambdaCompare = CreateCompareMethod();
            compare = LambdaCompare.Compile();
        }

        public Comparer(Func<Type, MemberInfo, int> memberOrder = null)
            : this(CompareOption.GetDefaultCompareOptions(typeof(T), memberOrder), memberOrder)
        {
        }

        private Expression<Func<T, T, int>> CreateCompareMethod()
        {
            var x = Expression.Parameter(typeof(T));
            var y = Expression.Parameter(typeof(T));

            return Expression.Lambda<Func<T, T, int>>(ComparerHelper.CreateComparerBody(null, null, x, y, CompareOptions, MembersOrder), x, y);
        }

        public int Compare(T x, T y)
        {
            return compare(x, y);
        }
    }

    internal static class ComparerHelper
    {
        internal static Expression CreateComparerBody(List<Expression> expressions, List<ParameterExpression> parameters, Expression x, Expression y, CompareOption[] compareOptions, Func<Type, MemberInfo, int> membersOrder)
        {
            var exitPoint = Expression.Label(typeof(int));
            List<Expression> list = new List<Expression>();
            List<ParameterExpression> variables = new List<ParameterExpression>();
            bool haveCmp = false;

            if (expressions != null)
                foreach (var expression in expressions)
                    list.Add(expression);

            if (parameters != null)
                foreach (var parameter in parameters)
                    variables.Add(parameter);

            if (DataType.IsPrimitiveType(x.Type) || x.Type == typeof(Guid))
                foreach (var command in GetCompareCommands(x, y, Expression.Variable(typeof(int)), exitPoint, x.Type, compareOptions[0], true))
                    list.Add(command);
            else
            {
                int i = 0;
                var cmp = Expression.Variable(typeof(int));
                foreach (var field in DataTypeUtils.GetPublicMembers(x.Type, membersOrder))
                {
                    if (!haveCmp && (field.GetPropertyOrFieldType() == typeof(char) || field.GetPropertyOrFieldType() == typeof(byte[]) || field.GetPropertyOrFieldType() == typeof(Decimal) || field.GetPropertyOrFieldType() == typeof(String)))
                    {
                        haveCmp = true;
                        variables.Add(cmp);
                    }

                    foreach (var command in GetCompareCommands(Expression.PropertyOrField(x, field.Name), Expression.PropertyOrField(y, field.Name), cmp, exitPoint, field.GetPropertyOrFieldType(), compareOptions[i++], i == DataTypeUtils.GetPublicMembers(x.Type, membersOrder).Count()))
                        list.Add(command);
                }
            }

            return Expression.Block(typeof(int), variables, list);
        }


        private static IEnumerable<Expression> GetCompareCommands(Expression x, Expression y, ParameterExpression cmp, LabelTarget exitPoint, Type type, CompareOption compareOption, bool isLastCompare)
        {
            var field1 = x;
            var field2 = y;

            var invertCompare = compareOption.SortOrder == SortOrder.Descending;

            if (type == typeof(bool))
            {
                //if (field1 != field2)
                //{
                //    if (!field1)
                //        return -1;
                //    else
                //        return 1;
                //}

                int less = !invertCompare ? -1 : 1;
                int greater = !invertCompare ? 1 : -1;

                yield return Expression.IfThen(Expression.NotEqual(field1, field2),
                               Expression.IfThenElse(Expression.Not(field1),
                                   Expression.Return(exitPoint, Expression.Constant(less)),
                                   Expression.Return(exitPoint, Expression.Constant(greater))));

                if (isLastCompare)
                    yield return Expression.Label(exitPoint, Expression.Constant(0));
            }
            else if(type == typeof(Guid))
            {
                if (!isLastCompare)
                {
                    //cmp = field1.CompareTo(field2);
                    //if (cmp != 0)
                    //    return cmp;

                    yield return Expression.Assign(cmp, Expression.Call(field1, typeof(Guid).GetMethod("CompareTo", new Type[] { typeof(Guid) }), field2));
                    yield return Expression.IfThen(Expression.NotEqual(cmp, Expression.Constant(0)),
                                   Expression.Return(exitPoint, cmp));
                }
                else
                {
                    //return field1.CompareTo(field2);

                    yield return Expression.Label(exitPoint, Expression.Call(field1, typeof(Guid).GetMethod("CompareTo", new Type[] { typeof(Guid) }), field2));
                }
            }
            else if (type == typeof(byte[]))
            {
                Debug.Assert(compareOption.ByteOrder != ByteOrder.Unspecified);

                var order = compareOption.ByteOrder;
                var comparerType = (order == ByteOrder.BigEndian) ? typeof(BigEndianByteArrayComparer) : typeof(LittleEndianByteArrayComparer);
                var instance = Expression.Field(null, comparerType, "Instance");
                var compare = comparerType.GetMethod("Compare", new Type[] { typeof(byte[]), typeof(byte[]) });
                var call = !invertCompare ? Expression.Call(instance, compare, field1, field2) : Expression.Call(instance, compare, field2, field1);

                if (!isLastCompare)
                {
                    //cmp = BigEndianByteArrayComparer.Instance.Compare(field1, field2);
                    //if (cmp != 0)
                    //    return cmp;

                    yield return Expression.Assign(cmp, call);
                    yield return Expression.IfThen(Expression.NotEqual(cmp, Expression.Constant(0)),
                                   Expression.Return(exitPoint, cmp));
                }
                else
                {
                    //return BigEndianByteArrayComparer.Instance.Compare(field1, field2); 

                    yield return Expression.Label(exitPoint, call);
                }
            }
            else if (type == typeof(char))
            {
                var int1 = Expression.Convert(field1, typeof(int));
                var int2 = Expression.Convert(field2, typeof(int));

                var substract = !invertCompare ? Expression.Subtract(int1, int2) : Expression.Subtract(int2, int1);

                if (!isLastCompare)
                {
                    //cmp = (int)field1 - (int)field2;
                    //if (cmp != 0)
                    //    return cmp;

                    yield return Expression.Assign(cmp, substract);
                    yield return Expression.IfThen(Expression.NotEqual(cmp, Expression.Constant(0)),
                                   Expression.Return(exitPoint, cmp));
                }
                else
                {
                    //return field1 - field2;

                    yield return Expression.Label(exitPoint, substract);
                }
            }
            else if (type == typeof(DateTime))
            {
                //long ticks1 = field1.Ticks;
                //long ticks2 = field2.Ticks;
                //if (ticks1 < ticks2)
                //    return -1;
                //else if (ticks1 > ticks2)
                //    return 1;

                var ticks1 = Expression.Variable(typeof(long));
                var ticks2 = Expression.Variable(typeof(long));

                var assign1 = Expression.Assign(ticks1, Expression.Property(field1, "Ticks"));
                var assign2 = Expression.Assign(ticks2, Expression.Property(field2, "Ticks"));

                int less = !invertCompare ? -1 : 1;
                int greater = !invertCompare ? 1 : -1;

                var @if = Expression.IfThenElse(Expression.LessThan(ticks1, ticks2),
                                Expression.Return(exitPoint, Expression.Constant(less)),
                                Expression.IfThen(Expression.GreaterThan(ticks1, ticks2),
                                    Expression.Return(exitPoint, Expression.Constant(greater))));

                yield return Expression.Block(new ParameterExpression[] { ticks1, ticks2 }, assign1, assign2, @if);

                if (isLastCompare)
                    yield return Expression.Label(exitPoint, Expression.Constant(0));
            }
            else if (type == typeof(Decimal))
            {
                var type_ = typeof(System.Collections.Generic.Comparer<>).MakeGenericType(typeof(decimal));
                var @default = Expression.Property(null, type_, "Default");
                var compare = type_.GetProperty("Default").PropertyType.GetMethod("Compare", new Type[] { typeof(decimal), typeof(decimal) });
                var call = !invertCompare ? Expression.Call(@default, compare, field1, field2) : Expression.Call(@default, compare, field2, field1);

                if (!isLastCompare)
                {
                    //cmp = Comparer<T>.Default.Compare(field1, field2);
                    //if (cmp != 0)
                    //    return cmp;

                    yield return Expression.Assign(cmp, call);
                    yield return Expression.IfThen(Expression.NotEqual(cmp, Expression.Constant(0)),
                                   Expression.Return(exitPoint, cmp));
                }
                else
                {
                    //return Comparer<T>.Default.Compare(field1, field2);

                    yield return Expression.Label(exitPoint, call);
                }
            }
            else if (type == typeof(String))
            {
                var type_ = typeof(string);
                var compare = type_.GetMethod("Compare", new Type[] { typeof(string), typeof(string), typeof(bool) });
                bool optionIgnoreCase = compareOption.IgnoreCase;

                var ignoreCase = optionIgnoreCase ? Expression.Constant(optionIgnoreCase) : Expression.Constant(false);
                var call = !invertCompare ? Expression.Call(compare, field1, field2, ignoreCase) : Expression.Call(compare, field2, field1, ignoreCase);

                if (!isLastCompare)
                {
                    //cmp = String.Compare(field1, field2, ignoreCase);
                    //if (cmp != 0)
                    //    return cmp;

                    yield return Expression.Assign(cmp, call);
                    yield return Expression.IfThen(Expression.NotEqual(cmp, Expression.Constant(0)),
                                   Expression.Return(exitPoint, cmp));
                }
                else
                {
                    //return String.Compare(field1, field2, ignoreCase);

                    yield return Expression.Label(exitPoint, call);
                }
            }
            else if (type == typeof(SByte) ||
                     type == typeof(Byte) ||
                     type == typeof(Int16) ||
                     type == typeof(Int32) ||
                     type == typeof(UInt32) ||
                     type == typeof(UInt16) ||
                     type == typeof(Int64) ||
                     type == typeof(UInt64) ||
                     type == typeof(Single) ||
                     type == typeof(Double))
            {
                //if (field1 < field2)
                //    return -1;
                //else if (field1 > field2)
                //    return 1;

                int less = !invertCompare ? -1 : 1;
                int greater = !invertCompare ? 1 : -1;

                yield return Expression.IfThenElse(Expression.LessThan(field1, field2),
                                Expression.Return(exitPoint, Expression.Constant(less, typeof(int))),
                                Expression.IfThen(Expression.GreaterThan(field1, field2),
                                   Expression.Return(exitPoint, Expression.Constant(greater))));

                if (isLastCompare)
                    yield return Expression.Label(exitPoint, Expression.Constant(0));
            }
            else
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

    //public class Expamples : IComparer<Bar>
    //{
    //    public int Compare(Bar x, Bar y)
    //    {
    //        int cmp;

    //        cmp = string.Compare(x.Name,y.Name,false);
    //        if (cmp != 0)
    //            return cmp;

    //        if (x.Value < y.Value)
    //            return -1;
    //        else if (x.Value > y.Value)
    //            return 1;

    //        if (x.LongValue < y.LongValue)
    //            return -1;
    //        else if (x.LongValue > y.LongValue)
    //            return 1;

    //        if (x.Percents < y.Percents)
    //            return -1;
    //        else if (x.Percents > y.Percents)
    //            return 1;

    //        return 0;
    //    }
    //}

    #endregion
}
