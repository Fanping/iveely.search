using System;
using NDatabase.Api.Query;
using NDatabase.Core.Query.Criteria.Evaluations;
using NDatabase.Meta;
using NDatabase.Tool.Wrappers;

namespace NDatabase.Core.Query.Criteria
{
    internal sealed class QueryConstraint : IInternalConstraint
    {
        /// <summary>
        ///   The name of the attribute involved by this criterion
        /// </summary>
        private readonly string _attributeName;

        /// <summary>
        ///   The query containing the criterion
        /// </summary>
        private readonly IQuery _query;

        private readonly object _theObject;
        private IEvaluation _evaluation;

        public QueryConstraint(IQuery query, string fieldName, object theObject)
        {
            if (query == null)
                throw new ArgumentNullException("query");

            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException("fieldName");

            _query = query;
            _attributeName = fieldName;
            _theObject = theObject;

            ((IInternalQuery) _query).Add(this);

            _evaluation = new EqualsEvaluation(_theObject, _attributeName, _query);
        }

        #region IInternalConstraint Members

        public bool Match(object valueToMatch)
        {
            return _evaluation == null || _evaluation.Evaluate(valueToMatch);
        }

        public IConstraint Equal()
        {
            if (IsPreEvaluationTheGreater())
            {
                _evaluation = new ComparisonEvaluation(_theObject, _attributeName,
                                                                       ComparisonConstraint.ComparisonTypeGe);
                return this;
            }

            if (IsPreEvaluationTheSmaller())
            {
                _evaluation = new ComparisonEvaluation(_theObject, _attributeName,
                                                                       ComparisonConstraint.ComparisonTypeLe);
                return this;
            }

            _evaluation = new EqualsEvaluation(_theObject, _attributeName, _query);
            return this;
        }

        public IConstraint Identity()
        {
            _evaluation = new IdentityEvaluation(_theObject, _attributeName, _query);
            return this;
        }

        public IConstraint Like()
        {
            _evaluation = new LikeEvaluation(_theObject, _attributeName);
            return this;
        }

        public IConstraint InvariantLike()
        {
            _evaluation = new LikeEvaluation(_theObject, _attributeName, false);
            return this;
        }

        public IConstraint Contains()
        {
            _evaluation = new ContainsEvaluation(_theObject, _attributeName, _query);
            return this;
        }

        public IConstraint Greater()
        {
            _evaluation = new ComparisonEvaluation(_theObject, _attributeName,
                                                   ComparisonConstraint.ComparisonTypeGt);
            return this;
        }

        public IConstraint Smaller()
        {
            _evaluation = new ComparisonEvaluation(_theObject, _attributeName,
                                                   ComparisonConstraint.ComparisonTypeLt);
            return this;
        }

        public IConstraint SizeEq()
        {
            _evaluation = new CollectionSizeEvaluation(_theObject, _attributeName, _query, CollectionSizeEvaluation.SizeEq);
            return this;
        }

        public IConstraint SizeNe()
        {
            _evaluation = new CollectionSizeEvaluation(_theObject, _attributeName, _query, CollectionSizeEvaluation.SizeNe);
            return this;
        }

        public IConstraint SizeGt()
        {
            _evaluation = new CollectionSizeEvaluation(_theObject, _attributeName, _query, CollectionSizeEvaluation.SizeGt);
            return this;
        }

        public IConstraint SizeGe()
        {
            _evaluation = new CollectionSizeEvaluation(_theObject, _attributeName, _query, CollectionSizeEvaluation.SizeGe);
            return this;
        }

        public IConstraint SizeLt()
        {
            _evaluation = new CollectionSizeEvaluation(_theObject, _attributeName, _query, CollectionSizeEvaluation.SizeLt);
            return this;
        }

        public object GetObject()
        {
            return _theObject;
        }

        public IConstraint EndsWith(bool isCaseSensitive)
        {
            _evaluation = new EndsWithEvaluation(_theObject, _attributeName, isCaseSensitive);
            return this;
        }

        public IConstraint StartsWith(bool isCaseSensitive)
        {
            _evaluation = new StartsWithEvaluation(_theObject, _attributeName, isCaseSensitive);
            return this;
        }

        public IConstraint SizeLe()
        {
            _evaluation = new CollectionSizeEvaluation(_theObject, _attributeName, _query, CollectionSizeEvaluation.SizeLe);
            return this;
        }

        public bool CanUseIndex()
        {
            return _evaluation is EqualsEvaluation || _evaluation is IdentityEvaluation;
        }

        public AttributeValuesMap GetValues()
        {
            var equalsEvaluation = _evaluation as EqualsEvaluation;
            var identityEvaluation = _evaluation as IdentityEvaluation;

            return equalsEvaluation != null
                       ? equalsEvaluation.GetValues()
                       : identityEvaluation != null
                             ? identityEvaluation.GetValues()
                             : new AttributeValuesMap();
        }

        /// <summary>
        ///   An abstract criterion only restrict one field =&gt; it returns a list of one field!
        /// </summary>
        /// <returns> The list of involved field of the criteria </returns>
        public IOdbList<string> GetAllInvolvedFields()
        {
            return new OdbList<string>(1) {_attributeName};
        }

        public IConstraint And(IConstraint with)
        {
            return new And(_query).Add(this).Add(with);
        }

        public IConstraint Or(IConstraint with)
        {
            return new Or(_query).Add(this).Add(with);
        }

        public IConstraint Not()
        {
            return new Not(_query, this);
        }

        #endregion

        public override string ToString()
        {
            return _evaluation == null ? base.ToString() : _evaluation.ToString();
        }

        private bool IsPreEvaluationTheGreater()
        {
            var evaluation = _evaluation as ComparisonEvaluation;
            return evaluation != null && evaluation.ComparisonType == ComparisonConstraint.ComparisonTypeGt;
        }

        private bool IsPreEvaluationTheSmaller()
        {
            var evaluation = _evaluation as ComparisonEvaluation;
            return evaluation != null && evaluation.ComparisonType == ComparisonConstraint.ComparisonTypeLt;
        }
    }
}
