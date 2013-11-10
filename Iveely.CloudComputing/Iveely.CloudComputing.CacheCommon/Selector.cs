using System.Collections.Generic;
using System.Net;
using Iveely.CloudComputing.Configuration;
using Iveely.Framework.Network;
using Iveely.Framework.Network.Synchronous;
using Iveely.Framework.Text;

namespace Iveely.CloudComputing.CacheCommon
{
    /// <summary>
    /// 目标缓存存储结点选择器
    /// </summary>
    public class Selector
    {
        /// <summary>
        /// Ketama node locator use to select which node to set or get
        /// </summary>
        private readonly KetamaNodeLocator _nodeLocator;

        /// <summary>
        /// The max node copy
        /// </summary>
        private const int MaxNodeCopy = 100;

        /// <summary>
        /// Single instance
        /// </summary>
        private static Selector _instance;

        /// <summary>
        /// object lock
        /// </summary>
        private static readonly object Lock = new object();

        private Client _client;

        /// <summary>
        /// Get the instance of the selector
        /// </summary>
        /// <returns></returns>
        public static Selector GetInstance()
        {
            lock (Lock)
            {
                if (_instance == null)
                {
                    _instance = new Selector();
                }
            }
            return _instance;
        }

        /// <summary>
        /// Build selector
        /// </summary>
        private Selector()
        {
            List<string> nodeList = new List<string>();
            nodeList.Add(Dns.GetHostName());
            _nodeLocator = new KetamaNodeLocator(nodeList, MaxNodeCopy);
        }

        /// <summary>
        /// Set the value of the key
        /// </summary>
        public void SetItem(object key, object value, bool overrides)
        {
            int code = key.GetHashCode();
            string host = _nodeLocator.GetNodeForKey(code);
            Message message = new Message(host, Message.CommandType.Set, key, value, 1, null, overrides);
            _client = new Client(host, SettingItem.GetInstance().CacheNodePort);

            Packet packet = new CachePacket(Serializer.SerializeToBytes(message));
            packet.WaiteCallBack = false;
            _client.Send<Message>(packet);
        }

        public void SetItems(IEnumerable<object> keys, object sameValue, bool overrides)
        {
            Dictionary<string, Message> list = new Dictionary<string, Message>();
            foreach (var key in keys)
            {
                int code = key.GetHashCode();
                string host = _nodeLocator.GetNodeForKey(code);
                if (list.ContainsKey(host))
                {
                    list[host].Values.Add(key);
                }
                else
                {
                    Message cacheMsg = new Message(host, Message.CommandType.SetList, key, sameValue, 1, new[] { key },
                                                             overrides);
                    list.Add(host, cacheMsg);
                }
            }

            foreach (var host in list.Keys)
            {
                _client = new Client(host, SettingItem.GetInstance().CacheNodePort);
                Packet packet = new CachePacket(Serializer.SerializeToBytes(list[host]));
                packet.WaiteCallBack = false;
                _client.Send<Message>(packet);
            }
        }

        /// <summary>
        /// Get the value base on the key
        /// </summary>
        public object GetItem(object key)
        {
            int code = key.GetHashCode();
            string host = _nodeLocator.GetNodeForKey(code);
            _client = new Client(host, SettingItem.GetInstance().CacheNodePort);
            Message message = new Message(host, Message.CommandType.Get, key, null);
            Packet packet = new CachePacket(Serializer.SerializeToBytes(message));
            packet.WaiteCallBack = true;
            message = _client.Send<Message>(packet);
            return message.Value;
        }

        /// <summary>
        /// Get keys by value
        /// </summary>
        public object[] GetKeyByValue(object value, int topN, object changeValue)
        {
            List<string> hosts = SettingItem.GetInstance().CacherCollections;
            List<object> keys = new List<object>();
            foreach (var host in hosts)
            {
                _client = new Client(host, SettingItem.GetInstance().CacheNodePort);
                Message message = new Message(host, Message.CommandType.GetList, changeValue, value, topN);
                Packet packet = new CachePacket(Serializer.SerializeToBytes(message));
                packet.WaiteCallBack = true;
                message = _client.Send<Message>(packet);
                keys.AddRange(message.Values);
            }
            return keys.ToArray();
        }
    }
}
