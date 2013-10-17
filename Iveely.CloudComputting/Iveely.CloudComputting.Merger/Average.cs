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

namespace Iveely.CloudComputting.Merger
{
    /// <summary>
    /// 平均数运算
    /// </summary>
    public class Average : Operate
    {
        private const string OperateType = "average";

        private string flag;

        public Average(string appTimeStamp, string appName)
            : base(appTimeStamp, appName)
        {
            this.AppName = appName;
            this.AppTimeStamp = appTimeStamp;
            flag = OperateType + "_" + appTimeStamp + "_" + appName;
        }

        public override T Compute<T>(T val)
        {
            lock (Table)
            {
                if (Table[flag] == null)
                {
                    Table.Add(flag, val);
                    CountTable.Add(flag, 1);
                }
                else
                {
                    double average = (double.Parse(Table[flag].ToString()) + double.Parse(val.ToString())) / 2.0;
                    Table[flag] = average;
                    int count = int.Parse(CountTable[flag].ToString());
                    CountTable[flag] = count + 1;
                }
            }
            if (Waite(flag))
            {
                T t = (T)Convert.ChangeType(Table[flag], typeof(T));
                return t;
            }
            throw new TimeoutException();
        }
    }
}
