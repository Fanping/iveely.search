using System;
using NDatabase.Api;
using NDatabase.Core.Query.Execution;
using NDatabase.Meta;

namespace NDatabase.Core.Query.Values
{
    /// <summary>
    ///   An action to compute the average value of a field
    /// </summary>
    internal sealed class AverageValueAction : AbstractQueryFieldAction
    {
        private readonly int _scale;
        private Decimal _average;
        private int _nbValues;
        private Decimal _totalValue;

        private const int ScaleForAverageDivision = 2;

        public AverageValueAction(string attributeName, string alias) : base(attributeName, alias, false)
        {
            _totalValue = new Decimal(0);
            _nbValues = 0;
            AttributeName = attributeName;
            _scale = ScaleForAverageDivision;
        }

        public override void Execute(OID oid, AttributeValuesMap values)
        {
            var n = Convert.ToDecimal(values[AttributeName]);
            _totalValue = Decimal.Add(_totalValue, ValuesUtil.Convert(n));
            _nbValues++;
        }

        public override object GetValue()
        {
            return _average;
        }

        public override void End()
        {
            var result = Decimal.Divide(_totalValue, _nbValues);
            _average = Decimal.Round(result, _scale, MidpointRounding.ToEven);
            //TODO: should we use _roundType here?
//            _average = Decimal.Round(result, _scale, _roundType);
        }

        public override void Start()
        {
        }

        public override IQueryFieldAction Copy()
        {
            return new AverageValueAction(AttributeName, Alias);
        }
    }
}
