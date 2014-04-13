/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：13896622743
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Iveely.Framework.Text
{
    /// <summary>
    /// 字符串加密解密
    /// </summary>
    public class StringCrypt
    {
        private static string key = "Iveely50";  

        /// <summary>
        /// 加密字符串
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Encrypt(string text)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = Encoding.UTF8.GetBytes(text);
            des.Key = Encoding.UTF8.GetBytes(key);
            des.IV = Encoding.UTF8.GetBytes(key);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            StringBuilder ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            return ret.ToString();
        }

        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Decrypt(string text)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = new byte[text.Length / 2];
            for (int x = 0; x < text.Length / 2; x++)
            {
                int i = (Convert.ToInt32(text.Substring(x * 2, 2), 16));
                inputByteArray[x] = (byte)i;
            }
            des.Key = Encoding.UTF8.GetBytes(key);
            des.IV = Encoding.UTF8.GetBytes(key);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }
}
