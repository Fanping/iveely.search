using Iveely.STSdb4.Data;
using System;

namespace Iveely.STSdb4.Database
{
    public class XFile : XStream
    {
        public XFile(ITable<IData, IData> table)
            : base(table)
        {
        }
    }
}
