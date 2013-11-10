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
using System.Text;
using System.Threading.Tasks;

namespace Iveely.CloudComputing.Merger
{
    /// <summary>
    /// 应用程序操作集
    /// </summary>
    public abstract class Operate
    {
        /// <summary>
        /// 应用程序时间戳
        /// </summary>
        public string AppTimeStamp
        {
            get;
            protected set;
        }

        /// <summary>
        /// 应用程序名称
        /// </summary>
        public string AppName
        {
            get;
            protected set;
        }

        public static int ExpectCount = -1;

        /// <summary>
        /// 个数统计表
        /// （例如有3个worker在工作，则需要3个worker的求和完毕方能退出）
        /// </summary>
        public static Hashtable CountTable;

        /// <summary>
        /// 计算表
        /// </summary>
        public static Hashtable Table;

        public Operate(string appTimeStamp, string appName)
        {
            this.AppTimeStamp = appTimeStamp;
            this.AppName = appName;
            if (Table == null)
            {
                Table = new Hashtable();
                CountTable = new Hashtable();
            }

            if (ExpectCount == -1)
            {
                ExpectCount = StateAPI.StateHelper.GetChildren("ISE://system/state/worker").Count();
            }

        }

        public abstract T Compute<T>(T val);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flag">"sum_[timestamp]_[appname]"</param>
        /// <returns></returns>
        public bool Waite(string flag)
        {
            //BUG:什么时候清理CountTable中的数据？
            int allowSeconds = 10;
            DateTime dateTime = DateTime.UtcNow;
            while ((DateTime.UtcNow - dateTime).TotalSeconds <= allowSeconds)
            {
                int actualCount = int.Parse(CountTable[flag].ToString());
                if (actualCount == ExpectCount)
                {
                    return true;
                }
            }
            return false;
        }

        public void Remove(string flag)
        {

            if (CountTable.ContainsKey(flag))
            {
                CountTable.Remove(flag);
                Table.Remove(flag);
            }
        }
    }
}
