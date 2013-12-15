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
using Iveely.CloudComputing.Client;

namespace Iveely.CloudComputing.Example
{
    /// <summary>
    /// 出租车轨迹类
    /// </summary>
    public class TaxiPath : IComparable
    {
        public long TaxiId { get; private set; }

        public long Time { get; private set; }

        public double LocationX { get; private set; }

        public double LocationY { get; private set; }

        public TaxiPath(long taxiId, long time, double locationX, double locationY)
        {
            this.TaxiId = taxiId;
            this.Time = time;
            this.LocationX = locationX;
            this.LocationY = locationY;
        }

        public override string ToString()
        {
            return TaxiId + "," + Time + "," + LocationX + "," + LocationY;
        }

        public int CompareTo(object obj)
        {
            TaxiPath taxiPath = obj as TaxiPath;
            if (taxiPath == null)
            {
                return -1;
            }
            else
            {
                if (this.TaxiId > taxiPath.TaxiId)
                {
                    return (int)((int)TaxiId - taxiPath.TaxiId);
                }
                else if (this.TaxiId == taxiPath.TaxiId)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
        }
    }

    public class TaxiFinder : Application
    {
        public override void Run(object[] args)
        {
            //1.获取所有记录
            this.Init(args);
            string[] lines = ReadAllText("taxi.data", false);
            WriteToConsole("Total lines count:" + lines.Count());

            //2.分别分析
            List<TaxiPath> taxiPaths = new List<TaxiPath>();
            long maxErrorLine = 100;
            foreach (var line in lines)
            {
                if (maxErrorLine < 1)
                {
                    WriteToConsole("Too many error lines, app has exit.");
                    break;
                }
                string[] columns = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (columns.Length == 9)
                {
                    long taxiId = long.Parse(columns[0]);
                    long time = long.Parse(columns[3]);
                    double locationX = double.Parse(columns[4]);
                    double locationY = double.Parse(columns[5]);
                    TaxiPath taxiPath = new TaxiPath(taxiId, time, locationX, locationY);
                    taxiPaths.Add(taxiPath);
                }
                else
                {
                    maxErrorLine--;
                }
            }
            taxiPaths.Sort();
            WriteList(taxiPaths, "taxi.path", false);
            WriteToConsole("Finished.");
        }
    }
}
