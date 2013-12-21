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
using System.Threading;
using System.Threading.Tasks;
using Iveely.CloudComputing.StateAPI;
using Iveely.Framework.Log;
using Iveely.Framework.Network;
using Iveely.Framework.Text;

namespace Iveely.CloudComputing.Worker
{
    /// <summary>
    /// 监控器
    /// (用于处理客户端发来的请求)
    /// </summary>
    [Serializable]
    public class Runner
    {
        private Thread _thread;

        public RunningStatus Status;

        private string _machineName;

        private int _servicePort;

        private string _runningPath;

        //private string _status;

        public Runner(ref RunningStatus status)
        {
            Status = status;
        }

        public void StartRun(string machineName, int servicePort)
        {
            _machineName = machineName;
            _servicePort = servicePort;
            byte[] dataBytes = Status.Packet.Data;
            string sourceCode = Encoding.UTF8.GetString(dataBytes);
            _runningPath = "ISE://application/" + Status.Packet.TimeStamp + "/" + Status.Packet.AppName + "/" +
                                 _machineName + "," + _servicePort;
            Logger.Info("Running path " + _runningPath);
            _thread = new Thread(Excute);
            _thread.Start(sourceCode);
        }

        private void Excute(object obj)
        {
            try
            {
                Status.Description = "Running";
                StateHelper.Put(_runningPath, "Start runing...");
                List<string> references = new List<string>();
                references.Add("Iveely.CloudComputing.Client.exe");
                references.Add("Iveely.Framework.dll");
                references.Add("System.Xml.dll");
                references.Add("System.Xml.Linq.dll");
                references.Add("NDatabase3.dll");
                CodeCompiler.Execode(obj.ToString(), Status.Packet.ClassName, references,
                    new object[] { Status.Packet.ReturnIp, Status.Packet.Port, _machineName, _servicePort, Status.Packet.TimeStamp, Status.Packet.AppName });
                StateHelper.Put(_runningPath, "Finished with success!");
                Program.SetStatus(Status.Packet.AppName, "Success");
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                StateHelper.Put(_runningPath, "Finished with " + exception);
                Program.SetStatus(Status.Packet.AppName, "Fisnihed with " + exception);
            }
        }

        public void Kill()
        {
            if (_thread.IsAlive)
            {
                _thread.Abort();
                StateHelper.Put(_runningPath, "Killed by user.");
                Program.SetStatus(Status.Packet.AppName, "Killed by user");
            }
        }

        public string GetStatus()
        {
            return Status.Description;
        }
    }
}
