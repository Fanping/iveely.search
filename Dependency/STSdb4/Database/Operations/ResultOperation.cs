using STSdb4.Data;
using STSdb4.General.Collections;
using STSdb4.WaterfallTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STSdb4.Database.Operations
{

    public class TryGet_Result : PointOperation
    {
        public readonly IData Record;
        public readonly bool Exist;

        public TryGet_Result(IData key, IData record, bool exist)
            : base(OperationAction.TRY_GET_RESULT, key)
        {
            Record = record;
            Exist = exist;
        }
    }

    public class ForwardOperation_Result : RangeOperation
    {
        public readonly IEnumerable<KeyValuePair<IData, IData>> Set;
        public readonly int Count;

        public ForwardOperation_Result(IData from, int count, IEnumerable<KeyValuePair<IData, IData>> set)
            : base(OperationAction.FORWARD_RESULT, from, from)
        {
            Count = count;
            Set = set;
        }
    }

    public class BackwardOperation_Result : RangeOperation
    {
        public readonly IEnumerable<KeyValuePair<IData, IData>> Set;
        public readonly int Count;

        public BackwardOperation_Result(IData from, int count, IEnumerable<KeyValuePair<IData, IData>> set)
            : base(OperationAction.BACKWARD_RESULT, from, from)
        {
            Count = count;
            Set = set;
        }
    }

    public class CountOperation_Result : OverallOperation
    {
        public readonly long Count;

        public CountOperation_Result(long count)
            : base(OperationAction.COUNT_RESULT)
        {
            Count = count;
        }
    }

    public class ExceptionOperation_Result : OverallOperation
    {
        public readonly string Excetion;

        public ExceptionOperation_Result(string exception)
            : base(OperationAction.EXCETION_RESULT)
        {
            Excetion = exception;
        }
    }
}
