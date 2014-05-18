using Iveely.General.Comparers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Iveely.General.Extensions;

namespace Iveely.Data
{
    public class Transformer<T1, T2> : ITransformer<T1, T2>
    {
        public readonly Func<T1, T2> to;
        public readonly Func<T2, T1> from;

        public Type Type1 { get; private set; }
        public Type Type2 { get; private set; }

        public Func<Type, MemberInfo, int> MembersOrder1 { get; private set; }
        public Func<Type, MemberInfo, int> MembersOrder2 { get; private set; }

        public Expression<Func<T1, T2>> LambdaTo { get; private set; }
        public Expression<Func<T2, T1>> LambdaFrom { get; private set; }

        public Transformer(Func<Type, MemberInfo, int> membersOrder1 = null, Func<Type, MemberInfo, int> membersOrder2 = null)
        {
            if (!TransformerHelper.CheckCompatible(typeof(T1), typeof(T2), new HashSet<Type>(), membersOrder1, membersOrder2))
                throw new ArgumentException(String.Format("{0} not compatible with {1}", typeof(T1).ToString(), typeof(T2).ToString()));

            Type1 = typeof(T1);
            Type2 = typeof(T2);

            MembersOrder1 = membersOrder1;
            MembersOrder2 = membersOrder2;

            LambdaTo = CreateToMethod();
            to = LambdaTo.Compile();

            LambdaFrom = CreateFromMethod();
            from = LambdaFrom.Compile();
        }

        private Expression<Func<T1, T2>> CreateToMethod()
        {
            var value1 = Expression.Parameter(Type1);
            var value2 = Expression.Variable(Type2);

            List<Expression> list = new List<Expression>();
            if (TransformerHelper.IsEqualsTypes(Type1, Type2))
                list.Add(Expression.Label(Expression.Label(Type1), value1));
            else
            {
                list.Add(TransformerHelper.BuildBody(value2, value1, MembersOrder2, MembersOrder1));
                list.Add(Expression.Label(Expression.Label(Type2), value2));
            }

            return Expression.Lambda<Func<T1, T2>>(TransformerHelper.IsEqualsTypes(Type1, Type2) ? list[0] : Expression.Block(typeof(T2), new ParameterExpression[] { value2 }, list), value1);
        }

        private Expression<Func<T2, T1>> CreateFromMethod()
        {
            var value2 = Expression.Parameter(Type2);
            var value1 = Expression.Variable(Type1);

            List<Expression> list = new List<Expression>();
            if (TransformerHelper.IsEqualsTypes(Type1, Type2))
                list.Add(Expression.Label(Expression.Label(Type2), value2));
            else
            {
                list.Add(TransformerHelper.BuildBody(value1, value2, MembersOrder1, MembersOrder2));
                list.Add(Expression.Label(Expression.Label(Type1), value1));
            }

            return Expression.Lambda<Func<T2, T1>>(TransformerHelper.IsEqualsTypes(Type1, Type2) ? list[0] : Expression.Block(typeof(T1), new ParameterExpression[] { value1 }, list), value2);
        }


        public T2 To(T1 value1)
        {
            return to(value1);
        }

        public T1 From(T2 value2)
        {
            return from(value2);
        }
    }

