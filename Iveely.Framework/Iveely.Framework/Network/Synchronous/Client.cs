/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Iveely.Framework.Text;

namespace Iveely.Framework.Network.Synchronous
{
    /// <summary>
    /// 同步网络通信(客户端)
    /// </summary>
    public class Client
    {
        /// <summary>
        /// 服务端端口
        /// </summary>
        private readonly int _port;

        /// <summary>
        /// 服务端IP或者主机名
        /// </summary>
        private readonly string _server;

        /// <summary>
        /// 网络传输最大容量
        /// </summary>
        private int _maxTransferSize = 1024 * 1024 * 10 * 10;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="server">服务器IP或者主机名</param>
        /// <param name="port">访问服务器端口</param>
        public Client(string server, int port)
        {
            _port = port;
            _server = server;
        }

        /// <summary>
        /// 发送消息到
        /// </summary>
        /// <param name="packet">发送给服务器的信息</param>
        /// <returns>返回发送状态</returns>
        public T Send<T>(Packet packet)
        {
            if (packet.Data == null)
            {
                throw new NotSupportedException("信息包中的内容不支持为NULL！");
            }
            T result = default(T);
            TcpClient client = null;
            try
            {
                //连接到终端
                client = new TcpClient();
                client.Connect(_server, _port);

                //将即将发送的消息转换为字节数组
                byte[] sendDataBytes = Serializer.SerializeToBytes(packet);
                byte[] sendBytesLength = BitConverter.GetBytes(sendDataBytes.Length);
                List<byte> sendList = new List<byte>();
                sendList.AddRange(sendBytesLength);
                sendList.AddRange(sendDataBytes);



                // 获取和服务端会话的流
                using (NetworkStream netStream = client.GetStream())
                {
                    //发送消息
                    netStream.Write(sendList.ToArray(), 0, sendList.Count);
                    //netStream.Flush();

                    //确认是否响应消息
                    if (packet.WaiteCallBack)
                    {
                        byte[] reciveDataLength = new byte[4];
                        netStream.Read(reciveDataLength, 0, 4);
                        int reciveLength = BitConverter.ToInt32(reciveDataLength, 0);
                        if (reciveLength != 0)
                        {
                            byte[] reciveBytes = new byte[reciveLength];
                            netStream.Read(reciveBytes, 0, reciveLength);
                            result = Serializer.DeserializeFromBytes<T>(reciveBytes);
                        }
                    }
                    netStream.Flush();
                }
            }
            finally
            {
                //如果客户端还在
                if (client != null)
                    //关闭
                    client.Close();
            }
            return result;
        }

        /// <summary>
        /// 设定网络传输最大容量
        /// </summary>
        /// <param name="size">容量大小(单位：b，默认10M)</param>
        public void SetMaxTransferSize(int size)
        {
            if (size > 0)
            {
                _maxTransferSize = size;
            }
        }
    }
}
