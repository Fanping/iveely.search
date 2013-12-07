using System;
using NDatabase.Exceptions;
using NDatabase.Tool.Wrappers;

namespace NDatabase.Core.Query.Criteria.Evaluations
{
    internal sealed class LikeEvaluation : AEvaluation
    {
        private const string LikePattern = "(.)*{0}(.)*";
        private readonly bool _isCaseSensitive;

        public LikeEvaluation(object theObject, string attributeName, bool isCaseSensitive = true) 
            : base(theObject, attributeName)
        {
            _isCaseSensitive = isCaseSensitive;
        }

        public override bool Evaluate(object candidate)
        {
            string regExp;
            if (candidate == null)
                return false;

            candidate = AsAttributeValuesMapValue(candidate);

            if (candidate == null)
                return false;

            // Like operator only work with String
            if (!(candidate is string))
            {
                throw new OdbRuntimeException(
                    NDatabaseError.QueryAttributeTypeNotSupportedInLikeExpression.AddParameter(
                        candidate.GetType().FullName));
            }

            var value = (string)candidate;
            var criterionValue = (string)TheObject;

            if (criterionValue.IndexOf("%", StringComparison.Ordinal) != -1)
            {
                regExp = criterionValue.Replace("%", "(.)*");

                return _isCaseSensitive
                           ? OdbString.Matches(regExp, value)
                           : OdbString.Matches(regExp.ToLower(), value.ToLower());
            }

            if (!_isCaseSensitive)
            {
                criterionValue = criterionValue.ToLower();
                value = value.ToLower();
            }

            regExp = string.Format(LikePattern, criterionValue);
            return OdbString.Matches(regExp, value);
        }
    }
}
