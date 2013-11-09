/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：13896622743
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iveely.Framework.Network;

namespace Iveely.CloudComputting.StateCommon
{
    /// <summary>
    /// 状态包
    /// </summary>
    [Serializable]
    public class StatePacket : Packet
    {
        /// <summary>
        /// 状态包传送类型
        /// </summary>
        public enum Type
        {
            Children,
            IsExists,
            Delete,
            Add,
            Modify,
            Get,
            GetAvailavleWorker,
            StoredData,
            // Temp data when computing
            TempData,
            // Run time lib
            Runtime,
            // sum
            DataSum,
            // average
            DataAvg,
            // count
            DataCount,
            // distinct
            DataDis,
            //TopN
            DataTopN,
            //CacheMessage
            CacheMessage,
            //Run App
            RunApp,
            //Kill App
            KillApp,
            //Rename
            Rename

        }

        /// <summary>
        /// 包名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public Type PType { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
       // public new byte[] Data { get; set; }

        /// <summary>
        /// 此包是否需要回复
        /// </summary>
        public new bool WaiteCallBack { get; set; }

        public StatePacket(string path, Type type, byte[] data, string name = "No Name", bool waiteCallBack = true)
        {
            PType = type;
            Path = path;
            Data = data;
            WaiteCallBack = waiteCallBack;
            Name = name;
        }
    }
}
