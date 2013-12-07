using System;
using System.Text;
using NDatabase.Exceptions;
using NDatabase.Meta.Compare;

namespace NDatabase.Core.Query.Criteria.Evaluations
{
    internal sealed class ComparisonEvaluation : AEvaluation
    {
        private readonly int _comparisonType;

        public ComparisonEvaluation(object theObject, string attributeName, int comparisonType) 
            : base(theObject, attributeName)
        {
            if (!(theObject is IComparable))
                throw new ArgumentException("Value need to implement IComparable", "theObject");

            _comparisonType = comparisonType;
        }

        internal int ComparisonType
        {
            get { return _comparisonType; }
        }

        public override bool Evaluate(object candidate)
        {
            if (candidate == null && TheObject == null)
                return true;

            candidate = AsAttributeValuesMapValue(candidate);

            if (candidate == null)
                return false;

            if (!(candidate is IComparable))
            {
                throw new OdbRuntimeException(
                    NDatabaseError.QueryComparableCriteriaAppliedOnNonComparable.AddParameter(
                        candidate.GetType().FullName));
            }

            var comparable1 = (IComparable)candidate;
            var comparable2 = (IComparable)TheObject;

            switch (ComparisonType)
            {
                case ComparisonConstraint.ComparisonTypeGt:
                    {
                        return AttributeValueComparator.Compare(comparable1, comparable2) > 0;
                    }

                case ComparisonConstraint.ComparisonTypeGe:
                    {
                        return AttributeValueComparator.Compare(comparable1, comparable2) >= 0;
                    }

                case ComparisonConstraint.ComparisonTypeLt:
                    {
                        return AttributeValueComparator.Compare(comparable1, comparable2) < 0;
                    }

                case ComparisonConstraint.ComparisonTypeLe:
                    {
                        return AttributeValueComparator.Compare(comparable1, comparable2) <= 0;
                    }
            }

            throw new OdbRuntimeException(NDatabaseError.QueryUnknownOperator.AddParameter(ComparisonType));
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();
            buffer.Append(AttributeName).Append(" ").Append(GetOperator()).Append(" ").Append(TheObject);
            return buffer.ToString();
        }

        private string GetOperator()
        {
            switch (ComparisonType)
            {
                case ComparisonConstraint.ComparisonTypeGt:
                    return ">";
                case ComparisonConstraint.ComparisonTypeGe:
                    return ">=";
                case ComparisonConstraint.ComparisonTypeLt:
                    return "<";
                case ComparisonConstraint.ComparisonTypeLe:
                    return "<=";}
            return "?";
        }
    }
}
