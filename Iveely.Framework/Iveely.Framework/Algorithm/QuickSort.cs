/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：13896622743
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
        private readonly T[] _array;

        public QuickSort(T[] array)
        {
            this._array = array;

        }

        public T[] GetResult()
        {
            return Sort(this._array, 0, _array.Length - 1);
        }

        private T[] Sort(T[] ar, int low, int high)
        {
            int left = low;
            int right = high;
            T baseValue = ar[low];
            while (left < right)
            {
                while (left < right && ar[right].CompareTo(baseValue) >= 0)
                {
                    right--;
                }
                if (left < right)
                {
                    ar[left] = ar[right];
                    left++;
                }

                while (left < right && ar[left].CompareTo(baseValue) < 0)
                {
                    left++;
                }
                if (left < right)
                {
                    ar[right] = ar[left];
                    right--;
                }
            }

            ar[left] = baseValue;
            if (left < high && left >= low)
            {
                Sort(ar, 0, left);
                Sort(ar, left + 1, high);
            }
            return ar;
        }

        //private int CompareTo(object obj)
        //{
        //    double thisValue = (double)Convert.ChangeType(this, typeof(double));
        //    double compareValue = (double)Convert.ChangeType(obj, typeof(double));
        //    return (int)((int)thisValue - compareValue);
        //}


    }
}
