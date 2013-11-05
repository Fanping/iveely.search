/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：13896622743
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections.Generic;
using System.Net;
using Iveely.CloudComputting.CacheCommon;
using Iveely.CloudComputting.Configuration;
using Iveely.Framework.Log;
using Iveely.Framework.Network;
using Iveely.Framework.Network.Synchronous;
using Iveely.Framework.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iveely.CloudComputting.Cache
{
    /// <summary>
    /// 缓存执行者
    /// </summary>
#if DEBUG
    [TestClass]
#endif
    public class Executor
    {
        /// <summary>
        /// 连接主机
        /// </summary>
        private readonly string _host = Dns.GetHostName();

        /// <summary>
        /// 服务端口号
        /// </summary>
        private readonly int _listenPort = SettingItem.GetInstance().CacheNodePort;

        /// <summary>
        /// 
        /// </summary>
        private readonly Server _server;

        /// <summary>
        /// 环形哈希存储器
        /// </summary>
        private readonly CyclingHash _table;

        /// <summary>
        /// 构造方法
        /// </summary>
        public Executor()
        {
            _table = new CyclingHash();
            _server = new Server(_host, _listenPort, ProcessRequest);
        }

        /// <summary>
        /// 启动缓存服务
        /// </summary>
        public void Start()
        {
            Logger.Info(string.Format("Start memory cache {0}:{1}", _host, _listenPort));
            _server.Listen();
        }

        /// <summary>
        /// 终止缓存服务
        /// </summary>
        public void Stop()
        {
            Logger.Info(string.Format("Stop memory cache {0}:{1}", _host, _listenPort));
            _server.StopListening();
        }

        /// <summary>
        /// 处理操纵请求
        /// </summary>
        /// <param name="packet"></param>
        public byte[] ProcessRequest(byte[] bytes)
        {
            try
            {
                
                CachePacket packet = Serializer.DeserializeFromBytes<CachePacket>(bytes);
                Message message = Serializer.DeserializeFromBytes<Message>(packet.Data);
                if (Message.CommandType.Set == message.Command)
                {
                    SetItem(message.Key, message.Value, message.Overrrides);
                }
                else if (Message.CommandType.Get == message.Command)
                {
                    object value = GetItem(message.Key);
                    message.Value = value;
                }
                else if (Message.CommandType.SetList == message.Command)
                {
                    List<object> keys = message.Values;
                    foreach (var key in keys)
                    {
                        SetItem(key, message.Value, message.Overrrides);
                    }
                }
                else
                {
                    message.Values = new List<object>(GetKeyByValue(message.Value, message.Key, message.TopN));
                }
                return Serializer.SerializeToBytes(message);

            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
            return null;
        }

        /// <summary>
        /// 添加新缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="value">关键字对应的值</param>
        /// <param name="overrides">若已存在该关键词，是否覆盖</param>
        private void SetItem(object key, object value, bool overrides)
        {
            if (_table.ContainsKey(key) && !overrides)
            {
                return;
            }
            _table.Add(key, value);
        }

        /// <summary>
        /// 获取当前关键词对应的值
        /// </summary>
        private object GetItem(object key)
        {
            return _table.GetValue(key);
        }

        /// <summary>
        /// 通过值获取关键词集合
        /// </summary>
        /// <param name="value"></param>
        /// <param name="changeValue"></param>
        /// <returns></returns>
        private object[] GetKeyByValue(object value, object changeValue, int topN)
        {
            return _table.ReadByValue(value, changeValue, topN);
        }

#if DEBUG
        [TestMethod]
        public void TestSetAndGetItem()
        {
            for (int i = 0; i < 10; i++)
            {
                SetItem(i, i, false);
            }

            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(GetItem(i), i);
            }
        }

        [TestMethod]
        public void TestGetKeyByValue()
        {
            for (int i = 0; i < 10; i++)
            {
                SetItem(i + "Key", 1, false);
            }

            //读取出来之后，会按照插入的逆序进行输出
            object[] objects = GetKeyByValue(1, -1, 10);
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(objects[i], (9 - i) + "Key");
            }

            Assert.AreEqual(GetItem(6 + "Key"), -1);
        }
#endif

    }
}
