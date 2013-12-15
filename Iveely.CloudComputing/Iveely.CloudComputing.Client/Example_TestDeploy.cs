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
    public class Example_TestDeploy : Application
    {
        public override void Run(object[] args)
        {
            //1. 初始化
            this.Init(args);

            //2. 准备数据
            List<int> numbers = new List<int>();
            for (int i = 0; i < 1000; i++)
            {
                numbers.Add(2);
            }

            //3. 测试Merger
            double workersTotalSum = Mathematics.Sum<double>(numbers.Sum());
            WriteToConsole("Workers' Total Sum:" + workersTotalSum);

            //4. 测试缓存
            SetCache("Workers Total Sum", workersTotalSum);
            double cacheTotalSum = GetCache<double>("Workers Total Sum");
            WriteToConsole("From Cache,Workers' Total Sum:" + cacheTotalSum);

            WriteToConsole("Test Finished.");
        }
    }
}
