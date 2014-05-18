using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iveely.WaterfallTree;
using Iveely.Data;
using Iveely.General.Collections;

namespace Iveely.Database.Operations
{
    public abstract class RangeOperation : IOperation
    {
        private readonly IData from;
        private readonly IData to;

        protected RangeOperation(int action, IData from, IData to)
        {
            Code = action;
            this.from = from;
            this.to = to;
        }

        protected RangeOperation(int action)
        {
            Code = action;
        }

        public int Code { get; private set; }

        public OperationScope Scope
        {
            get { return OperationScope.Range; }
        }

        public IData FromKey
        {
            get { return from; }
        }

        public IData ToKey
        {
            get { return to; }
        }
    }

    public class DeleteRangeOperation : RangeOperation
    {
        public DeleteRangeOperation(IData from, IData to)
            : base(OperationCode.DELETE_RANGE, from, to)
        {
        }
    }
}