    public static class TransformerHelper
    {
        public static Expression BuildBody(Expression Value1, Expression Value2, Func<Type, MemberInfo, int> membersOrder1, Func<Type, MemberInfo, int> membersOrder2)
        {
            var type1 = Value1.Type;
            var type2 = Value2.Type;

            if (type1 == typeof(Guid) || type2 == typeof(Guid))
                return Expression.Assign(Value1,
                        type1 == typeof(Guid) ?
                        Value2.Type == typeof(Guid) ? Value2 : Expression.New(type1.GetConstructor(new Type[] { typeof(byte[]) }), Value2) :
                            Expression.Call(Value2, type2.GetMethod("ToByteArray"))
                    );

            if (type1.IsEnum || type2.IsEnum)
                return Expression.Assign(Value1, Expression.Convert(Value2, type1));

            if (IsEqualsTypes(type1, type2))
                return Expression.Assign(Value1, Value2);

            if (IsNumberType(type1) && IsNumberType(type2))
                return Expression.Assign(Value1, Expression.Convert(Value2, type1));

            if (type1.IsKeyValuePair())
            {
                var key = Expression.Variable(type1.GetGenericArguments()[0]);
                var value = Expression.Variable(type1.GetGenericArguments()[1]);

                return Expression.Assign(Value1,
                    Expression.New((typeof(KeyValuePair<,>).MakeGenericType(key.Type, value.Type)).GetConstructor(new Type[] { key.Type, value.Type }),
                        Expression.Block(key.Type,
                            new ParameterExpression[] { key },
                            BuildBody(key, Expression.PropertyOrField(Value2, type2.IsKeyValuePair() ? "Key" : DataTypeUtils.GetPublicMembers(Value2.Type, membersOrder2).First().Name), membersOrder1, membersOrder2),
                            Expression.Label(Expression.Label(key.Type), key)),
                        Expression.Block(value.Type,
                            new ParameterExpression[] { value },
                            BuildBody(value, Expression.PropertyOrField(Value2, type2.IsKeyValuePair() ? "Value" : DataTypeUtils.GetPublicMembers(Value2.Type, membersOrder2).Last().Name), membersOrder1, membersOrder2),
                            Expression.Label(Expression.Label(value.Type), value))
                    ));
            }

            if (type1.IsList() || type1.IsArray)
            {
                var element = Expression.Variable(type1.IsArray ? type1.GetElementType() : type1.GetGenericArguments()[0]);

                var block = Expression.Block(new ParameterExpression[] { element },
                    Expression.Assign(Value1, Expression.New(Value1.Type.GetConstructor(new Type[] { typeof(int) }), Expression.PropertyOrField(Value2, type2.IsList() ? "Count" : "Length"))),
                    Value2.For(i =>
                    {
                        return type2.IsList() ?
                            (Expression)Expression.Call(Value1, type1.GetMethod("Add"), BuildBody(element, Value2.This(i), membersOrder1, membersOrder2)) :
                            Expression.Assign(Expression.ArrayAccess(Value1, i), BuildBody(element, Expression.ArrayAccess(Value2, i), membersOrder1, membersOrder2));
                    },
                    Expression.Label())
                    );

                return Expression.IfThenElse(Expression.NotEqual(Value2, Expression.Constant(null)),
                    block,
                    Expression.Assign(Value1, Expression.Constant(null, Value1.Type)));
            }

            if (type1.IsDictionary())
            {
                if (!DataType.IsPrimitiveType(type1.GetGenericArguments()[0]) && type1.GetGenericArguments()[0] == typeof(Guid))
                    throw new NotSupportedException(String.Format("Dictionary<{0}, TValue>", type1.GetGenericArguments()[0]));

                var key = Expression.Variable(type1.GetGenericArguments()[0]);
                var value = Expression.Variable(type1.GetGenericArguments()[1]);

                var block = Expression.Block(new ParameterExpression[] { key, value },
                    Expression.Assign(Value1, type2.GetGenericArguments()[0] == typeof(byte[]) ?
                        Expression.New(type1.GetConstructor(new Type[] { typeof(int), typeof(IEqualityComparer<byte[]>) }), Expression.PropertyOrField(Value2, "Count"), Expression.Field(null, typeof(BigEndianByteArrayEqualityComparer), "Instance")) :
                        Expression.New(type1.GetConstructor(new Type[] { typeof(int) }), Expression.PropertyOrField(Value2, "Count"))),
                    Value2.ForEach(current =>
                    Expression.Call(Value1, type1.GetMethod("Add"),
                        BuildBody(key, Expression.Property(current, "Key"), membersOrder1, membersOrder2),
                        BuildBody(value, Expression.Property(current, "Value"), membersOrder1, membersOrder2)),
                    Expression.Label()
                    ));

                return Expression.IfThenElse(Expression.NotEqual(Value2, Expression.Constant(null)),
                    block,
                    Expression.Assign(Value1, Expression.Constant(null, Value1.Type)));
            }

            if (type1.IsNullable())
            {
                var data1Var = Expression.Variable(Value1.Type);
                var data2Var = Expression.Variable(Value2.Type);

                List<Expression> list = new List<Expression>();

                var constructParam = Expression.PropertyOrField(data2Var, type2.IsNullable() ? "Value" : DataTypeUtils.GetPublicMembers(type2, membersOrder2).First().Name);

                var block = Expression.Block(new ParameterExpression[] { data1Var, data2Var },
                        Expression.Assign(data2Var, Value2),
                        Expression.Assign(data1Var, Expression.New(
                            type1.GetConstructor(new Type[] { type1.GetGenericArguments()[0] }),
                                constructParam.GetType() == type1.GetGenericArguments()[0] ?
                                (Expression)constructParam :
                                (Expression)Expression.Convert(constructParam, type1.GetGenericArguments()[0]))),
                        Expression.Assign(Value1, data1Var)
                    );

                return Expression.IfThenElse(Expression.NotEqual(Value2, Expression.Constant(null, type2)),
                        block,
                        Expression.Assign(Value1, Expression.Constant(null, type1))
                    );
            }

            if (type1.IsClass || type1.IsStruct())
            {
                var data1Var = Expression.Variable(Value1.Type);
                var data2Var = Expression.Variable(Value2.Type);

                List<Expression> list = new List<Expression>();
                list.Add(Expression.Assign(data1Var, Expression.New(data1Var.Type)));

                List<MemberInfo> members1 = DataTypeUtils.GetPublicMembers(Value1.Type, membersOrder1).ToList();

                List<MemberInfo> members2 = new List<MemberInfo>();
                if (type2.IsKeyValuePair() || type2.IsNullable())
                {
                    if (type2.IsKeyValuePair())
                        members2.Add(type2.GetMember("Key")[0]);

                    members2.Add(type2.GetMember("Value")[0]);
                }
                else
                    members2 = DataTypeUtils.GetPublicMembers(Value2.Type, membersOrder2).ToList();

                for (int i = 0; i < members1.Count; i++)
                    list.Add(BuildBody(Expression.PropertyOrField(data1Var, members1[i].Name), Expression.PropertyOrField(data2Var, members2[i].Name), membersOrder1, membersOrder2));

                list.Add(Expression.Assign(Value1, data1Var));

                if ((type1.IsStruct() || type2.IsStruct()) && !type2.IsNullable())
                {
                    list.Insert(0, Expression.Assign(data2Var, Value2));
                    list.Add(Expression.Label(Expression.Label(Value1.Type), Value1));
                    return Expression.Block(type1, new ParameterExpression[] { data1Var, data2Var }, list);
                }

                return Expression.Block(type1, new ParameterExpression[] { data2Var },
                    Expression.Assign(data2Var, Value2),
                    Expression.IfThenElse(Expression.NotEqual(data2Var, Expression.Constant(null)),
                        Expression.Block(new ParameterExpression[] { data1Var }, list),
                        Expression.Assign(Value1, Expression.Constant(null, type1))),
                    Expression.Label(Expression.Label(type1), Value1)
                    );
            }

            throw new NotSupportedException(type1.ToString());
        }

