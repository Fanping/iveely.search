using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Iveely.Data
{
    public class DataComparer : IComparer<IData>
    {
        public Type DataType { get; private set; }
        public Type Type { get; private set; }

        public CompareOption[] CompareOptions { get; private set; }
        public Func<Type, MemberInfo, int> MembersOrder { get; private set; }

        public readonly Func<IData, IData, int> dataComparer;
        public Expression<Func<IData, IData, int>> LambdaDataComparer { get; private set; }

        public DataComparer(Type type, CompareOption[] compareOptions, Func<Type, MemberInfo, int> membersOrder = null)
        {
            Type = type;
            DataType = typeof(Data<>).MakeGenericType(type);

            CompareOption.CheckCompareOptions(type, compareOptions, membersOrder);
            CompareOptions = compareOptions;
            MembersOrder = membersOrder;

            LambdaDataComparer = CreateDataCompareMethod();
            dataComparer = LambdaDataComparer.Compile();
        }

        public DataComparer(Type type, Func<Type, MemberInfo, int> membersOrder = null)
            : this(type, CompareOption.GetDefaultCompareOptions(type, membersOrder), membersOrder)
        {
        }

        private Expression<Func<IData, IData, int>> CreateDataCompareMethod()
        {
            var x = Expression.Parameter(typeof(IData));
            var y = Expression.Parameter(typeof(IData));

            List<Expression> list = new List<Expression>();
            List<ParameterExpression> parameters = new List<ParameterExpression>();

            var value1 = Expression.Variable(Type, "value1");
            parameters.Add(value1);
            list.Add(Expression.Assign(value1, Expression.Convert(x, DataType).Value()));

            var value2 = Expression.Variable(Type, "value2");
            parameters.Add(value2);
            list.Add(Expression.Assign(value2, Expression.Convert(y, DataType).Value()));

            return Expression.Lambda<Func<IData, IData, int>>(ComparerHelper.CreateComparerBody(list, parameters, value1, value2, CompareOptions, MembersOrder), x, y);
        }

        public int Compare(IData x, IData y)
        {
            return dataComparer(x, y);
        }
    }
}
