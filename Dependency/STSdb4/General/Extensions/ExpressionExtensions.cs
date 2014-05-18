using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Iveely.General.Extensions
{
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Creates BlockExpression in format:
        /// 
        /// using (variable = createNew)
        /// {
        ///     body;
        /// }
        /// 
        /// or:
        /// 
        /// var variable = createNew;
        /// try
        /// {
        ///     body;
        /// }
        /// finally
        /// {
        ///     variable.Dispose();
        /// }
        /// 
        /// </summary>
        /// <param name="variable">variable.Type must be IDisposable</param>
        /// <param name="createNew"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static BlockExpression Using(this ParameterExpression variable, NewExpression createNew, Expression body)
        {
            if (!variable.Type.IsInheritInterface(typeof(IDisposable)))
                throw new ArgumentException(String.Format("Type {0} is not IDisposable.", variable.Type.Name));

            return Expression.Block(new ParameterExpression[] { variable },
                Expression.Assign(variable, createNew),
                Expression.TryFinally(body, Expression.Call(variable, "Dispose", new Type[] { })));
        }

        /// <summary>
        /// Creates BlockExpression in format:
        /// 
        /// using (variable)
        /// {
        ///     body;
        /// }
        /// 
        /// or:
        /// 
        /// try
        /// {
        ///     body;
        /// }
        /// finally
        /// {
        ///     variable.Dispose();
        /// }
        /// </summary>
        /// <param name="variable">variable.Type must be IDisposable</param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static BlockExpression Using(this ParameterExpression variable, Expression body)
        {
            if (!variable.Type.IsInheritInterface(typeof(IDisposable)))
                throw new ArgumentException(String.Format("Type {0} is not IDisposable.", variable.Type.Name));

            return Expression.Block(new ParameterExpression[] { variable },
                Expression.TryFinally(body, Expression.Call(variable, "Dispose", new Type[] { })));
        }

        /// <summary>
        /// 
        ///<para> var enumerator = enumerable.GetEnumerator();</para>
        ///<para></para>
        ///<para>while (enumerator.MoveNext())</para>
        ///<para>{</para>
        ///<para>    action(enumerator.Current);</para>
        ///<para>}</para>
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="action"></param>
        /// <param name="break"></param>
        /// <returns></returns>
        public static Expression ForEach(this Expression enumerable, Func<Expression, Expression> action, LabelTarget @break)
        {
            Type ienumerable = enumerable.Type.GetInterfaces().Where(x => x.Name == "IEnumerable`1").FirstOrDefault();
            if (ienumerable == null)
                throw new ArgumentException("enumerable.Type does not implement IEnumerable<> interface");

            MethodInfo getEnumerator = enumerable.Type.GetMethod("GetEnumerator");

            var enumerator = Expression.Variable(getEnumerator.ReturnType, "enumerator");
            var enumeratorAssign = Expression.Assign(enumerator, Expression.Call(enumerable, getEnumerator));

            var moveNext = Expression.Call(enumerator, typeof(IEnumerator).GetMethod("MoveNext"));

            var @while = Expression.Loop(Expression.IfThenElse(Expression.IsTrue(moveNext),
                action(Expression.Property(enumerator, "Current")),
                Expression.Break(@break)), @break);

            return Expression.Block(new ParameterExpression[] { enumerator }, enumeratorAssign, @while);
        }

        public static Expression For(this Expression collection, Func<Expression, Expression> action, LabelTarget @break, Expression length)
        {
            var i = Expression.Variable(length.Type);
            var assign = Expression.Assign(i, Expression.Default(length.Type));

            var @for = Expression.Loop(Expression.IfThenElse(Expression.LessThan(i, length),
                Expression.Block(
                    action(i), 
                    Expression.PostIncrementAssign(i)
                    ),
                Expression.Break(@break)), @break);

            return Expression.Block(new ParameterExpression[] { i }, assign, @for);
        }

        /// <summary>
        /// 
        ///<para>for (int i = 0; i &lt length; i++)</para>
        ///<para>{</para>
        ///<para>   action(i);</para>
        ///<para>}</para>
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="action"></param>
        /// <param name="break"></param>
        /// <returns></returns>
        public static Expression For(this Expression collection, Func<Expression, Expression> action, LabelTarget @break)
        {
            Type type = collection.Type;

            if (type.IsArray)
                return For(collection, action, @break, Expression.ArrayLength(collection));
            else
                return For(collection, action, @break, Expression.PropertyOrField(collection, "Count"));
        }

        public static Expression This(this Expression collection, params Expression[] indexes)
        {
            return Expression.Property(collection, "Item", indexes);
        }
    }
}
