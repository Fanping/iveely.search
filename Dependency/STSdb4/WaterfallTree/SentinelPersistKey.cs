using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Iveely.General.Persist;
using Iveely.Data;
using Iveely.WaterfallTree;

namespace Iveely.WaterfallTree
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
