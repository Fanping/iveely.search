using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Iveely.WaterfallTree
{
    public class TypeCache
    {
        private static readonly ConcurrentDictionary<string, Type> cache = new ConcurrentDictionary<string, Type>();

        public static Type GetType(string fullName)
        {
            var type = Type.GetType(fullName, false);
            if (type != null)
                return type;

            return cache.GetOrAdd(fullName, (x) =>
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = assembly.GetType(fullName);
                    if (type != null)
                        return type;
                }

                return null; //once return null - always return null
            });
        }
    }
}
