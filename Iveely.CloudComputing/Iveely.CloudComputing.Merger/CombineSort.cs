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
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Iveely.Framework.Algorithm;

namespace Iveely.CloudComputing.Merger
{
    public class CombineSort : Operate
    {
        private const string OperateType = "combine_sort";

        private string flag;

        public CombineSort(string appTimeStamp, string appName)
            : base(appTimeStamp, appName)
        {
            this.AppName = appName;
            this.AppTimeStamp = appTimeStamp;
            flag = OperateType + "_" + appTimeStamp + "_" + appName;
        }

        //public override T Compute<T>(T val)
        //{
        //    return default(T);
        //}

        public T[] ArrayCompute<T>(T[] val)
        {
            lock (Table)
            {
                if (Table[flag] == null)
                {
                    Table.Add(flag, ChangeType(val));
                    CountTable.Add(flag, 1);
                }
                else
                {
                    double[] list = (double[])Table[flag];
                    CombineSort<double> combine = new CombineSort<double>();
                    list = combine.GetResult(list, ChangeType(val));
                    Table[flag] = list;
                    int count = int.Parse(CountTable[flag].ToString());
                    CountTable[flag] = count + 1;
                }
            }
            if (Waite(flag))
            {
                return Array.ConvertAll<double, T>((double[])Table[flag],
                    delegate(double n) { return (T)Convert.ChangeType(n, typeof(T)); });
            }
            throw new TimeoutException();
        }

        public override T Compute<T>(T val)
        {
            throw new NotImplementedException();
        }

        private double[] ChangeType<T>(T[] val)
        {
            return Array.ConvertAll<T, double>(val, delegate(T n) { return double.Parse(n.ToString()); });
        }
    }



}
