using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Iveely.CloudComputing.CacheAPI;
using Iveely.CloudComputing.StateAPI;
using Iveely.Framework.Network;
using Iveely.Framework.Text;

namespace Iveely.CloudComputing.Client
{
    /// <summary>
    /// 客户应用程序
    /// </summary>
    public abstract class Application
    {
        protected Framework.Network.Synchronous.Client Sender;

        public static object[] Parameters;

        public abstract void Run(object[] args);

        public void Init(object[] args)
        {
            Parameters = args;
        }

        #region 交互操作

        public void WriteToConsole(string information)
        {
            if (Sender == null)
            {
                //BUG:传递parameter这个参数，非常不友好
                string fromIp = Parameters[0].ToString();
                string port = Parameters[1].ToString();
                Sender = new Framework.Network.Synchronous.Client(fromIp, int.Parse(port));
            }
            Packet packet = new Packet(Serializer.SerializeToBytes("[result from:" + Parameters[2] + ",+" + Parameters[3] + "] " + information));
            //无需等待反馈
            packet.WaiteCallBack = false;
            Sender.Send<Packet>(packet);
        }

        public void GetHtml(string url, ref string title, ref string content, ref List<string> childrenLink)
        {
            if (childrenLink == null) throw new ArgumentNullException("childrenLink");
            Html html = Html.CreatHtml(new Uri(url));
            if (html != null)
            {
                title = html.Title;
                content = html.Content;
                childrenLink = html.ChildrenLink.Select(o => o.ToString()).ToList();
            }
            else
            {
                childrenLink = new List<string>();
            }
        }

        #endregion

        #region 文本操作

        /// <summary>
        /// 写单行数据
        /// （如果文件存在，则追加）
        /// </summary>
        /// <param name="line">数据</param>
        /// <param name="fileName">文件名</param>
        /// <param name="globalFile">是否是全局文件</param>
        public void WriteText(string line, string fileName, bool globalFile)
        {
            //1. 检查根目录是否存在
            string rootFolder = GetRootFolder();
            if (!Directory.Exists(rootFolder))
            {
                Directory.CreateDirectory(rootFolder);
            }

            //2. 写文件数据
            string filePath = GetFilePath(fileName, globalFile);
            File.AppendAllText(filePath, line, Encoding.UTF8);
            FileInfo fileInfo = new FileInfo(filePath);

            //3. 写到状态树
            if (globalFile)
            {
                StateAPI.StateHelper.Put("ISE://File/" + fileName + ".global", fileInfo.Length / 1024);
            }
            else
            {
                List<string> workers = new List<string>(StateHelper.GetChildren("ISE://system/state/worker"));
                int count = 1;
                if (workers.Count > 0)
                {
                    count = workers.Count;
                }
                StateAPI.StateHelper.Put("ISE://File/" + fileName + ".part", fileInfo.Length / 1024 * count);
            }
        }

        public void WriteList<T>(List<T> data, string fileName, bool global)
        {
            StringBuilder builder = new StringBuilder();
            foreach (T d in data)
            {
                builder.AppendLine(d.ToString());
            }
            WriteText(builder.ToString(), fileName, global);
        }

        /// <summary>
        /// 写多行数据
        /// （如果文件存在，则追加）
        /// </summary>
        /// <param name="line">数据</param>
        /// <param name="fileName">文件名</param>
        /// <param name="parameters">其它参数</param>
        public void WriteAllText(string[] lines, string fileName, bool globalFile)
        {
            //1. 构建contents
            StringBuilder builder = new StringBuilder();
            foreach (var line in lines)
            {
                builder.AppendLine(line);
            }

            //2. 写入文件
            WriteText(builder.ToString(), fileName, globalFile);
        }


        public string ReadText(string fileName, bool globalFile)
        {
            string filePath = GetFilePath(fileName, globalFile);
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            throw new FileNotFoundException(fileName + " not found!");
        }


        public string[] ReadAllText(string fileName, bool globalFile)
        {
            string filePath = GetFilePath(fileName, globalFile);
            if (File.Exists(filePath))
            {
                return File.ReadAllLines(filePath);
            }
            throw new FileNotFoundException(fileName + " not found!");
        }

        public bool IsExist(string fileName, bool globalFile)
        {
            string filePath = GetFilePath(fileName, globalFile);
            return File.Exists(filePath);
        }

        public void Delete(string fileName, bool globalFile)
        {
            string filePath = GetFilePath(fileName, globalFile);
            File.Delete(filePath);
        }

        #endregion

        #region 缓存操作

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">缓存的key</param>
        /// <param name="value">缓存的value</param>
        public void SetCache(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new NullReferenceException("Key can not be null.");
            }
            key = Parameters[2].ToString() + Parameters[3] + Parameters[4] + Parameters[5] + ":" + key;
            Memory.Set(key, value);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T">缓存返回值类型</typeparam>
        /// <param name="key">缓存的key</param>
        /// <returns>返回值</returns>
        public T GetCache<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new NullReferenceException("Key can not be null.");
            }
            key = Parameters[2].ToString() + Parameters[3] + Parameters[4] + Parameters[5] + ":" + key;
            return GetGlobalCache<T>(key);
        }

        /// <summary>
        /// 设定全局缓存
        /// </summary>
        /// <param name="key">缓存的key</param>
        /// <param name="value">缓存的value</param>
        public void SetGlobalCache(string key, object value)
        {
            if (!string.IsNullOrEmpty(key))
            {
                Memory.Set(key, value);
            }
        }

        /// <summary>
        /// 获取全局缓存
        /// </summary>
        /// <typeparam name="T">value的类型</typeparam>
        /// <param name="key">缓存的key</param>
        /// <returns>返回缓存value</returns>
        public T GetGlobalCache<T>(string key)
        {
            object obj = Memory.Get(key);
            if (obj != null)
            {
                return (T)Convert.ChangeType(obj, typeof(T));
            }
            return default(T);
        }

        /// <summary>
        /// 通过value获取keys
        /// （将相同value的keys获取出）
        /// </summary>
        /// <param name="expression">value的值</param>
        /// <param name="keysCount">期望获取key的数量</param>
        /// <param name="changedValue">获取后改变key的值</param>
        /// <returns>返回keys</returns>
        public string[] GetKeysByValueFromCache(object expression, int keysCount, object changedValue)
        {
            object[] objects = Memory.GetKeysByValue(expression, keysCount, changedValue);
            List<string> keys = new List<string>();
            foreach (object obj in objects)
            {
                string key = obj.ToString();
                keys.Add(key.Substring(key.IndexOf(':') + 1, key.Length - key.IndexOf(':') - 1));
            }
            return keys.ToArray();
        }

        /// <summary>
        /// 设置缓存集
        /// （拥有相同value的keys）
        /// </summary>
        /// <param name="objects">keys</param>
        /// <param name="value">缓存值</param>
        public void SetListIntoCache(IEnumerable<object> objects, object value)
        {
            List<string> keys = new List<string>();
            foreach (object obj in objects)
            {
                string key = Parameters[2].ToString() + Parameters[3] + Parameters[4] + Parameters[5] + ":" + obj;
                keys.Add(key);
            }
            Memory.SetList(keys, value, false);
        }

        #endregion

        #region 其它私有方法

        private string GetRootFolder()
        {
            string rootFolder = Parameters[3].ToString();
            return rootFolder;
        }

        private string GetFilePath(string fileName, bool globalFile)
        {
            string filePath = GetRootFolder() + "/" + fileName;
            if (globalFile)
            {
                filePath += ".global";
            }
            else
            {
                filePath += ".part";
            }
            return filePath;
        }

        #endregion
    }
}
