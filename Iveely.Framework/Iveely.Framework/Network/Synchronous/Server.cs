/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：13896622743
 *版  本：0.1.0
 *Iveely = I void everything,except love you!
 *========================================*/

using System;
using System.Net.Sockets;
using System.Threading;
using Iveely.Framework.Log;
using Iveely.Framework.Text;

namespace Iveely.Framework.Network.Synchronous
{
    /// <summary>
    /// 同步网络通信(服务端)
    /// </summary>
    public class Server
    {
        /// <summary>
        /// (委托)服务端信息处理方式
        /// </summary>
        /// <param name="packet">客户端传递的信息包</param>
        /// <returns></returns>
        public delegate byte[] Processing(byte[] bytes);

        /// <summary>
        /// 服务端信息处理委托对象
        /// </summary>
        private readonly Processing _processing;

        /// <summary>
        /// 监听器
        /// </summary>
        private TcpListener _listener;

        /// <summary>
        /// 主机地址
        /// </summary>
        private string _address;

        /// <summary>
        /// IP端口
        /// </summary>
        private readonly int _port;

        /// <summary>
        /// 是否处理监听之中
        /// </summary>
        private bool _isListening;

        private int MaxVisitThread = 5;

        private int currentThreadCount = 0;

        /// <summary>
        /// 网络传输最大大小
        /// </summary>
        private int _maxTransferSize = 1024 * 1024 * 10;

        /// <summary>
        /// 一般构造方法
        /// </summary>
        /// <param name="host">IP地址</param>
        /// <param name="port">端口号</param>
        public Server(string host, int port, int maxVisitThread = 1)
        {
            _isListening = false;
            _address = host;
            _port = port;
            MaxVisitThread = maxVisitThread;
        }

        /// <summary>
        /// 带处理过程的构造方法
        /// </summary>
        /// <param name="host">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="processing">数据处理过程</param>
        public Server(string host, int port, Processing processing, int maxVisitThread = 1)
        {
            _isListening = false;
            _address = host;
            _port = port;
            _processing = processing;
            MaxVisitThread = maxVisitThread;
        }

        /// <summary>
        /// 监听
        /// </summary>
        public void Listen()
        {
            try
            {
                if (_isListening == false)
                {
                    //正开始监听
                    _isListening = true;

                    //初始化监听器
                    _listener = new TcpListener(_port);

                    //启动监听服务
                    _listener.Start();


                    while (true)
                    {
                        TcpClient client = _listener.AcceptTcpClient();

                        //用线程解决高并发问题，此处很重要
                        Thread thread = new Thread(ProcessClient);
                        thread.Start(client);
       
                    }
                }
                throw new Exception("服务器已经启动监听，本次启动无效！");
            }
            finally
            {
                //关闭监听
                StopListening();
            }
        }

        /// <summary>
        /// 关闭监听
        /// </summary>
        public void StopListening()
        {
            //如果在监听
            if (_isListening)
            {
                //设置为非监听状态
                _isListening = false;
                //关闭监听器
                _listener.Stop();
            }
        }

        /// <summary>
        /// 设定网络传输最大容量
        /// </summary>
        /// <param name="size">容量大小(单位：b，默认10M)</param>
        public void SetMaxTransferSize(int size)
        {
            if (!_isListening && size > 0)
            {
                _maxTransferSize = size;
            }
        }

        private void ProcessClient(object objClient)
        {

            TcpClient client = (TcpClient)objClient;
            //字节数组容器
            var reciveBytes = new byte[_maxTransferSize];
            var sendBytes = new byte[_maxTransferSize];

            //读取网络流
            using (NetworkStream netStream = client.GetStream())
            {
                //设定读超时
                netStream.ReadTimeout = 600000;
                netStream.Read(reciveBytes, 0, reciveBytes.Length);

                //转换为字节数组
                //Packet clientPacket = Serializer.DeserializeFromBytes<Packet>(bytes);
                sendBytes = _processing(reciveBytes);
                if (sendBytes != null)
                {
                    netStream.Write(sendBytes, 0, sendBytes.Length);
                    netStream.Flush();
                }
            }

            currentThreadCount--;
        }
    }
}
