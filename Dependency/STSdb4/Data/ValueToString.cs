using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iveely.General.Extensions;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Iveely.Data
{
    public class ValueToString<T>
    {
        public readonly Func<T, string> toString;
        public readonly Func<string, T> fromString;

        public IFormatProvider[] Providers { get; private set; }
        public char[] Delimiters { get; private set; }
        public Type Type { get; private set; }
        public Func<Type, MemberInfo, int> MembersOrder { get; private set; }

        public Expression<Func<T, string>> LambdaToString { get; private set; }
        public Expression<Func<string, T>> LambdaFromString { get; private set; }

        public ValueToString(IFormatProvider[] providers, char[] delimiters, Func<Type, MemberInfo, int> membersOrder = null)
        {
            if (!DataType.IsPrimitiveType(typeof(T)) && !typeof(T).HasDefaultConstructor())
                throw new NotSupportedException("No default constructor.");
            
            bool isSupported = DataTypeUtils.IsAllPrimitive(typeof(T));
            if (!isSupported)
                throw new NotSupportedException("Not all types are primitive.");

            var countOfType = DataType.IsPrimitiveType(typeof(T)) ? 1 : DataTypeUtils.GetPublicMembers(typeof(T), membersOrder).Count();

            if (providers.Length != countOfType)
                throw new ArgumentException("providers.Length != dataType.Length");

            Providers = providers;
            Delimiters = delimiters;

            Type = typeof(T);
            MembersOrder = membersOrder;

            LambdaFromString = CreateFromStringMethod();
            fromString = LambdaFromString.Compile();

            LambdaToString = CreateToStringMethod();
            toString = LambdaToString.Compile();
        }

        public ValueToString(char[] delimiters, Func<Type, MemberInfo, int> membersOrder = null)
            : this(StringHelper.GetDefaultProviders(typeof(T)), delimiters, membersOrder)
        {
        }

        public ValueToString(Func<Type, MemberInfo, int> membersOrder = null)
            : this(new char[] { ';' }, membersOrder)
        {
        }

        private Expression<Func<string, T>> CreateFromStringMethod()
        {
            var stringParam = Expression.Parameter(typeof(string), "item");
            List<Expression> list = new List<Expression>();

            var item = Expression.Variable(Type);

            if (!DataType.IsPrimitiveType(Type))
                list.Add(Expression.Assign(item, Expression.New(Type.GetConstructor(new Type[] { }))));

            list.Add(StringHelper.CreateFromBody(item, stringParam, Providers, Delimiters, MembersOrder));
            list.Add(Expression.Label(Expression.Label(Type), item));

            var body = Expression.Block(new ParameterExpression[] { item }, list);

            return Expression.Lambda<Func<string, T>>(body, new ParameterExpression[] { stringParam });
        }

        private Expression<Func<T, string>> CreateToStringMethod()
        {
            var item = Expression.Parameter(typeof(T));

            return Expression.Lambda<Func<T, string>>(StringHelper.CreateToBody(item, Providers, Delimiters[0], MembersOrder), new ParameterExpression[] { item });
        }

        public T FromString(string str)
        {
            return fromString(str);
        }

        public string ToString(T data)
        {
            return toString(data);
        }
    }

    public static class StringHelper
    {
        internal static Expression CreateToBody(Expression item, IFormatProvider[] providers, char delimiter, Func<Type, MemberInfo, int> membersOrder)
        {
            var stringBuilder = Expression.Variable(typeof(StringBuilder));


            if (DataType.IsPrimitiveType(item.Type))
                return Expression.Block(new ParameterExpression[] { stringBuilder },
                        Expression.Assign(stringBuilder, Expression.New(stringBuilder.Type.GetConstructor(new Type[] { }))),
                        GetAppendCommand(item, stringBuilder, providers[0]),
                        Expression.Label(Expression.Label(typeof(string)), Expression.Call(stringBuilder, typeof(object).GetMethod("ToString")))
                    );

            List<Expression> list = new List<Expression>();
            list.Add(Expression.Assign(stringBuilder, Expression.New(stringBuilder.Type.GetConstructor(new Type[] { }))));

            int i = 0;
            foreach (var member in DataTypeUtils.GetPublicMembers(item.Type, membersOrder))
            {
                list.Add(GetAppendCommand(Expression.PropertyOrField(item, member.Name), stringBuilder, providers[i]));

                if (i < item.Type.GetPublicReadWritePropertiesAndFields().Count() - 1)
                    list.Add(Expression.Call(stringBuilder, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(char) }), Expression.Constant(delimiter)));
                i++;
            }

            list.Add(Expression.Label(Expression.Label(typeof(string)), Expression.Call(stringBuilder, typeof(object).GetMethod("ToString"))));

            return Expression.Block(new ParameterExpression[] { stringBuilder }, list);
        }

        private static Expression GetAppendCommand(Expression member, ParameterExpression stringBuilder, IFormatProvider provider)
        {
            MethodCallExpression callToString;

            if (member.Type == typeof(byte[]))
            {
                var toHexMethod = typeof(ByteArrayExtensions).GetMethod("ToHex", new Type[] { typeof(byte[]) });
                callToString = Expression.Call(toHexMethod, member);
            }
            else
            {
                var toStringProvider = member.Type.GetMethod("ToString", new Type[] { typeof(IFormatProvider) });
                callToString = Expression.Call(member, toStringProvider, Expression.Constant(provider, typeof(IFormatProvider)));
            }

            var apendMethod = typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(String) });
            var callAppend = Expression.Call(stringBuilder, apendMethod, callToString);

            return callAppend;
        }

        public static Expression CreateFromBody(Expression item, ParameterExpression stringParam, IFormatProvider[] providers, char[] delimiters, Func<Type, MemberInfo, int> membersOrder)
        {
            var stringArray = Expression.Variable(typeof(string[]), "stringArray");

            if (DataType.IsPrimitiveType(item.Type))
                return Expression.Block(new ParameterExpression[] { stringArray },
                        Expression.Assign(stringArray, Expression.Call(stringParam, typeof(string).GetMethod("Split", new Type[] { typeof(char[]) }), new Expression[] { Expression.Constant(delimiters) })),
                        GetParseCommand(item, 0, stringArray, providers[0])
                       );

            List<Expression> list = new List<Expression>();
            list.Add(Expression.Assign(stringArray, Expression.Call(stringParam, typeof(string).GetMethod("Split", new Type[] { typeof(char[]) }), new Expression[] { Expression.Constant(delimiters) })));

            int i = 0;
            foreach (var member in DataTypeUtils.GetPublicMembers(item.Type, membersOrder))
                list.Add(GetParseCommand(Expression.PropertyOrField(item, member.Name), i, stringArray, providers[i++]));

            return Expression.Block(new ParameterExpression[] { stringArray }, list);
        }

        private static Expression GetParseCommand(Expression member, int index, ParameterExpression stringArray, IFormatProvider provider)
        {
            var sValue = Expression.ArrayAccess(stringArray, Expression.Constant(index));
            Expression value;

            if (member.Type == typeof(String))
            {
                value = sValue;
            }
            else if (member.Type == typeof(byte[]))
            {
                var hexParse = typeof(StringExtensions).GetMethod("ParseHex", new Type[] { typeof(string) });
                value = Expression.Call(hexParse, sValue);
            }
            else if (member.Type == typeof(char))
            {
                var parseMethod = member.Type.GetMethod("Parse", new Type[] { typeof(string) });
                value = Expression.Call(parseMethod, sValue);
            }
            else if (member.Type == typeof(bool))
            {
                var parseMethod = member.Type.GetMethod("Parse");
                value = Expression.Call(parseMethod, sValue);
            }
            else
            {
                var parseMethod = member.Type.GetMethod("Parse", new Type[] { typeof(string), typeof(IFormatProvider) });
                value = Expression.Call(parseMethod, sValue, Expression.Constant(provider, typeof(IFormatProvider)));
            }

            return Expression.Assign(member, value);
        }

        public static IFormatProvider[] GetDefaultProviders(Type type, Func<Type, MemberInfo, int> membersOrder = null)
        {
            if (DataType.IsPrimitiveType(type))
                return new IFormatProvider[] { GetDefaultProvider(type) };

            List<IFormatProvider> providers = new List<IFormatProvider>();
            foreach (var member in DataTypeUtils.GetPublicMembers(type, membersOrder))
                providers.Add(GetDefaultProvider(member.GetPropertyOrFieldType()));

            return providers.ToArray();
        }

        public static IFormatProvider GetDefaultProvider(Type type)
        {
            if (!DataType.IsPrimitiveType(type))
                throw new NotSupportedException(type.ToString());

            if (type == typeof(float) ||
                type == typeof(double) ||
                type == typeof(decimal))
            {
                NumberFormatInfo numberFormat = new NumberFormatInfo();
                numberFormat.CurrencyDecimalSeparator = ".";

                return numberFormat;
            }
            else if (type == typeof(DateTime))
            {
                DateTimeFormatInfo dateTimeFormat = new DateTimeFormatInfo();
                dateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
                dateTimeFormat.ShortTimePattern = "HH:mm:ss.fff";
                dateTimeFormat.LongDatePattern = dateTimeFormat.ShortDatePattern;
                dateTimeFormat.LongTimePattern = dateTimeFormat.ShortTimePattern;

                return dateTimeFormat;
            }
            else
                return null;
        }
    }
}
