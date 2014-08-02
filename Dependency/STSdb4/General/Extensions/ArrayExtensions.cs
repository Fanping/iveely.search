using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iveely.STSdb4.General.Extensions
{
    public static class ArrayExtensions
    {
        public static T[] Copy<T>(this T[] array)
        {
            T[] array2 = new T[array.Length];
            Array.Copy(array, array2, array.Length);

            return array2;
        }

        public static void InsertionSort<T>(this T[] array, int index, int count, IComparer<T> comparer)
        {
            int limit = index + count;
            for (int i = index + 1; i < limit; i++)
            {
                var item = array[i];

                int j = i - 1;
                while (comparer.Compare(array[j], item) > 0)
                {
                    array[j + 1] = array[j];
                    j--;
                    if (j < index)
                        break;
                }

                array[j + 1] = item;
            }
        }

        public static void InsertionSort<T>(this T[] array, IComparer<T> comparer)
        {
            InsertionSort<T>(array, 0, array.Length, comparer);
        }

        public static T[] Middle<T>(this T[] buffer, int offset, int length)
        {
            T[] middle = new T[length];
            Array.Copy(buffer, offset, middle, 0, length);
            return middle;
        }

        public static T[] Left<T>(this T[] buffer, int length)
        {
            return buffer.Middle(0, length);
        }

        public static T[] Right<T>(this T[] buffer, int length)
        {
            return buffer.Middle(buffer.Length - length, length);
        }

        public static string ToString<T>(this T[] array, string separator)
        {
            return "{" + String.Join<T>(separator, array) + "}";
        }

        public static List<T> CreateList<T>(this T[] array, int count)
        {
            List<T> list = new List<T>();

            list.SetArray(array);
            list.SetCount(count);
            //list.IncrementVersion();

            return list;
        }
    }
}
