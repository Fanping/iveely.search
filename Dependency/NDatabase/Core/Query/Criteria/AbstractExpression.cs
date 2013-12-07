using System;
using NDatabase.Api.Query;
using NDatabase.Meta;
using NDatabase.Tool.Wrappers;

namespace NDatabase.Core.Query.Criteria
{
    internal abstract class AbstractExpression : IInternalConstraint
    {
        private readonly IQuery _query;

        protected AbstractExpression(IQuery query)
        {
            _query = query;
            ((IInternalQuery)_query).Add(this);
        }

        #region IExpression Members

        public virtual bool CanUseIndex()
        {
            return false;
        }

        public abstract IOdbList<string> GetAllInvolvedFields();

        public abstract AttributeValuesMap GetValues();

        public abstract bool Match(object arg1);

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

        public IConstraint Equal()
        {
            throw new NotSupportedException();
        }

        public IConstraint Identity()
        {
            throw new NotSupportedException();
        }

        public virtual IConstraint Like()
        {
            throw new NotSupportedException();
        }

        public virtual IConstraint InvariantLike()
        {
            throw new NotSupportedException();
        }

        public virtual IConstraint Contains()
        {
            throw new NotSupportedException();
        }

        public IConstraint Greater()
        {
            throw new NotSupportedException();
        }

        public IConstraint Smaller()
        {
            throw new NotSupportedException();
        }

        public IConstraint SizeLe()
        {
            throw new NotSupportedException();
        }

        public IConstraint SizeEq()
        {
            throw new NotSupportedException();
        }

        public IConstraint SizeNe()
        {
            throw new NotSupportedException();
        }

        public IConstraint SizeGt()
        {
            throw new NotSupportedException();
        }

        public IConstraint SizeGe()
        {
            throw new NotSupportedException();
        }

        public IConstraint SizeLt()
        {
            throw new NotSupportedException();
        }

        public object GetObject()
        {
            throw new NotSupportedException();
        }

        public IConstraint EndsWith(bool isCaseSensitive)
        {
            throw new NotSupportedException();
        }

        public IConstraint StartsWith(bool isCaseSensitive)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
