using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NDatabase.Core.Query.Linq
{
    internal static class OptimizeableMethodConstrains
    {
        public static bool CanBeOptimized(MethodInfo method)
        {
            return IsIListOrICollectionOfTMethod(method) || IsStringMethod(method);
        }

        public static bool IsStringMethod(MethodInfo method)
        {
            return method.DeclaringType == typeof (string);
        }

        public static bool IsIListOrICollectionOfTMethod(MethodInfo method)
        {
            var declaringType = method.DeclaringType;
            
            return IsGenericInstanceOf(declaringType, typeof (ICollection<>))
                   || typeof (IList).IsAssignableFrom(declaringType)
                   || declaringType == typeof (Enumerable);
        }

        private static bool IsGenericInstanceOf(Type enumerable, Type type)
        {
            return enumerable.IsGenericType && enumerable.GetGenericTypeDefinition() == type;
        }
    }
}