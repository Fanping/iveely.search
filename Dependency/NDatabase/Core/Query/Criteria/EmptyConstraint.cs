using NDatabase.Api.Query;
using NDatabase.Meta;
using NDatabase.Tool.Wrappers;

namespace NDatabase.Core.Query.Criteria
{
    internal sealed class EmptyConstraint : IInternalConstraint
    {
        private readonly object _theObject;

        public EmptyConstraint(object theObject)
        {
            _theObject = theObject;
        }

        public IConstraint And(IConstraint with)
        {
            return with;
        }

        public IConstraint Or(IConstraint with)
        {
            return with;
        }

        public IConstraint Equal()
        {
            return this;
        }

        public IConstraint Greater()
        {
            return this;
        }

        public IConstraint Smaller()
        {
            return this;
        }

        public IConstraint Identity()
        {
            return this;
        }

        public IConstraint Like()
        {
            return this;
        }

        public IConstraint Contains()
        {
            return this;
        }

        public IConstraint Not()
        {
            return this;
        }

        public IConstraint InvariantLike()
        {
            return this;
        }

        public IConstraint SizeLe()
        {
            return this;
        }

        public IConstraint SizeEq()
        {
            return this;
        }

        public IConstraint SizeNe()
        {
            return this;
        }

        public IConstraint SizeGt()
        {
            return this;
        }

        public IConstraint SizeGe()
        {
            return this;
        }

        public IConstraint SizeLt()
        {
            return this;
        }

        public object GetObject()
        {
            return _theObject;
        }

        public IConstraint EndsWith(bool isCaseSensitive)
        {
            return this;
        }

        public IConstraint StartsWith(bool isCaseSensitive)
        {
            return this;
        }

        public bool CanUseIndex()
        {
            return true;
        }

        public AttributeValuesMap GetValues()
        {
            return new AttributeValuesMap();
        }

        public IOdbList<string> GetAllInvolvedFields()
        {
            return new OdbList<string>();
        }

        public bool Match(object @object)
        {
            return true;
        }
    }
}