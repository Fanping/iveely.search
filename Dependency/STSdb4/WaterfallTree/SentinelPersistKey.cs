using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Iveely.STSdb4.General.Persist;
using Iveely.STSdb4.Data;
using Iveely.STSdb4.WaterfallTree;

namespace Iveely.STSdb4.WaterfallTree
{
    public class SentinelPersistKey : IPersist<IData>
    {
        public static readonly SentinelPersistKey Instance = new SentinelPersistKey();

        public void Write(BinaryWriter writer, IData item)
        {
        }

        public IData Read(BinaryReader reader)
        {
            return null;
        }
    }
}
