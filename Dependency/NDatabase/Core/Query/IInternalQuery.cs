using System;
using System.Collections.Generic;
using NDatabase.Api;
using NDatabase.Api.Query;

namespace NDatabase.Core.Query
{
    internal interface IInternalQuery : IQuery
    {
        IQueryExecutionPlan GetExecutionPlan();

        void SetExecutionPlan(IQueryExecutionPlan plan);

        IQueryEngine GetQueryEngine();

        void SetQueryEngine(IQueryEngine storageEngine);

        void Add(IConstraint criterion);

        /// <summary>
        ///   Returns true if the query has an order by clause
        /// </summary>
        /// <returns> true if has an order by flag </returns>
        bool HasOrderBy();

        /// <summary>
        ///   Returns the field names of the order by
        /// </summary>
        /// <returns> The array of fields of the order by </returns>
        IList<string> GetOrderByFieldNames();

        /// <returns> the type of the order by - NONE, DESC, ASC </returns>
        OrderByConstants GetOrderByType();

        Type UnderlyingType { get; }
    }
}