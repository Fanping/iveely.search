using System;
using NDatabase.Api;
using NDatabase.Core.Query.Execution;
using NDatabase.Meta;

namespace NDatabase.Core.Query.Values
{
    /// <summary>
    ///   An action to compute the max value of a field
    /// </summary>
    internal sealed class MaxValueAction : AbstractQueryFieldAction
    {
        private Decimal _maxValue;

        private OID _oidOfMaxValues;

        public MaxValueAction(string attributeName, string alias) : base(attributeName, alias, false)
        {
            _maxValue = new Decimal(long.MinValue);
            _oidOfMaxValues = null;
        }

        public override void Execute(OID oid, AttributeValuesMap values)
        {
            var number = Convert.ToDecimal(values[AttributeName]);
            var bd = ValuesUtil.Convert(number);
            if (bd.CompareTo(_maxValue) <= 0)
                return;

            _oidOfMaxValues = oid;
            _maxValue = bd;
        }

        public override object GetValue()
        {
            return _maxValue;
        }

        public override void End()
        {
        }

        public override void Start()
        {
        }

        public OID GetOidOfMaxValues()
        {
            return _oidOfMaxValues;
        }

        public override IQueryFieldAction Copy()
        {
            return new MaxValueAction(AttributeName, Alias);
        }
    }
}
