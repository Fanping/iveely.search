using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iveely.STSdb4.WaterfallTree;
using Iveely.STSdb4.Data;

namespace Iveely.STSdb4.Database.Operations
{
    public abstract class OverallOperation : IOperation
    {
        public OverallOperation(int action)
        {
            Code = action;
        }

        public int Code { get; private set; }

        public OperationScope Scope
        {
            get { return OperationScope.Overall; }
        }

        public IData FromKey
        {
            get { return null; }
        }

        public IData ToKey
        {
            get { return null; }
        }
    }

    public class ClearOperation : OverallOperation
    {
        public ClearOperation()
            : base(OperationCode.CLEAR)
        {
        }
    }
}
