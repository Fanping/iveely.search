using Iveely.STSdb4.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iveely.STSdb4.General.Collections
{
    public interface IOrderedSetFactory
    {
        IOrderedSet<IData, IData> Create();
    }
}
