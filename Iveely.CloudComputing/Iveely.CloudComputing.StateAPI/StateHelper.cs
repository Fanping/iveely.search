/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using Iveely.CloudComputing.Configuration;
using Iveely.CloudComputing.StateCommon;
using Iveely.Framework.Log;
using Iveely.Framework.Network.Synchronous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iveely.Framework.Text;

namespace Iveely.CloudComputing.StateAPI
{
    /// <summary>
    /// Sync information with zookeeper
    /// </summary>
    public class StateHelper
    {
        private static Client[] _clients;

        /// <summary>
        /// Put the path into the tree
        /// </summary>
        /// <param name="path">The path which want to store</param>
        /// <param name="data"></param>
        public static bool Put<T>(string path, T data)
        {
            CheckConnect();
            bool isSuccess = true;
            StatePacket packet = new StatePacket(path, StatePacket.Type.Add, Serializer.SerializeToBytes(data));
            Logger.Info("put path " + path);
            foreach (var client in _clients)
            {
                isSuccess &= client.Send<bool>(packet);
            }
            return isSuccess;
        }

        /// <summary>
        /// Get available supervisors
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="needRecord"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetAvailableSupervisors(string path, Object data, bool needRecord = true)
        {
            CheckConnect();
            StatePacket supervisorPacket = new StatePacket("/root/supervisor/score/", StatePacket.Type.GetAvailavleWorker, Serializer.SerializeToBytes("Null"));
            Object obj = _clients[0].Send<Object>(supervisorPacket);
            if (obj != null)
            {
                return (IEnumerable<string>)obj;
            }
            return null;
        }

        /// <summary>
        /// Is the path exist 
        /// </summary>
        /// <param name="path">the full path</param>
        /// <returns>true is exist, false is not</returns>
        public static bool IsExist(string path)
        {
            CheckConnect();
            StatePacket packet = new StatePacket(path, StatePacket.Type.IsExists, Serializer.SerializeToBytes("Check is exist"));
            bool isExist = _clients[0].Send<bool>(packet);
            return isExist;
        }

        public static void Rename(string path, string nodeName)
        {
            CheckConnect();
            StatePacket packet = new StatePacket(path, StatePacket.Type.Rename,
                System.Text.Encoding.UTF8.GetBytes(nodeName));
            //1207
            packet.WaiteCallBack = false;
            _clients[0].Send<bool>(packet);
        }

        /// <summary>
        /// Delete the path in the tree
        /// </summary>
        /// <param name="path">The path which want to delete</param>
        public static bool Delete(string path)
        {
            CheckConnect();
            bool isDeleted = true;
            StatePacket packet = new StatePacket(path, StatePacket.Type.Delete, Serializer.SerializeToBytes("delete data"));
            foreach (var client in _clients)
            {
                isDeleted &= client.Send<bool>(packet);
            }
            return isDeleted;
        }

        /// <summary>
        /// Get children of the <param name="path"/>
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetChildren(string path)
        {
            CheckConnect();
            StatePacket packet = new StatePacket(path, StatePacket.Type.Children, Serializer.SerializeToBytes("get children"));
            List<string> obj = _clients[0].Send<List<string>>(packet);
            if (obj == null)
            {
                return new List<string>();
            }
            obj.Sort();
            return obj;
        }


        /// <summary>
        /// Get data by path
        /// </summary>
        /// <param name="path">the path that data stored</param>
        /// <returns>return data</returns>
        public static T Get<T>(string path)
        {
            try
            {
                CheckConnect();
                StatePacket packet = new StatePacket(path, StatePacket.Type.Get, Serializer.SerializeToBytes("get data"));
                T obj = _clients[0].Send<T>(packet);
                return obj;
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                return default(T);
            }

        }

        public static void UpdateState(string appName, ExcuteType excuteType, ExcuteState excuteState, double statePercentage, Exception exception = null)
        {
            //CheckConnect();
            //var stateDesc = new StateDescription
            //    (
            //    appName,
            //    excuteType,
            //    excuteState,
            //    statePercentage,
            //    exception
            //    );
            //StatePacket packet = new StatePacket("", StatePacket.Type.AddMonitor, Serializer.SerializeToBytes(stateDesc));
            //foreach (var client in _clients)
            //{
            //    client.Send<string>(packet);
            //}
        }

        /// <summary>
        /// Check Connect to server
        /// </summary>
        private static void CheckConnect()
        {
            if (_clients == null)
            {
                string[] hosts = SettingItem.GetInstance().StateCenterHosts.ToArray();
                _clients = new Client[hosts.Count()];
                int port = SettingItem.GetInstance().StateCenterPort;
                for (int i = 0; i < hosts.Count(); i++)
                {
                    _clients[i] = new Client(hosts[i], port);
                }
            }
        }

    }
}
