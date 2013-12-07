using System.Collections.Generic;
using System.Linq.Expressions;
using NDatabase.Api.Query;

namespace NDatabase.Core.Query.Linq
{
    internal sealed class OrderByAscendingClauseVisitor : OrderByClauseVisitorBase
    {
        private static readonly Dictionary<Expression, IQueryBuilderRecord> Cache =
            new Dictionary<Expression, IQueryBuilderRecord>();

        protected override Dictionary<Expression, IQueryBuilderRecord> GetCachingStrategy()
        {
            return Cache;
        }

        protected override void ApplyDirection(IQuery query)
        {
            query.OrderAscending();
        }
    }
}