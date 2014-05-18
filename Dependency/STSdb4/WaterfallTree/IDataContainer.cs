using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Iveely.Data;
using Iveely.General.Collections;

namespace Iveely.WaterfallTree
{
    public interface IDataContainer : IOrderedSet<IData, IData>
    {
        double FillPercentage { get; }

        bool IsEmpty { get; }
        /// <summary>
        /// Exclude and returns the right half part of the ordered set.
        /// </summary>
        IDataContainer Split(double percentage);

        /// <summary>
        /// Merge the specified set to the current set. The engine ensures, that all keys from the one set are less/greater than all keys from the other set.
        /// </summary>
        void Merge(IDataContainer data);

        IData FirstKey { get; }
        IData LastKey { get; }
    }
}
