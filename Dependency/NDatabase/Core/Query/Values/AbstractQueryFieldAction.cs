using NDatabase.Api;
using NDatabase.Core.Query.Execution;
using NDatabase.Meta;

namespace NDatabase.Core.Query.Values
{
    internal abstract class AbstractQueryFieldAction : IQueryFieldAction
    {
        protected readonly string Alias;
        protected string AttributeName;

        private IInstanceBuilder _instanceBuilder;
        private bool _isMultiRow;
        private bool _returnInstance;

        protected AbstractQueryFieldAction(string attributeName, string alias, bool isMultiRow)
        {
            AttributeName = attributeName;
            Alias = alias;
            _isMultiRow = isMultiRow;
        }

        #region IQueryFieldAction Members

        public virtual string GetAttributeName()
        {
            return AttributeName;
        }

        public virtual string GetAlias()
        {
            return Alias;
        }

        public virtual bool IsMultiRow()
        {
            return _isMultiRow;
        }

        public virtual void SetMultiRow(bool isMultiRow)
        {
            _isMultiRow = isMultiRow;
        }

        public virtual void SetReturnInstance(bool returnInstance)
        {
            _returnInstance = returnInstance;
        }

        public abstract IQueryFieldAction Copy();

        public abstract void End();

        public abstract object GetValue();

        public abstract void Start();

        #endregion

        public abstract void Execute(OID oid, AttributeValuesMap values);

        internal IInstanceBuilder GetInstanceBuilder()
        {
            return _instanceBuilder;
        }

        internal void SetInstanceBuilder(IInstanceBuilder instanceBuilder)
        {
            _instanceBuilder = instanceBuilder;
        }

        protected bool ReturnInstance()
        {
            return _returnInstance;
        }
    }
}