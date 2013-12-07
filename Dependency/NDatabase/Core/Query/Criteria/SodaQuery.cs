using System;
using System.Collections;
using NDatabase.Api;
using NDatabase.Api.Query;
using NDatabase.Tool.Wrappers;

namespace NDatabase.Core.Query.Criteria
{
    internal class SodaQuery : AbstractQuery
    {
        private string _attributeName;

        public SodaQuery(Type underlyingType) 
            : base(underlyingType)
        {
        }

        public bool HasCriteria()
        {
            return Constraint != null;
        }

        public bool Match(IDictionary map)
        {
            return Constraint == null || Constraint.Match(map);
        }

        public IConstraint GetCriteria()
        {
            return Constraint;
        }

        public override string ToString()
        {
            return Constraint == null
                       ? "no criterion"
                       : Constraint.ToString();
        }

        public virtual IOdbList<string> GetAllInvolvedFields()
        {
            return Constraint == null
                       ? new OdbList<string>()
                       : Constraint.GetAllInvolvedFields();
        }

        public override void Add(IConstraint criterion)
        {
            if (criterion == null)
                return;

            Constraint = (IInternalConstraint) criterion;
        }

        public override IQuery Descend(string attributeName)
        {
            if (string.IsNullOrEmpty(attributeName))
                throw new ArgumentNullException("attributeName", "Attribute name name cannot be null or empty");

            _attributeName = _attributeName == null ? attributeName : string.Join(".", _attributeName, attributeName);

            return this;
        }

        public override IQuery OrderAscending()
        {
            if (string.IsNullOrEmpty(_attributeName))
                throw new ArgumentException("Descend field not set.");

            OrderByFields.Add(_attributeName);
            OrderByType = OrderByConstants.OrderByAsc;
            _attributeName = null;

            return this;
        }

        public override IQuery OrderDescending()
        {
            if (string.IsNullOrEmpty(_attributeName))
                throw new ArgumentException("Descend field not set.");

            OrderByFields.Add(_attributeName);
            OrderByType = OrderByConstants.OrderByDesc;
            _attributeName = null;

            return this;
        }

        public override IConstraint Constrain(object value)
        {
            if (string.IsNullOrEmpty(_attributeName))
            {
                return new EmptyConstraint(value);
            }
            
            return new QueryConstraint(this, ApplyAttributeName(), value);
        }

        public override IObjectSet<TItem> Execute<TItem>()
        {
            return ((IInternalQuery)this).GetQueryEngine().GetObjects<TItem>(this, true, -1, -1);
        }

        public override IObjectSet<TItem> Execute<TItem>(bool inMemory)
        {
            return ((IInternalQuery)this).GetQueryEngine().GetObjects<TItem>(this, inMemory, -1, -1);
        }

        public override IObjectSet<TItem> Execute<TItem>(bool inMemory, int startIndex, int endIndex)
        {
            return ((IInternalQuery)this).GetQueryEngine().GetObjects<TItem>(this, inMemory, startIndex, endIndex);
        }

        private string ApplyAttributeName()
        {
            var attributeName = String.Copy(_attributeName);
            _attributeName = null;

            return attributeName;
        }
    }
}