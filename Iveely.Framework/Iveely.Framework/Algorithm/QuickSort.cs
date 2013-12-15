/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iveely.Framework.Algorithm
{
    /// <summary>
    /// 快速排序
    /// </summary>
    public class QuickSort<T> where T : IComparable
    {
        private T[] _array;

        public T[] GetResult(T[] array)
        {
            if (array == null)
            {
                return null;
            }
            this._array = array;
            Sort(this._array, 0, _array.Length - 1);
            return _array;
        }

        private void Sort(T[] numbers, int left, int right)
        {
            if (left < right)
            {
                T middle = numbers[(left + right) / 2];
                int i = left - 1;
                int j = right + 1;
                while (true)
                {
                    while (numbers[++i].CompareTo(middle) < 0) ;

                    while (numbers[--j].CompareTo(middle) > 0) ;

                    if (i >= j)
                        break;

                    Swap(numbers, i, j);
                }

                Sort(numbers, left, i - 1);
                Sort(numbers, j + 1, right);
            }
        }

        private void Swap(T[] numbers, int i, int j)
        {
            T number = numbers[i];
            numbers[i] = numbers[j];
            numbers[j] = number;
        }


    }
}
