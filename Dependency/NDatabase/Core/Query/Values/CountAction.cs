using System;
using NDatabase.Api;
using NDatabase.Core.Query.Execution;
using NDatabase.Meta;

namespace NDatabase.Core.Query.Values
{
    /// <summary>
    ///   An action to count objects of a query
    /// </summary>
    internal sealed class CountAction : AbstractQueryFieldAction
    {
        private static readonly Decimal One = new decimal(1);

        private Decimal _count;

        public CountAction(string alias) : base(alias, alias, false)
        {
            _count = new decimal(0);
        }

        public override void Execute(OID oid, AttributeValuesMap values)
        {
            _count = Decimal.Add(_count, One);
        }

        public Decimal GetCount()
        {
            return _count;
        }

        public override object GetValue()
        {
            return _count;
        }

        public override void End()
        {
        }

        // Nothing to do
        public override void Start()
        {
        }

        // Nothing to do
        public override IQueryFieldAction Copy()
        {
            return new CountAction(Alias);
        }
    }
}
