/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Iveely.CloudComputing.Merger
{
    /// <summary>
    /// 求和运算
    /// </summary>
    public class Sum : Operate
    {
        private const string OperateType = "sum";

        private string flag;

        public Sum(string appTimeStamp, string appName)
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
                    double sum = double.Parse(Table[flag].ToString()) + double.Parse(val.ToString());
                    Table[flag] = sum;
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
