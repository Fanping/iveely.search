/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iveely.Framework.Text
{
    /// <summary>
    /// 文件数据本地存储
    /// </summary>
    [Serializable]
#if DEBUG
    [TestClass]
#endif
    public class LocalStore<T>
    {
        [Serializable]
        internal class Index
        {
            private static object mutex;

            /// <summary>
            /// 文件索引
            /// </summary>
            private Hashtable _table;

            /// <summary>
            /// 每个文件中存放的记录数
            /// </summary>
            private int _fileSize;

            /// <summary>
            /// 当前文件编号
            /// </summary>
            private long _fileId;

            /// <summary>
            /// 当前文件中的记录编号
            /// </summary>
            private int _recoredId;

            /// <summary>
            /// 数据文件存放文件夹
            /// </summary>
            private string _dataStoreFolder;

            private List<T> _currentData;

            public Index(string dataStoreFolder, int fileSize)
            {
                mutex = new object();
                _table = new Hashtable();
                _fileId = 0;
                _fileSize = 100;
                _dataStoreFolder = dataStoreFolder;
                _fileSize = fileSize;
                _currentData = new List<T>();
            }

            /// <summary>
            /// 存储数据
            /// </summary>
            /// <param name="obj">存储对象</param>
            /// <returns>true为已经将之前的所有记录存放到文件，false为还在内存中</returns>
            public bool Store(T obj)
            {
                lock (mutex)
                {
                    if (_table.ContainsKey(obj.GetHashCode()))
                    {
                        return false;
                    }
                    _currentData.Add(obj);
                    _recoredId++;
                    _table.Add(obj.GetHashCode(), _fileId + "." + _recoredId);

                    if (_currentData.Count >= _fileSize)
                    {
                        if (obj is string)
                        {
                            List<string> allLines = _currentData.ConvertAll(innerObj => string.Format("{0}", innerObj));
                            File.WriteAllLines(_dataStoreFolder + "\\" + _fileId, allLines.ToArray());
                        }
                        else
                        {
                            Serializer.SerializeToFile(_currentData, _dataStoreFolder + "\\" + _fileId);
                        }
                        _fileId++;
                        _recoredId = 0;
                        _currentData.Clear();
                        return true;
                    }
                    return false;
                }
            }

            public void ForceStore()
            {
                Serializer.SerializeToFile(_currentData, _dataStoreFolder + "\\force_" + _fileId);
            }

            public T Read(int hashCode)
            {
                //获取文件编号和记录编号
                if (_table[hashCode] != null)
                {
                    string ids = _table[hashCode].ToString();
                    if (ids == null || !ids.Contains("."))
                    {
                        return default(T);
                    }
                    string[] idStrings = ids.Split(new[] { '.' });
                    string fileId = idStrings[0];
                    int recredId = int.Parse(idStrings[1]) - 1;

                    //如果是还没存进文件的
                    if (_fileId.ToString() == fileId)
                    {
                        if (_currentData.Count > recredId)
                            return _currentData[recredId];
                        return default(T);
                    }

                    //如果已经存到文件
                    string filePath = _dataStoreFolder + "\\" + fileId;
                    if (File.Exists(filePath))
                    {

                        List<T> tempData = Serializer.DeserializeFromFile<List<T>>(filePath);
                        if (tempData != null && tempData.Count >= recredId)
                        {
                            return tempData[recredId];
                        }
                        else
                        {
                            string[] lines = File.ReadAllLines(filePath);
                            if (lines.Length > recredId)
                            {
                                return (T)Convert.ChangeType(lines[recredId], typeof(T));
                            }
                        }

                    }
                }
                return default(T);
            }
        }

        /// <summary>
        /// 索引文件
        /// </summary>
        private Index fileIndex;

        /// <summary>
        /// 索引文件存放路径
        /// </summary>
        private string _indexStorePath;

        /// <summary>
        /// 本地文件存储
        /// </summary>
        /// <param name="indexStorePath">索引存储位置</param>
        /// <param name="dataStoreFolder">数据文件存放目录</param>
        /// <param name="fileSize">每个文件允许的记录大小</param>
        public LocalStore(string indexStorePath, string dataStoreFolder, int fileSize)
        {
            if (File.Exists(indexStorePath))
            {
                fileIndex = Serializer.DeserializeFromFile<Index>(indexStorePath);
            }
            else
            {
                if (!Directory.Exists(dataStoreFolder))
                {
                    Directory.CreateDirectory(dataStoreFolder);
                }
                fileIndex = new Index(dataStoreFolder, fileSize);
            }
            _indexStorePath = indexStorePath;
        }

        /// <summary>
        /// 存储单个对象
        /// </summary>
        /// <param name="obj"></param>
        public void Write(T obj)
        {
            if (fileIndex.Store(obj))
            {
                int retryCount = 3;
                while (retryCount > 0)
                {
                    try
                    {
                        Serializer.SerializeToFile(fileIndex, _indexStorePath);
                        break;
                    }
                    catch
                    {
                        Thread.Sleep(100);
                        retryCount--;
                    }
                }
            }
        }

        /// <summary>
        /// 存储多个对象
        /// </summary>
        /// <param name="objs"></param>
        public void Write(T[] objs)
        {
            foreach (var obj in objs)
            {
                Write(obj);
            }
        }

        /// <summary>
        /// 读取对象
        /// </summary>
        /// <param name="objHashCode">对象的哈希值</param>
        /// <returns>返回该对象</returns>
        public T Read(int objHashCode)
        {
            return fileIndex.Read(objHashCode);
        }

        public void ForceStore()
        {
            fileIndex.ForceStore();
        }

#if DEBUG
        [TestMethod]
        public void TestLocalStore()
        {
            LocalStore<int> store = new LocalStore<int>("store_Index", "sotredata", 10);
            //for (int i = 0; i < 100; i++)
            //{
            //    store.Write(i);
            //}
            int val = store.Read(5);
            if (val == 5)
            {

            }
        }
#endif
    }
}
