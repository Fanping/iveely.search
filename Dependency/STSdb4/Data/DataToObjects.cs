using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Iveely.STSdb4.General.Extensions;

namespace Iveely.STSdb4.Data
{
    public class DataToObjects
    {
        public readonly Func<object[], IData> fromObjects;
        public readonly Func<IData, object[]> toObjects;

        public Type Type { get; private set; }
        public Func<Type, MemberInfo, int> MembersOrder { get; private set; }

        public Expression<Func<object[], IData>> LambdaFromObjects { get; private set; }
        public Expression<Func<IData, object[]>> LambdaToObjects { get; private set; }

        public DataToObjects(Type type, Func<Type, MemberInfo, int> membersOrder = null)
        {
            if (!DataType.IsPrimitiveType(type) && !type.HasDefaultConstructor())
                throw new NotSupportedException("No default constructor.");

            bool isSupported = DataTypeUtils.IsAllPrimitive(type);
            if (!isSupported)
                throw new NotSupportedException("Not all types are primitive.");

            Type = type;
            MembersOrder = membersOrder;

            LambdaFromObjects = CreateFromObjectsMethod();
            fromObjects = LambdaFromObjects.Compile();

            LambdaToObjects = CreateToObjectsMethod();
            toObjects = LambdaToObjects.Compile();
        }

        private Expression<Func<object[], IData>> CreateFromObjectsMethod()
        {
            var objectArray = Expression.Parameter(typeof(object[]), "item");
            var data = Expression.Variable(typeof(Data<>).MakeGenericType(Type));

            List<Expression> list = new List<Expression>();
            list.Add(Expression.Assign(data, Expression.New(data.Type.GetConstructor(new Type[] { }))));

            if (!DataType.IsPrimitiveType(Type))
                list.Add(Expression.Assign(data.Value(), Expression.New(data.Value().Type.GetConstructor(new Type[] { }))));

            list.Add(ObjectsHelper.FromObjects(data.Value(), objectArray, MembersOrder));
            list.Add(Expression.Label(Expression.Label(typeof(IData)), data));

            var body = Expression.Block(typeof(IData), new ParameterExpression[] { data }, list);

            return Expression.Lambda<Func<object[], IData>>(body, objectArray);
        }

        private Expression<Func<IData, object[]>> CreateToObjectsMethod()
        {
            var data = Expression.Parameter(typeof(IData), "data");

            var d = Expression.Variable(typeof(Data<>).MakeGenericType(Type), "d");
            var body = Expression.Block(new ParameterExpression[] { d }, Expression.Assign(d, Expression.Convert(data, d.Type)), ObjectsHelper.ToObjects(d.Value(), MembersOrder));

            return Expression.Lambda<Func<IData, object[]>>(body, data);
        }

        public IData FromObjects(object[] item)
        {
            return fromObjects(item);
        }

        public object[] ToObjects(IData item)
        {
            return toObjects(item);
        }
    }
}
