using System;
using System.Collections.Generic;
using NDatabase.Api;
using NDatabase.Api.Query;
using NDatabase.Core.Query.Criteria;
using NDatabase.Exceptions;

namespace NDatabase.Core.Query
{
    internal abstract class AbstractQuery : IInternalQuery
    {
        protected IInternalConstraint Constraint;
        private readonly Type _underlyingType;

        private IQueryExecutionPlan _executionPlan;
        protected readonly List<string> OrderByFields;

        /// <summary>
        ///   The OID attribute is used when the query must be restricted the object with this OID
        /// </summary>
        private OID _oidOfObjectToQuery;

        protected OrderByConstants OrderByType;

        [NonPersistent] private IQueryEngine _storageEngine;

        protected AbstractQuery(Type underlyingType)
        {
            if (underlyingType == null)
                throw new ArgumentNullException("underlyingType");

            if (underlyingType.IsValueType)
                throw new ArgumentException("Underlying type for query cannot to be value type.", "underlyingType");

            OrderByType = OrderByConstants.OrderByNone;
            _underlyingType = underlyingType;
            OrderByFields = new List<string>();
        }

        #region IInternalQuery Members

        IQueryExecutionPlan IInternalQuery.GetExecutionPlan()
        {
            if (_executionPlan == null)
                throw new OdbRuntimeException(NDatabaseError.ExecutionPlanIsNullQueryHasNotBeenExecuted);
            return _executionPlan;
        }

        void IInternalQuery.SetExecutionPlan(IQueryExecutionPlan plan)
        {
            _executionPlan = plan;
        }

        IQueryEngine IInternalQuery.GetQueryEngine()
        {
            return _storageEngine;
        }

        void IInternalQuery.SetQueryEngine(IQueryEngine storageEngine)
        {
            _storageEngine = storageEngine;
        }

        #endregion

        #region IQuery Members

        public abstract IObjectSet<TItem> Execute<TItem>();
        public abstract IObjectSet<TItem> Execute<TItem>(bool inMemory) where TItem : class;
        public abstract IObjectSet<TItem> Execute<TItem>(bool inMemory, int startIndex, int endIndex) where TItem : class;

        public abstract IQuery OrderAscending();
        public abstract IQuery OrderDescending();

        public IList<string> GetOrderByFieldNames()
        {
            return OrderByFields;
        }

        public OrderByConstants GetOrderByType()
        {
            return OrderByType;
        }

        public bool HasOrderBy()
        {
            return !OrderByType.IsOrderByNone();
        }

        public OID GetOidOfObjectToQuery()
        {
            return _oidOfObjectToQuery;
        }

        public Type UnderlyingType
        {
            get { return _underlyingType; }
        }

        public abstract IQuery Descend(string attributeName);
        public abstract void Add(IConstraint criterion);

        /// <summary>
        ///   Returns true is query must apply on a single object OID
        /// </summary>
        public bool IsForSingleOid()
        {
            return _oidOfObjectToQuery != null;
        }

        public long Count()
        {
            return ((IInternalQuery) this).GetQueryEngine().Count(_underlyingType, Constraint);
        }

        #endregion

        internal void SetOidOfObjectToQuery(OID oidOfObjectToQuery)
        {
            _oidOfObjectToQuery = oidOfObjectToQuery;
        }

        public abstract IConstraint Constrain(object value);
    }
}