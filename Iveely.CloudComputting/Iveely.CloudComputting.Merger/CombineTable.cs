/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：13896622743
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iveely.CloudComputting.Merger
{
    public class CombineTable : Operate
    {
        private const string OperateType = "combine_table";

        private string flag;

        public CombineTable(string appTimeStamp, string appName)
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
                    Hashtable oldTable = (Hashtable)Table[flag];
                    Hashtable currentTable = (Hashtable)(object)val;
                    foreach (DictionaryEntry dictionaryEntry in currentTable)
                    {
                        if (oldTable.ContainsKey(dictionaryEntry.Key))
                        {
                            double oldValue = double.Parse(oldTable[dictionaryEntry.Key].ToString());
                            oldTable[dictionaryEntry.Key] = oldValue + double.Parse(dictionaryEntry.Value.ToString());
                        }
                        else
                        {
                            oldTable.Add(dictionaryEntry.Key, dictionaryEntry.Value);
                        }
                    }

                    Table[flag] = oldTable;
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
