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

namespace Iveely.Framework.Algorithm
{
    /// <summary>
    ///   K-means 算法
    /// </summary>
    public class Kmeans<T>
    {
        #region 内部类

        /// <summary>
        ///   子元素的扩充
        /// </summary>
        public class Subset
        {
            #region 属性 OR 字段

            /// <summary>
            ///   元素属于哪一类
            /// </summary>
            public int Belong { get; set; }

            /// <summary>
            ///   元素值
            /// </summary>
            public T Data { get; set; }

            /// <summary>
            ///   距离聚簇中心的距离
            /// </summary>
            public double Distance { get; set; }

            #endregion

            #region 公有方法

            public Subset(int belong, T data)
            {
                this.Belong = belong;
                this.Data = data;
                this.Distance = Double.MaxValue;
            }

            #endregion
        }

        #endregion

        #region 属性 OR 字段

        #region Delegates

        /// <summary>
        ///   委托的计算距离方法
        /// </summary>
        /// <returns> 返回(a,b)的距离 </returns>
        public delegate double GetDistance(T a, T b);

        #endregion

        /// <summary>
        ///   聚集质点集合
        /// </summary>
        private readonly Subset[] _cluster;

        /// <summary>
        ///   聚集质点数
        /// </summary>
        private readonly int _particle;

        /// <summary>
        ///   实际计算距离执行方法
        /// </summary>
        private GetDistance _getlength;

        /// <summary>
        ///   迭代次数
        /// </summary>
        private int _iteration;

        /// <summary>
        ///   用于计算的集合们
        /// </summary>
        public List<Subset> Data { get; private set; }

        #endregion

        #region 公有方法

        public Kmeans(int particle, int iteration = 1000)
        {
            this._particle = particle;
            this._iteration = iteration;
            this._cluster = new Subset[particle];
            for (int i = 0; i < particle; i++)
            {
                this._cluster[i] = new Subset(i, default(T));
            }
            this.Data = new List<Subset>();
        }

        public void SetData(List<T> datas, GetDistance distance)
        {
            //随机分配到各个中心
            for (int i = 0; i < datas.Count; i++)
            {
                this.Data.Add(new Subset(i % this._particle, datas[i]));
            }
            if (this._getlength == null)
            {
                this._getlength = distance;
            }
            //第一次分配中心点
            for (int i = 0; i < this._particle; i++)
            {
                this._cluster[i].Data = this.Data[this.Data.Count / (i + 1) - 1].Data;
            }
            this.SetCenter();
        }

        #endregion

        #region 私有方法

        /// <summary>
        ///   计算聚簇中心
        /// </summary>
        /// <returns> true表示中心未改变(则无需迭代) </returns>
        private bool SetCenter()
        {
            // 1. 初始化聚点距离
            var distances = new List<double>();
            var counts = new List<long>();
            for (int i = 0; i < this._particle; i++)
            {
                distances.Add(0.0);
                counts.Add(0);
            }

            // 2. 计算每一个聚簇中的点到聚簇点距离之和
            foreach (var data in this.Data)
            {
                distances[data.Belong] += data.Distance;
                counts[data.Belong]++;
            }

            // 3. 计算平均距离
            var avgs = new List<double>();
            for (int i = 0; i < this._particle; i++)
            {
                avgs.Add(distances[i] / counts[i]);
            }

            // 4. 找出聚簇中心点
            bool flag = true;
            foreach (var data in this.Data)
            {
                double len = Math.Abs(data.Distance - avgs[data.Belong]);
                if (len < this._cluster[data.Belong].Distance)
                {
                    flag = false;
                    this._cluster[data.Belong].Distance = len;
                    this._cluster[data.Belong].Data = data.Data;
                }
            }
            return flag;
        }

        private T[] Compute()
        {
            do
            {
                for (int i = 0; i < this.Data.Count; i++)
                {
                    for (int j = 0; j < this._cluster.Length; j++)
                    {
                        double length = this._getlength(this.Data[i].Data, this._cluster[j].Data);
                        if (this.Data[i].Distance > length)
                        {
                            this.Data[i].Distance = length;
                            this.Data[i].Belong = j;
                        }
                    }
                }
            } while (this._iteration-- > 0 && !this.SetCenter());

            return (from data in this._cluster select data.Data).ToArray();
        }

        #endregion

        #region 测试

        /// <summary>
        ///   一元组距离算法
        /// </summary>
        /// <param name="a"> </param>
        /// <param name="b"> </param>
        /// <returns> </returns>
        private static double Ugram_GetLenTest(int a, int b)
        {
            return Math.Abs(a - b) * 1.0;
        }

        /// <summary>
        ///   一元组测试
        /// </summary>
        public static void Ugram_Test()
        {
            var kmeans = new Kmeans<int>(2, 2000);
            var datas = new List<int>();
            for (int i = -50; i < 100; i++)
            {
                datas.Add(i);
            }

            //for (int i = 100; i < 200; i++)
            //{
            //    datas.Add(i * 10);
            //}

            kmeans.SetData(datas, Ugram_GetLenTest);
            int[] centers = kmeans.Compute();
            foreach (int center in centers)
            {
                Console.WriteLine(center);
            }
        }

        #endregion
    }
}
