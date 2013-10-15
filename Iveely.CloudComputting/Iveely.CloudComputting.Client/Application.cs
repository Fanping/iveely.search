using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Iveely.Framework.Network;
using Iveely.Framework.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iveely.CloudComputting.Client
{
    /// <summary>
    /// 客户应用程序
    /// </summary>
    public abstract class Application
    {
        protected Framework.Network.Synchronous.Client Sender;

        public abstract void Run();

        public void DiagnosticsWrite(string information)
        {
            if (Sender == null)
            {
                //BUG:ip和端口需要改正
                string fromIp = Dns.GetHostName();
                string port = "8002";
                Sender = new Framework.Network.Synchronous.Client(fromIp, int.Parse(port));
            }
            Packet packet = new Packet(Serializer.SerializeToBytes("[result from:" + Dns.GetHostName() + "] " + information));
            //无需等待反馈
            packet.WaiteCallBack = false;
            Sender.Send<Packet>(packet);
        }

#if DEBUG

        [TestMethod]
        public void TestDiagnosticesWrite()
        {
            DiagnosticsWrite("Hello world!");
        }

#endif
    }
}
