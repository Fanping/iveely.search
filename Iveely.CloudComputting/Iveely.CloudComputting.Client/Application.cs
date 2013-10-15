using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Iveely.CloudComputting.StateAPI;
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

        public abstract void Run(object[] args);

        public void DiagnosticsWrite(string information, object[] parameters)
        {
            if (Sender == null)
            {
                //BUG:传递parameter这个参数，非常不友好
                string fromIp = parameters[0].ToString();
                string port = parameters[1].ToString();
                Sender = new Framework.Network.Synchronous.Client(fromIp, int.Parse(port));
            }
            Packet packet = new Packet(Serializer.SerializeToBytes("[result from:" + parameters[2] + ",+" + parameters[3] + "] " + information));
            //无需等待反馈
            packet.WaiteCallBack = false;
            Sender.Send<Packet>(packet);
        }
    }
}
