using NDatabase.Meta;

namespace NDatabase.Core.Query.Criteria.Evaluations
{
    internal abstract class AEvaluation : IEvaluation
    {
        protected readonly string AttributeName;
        protected readonly object TheObject;

        protected AEvaluation(object theObject, string attributeName)
        {
            TheObject = theObject;
            AttributeName = attributeName;
        }

        #region IEvaluation Members

        public abstract bool Evaluate(object candidate);

        #endregion

        protected bool IsNative()
        {
            return TheObject == null || OdbType.IsNative(TheObject.GetType());
        }

        protected object AsAttributeValuesMapValue(object valueToMatch)
        {
            // If it is a AttributeValuesMap, then gets the real value from the map
            var attributeValues = valueToMatch as AttributeValuesMap;

            return attributeValues != null
                       ? attributeValues[AttributeName]
                       : valueToMatch;
        }
    }
}