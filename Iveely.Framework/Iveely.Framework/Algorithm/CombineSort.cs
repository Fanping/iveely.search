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

namespace Iveely.Framework.Algorithm
{
    /// <summary>
    /// 归并排序
    /// </summary>
    public class CombineSort<T>
    {
        private List<double> _arrayA;

        private List<double> _arrayB;

        public T[] GetResult(T[] arrayA, T[] arrayB)
        {
            if (arrayA == null)
            {
                return arrayB;
            }
            else if (arrayB == null)
            {
                return arrayA;
            }
            else
            {

                this._arrayA = new List<double>(Array.ConvertAll<T, double>(arrayA, delegate(T n) { return int.Parse(n.ToString()); }));
                this._arrayB = new List<double>(Array.ConvertAll<T, double>(arrayB, delegate(T n) { return int.Parse(n.ToString()); }));
                return Sort();
            }
        }

        private T[] Sort()
        {
            List<double> temp = new List<double>();
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
            return Array.ConvertAll<double, T>(temp.ToArray(),
                delegate(double n) { return (T)Convert.ChangeType(n, typeof(T)); });
        }
    }
}
