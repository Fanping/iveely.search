using Iveely.STSdb4.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iveely.STSdb4.WaterfallTree
{
    public enum OperationScope : byte
    {
        Point,
        Range,
        Overall
    }

    public interface IOperation
    {
        int Code { get; }
        OperationScope Scope { get; }

        IData FromKey { get; }
        IData ToKey { get; }
    }
}