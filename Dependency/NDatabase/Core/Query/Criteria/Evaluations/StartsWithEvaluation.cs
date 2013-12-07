using System;
using NDatabase.Exceptions;

namespace NDatabase.Core.Query.Criteria.Evaluations
{
    internal sealed class StartsWithEvaluation : AEvaluation
    {
        private readonly bool _isCaseSensitive;

        public StartsWithEvaluation(object theObject, string attributeName, bool isCaseSensitive) 
            : base(theObject, attributeName)
        {
            _isCaseSensitive = isCaseSensitive;
        }

        public override bool Evaluate(object candidate)
        {
            candidate = AsAttributeValuesMapValue(candidate);

            if (candidate == null && TheObject == null)
                return true;

            if (candidate == null)
                return false;

            var candidateAsString = candidate as string;
            
            return candidateAsString != null && CheckIfStringStartsWithValue(candidateAsString);
        }

        private bool CheckIfStringStartsWithValue(string candidateAsString)
        {
            var theObjectAsString = TheObject as string;

            if (theObjectAsString != null)
            {
                return candidateAsString.StartsWith(theObjectAsString,
                                                    _isCaseSensitive
                                                        ? StringComparison.Ordinal
                                                        : StringComparison.OrdinalIgnoreCase);
            }

            throw new OdbRuntimeException(
                NDatabaseError.QueryStartsWithConstraintTypeNotSupported.AddParameter(TheObject.GetType().FullName));
        }
    }
}