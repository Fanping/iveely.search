/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System.Collections.Generic;
using System.Net;

namespace Iveely.CloudComputing.Configuration
{
    public class SettingItem
    {
        /// <summary>
        /// The ip of the state center
        /// </summary>
        public List<string> StateCenterHosts { get; set; }

        /// <summary>
        /// The port of the state center
        /// </summary>
        public int StateCenterPort { get; set; }

        /// <summary>
        /// When create file, we should wait the data size reach the tholdhold and send to writer
        /// </summary>
        public long SendDataSizeTholdhold { get; set; }

        /// <summary>
        /// The path of read task
        /// </summary>
        public string ReadTaskBasePathonCenter { get; set; }

        /// <summary>
        /// The path of write task
        /// </summary>
        public string WriteTaskBasePathonCenter { get; set; }

        /// <summary>
        /// The virtualfile system base path on state center
        /// </summary>
        public string VirtualFileSystemBasePath { get; set; }

        /// <summary>
        /// The max run time for user's application
        /// </summary>
        public int UserAppMaxRunningTime { get; set; }

        /// <summary>
        /// The max run time for system's application
        /// </summary>
        public int SysAppMaxRunningTime { get; set; }

        /// <summary>
        /// The port for memery cacher
        /// </summary>
        public int CacheNodePort { get; set; }

        /// <summary>
        /// The collection of the cachers
        /// </summary>
        public List<string> CacherCollections { get; set; }

        /// <summary>
        /// The collection of the mergers
        /// </summary>
        public List<string> MergerCollections { get; set; }

        /// <summary>
        /// The max app in running
        /// </summary>
        public int MaxAppToRun { get; set; }

        /// <summary>
        /// The worker port
        /// </summary>
        public int WorkerStartPort { get; set; }

        /// <summary>
        /// THe number of worker in each machine
        /// </summary>
        public int WorkerNumber { get; set; }

        /// <summary>
        /// IP of the merger server
        /// </summary>
        public string MergeServerIP { get; set; }

        /// <summary>
        /// The colloection of workers
        /// </summary>
        public List<string> WorkerCollections { get; set; }

        private static SettingItem _configration;

        private SettingItem()
        {
            MergerCollections = new List<string>();
            CacherCollections = new List<string>();
            WorkerCollections = new List<string>();
            StateCenterHosts = new List<string>();
        }

        public void Save()
        {
            ConfigManager.SaveConfigiration(this);
        }

        public static SettingItem GetInstance()
        {
            if (_configration == null)
            {
                _configration = ConfigManager.GetConfigration();
                if (_configration == null)
                {
                    _configration = GetDefaultConfigration();
                }
            }
            return _configration;
        }

        private static SettingItem GetDefaultConfigration()
        {
            SettingItem configration = new SettingItem();
            configration.SendDataSizeTholdhold = 10000;
            configration.ReadTaskBasePathonCenter = "/root/task/read/";
            configration.WriteTaskBasePathonCenter = "/root/task/write/";
            configration.VirtualFileSystemBasePath = "/root/filesystem/ise/";
            configration.StateCenterHosts.Add(Dns.GetHostName());
            configration.StateCenterPort = 3026;
            configration.UserAppMaxRunningTime = 5;
            configration.SysAppMaxRunningTime = 27 * 7;
            configration.CacheNodePort = 8081;
            configration.CacherCollections.Add(Dns.GetHostName());
            configration.MergerCollections.Add(Dns.GetHostName());
            configration.WorkerCollections.Add(Dns.GetHostName());
            configration.MergeServerIP = "127.0.0.1";
            configration.MaxAppToRun = 10;
            configration.WorkerStartPort = 2000;
            configration.WorkerNumber = 3;
            return configration;
        }
    }
}