using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Iveely.General.Extensions;
using Iveely.General.Comparers;

namespace Iveely.Data
{
    public class DataEqualityComparer : IEqualityComparer<IData>
    {
        public readonly Func<IData, IData, bool> equals;
        public readonly Func<IData, int> getHashCode;

        public Expression<Func<IData, IData, bool>> LambdaEquals { get; private set; }
        public Expression<Func<IData, int>> LambdaGetHashCode { get; private set; }

        public Type Type { get; private set; }

        public Func<Type, MemberInfo, int> MembersOrder { get; private set; }
        public CompareOption[] CompareOptions { get; private set; }

        public DataEqualityComparer(Type type, CompareOption[] compareOptions, Func<Type, MemberInfo, int> membersOrder = null)
        {
            Type = type;

            CompareOption.CheckCompareOptions(type, compareOptions, membersOrder);

            CompareOptions = compareOptions;
            MembersOrder = membersOrder;

            LambdaEquals = CreateEqualsMethod();
            equals = LambdaEquals.Compile();

            LambdaGetHashCode = CreateGetHashCodeMethod();
            getHashCode = LambdaGetHashCode.Compile();
        }

        public DataEqualityComparer(Type type, Func<Type, MemberInfo, int> membersOrder = null)
            : this(type, CompareOption.GetDefaultCompareOptions(type, membersOrder), membersOrder)
        {
        }

        private Expression<Func<IData, IData, bool>> CreateEqualsMethod()
        {
            var x = Expression.Parameter(typeof(IData));
            var y = Expression.Parameter(typeof(IData));
            var xValue = Expression.Variable(Type);
            var yValue = Expression.Variable(Type);

            var dataType = typeof(Data<>).MakeGenericType(Type);

            var body = Expression.Block(typeof(bool), new ParameterExpression[] { xValue, yValue },
                    Expression.Assign(xValue, Expression.Convert(x, dataType).Value()),
                    Expression.Assign(yValue, Expression.Convert(y, dataType).Value()),
                    EqualityComparerHelper.CreateEqualsBody(xValue, yValue, CompareOptions, MembersOrder)
                );
            var lambda = Expression.Lambda<Func<IData, IData, bool>>(body, x, y);

            return lambda;
        }

        private Expression<Func<IData, int>> CreateGetHashCodeMethod()
        {
            var obj = Expression.Parameter(typeof(IData));
            var objValue = Expression.Variable(Type);

            var dataType = typeof(Data<>).MakeGenericType(Type);

            var body = Expression.Block(typeof(int), new ParameterExpression[] { objValue },
                Expression.Assign(objValue, Expression.Convert(obj, dataType).Value()),
                EqualityComparerHelper.CreateGetHashCodeBody(objValue, MembersOrder)
                );
            var lambda = Expression.Lambda<Func<IData, int>>(body, obj);

            return lambda;
        }
        
        public bool Equals(IData x, IData y)
        {
            return equals(x, y);
        }

        public int GetHashCode(IData obj)
        {
            return getHashCode(obj);
        }
    }
}
