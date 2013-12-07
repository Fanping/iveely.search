using System;
using NDatabase.Api;
using NDatabase.Core.Query.Execution;
using NDatabase.Meta;

namespace NDatabase.Core.Query.Values
{
    internal sealed class SumAction : AbstractQueryFieldAction
    {
        private Decimal _sum;

        public SumAction(string attributeName, string alias) : base(attributeName, alias, false)
        {
            _sum = new Decimal(0);
        }

        public override void Execute(OID oid, AttributeValuesMap values)
        {
            var number = Convert.ToDecimal(values[AttributeName]);
            _sum = Decimal.Add(_sum, ValuesUtil.Convert(number));
        }

        public Decimal GetSum()
        {
            return _sum;
        }

        public override object GetValue()
        {
            return _sum;
        }

        public override void End()
        {
        }

        public override void Start()
        {
        }

        public override IQueryFieldAction Copy()
        {
            return new SumAction(AttributeName, Alias);
        }
    }
}
