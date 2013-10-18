using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Iveely.CloudComputting.CacheAPI;
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

        public static object[] parameters;

        public abstract void Run(object[] args);

        public void Init(object[] args)
        {
            parameters = args;
        }

        #region 交互操作

        public void WriteToConsole(string information)
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

        #endregion

        #region 文本操作

        /// <summary>
        /// 写单行数据
        /// （如果文件存在，则追加）
        /// </summary>
        /// <param name="line">数据</param>
        /// <param name="fileName">文件名</param>
        /// <param name="parameters">其它参数</param>
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
            File.AppendAllText(filePath, line);
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

        public void SetCache(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new NullReferenceException("Key can not be null.");
            }
            key += parameters[2].ToString() + parameters[3] + parameters[4] + parameters[5];
            Memory.Set(key, value);
        }

        public T GetCache<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new NullReferenceException("Key can not be null.");
            }
            key += parameters[2].ToString() + parameters[3] + parameters[4] + parameters[5];
            object obj = Memory.Get(key);
            return (T)Convert.ChangeType(obj, typeof(T));
        }

        #endregion

        #region 其它私有方法

        private string GetRootFolder()
        {
            string rootFolder = parameters[3].ToString();
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
