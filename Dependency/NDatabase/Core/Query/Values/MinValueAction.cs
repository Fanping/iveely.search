using System;
using NDatabase.Api;
using NDatabase.Core.Query.Execution;
using NDatabase.Meta;

namespace NDatabase.Core.Query.Values
{
    /// <summary>
    ///   An action to compute the max value of a field
    /// </summary>
    internal sealed class MinValueAction : AbstractQueryFieldAction
    {
        private Decimal _minValue;
        private OID _oidOfMinValues;

        public MinValueAction(string attributeName, string alias) : base(attributeName, alias, false)
        {
            _minValue = new Decimal(long.MaxValue);
            _oidOfMinValues = null;
        }

        public override void Execute(OID oid, AttributeValuesMap values)
        {
            var number = Convert.ToDecimal(values[AttributeName]);
            var bd = ValuesUtil.Convert(number);
            if (_minValue.CompareTo(bd) <= 0) 
                return;

            _oidOfMinValues = oid;
            _minValue = bd;
        }

        public override object GetValue()
        {
            return _minValue;
        }

        public override void End()
        {
        }

        public override void Start()
        {
        }

        public OID GetOidOfMinValues()
        {
            return _oidOfMinValues;
        }

        public override IQueryFieldAction Copy()
        {
            return new MinValueAction(AttributeName, Alias);
        }
    }
}
