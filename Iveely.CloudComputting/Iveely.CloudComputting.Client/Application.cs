using System;
using System.Collections.Generic;
using System.IO;
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

        /// <summary>
        /// 写单行数据
        /// （如果文件存在，则追加）
        /// </summary>
        /// <param name="line">数据</param>
        /// <param name="fileName">文件名</param>
        /// <param name="parameters">其它参数</param>
        public void WriteText(string line, string fileName, object[] parameters)
        {
            //1. 检查根目录是否存在
            string rootFolder = GetRootFolder(parameters);
            if (!Directory.Exists(rootFolder))
            {
                Directory.CreateDirectory(rootFolder);
            }

            //2. 写文件数据
            string filePath = rootFolder + "/" + fileName;
            File.AppendAllText(filePath, line);
        }

        public string ReadText(string fileName, object[] parameters)
        {
            string filePath = GetRootFolder(parameters) + "/" + fileName;
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
        public void WriteAllText(string[] lines, string fileName, object[] parameters)
        {
            //1. 构建contents
            StringBuilder builder = new StringBuilder();
            foreach (var line in lines)
            {
                builder.AppendLine(line);
            }

            //2. 写入文件
            WriteText(builder.ToString(), fileName, parameters);
        }

        public string[] ReadAllText(string fileName, object[] parameters)
        {
            string filePath = GetRootFolder(parameters) + "/" + fileName;
            if (File.Exists(filePath))
            {
                return File.ReadAllLines(filePath);
            }
            throw new FileNotFoundException(fileName + " not found!");
        }

        private string GetRootFolder(object[] parameters)
        {
            string rootFolder = parameters[3].ToString();
            return rootFolder;
        }
    }
}
