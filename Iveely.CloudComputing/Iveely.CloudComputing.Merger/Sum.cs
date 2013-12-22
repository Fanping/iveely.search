/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;

namespace Iveely.CloudComputing.Merger
{
    /// <summary>
    /// 求和运算
    /// </summary>
    public class Sum : Operate
    {
        private const string OperateType = "sum";

        private readonly string _flag;

        public Sum(string appTimeStamp, string appName)
            : base(appTimeStamp, appName)
        {
            this.AppName = appName;
            this.AppTimeStamp = appTimeStamp;
            _flag = OperateType + "_" + appTimeStamp + "_" + appName;
        }

        public override T Compute<T>(T val)
        {
            lock (Table)
            {
                if (Table[_flag] == null)
                {
                    Table.Add(_flag, val);
                    CountTable.Add(_flag, 1);
                }
                else
                {
                    double sum = double.Parse(Table[_flag].ToString()) + double.Parse(val.ToString());
                    Table[_flag] = sum;
                    int count = int.Parse(CountTable[_flag].ToString());
                    CountTable[_flag] = count + 1;
                }
            }
            if (Waite(_flag))
            {
                T t = (T)Convert.ChangeType(Table[_flag], typeof(T));
                return t;
            }
            throw new TimeoutException();
        }
    }
}
