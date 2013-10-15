using System;
using System.Threading;
using System.Diagnostics;
using System.Net;
using Iveely.CloudComputting.Configuration;
using Iveely.Framework.Log;
using log4net;

namespace Iveely.CloudComputting.StateCenter
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
            StandAlone();

            // 2. 启动还原
            State.Restore();

            // 5. 启动记录状态服务
            Logger.Info("State Center is start listening...");
            StateServer.Listen();

            State.Put("ISE://system/state/state center", "state center start running.");
        }

        /// <summary>
        /// 单进程运行
        /// </summary>
        private static void StandAlone()
        {
            Process currentProcess = Process.GetCurrentProcess();
            foreach (Process item in Process.GetProcessesByName(currentProcess.ProcessName))
            {
                if (item.Id != currentProcess.Id &&
                (item.StartTime - currentProcess.StartTime).TotalMilliseconds <= 0)
                {
                    Logger.Error("Error:In a physical machine, application only allow one instance.\nPress any key to end ...");
                    item.Kill();
                    item.WaitForExit();
                    break;
                }
            }
        }
    }
}
