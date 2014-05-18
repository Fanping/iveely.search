using Iveely.Data;
using Iveely.General.Persist;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iveely.WaterfallTree
{
    public class TypeEngine
    {
        private static readonly ConcurrentDictionary<Type, TypeEngine> map = new ConcurrentDictionary<Type, TypeEngine>();

        public IComparer<IData> Comparer { get; set; }
        public IEqualityComparer<IData> EqualityComparer { get; set; }
        public IPersist<IData> Persist { get; set; }
        public IIndexerPersist<IData> IndexerPersist { get; set; }

        public TypeEngine()
        {
        }

        private static TypeEngine Create(Type type)
        {
            TypeEngine descriptor = new TypeEngine();

            descriptor.Persist = new DataPersist(type, null, AllowNull.AllButTop);

            if (DataTypeUtils.IsAllPrimitive(type) || type == typeof(Guid))
            {
                descriptor.Comparer = new DataComparer(type);
                descriptor.EqualityComparer = new DataEqualityComparer(type);

                if (type != typeof(Guid))
                    descriptor.IndexerPersist = new DataIndexerPersist(type);
            }

            return descriptor;
        }

        public static TypeEngine Default(Type type)
        {
            return map.GetOrAdd(type, Create(type));
        }
    }
}
