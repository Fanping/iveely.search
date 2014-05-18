using Iveely.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iveely.General.Collections
{
    public interface IOrderedSetFactory
    {
        IOrderedSet<IData, IData> Create();
    }
}
