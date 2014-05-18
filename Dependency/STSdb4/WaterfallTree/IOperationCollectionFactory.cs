using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iveely.WaterfallTree
{
    public interface IOperationCollectionFactory
    {
        IOperationCollection Create(int capacity);
        IOperationCollection Create(IOperation[] operations, int commonAction, bool areAllMonotoneAndPoint);
    }
}
