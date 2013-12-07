using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NDatabase.Api.Query;

namespace NDatabase.Core.Query.Linq
{
    internal sealed class LinqQueryable<TElement> : ILinqQueryable<TElement>, IQueryProvider
    {
        private readonly Expression _expression;
        private readonly ILinqQuery<TElement> _query;

        private LinqQueryable(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            if (!typeof (IQueryable<TElement>).IsAssignableFrom(expression.Type))
                throw new ArgumentOutOfRangeException("expression");

            _expression = expression;
        }

        public LinqQueryable(ILinqQuery<TElement> query)
        {
            _expression = Expression.Constant(this);
            _query = query;
        }

        #region ILinqQueryable<TElement> Members

        public IEnumerator<TElement> GetEnumerator()
        {
            return Execute<IEnumerable<TElement>>(_expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Expression Expression
        {
            get { return _expression; }
        }

        public Type ElementType
        {
            get { return typeof (TElement); }
        }

        public IQueryProvider Provider
        {
            get { return this; }
        }

        public ILinqQuery GetQuery()
        {
            return _query;
        }

        #endregion

        #region IQueryProvider Members

        public IQueryable<T> CreateQuery<T>(Expression expression)
        {
            return new LinqQueryable<T>(expression);
        }

        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = TypeSystem.GetElementType(expression.Type);

            try
            {
                return
                    (IQueryable)
                    Activator.CreateInstance(typeof (LinqQueryable<>).MakeGenericType(elementType),
                                             new object[] {expression});
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return Expression.Lambda<Func<TResult>>(TranslateQuery(expression)).Compile().Invoke();
        }

        public object Execute(Expression expression)
        {
            return Expression.Lambda(TranslateQuery(expression)).Compile().DynamicInvoke();
        }

        #endregion

        private static Expression TranslateQuery(Expression expression)
        {
            return LinqQueryTranslator.Translate(expression);
        }
    }
}