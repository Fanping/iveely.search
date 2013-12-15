using System;
using System.Threading;
using System.Diagnostics;
using System.Net;
using Iveely.CloudComputing.Configuration;
using Iveely.Framework.Log;
using log4net;

namespace Iveely.CloudComputing.StateCenter
{
    public class Program
    {
        /// <summary>
        /// 状态中心的服务
        /// </summary>
        private static readonly Service StateServer =
            new Service(Dns.GetHostName(), SettingItem.GetInstance().StateCenterPort);

        static void Main()
        {
            // 1. 单进程运行
            Framework.Process.RunningState.StandAlone();

            // 2. 启动还原
            //State.Restore();

            // 5. 启动记录状态服务
            Logger.Info("State Center is start listening...");
            StateServer.Listen();

            State.Put("ISE://system/state/state center", "state center start running.");
        }
    }
}
