using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Iveely.STSdb4.General.Extensions;
using System.Reflection;

namespace Iveely.STSdb4.Data
{
    public class DataToString
    {
        public readonly Func<IData, string> toString;
        public readonly Func<string, IData> fromString;

        public IFormatProvider[] Providers { get; private set; }
        public char[] Delimiters { get; private set; }
        public Type Type { get; private set; }
        public Func<Type, MemberInfo, int> MembersOrder { get; private set; }

        public Expression<Func<IData, string>> LambdaToString { get; private set; }
        public Expression<Func<string, IData>> LambdaFromString { get; private set; }

        public DataToString(Type type, IFormatProvider[] providers, char[] delimiters, Func<Type, MemberInfo, int> membersOrder = null)
        {
            var typeCount = DataType.IsPrimitiveType(type) ? 1 : DataTypeUtils.GetPublicMembers(type, membersOrder).Count();

            if (providers.Length != typeCount)
                throw new ArgumentException("providers.Length != dataType.Length");

            Providers = providers;
            Delimiters = delimiters;

            Type = type;
            MembersOrder = membersOrder;

            LambdaFromString = CreateFromStringMethod();
            fromString = LambdaFromString.Compile();

            LambdaToString = CreateToStringMethod();
            toString = LambdaToString.Compile();
        }

        public DataToString(Type type, char[] delimiters, Func<Type, MemberInfo, int> membersOrder = null)
            : this(type, StringHelper.GetDefaultProviders(type, membersOrder), delimiters, membersOrder)
        {
        }

        public DataToString(Type type, Func<Type, MemberInfo, int> membersOrder = null)
            : this(type, new char[] { ';' }, membersOrder)
        {
        }

        private Expression<Func<string, IData>> CreateFromStringMethod()
        {
            var stringParam = Expression.Parameter(typeof(string), "item");
            List<Expression> list = new List<Expression>();

            var data = Expression.Variable(typeof(Data<>).MakeGenericType(Type), "d");

            list.Add(Expression.Assign(data, Expression.New(data.Type.GetConstructor(new Type[] { }))));

            if (!DataType.IsPrimitiveType(Type))
                list.Add(Expression.Assign(data.Value(), Expression.New(Type.GetConstructor(new Type[] { }))));

            list.Add(StringHelper.CreateFromBody(data.Value(), stringParam, Providers, Delimiters, MembersOrder));
            list.Add(Expression.Label(Expression.Label(typeof(Data<>).MakeGenericType(Type)), data));

            var body = Expression.Block(new ParameterExpression[] { data }, list);

            return Expression.Lambda<Func<string, IData>>(body, new ParameterExpression[] { stringParam });
        }

        private Expression<Func<IData, string>> CreateToStringMethod()
        {
            var data = Expression.Parameter(typeof(IData), "data");
            var d = Expression.Variable(typeof(Data<>).MakeGenericType(Type), "d");

            List<Expression> list = new List<Expression>();
            list.Add(Expression.Assign(d, Expression.Convert(data, typeof(Data<>).MakeGenericType(Type))));
            list.Add(StringHelper.CreateToBody(d.Value(), Providers, Delimiters[0], MembersOrder));

            var body = Expression.Block(new ParameterExpression[] { d }, list);

            return Expression.Lambda<Func<IData, string>>(body, data);
        }

        public IData FromString(string str)
        {
            return fromString(str);
        }

        public string ToString(IData data)
        {
            return toString(data);
        }
    }
}
