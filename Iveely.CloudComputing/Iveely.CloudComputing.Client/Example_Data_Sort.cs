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
using Iveely.CloudComputing.Client;
using Iveely.Framework.Algorithm;

namespace Iveely.CloudComputing.Example
{
    public class Example_Data_Sort : Application
    {
        public override void Run(object[] args)
        {
            //1. 初始化
            this.Init(args);

            //2. 准备数据
            int[] array = new int[10000];
            for (int i = 0; i < 10000; i++)
            {
                Random random = new Random(i);
                array[i] = random.Next(0, 10000);
            }

            //3. 开始排序
            Mathematics.CombineSort(array);

        }
    }
}
