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

namespace Iveely.Framework.Algorithm
{
    /// <summary>
    /// 归并排序
    /// </summary>
    public class CombineSort<T> where T : IComparable
    {
        private readonly List<T> _arrayA;

        private readonly List<T> _arrayB;

        public CombineSort(T[] arrayA, T[] arrayB)
        {
            if (arrayA == null || arrayB == null)
            {
                throw new ArgumentNullException();
            }
            this._arrayA = new List<T>(arrayA);
            this._arrayB = new List<T>(arrayB);
        }

        public T[] GetResult()
        {
            return Sort();
        }

        public T[] Sort()
        {
            List<T> temp = new List<T>();
            while (_arrayA.Count > 0 && _arrayB.Count > 0)
            {
                if (_arrayA[0].CompareTo(_arrayB[0]) <= 0)
                {
                    temp.Add(_arrayA[0]);
                    _arrayA.RemoveAt(0);
                }
                else
                {
                    temp.Add(_arrayB[0]);
                    _arrayB.RemoveAt(0);
                }
            }
            if (_arrayA.Count > 0)
            {
                for (int i = 0; i < _arrayA.Count; i++)
                {
                    temp.Add(_arrayA[i]);
                }
            }
            if (_arrayB.Count > 0)
            {
                for (int i = 0; i < _arrayB.Count; i++)
                {
                    temp.Add(_arrayB[i]);
                }
            }
            return temp.ToArray();
        }
    }
}
