using Iveely.Data;
using Iveely.Database;
using Iveely.General.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iveely.WaterfallTree
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
