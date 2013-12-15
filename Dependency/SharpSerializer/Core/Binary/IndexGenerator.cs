#region Copyright � 2010 Pawel Idzikowski [idzikowski@sharpserializer.com]

//  ***********************************************************************
//  Project: sharpSerializer
//  Web: http://www.sharpserializer.com
//  
//  This software is provided 'as-is', without any express or implied warranty.
//  In no event will the author(s) be held liable for any damages arising from
//  the use of this software.
//  
//  Permission is granted to anyone to use this software for any purpose,
//  including commercial applications, and to alter it and redistribute it
//  freely, subject to the following restrictions:
//  
//      1. The origin of this software must not be misrepresented; you must not
//        claim that you wrote the original software. If you use this software
//        in a product, an acknowledgment in the product documentation would be
//        appreciated but is not required.
//  
//      2. Altered source versions must be plainly marked as such, and must not
//        be misrepresented as being the original software.
//  
//      3. This notice may not be removed or altered from any source distribution.
//  
//  ***********************************************************************

#endregion

using System.Collections.Generic;

namespace Polenter.Serialization.Core.Binary
{
    /// <summary>
    ///   Is used to store types and property names in lists. Contains only unique elements and gives index of the item back.
    ///   During deserialization this index is read from stream and then replaced with an appropriate value from the list.
    /// </summary>
    /// <typeparam name = "T"></typeparam>
    internal sealed class IndexGenerator<T>
    {
        private readonly List<T> _items = new List<T>();


        public IList<T> Items
        {
            get { return _items; }
        }

        /// <summary>
        ///   if the item exist, it gives its index back, otherweise the item is added and its new index is given back
        /// </summary>
        /// <param name = "item"></param>
        /// <returns></returns>
        public int GetIndexOfItem(T item)
        {
            int index = _items.IndexOf(item);

            // item was found
            if (index > -1) return index;

            // item was not found
            _items.Add(item);
            return _items.Count - 1;
        }
    }
}