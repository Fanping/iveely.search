/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：13896622743
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using Iveely.Framework.Log;

namespace Iveely.Framework.Process
{
    public class RunningState
    {
        /// <summary>
        /// 单进程运行
        /// </summary>
        public static void StandAlone()
        {
            System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            foreach (System.Diagnostics.Process item in System.Diagnostics.Process.GetProcessesByName(currentProcess.ProcessName))
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
