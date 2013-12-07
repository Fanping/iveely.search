using System;
using System.Collections.Generic;
using System.Linq;

namespace NDatabase.Core.Query.Linq
{
    internal static class TypeSystem
    {
        internal static Type GetElementType(Type seqType)
        {
            var iEnumerable = FindIEnumerable(seqType);

            return iEnumerable == null
                       ? seqType
                       : iEnumerable.GetGenericArguments()[0];
        }

        private static Type FindIEnumerable(Type seqType)
        {
            if (seqType == null || seqType == typeof (string))
                return null;

            if (seqType.IsArray)
                return typeof (IEnumerable<>).MakeGenericType(seqType.GetElementType());

            if (seqType.IsGenericType)
            {
                foreach (var typeArguments in seqType.GetGenericArguments())
                {
                    var iEnumerable = typeof (IEnumerable<>).MakeGenericType(typeArguments);

                    if (iEnumerable.IsAssignableFrom(seqType))
                        return iEnumerable;
                }
            }

            var interfaces = seqType.GetInterfaces();

            if (interfaces.Length > 0)
            {
                foreach (var iEnumerable in interfaces.Select(FindIEnumerable).Where(iEnumerable => iEnumerable != null))
                    return iEnumerable;
            }

            return seqType.BaseType != null && seqType.BaseType != typeof (object)
                       ? FindIEnumerable(seqType.BaseType)
                       : null;
        }
    }
}
