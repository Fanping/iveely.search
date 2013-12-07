/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iveely.Framework.Text
{
    /// <summary>
    /// 数据序列化
    /// </summary>
#if DEBUG
    [TestClass]
#endif
    public class Serializer
    {

        private static object lockDeserObject = -1;

        private static object lockSerObject = -1;

        /// <summary>
        /// 将数据以二进制方式序列化到byte数组
        /// </summary>
        /// <param name="obj">被序列化的对象</param>
        /// <returns>序列化后的byte数组</returns>
        public static byte[] SerializeToBytes(Object obj)
        {
            lock (lockSerObject)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    binaryFormatter.TypeFormat = System.Runtime.Serialization.Formatters.FormatterTypeStyle.TypesAlways;
                    binaryFormatter.Serialize(stream, obj);
                    byte[] bytes = stream.ToArray();
                    return bytes;
                }
            }
        }

        /// <summary>
        /// 将数据以XML序列化方式到字符串
        /// </summary>
        /// <param name="t">被序列化的对象</param>
        /// <returns>序列化后的字符串</returns>
        public static string SerializeToString<T>(T t)
        {
            lock (lockSerObject)
            {
                using (StringWriter stringWriter = new StringWriter())
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(t.GetType());
                    xmlSerializer.Serialize(stringWriter, t);
                    return stringWriter.ToString();
                }
            }
        }

        /// <summary>
        /// 序列化对象到文件
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="t">对象</param>
        /// <param name="fileName">文件（路径）名</param>
        public static void SerializeToFile<T>(T t, string fileName)
        {
            lock (lockSerObject)
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                byte[] content = SerializeToBytes(t);
                File.WriteAllBytes(fileName, content);
            }
        }

        /// <summary>
        /// 根据byte数组，反序列化
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="bytes">序列化后的byte数组</param>
        /// <returns>反序列化还原后的对象</returns>
        public static T DeserializeFromBytes<T>(byte[] bytes)
        {
            lock (lockDeserObject)
            {
                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    stream.Position = 0;
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    binaryFormatter.TypeFormat = System.Runtime.Serialization.Formatters.FormatterTypeStyle.TypesAlways;
                    object obj = binaryFormatter.Deserialize(stream);
                    return (T)obj;
                }
            }

        }

        public static object[] DeserializeFromBytes(byte[] bytes)
        {
            lock (lockDeserObject)
            {
                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    binaryFormatter.TypeFormat = System.Runtime.Serialization.Formatters.FormatterTypeStyle.TypesAlways;
                    object obj = binaryFormatter.Deserialize(stream);
                    List<object> list = null;
                    if (obj is Array)
                    {
                        list = new List<object>(((Array)obj).Cast<object>());
                    }
                    return list.ToArray();
                }
            }
        }

        /// <summary>
        /// 将XML文档反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="content">XML序列化内容</param>
        /// <returns>反序列化还原后的对象</returns>
        public static T DeserializeFromString<T>(string content)
        {
            lock (lockDeserObject)
            {
                using (StringReader stringReader = new StringReader(content))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                    object obj = xmlSerializer.Deserialize(stringReader);
                    return (T)obj;
                }
            }
        }

        /// <summary>
        /// 从文件中反序列化对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static T DeserializeFromFile<T>(string fileName)
        {
            lock (lockDeserObject)
            {
                if (!File.Exists(fileName))
                {
                    throw new FileNotFoundException(fileName);
                }
                byte[] content = File.ReadAllBytes(fileName);
                return DeserializeFromBytes<T>(content);
            }
        }


#if DEBUG
        [TestMethod]
        public void Test_Serializer()
        {
            string info = "hello world!";
            byte[] infoBytes = SerializeToBytes(info);
            Assert.AreEqual(info, DeserializeFromBytes<string>(infoBytes));

            int[] array = new[] { 1, 2, 3 };
            byte[] arrayBytes = SerializeToBytes(array);
            int[] newInts = DeserializeFromBytes<int[]>(arrayBytes);
            Assert.IsTrue(array[1] == newInts[1]);

            object[] objects = DeserializeFromBytes(arrayBytes);
            Assert.IsTrue(objects.Length == 3);

            object[] myObjects = { 1, 2, 3, 4 };
            byte[] objBytes = SerializeToBytes(myObjects);
            int[] ints = DeserializeFromBytes<object[]>(objBytes).Cast<int>().ToArray();
            Assert.IsTrue(ints.Length == 4);
        }

        [TestMethod]
        public void Test_BinaryError()
        {
            string str = "CHINA";
            byte[] bytes = SerializeToBytes(str);
            List<byte> newBytes = new List<byte>(bytes);
            newBytes.RemoveAt(0);
            str = DeserializeFromBytes<string>(newBytes.ToArray());
        }

#endif
    }
}
