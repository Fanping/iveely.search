using System;
using System.Collections.Generic;

namespace NDatabase.Tool
{
    internal static class ListExtensions
    {
        internal static bool IsEmpty<TItem>(this IList<TItem> self)
        {
            if (self == null)
                throw new ArgumentNullException("self");

            return self.Count == 0;
        }

        internal static bool IsNotEmpty<TItem>(this IList<TItem> self)
        {
            return !self.IsEmpty(); 
        }
    }
}