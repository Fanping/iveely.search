using Iveely.Data;
using System;

namespace Iveely.Database
{
    public class XFile : XStream
    {
        public XFile(ITable<IData, IData> table)
            : base(table)
        {
        }
    }
}
