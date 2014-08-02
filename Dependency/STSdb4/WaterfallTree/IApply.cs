using Iveely.STSdb4.Data;
using Iveely.STSdb4.Database;
using Iveely.STSdb4.General.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iveely.STSdb4.WaterfallTree
{
    public interface IApply
    {
        /// <summary>
        /// Compact the operations and returns true, if the collection was modified.
        /// </summary>
        bool Internal(IOperationCollection operations);

        bool Leaf(IOperationCollection operations, IOrderedSet<IData, IData> data);

        Locator Locator { get; }
    }
}
