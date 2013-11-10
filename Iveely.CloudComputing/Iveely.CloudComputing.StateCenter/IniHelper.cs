using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Iveely.CloudComputing.StateCenter
{
    /// <summary>
    /// the operation on ini files
    /// </summary>
    public class IniHelper
    {

        protected string IniFileName = "Iveely.Dream.ini";

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        /*
        section: 要写入的段落名
        key: 要写入的键，如果该key存在则覆盖写入
        val: key所对应的值
        filePath: INI文件的完整路径和文件名
        */

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defVal, StringBuilder retVal, int size, string filePath);
        /*
        section：要读取的段落名
        key: 要读取的键
        defVal: 读取异常的情况下的缺省值
        retVal: key所对应的值，如果该key不存在则返回空值
        size: 值允许的大小
        filePath: INI文件的完整路径和文件名

        */


        /// <summary>
        /// 写入INI文件
        /// </summary>
        /// <param name="section">项目名称(如 [TypeName] )</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void WriteValue(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, IniFileName);
        }

        /// <summary>
        /// 读出INI文件
        /// </summary>
        /// <param name="section">项目名称(如 [TypeName] )</param>
        /// <param name="key">键</param>
        /// <param name="Default"></param>
        public string ReadValue(string section, string key, string Default)
        {
            StringBuilder temp = new StringBuilder(500);
            GetPrivateProfileString(section, key, Default, temp, 500, IniFileName);
            return temp.ToString();
        }
        /// <summary>
        /// 验证文件是否存在
        /// </summary>
        /// <returns>布尔值</returns>
        public bool ExistINIFile()
        {
            return File.Exists(IniFileName);
        }
        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="path">路径</param>
        private void NewDirectory(String path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        /// <summary>
        /// 添加一行注释
        /// </summary>
        /// <param name="notes">注释</param>
        public void AddNotes(string notes)
        {
            string filename = IniFileName;
            string path;
            path = Directory.GetParent(filename).ToString();
            NewDirectory(path);
            FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.BaseStream.Seek(0, SeekOrigin.End);
            sw.WriteLine(@";" + notes);
            sw.Flush();
            sw.Close();
            fs.Close();
            sw.Dispose();
            fs.Dispose();
        }
        /// <summary>
        /// 添加一行文本
        /// </summary>
        /// <param name="text">文本</param>
        public void AddText(string text)
        {
            string filename = IniFileName;
            string path;
            path = Directory.GetParent(filename).ToString();
            NewDirectory(path);
            FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.BaseStream.Seek(0, SeekOrigin.End);
            sw.WriteLine(text);
            sw.Flush();
            sw.Close();
            fs.Close();
            sw.Dispose();
            fs.Dispose();
        }

        #region 重载
        public void WriteValue(string section, string key, int value)
        {
            WriteValue(section, key, value.ToString(CultureInfo.InvariantCulture));
        }
        public void WriteValue(string section, string key, Boolean value)
        {
            WriteValue(section, key, value.ToString());
        }
        public void WriteValue(string section, string key, DateTime value)
        {
            WriteValue(section, key, value.ToString(CultureInfo.InvariantCulture));
        }
        public void WriteValue(string section, string key, object value)
        {
            WriteValue(section, key, value.ToString());
        }
        public int ReadValue(string section, string key, int Default)
        {
            return Convert.ToInt32(ReadValue(section, key, Default.ToString(CultureInfo.InvariantCulture)));
        }

        public bool ReadValue(string section, string key, bool Default)
        {
            return Convert.ToBoolean(ReadValue(section, key, Default.ToString()));
        }


        public DateTime ReadValue(string section, string key, DateTime Default)
        {
            return Convert.ToDateTime(ReadValue(section, key, Default.ToString(CultureInfo.InvariantCulture)));
        }

        public string ReadValue(string section, string key)
        {
            return ReadValue(section, key, "");
        }
        #endregion
    }
}
