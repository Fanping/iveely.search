using Iveely.CloudComputting.StateCenter.Annotations;
using Iveely.CloudComputting.StateCommon;
using Iveely.Framework.Log;
using Iveely.Framework.Text;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Iveely.CloudComputting.StateCenter
{
    public class Service
    {
        /// <summary>
        /// deal with memery data
        /// </summary>
        //public static MemoryMappedFileCommunicator MemoryDataProccess;

        private const int MaxReciveSize = 1024 * 1024 * 10;

        private static readonly object SyncRoot = new object();

        /// <summary>
        /// 监听状态
        /// </summary>
        private bool OnListening { get; set; }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port">端口号</param>
        public Service(string host, int port)
        {
            //监听状态false
            OnListening = false;
            //IP地址
            Address = host;//IPAddress.Parse(ip);
            //端口
            Port = port;
        }

        /// <summary>
        /// 监听器
        /// </summary>
        private TcpListener Listener { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        private String Address { [UsedImplicitly] get; set; }

        /// <summary>
        /// IP端口
        /// </summary>
        private int Port { get; set; }

        /// <summary>
        /// 监听
        /// </summary>
        public void Listen()
        {
            try
            {

                lock (SyncRoot)
                {
                    //初始化监听器
                    Listener = new TcpListener(Port);
                    //启动监听服务
                    Listener.Start();
                    //监听中
                    OnListening = true;
                }
                while (true)
                {
                    //等待客户端链接
                    TcpClient client = Listener.AcceptTcpClient();
                    //提示信息
                    // Common.Printf.WriteInfo("处理客户端:"+client.Client.RemoteEndPoint.ToString());
                    //处理客户端程序
                    //Log.Logger.Info(string.Format("Get client connect to server."));
                    ProcessClient(client);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
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
            if (OnListening)
            {
                lock (SyncRoot)
                {
                    //设置为非监听状态
                    OnListening = false;
                    //关闭监听器
                    Listener.Stop();
                }
            }
        }

        /// <summary>
        /// 处理客户端的连接
        /// </summary>
        /// <param name="client">客户端</param>
        private void ProcessClient(TcpClient client)
        {
            try
            {
                //字节数组容器
                var bytes = new byte[MaxReciveSize];
                //读取网络流
                using (NetworkStream netStream = client.GetStream())
                {
                    //设定读超时
                    netStream.ReadTimeout = 600000;
                    netStream.Read(bytes, 0, bytes.Length);
                    //转换为字节数组
                    bytes = OnReceive(bytes);
                    //执行回复
                    netStream.Write(bytes, 0, bytes.Length);

                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
            finally
            {
                //如果客户端不为空
                if (client != null)
                {
                    //记住关闭
                    client.Close();
                }
            }
        }

        /// <summary>
        /// 服务端接收客户端信息
        /// </summary>
        /// <param name="datas">数据信息</param>
        private static byte[] OnReceive(byte[] datas)
        {
            try
            {
                // 1.获取状态包
                StatePacket client = (Serializer.DeserializeFromBytes<StatePacket>(datas));


                // 2.Process message
                if (client.PType == StatePacket.Type.Add)
                {
                    try
                    {
                        State.Put(client.Path, client.Data);
                        Logger.Info("Added information to path-" + client.Path + "  value=" + client.Data);
                        State.Backup();
                        return Serializer.SerializeToBytes(true);
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(exception);
                        return Serializer.SerializeToBytes(false);
                    }

                }

                if (client.PType == StatePacket.Type.Children)
                {
                    try
                    {
                        //Log.Logger.Debug("Get chilren from path-" + client.Path);
                        IEnumerable<string> result = State.GetChildren(client.Path);
                        if (result == null)
                        {
                            return Serializer.SerializeToBytes(new List<string>());
                        }
                        return Serializer.SerializeToBytes(new List<string>(result));
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(exception);
                        return Serializer.SerializeToBytes(new List<string>());
                    }


                }
                if (client.PType == StatePacket.Type.Delete)
                {
                    try
                    {
                        State.Delete(client.Path);
                        Logger.Info("Delete from path-" + client.Path);
                        State.Backup();
                        return Serializer.SerializeToBytes(true);
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(exception);
                        return Serializer.SerializeToBytes(false);
                    }

                }
                if (client.PType == StatePacket.Type.Get)
                {
                    try
                    {
                        Logger.Info("Get from path-" + client.Path);
                        byte[] result = State.Get<byte[]>(client.Path);
                        if (result != null)
                        {
                            return result;
                        }
                        return Serializer.SerializeToBytes("NULL");
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(exception);
                        return Serializer.SerializeToBytes("NULL");
                    }

                }

                if (client.PType == StatePacket.Type.IsExists)
                {
                    try
                    {
                        Logger.Info("IsExists from path-" + client.Path);
                        return Serializer.SerializeToBytes(State.IsExist(client.Path));
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(exception);
                        return Serializer.SerializeToBytes(false);
                    }

                }
                if (client.PType == StatePacket.Type.Modify)
                {
                    try
                    {
                        State.Put(client.Path, client.Data);
                        Logger.Info("Modify from path-" + client.Path);
                        State.Backup();
                        return Serializer.SerializeToBytes(true);
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(exception);
                        return Serializer.SerializeToBytes(false);
                    }

                }
                if (client.PType == StatePacket.Type.GetAvailavleWorker)
                {
                    try
                    {
                        return Serializer.SerializeToBytes(State.SortByValue(client.Path));
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(exception);
                        return Serializer.SerializeToBytes(false);
                    }
                }
                if (client.PType == StatePacket.Type.StoredData)
                {
                    try
                    {
                        //MemoryDataProccess.Write(Serializer.SerializeToBytes(client.Data));
                        return Serializer.SerializeToBytes(true);
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(exception);
                        return Serializer.SerializeToBytes(false);
                    }
                }
                if (client.PType == StatePacket.Type.Rename)
                {
                    State.Rename(client.Path, System.Text.Encoding.UTF8.GetString(client.Data));
                    return Serializer.SerializeToBytes(false);
                }

            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
            return null;
        }

    }
}
