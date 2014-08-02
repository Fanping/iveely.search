using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iveely.STSdb4.Data
{
    public class Data<T> : IData
    {
        public T Value;

        public Data()
        {
        }

        public Data(T value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
