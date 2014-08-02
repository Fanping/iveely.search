using System;
using System.Collections.Generic;
using System.Threading;

namespace Iveely.Framework.Process
{
    /// <summary>
    /// 多线程模板类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ThreadManager<T>
    {

        #region -- 私有属性 --

        /// <summary>
        /// 开始执行的时间
        /// </summary>
        private DateTime StartDateTime = DateTime.Now;

        /// <summary>
        /// 总任务数量
        /// </summary>
        private int TotalTask = 0;

        /// <summary>
        /// 锁
        /// </summary>
        private object obj = new object();

        /// <summary>
        /// 计数器
        /// </summary>
        private int count = 0;

        /// <summary>
        /// 计数器,内部使用
        /// </summary>
        private int Count
        {
            set
            {
                lock (this.obj)
                    this.count = value;
            }

            get
            {
                return this.count;
            }
        }

        /// <summary>
        /// 每隔多少秒输出一次,默认30秒
        /// </summary>
        private int IntervalDisplay = 30;

        /// <summary>
        /// 是否以单线程执行
        /// </summary>
        private bool IsSingleThread = false;

        /// <summary>
        /// 数据
        /// </summary>
        private List<T> Items = null;

        /// <summary>
        /// 委托方法
        /// </summary>
        private event Func<T, object> threadItemDo;

        /// <summary>
        /// 当前任务Title
        /// </summary>
        private string title = null;

        private string defaultString = "==========================================================================";

        #endregion

        #region -- 设置参数 --

        /// <summary>
        /// 设置每隔多少秒输出一次,默认30秒
        /// </summary>
        /// <param name="second"></param>
        public void SetIntervalDisplay(int second)
        {
            this.IntervalDisplay = second;
        }

        /// <summary>
        /// 设置是否以单线程执行
        /// </summary>
        /// <param name="isSingleThread"></param>
        public void SetIsSingleThread(bool isSingleThread)
        {
            this.IsSingleThread = isSingleThread;

        }

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="list"></param>
        public void SetData(List<T> list)
        {
            Items = list;
        }

        /// <summary>
        /// 设置委托方法
        /// </summary>
        /// <param name="threadItemDo"></param>
        public void SetFunction(Func<T, object> threadItemDo)
        {
            this.threadItemDo = threadItemDo;
        }

        /// <summary>
        /// 设置Task Title
        /// </summary>
        /// <param name="title"></param>
        public void SetTitle(string title)
        {
            this.title = title;
        }

        #endregion



        #region -- 线程执行方法 --

        /// <summary>
        /// 入口方法
        /// </summary>

        public void Start()
        {
            ThreadPool.SetMaxThreads(20, 20);
            this.Init();
            if (this.IsSingleThread)
                this.DoSingleThread();
            else
                this.DoMultiThread();

        }

        /// <summary>
        /// 初始化方法
        /// </summary>
        private void Init()
        {
            if (this.Items == null || this.Items.Count == 0)
                throw new Exception("参数不正确");

            if (threadItemDo == null)
                throw new Exception("委托方法不正确");

            this.Count = this.Items.Count;
            this.TotalTask = this.Count;

        }

        /// <summary>
        /// 单线程执行
        /// </summary>

        private void DoSingleThread()
        {
            foreach (var item in this.Items)
            {
                var param = (T)item;
                threadItemDo(param);
                Count--;
            }
        }

        /// <summary>
        /// 多线程执行
        /// </summary>

        private void DoMultiThread()
        {
            foreach (var item in this.Items)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object obj)
                {
                    var param = (T)obj;
                    threadItemDo(param);
                    Count--;
                }), item);
            }

            AutoResetEvent mainAutoResetEvent = new AutoResetEvent(false);
            RegisteredWaitHandle registeredWaitHandle = null;
            registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(new AutoResetEvent(false), new WaitOrTimerCallback(delegate(object obj, bool timeout)
            {
                int workerThreads = 0;
                int maxWordThreads = 0;
                int compleThreads = 0;
                ThreadPool.GetAvailableThreads(out workerThreads, out compleThreads);
                ThreadPool.GetMaxThreads(out maxWordThreads, out compleThreads);

                //当可用的线数与池程池最大的线程相等时表示线程池中所有的线程已经完成
                if (workerThreads == maxWordThreads)
                {
                    mainAutoResetEvent.Set();
                    registeredWaitHandle.Unregister(null);
                }

            }), null, this.IntervalDisplay * 1000, false);
            mainAutoResetEvent.WaitOne();
        }

        /// <summary>
        /// 输出状态消息
        /// </summary>
        /// <returns></returns>
        private string Message()
        {
            var tp = (TimeSpan)(DateTime.Now - StartDateTime);
            string residueMsg = string.Empty;
            if (tp.TotalSeconds > 0)
            {
                if (TotalTask > Count)
                {
                    int residueSecond = (int)(Count / ((decimal)(TotalTask - Count) / (decimal)tp.TotalSeconds));
                    residueMsg = string.Format(",剩余时间:{0}:{1}:{2}", residueSecond / 3600, residueSecond / 60 % 60, residueSecond % 60);
                }
            }
            string msg = string.Format("[{5}][剩余任务:{0}{4},执行耗时:{1}:{2}:{3}]", this.Count, (int)(tp.TotalHours), tp.Minutes, tp.Seconds,
            residueMsg == string.Empty ? string.Empty : residueMsg, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            int n = (defaultString.Length - (msg.Length + 8));
            msg = "===" + msg;
            for (int index = 0; index < n - 3 - (residueMsg == string.Empty ? 0 : 4); index++)
                msg += "=";
            return msg;
        }
        #endregion

    }
}
