using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iveely.Data
{
    public interface ITransformer<T1, T2>
    {
        T2 To(T1 value1);
        T1 From(T2 value2);
    }
}