        public static bool CheckCompatible(Type type1, Type type2, HashSet<Type> cycleCheck, Func<Type, MemberInfo, int> membersOrder1 = null, Func<Type, MemberInfo, int> membersOrder2 = null)
        {
            if (type1 == typeof(Guid) || type1 == typeof(byte[]))
                return type2 == typeof(Guid) || type2 == typeof(byte[]);

            if (type1.IsEnum || type2.IsEnum)
                return (type1.IsEnum && type2.IsEnum) || (IsIntegerType(type1) || IsIntegerType(type2));

            if (DataType.IsPrimitiveType(type1))
                return (type1 == type2) || (IsNumberType(type1) && IsNumberType(type2));

            if (type1.IsArray)
                return CheckCompatible(type1.GetElementType(), type2.GetElementType(), cycleCheck, membersOrder1, membersOrder2);

            if (type1.IsList())
                return CheckCompatible(type1.GetGenericArguments()[0], type2.GetGenericArguments()[0], cycleCheck, membersOrder1, membersOrder2);

            if (type1.IsDictionary())
                return CheckCompatible(type1.GetGenericArguments()[0], type2.GetGenericArguments()[0], cycleCheck, membersOrder1, membersOrder2) && CheckCompatible(type1.GetGenericArguments()[1], type2.GetGenericArguments()[1], cycleCheck, membersOrder1, membersOrder2);

            if (type1.IsClass || type1.IsStruct())
            {
                List<Type> type1Slotes = new List<Type>();
                List<Type> type2Slotes = new List<Type>();

                if (type1.IsNullable())
                    type1Slotes.Add(type1.GetGenericArguments()[0]);

                if (type2.IsNullable())
                    type2Slotes.Add(type2.GetGenericArguments()[0]);

                if (type1.IsKeyValuePair())
                {
                    type1Slotes.Add(type1.GetGenericArguments()[0]);
                    type1Slotes.Add(type1.GetGenericArguments()[1]);
                }

                if (type2.IsKeyValuePair())
                {
                    type2Slotes.Add(type2.GetGenericArguments()[0]);
                    type2Slotes.Add(type2.GetGenericArguments()[1]);
                }

                foreach (var slote in DataTypeUtils.GetPublicMembers(type1, membersOrder1))
                    type1Slotes.Add(slote.GetPropertyOrFieldType());

                foreach (var slote in DataTypeUtils.GetPublicMembers(type2, membersOrder2))
                    type2Slotes.Add(slote.GetPropertyOrFieldType());

                if (type1Slotes.Count != type2Slotes.Count)
                    return false;

                for (int i = 0; i < type1Slotes.Count; i++)
                {
                    if (cycleCheck.Contains(type1Slotes[i]))
                        throw new NotSupportedException(String.Format("Type {0} has cycle declaration.", type1Slotes[i]));

                    cycleCheck.Add(type1Slotes[i]);
                    if (!CheckCompatible(type1Slotes[i], type2Slotes[i], cycleCheck, membersOrder1, membersOrder2))
                        return false;
                    cycleCheck.Remove(type1Slotes[i]);
                }

                return true;
            }

            throw new NotSupportedException(type2.ToString());
        }

        internal static bool IsNumberType(Type type)
        {
            return IsIntegerType(type) || IsDecimalType(type);
        }

        internal static bool IsIntegerType(Type type)
        {
            return type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(short) || type == typeof(ushort) || type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong);
        }

        internal static bool IsDecimalType(Type type)
        {
            return type == typeof(float) || type == typeof(double) || type == typeof(decimal);
        }

        internal static bool IsEqualsTypes(Type type1, Type type2)
        {
            if (type1.IsArray && type2.IsArray)
                return IsEqualsTypes(type1.GetElementType(), type2.GetElementType());

            if (type1.IsList() && type2.IsList())
                return IsEqualsTypes(type1.GetGenericArguments()[0], type2.GetGenericArguments()[0]);

            if ((type1.IsDictionary() && type2.IsDictionary()) || (type1.IsKeyValuePair() && type2.IsKeyValuePair()))
                return IsEqualsTypes(type1.GetGenericArguments()[0], type2.GetGenericArguments()[0]) && IsEqualsTypes(type1.GetGenericArguments()[1], type2.GetGenericArguments()[1]);

            if (type1 != type2)
                return false;

            return true;
        }
    }
}
