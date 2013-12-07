using System;
using System.Text;
using NDatabase.Api;
using NDatabase.Api.Query;
using NDatabase.Meta;
using NDatabase.Meta.Compare;

namespace NDatabase.Core.Query.Criteria.Evaluations
{
    internal sealed class EqualsEvaluation : AEvaluation
    {
        private readonly OID _oid;

        public EqualsEvaluation(object theObject, string attributeName, IQuery query)
            : base(theObject, attributeName)
        {
            if (IsNative() || theObject == null)
                return;

            // For non native object, we just need the oid of it
            _oid = ((IInternalQuery)query).GetQueryEngine().GetObjectId(theObject, false);
        }

        public override bool Evaluate(object candidate)
        {
            candidate = AsAttributeValuesMapValue(candidate);

            if (candidate == null && TheObject == null)
                return true;

            if (candidate == null || TheObject == null)
                return false;

            var oid = candidate as OID;
            if (oid != null && !IsNative())
                return _oid != null && _oid.Equals(oid);

            if (AttributeValueComparator.IsNumber(candidate) && AttributeValueComparator.IsNumber(TheObject))
                return AttributeValueComparator.Compare((IComparable) candidate, (IComparable) TheObject) == 0;

            return Equals(candidate, TheObject);
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();
            buffer.Append(AttributeName).Append(" = ").Append(TheObject);
            return buffer.ToString();
        }

        public AttributeValuesMap GetValues()
        {
            var map = new AttributeValuesMap();

            if (_oid != null)
                map.SetOid(_oid);
            else
                map.Add(AttributeName, TheObject);

            return map;
        }
    }
}