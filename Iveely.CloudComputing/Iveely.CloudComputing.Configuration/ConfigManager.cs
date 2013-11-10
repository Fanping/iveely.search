/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System.IO;
using System.Runtime.Serialization;

namespace Iveely.CloudComputing.Configuration
{
    internal class ConfigManager
    {
        private const string ConfigFileName = "config.dream.xml";

        public static SettingItem GetConfigration()
        {
            if (File.Exists(ConfigFileName))
            {
                byte[] bytes = File.ReadAllBytes(ConfigFileName);
                MemoryStream memStream = new MemoryStream(bytes, false);
                DataContractSerializer ser =
                    new DataContractSerializer(typeof(SettingItem));
                return (SettingItem)ser.ReadObject(memStream);
            }
            return null;
        }

        public static void SaveConfigiration(SettingItem configration)
        {
            using (var fs = new FileStream(ConfigFileName, FileMode.Create))
            {
                var dcs = new DataContractSerializer(typeof(SettingItem));
                dcs.WriteObject(fs, configration);
            }
        }
    }
}
