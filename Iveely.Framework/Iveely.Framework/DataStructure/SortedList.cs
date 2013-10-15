using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iveely.Framework.DataStructure
{

    /// <summary>
    ///   (按照数的大小)排序泛型列表
    /// </summary>
    /// <typeparam name="T"> 类型 </typeparam>
    [Serializable]
    public class SortedList<T> : List<T>
    {
        public SortedList(IEnumerable<T> collection)
        {
            foreach (T item in collection)
            {
                Add(item);
            }
        }

        public SortedList()
        {
        }

        /// <summary>
        ///   插入
        /// </summary>
        /// <param name="item"> </param>
        public new void Add(T item)
        {
            try
            {
                int position = BinarySearch(item);
                position = position > 0 ? position : ~position;
                Insert(position, item);
            }
            catch
            {
                base.Add(item);
            }
        }

        public void Modify(T item, int index)
        {
            RemoveAt(index);
            Add(item);
        }
    }
}
