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
using Iveely.Framework.Log;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Iveely.Dependency.Polenter.Serialization;

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

        private static readonly object LockDeserObject = -1;

        private static readonly object LockSerObject = -1;

        /// <summary>
        /// 将数据以二进制方式序列化到byte数组
        /// </summary>
        /// <param name="obj">被序列化的对象</param>
        /// <returns>序列化后的byte数组</returns>
        public static byte[] SerializeToBytes(Object obj)
        {
            if (obj == null)
            {
                return null;
            }
            lock (LockSerObject)
            {
                var stream = new MemoryStream();
                var settings = new SharpSerializerBinarySettings(BinarySerializationMode.SizeOptimized);
                var serializer = new SharpSerializer(settings);
                serializer.Serialize(obj, stream);
                return stream.GetBuffer();
            }
        }

        ///// <summary>
        ///// 将数据以XML序列化方式到字符串
        ///// </summary>
        ///// <param name="t">被序列化的对象</param>
        ///// <returns>序列化后的字符串</returns>
        //public static string SerializeToString<T>(T t)
        //{
        //    lock (lockSerObject)
        //    {
        //        using (StringWriter stringWriter = new StringWriter())
        //        {
        //            XmlSerializer xmlSerializer = new XmlSerializer(t.GetType());
        //            xmlSerializer.Serialize(stringWriter, t);
        //            return stringWriter.ToString();
        //        }
        //    }
        //}

        /// <summary>
        /// 序列化对象到文件
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="t">对象</param>
        /// <param name="fileName">文件（路径）名</param>
        public static void SerializeToFile<T>(T t, string fileName)
        {
            try
            {
                if (t == null)
                {
                    return;
                }
                lock (LockSerObject)
                {
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                    //文件流创建
                    FileStream fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                    //二进制对象
                    var binaryFormatter = new BinaryFormatter();
                    //执行序列化
                    binaryFormatter.Serialize(fileStream, t);
                    fileStream.Close();
                }
            }
            catch (Exception exception)
            {
                Logger.Warn(exception);
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
            if (bytes == null)
            {
                return default(T);
            }
            lock (LockDeserObject)
            {
                var stream = new MemoryStream(bytes);
                var settings = new SharpSerializerBinarySettings(BinarySerializationMode.SizeOptimized);
                var serializer = new SharpSerializer(settings);
                return (T)serializer.Deserialize(stream);
            }

        }

        public static object[] DeserializeFromBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }
            lock (LockDeserObject)
            {
                var stream = new MemoryStream(bytes);
                var settings = new SharpSerializerBinarySettings(BinarySerializationMode.SizeOptimized);
                var serializer = new SharpSerializer(settings);
                object obj = serializer.Deserialize(stream);
                List<object> list = null;
                if (obj is Array)
                {
                    list = new List<object>(((Array)obj).Cast<object>());
                }
                return list.ToArray();
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
            try
            {

                lock (LockDeserObject)
                {
                    if (!File.Exists(fileName))
                    {
                        throw new FileNotFoundException(fileName);
                    }
                    //文件流
                    var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    //fs.Seek(0, SeekOrigin.Begin);
                    //二进制对象
                    var binaryFormatter = new BinaryFormatter();
                    //执行序列化
                    Object obj = binaryFormatter.Deserialize(fileStream);
                    //关闭流，这个很重要
                    fileStream.Close();
                    return (T)obj;
                }

            }
            catch (Exception exception)
            {
                Logger.Warn(exception);
            }
            return default(T);
        }



#if DEBUG
        [TestMethod]
        public void Test_Serializer()
        {
            const string info = "hello world!";
            byte[] infoBytes = SerializeToBytes(info);
            Assert.AreEqual(info, DeserializeFromBytes<string>(infoBytes));

            int[] array = { 1, 2, 3 };
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
