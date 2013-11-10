using Iveely.Framework.DataStructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Iveely.Framework.Log;
using Iveely.Framework.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iveely.CloudComputing.StateCenter
{
    /// <summary>
    /// 状态存储集
    /// </summary>
#if DEBUG
    [TestClass]
#endif
    [Serializable]
    public class State
    {
        /// <summary>
        /// 状态树
        /// </summary>
        private static MultiTree _tree = new MultiTree();

        /// <summary>
        /// Max appending for store
        /// </summary>
        private static readonly List<string> Appends = new List<string>();


        /// <summary>
        /// 存储状态信息
        /// </summary>
        /// <param name="path">状态路径</param>
        /// <param name="data">状态数据值</param>
        public static void Put(string path, object data)
        {
            path = FilterIse(path);
            string[] catogories = path.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < catogories.Length; i++)
            {
                string parentName = string.Empty;
                if (i > 0)
                {
                    for (int j = 0; j < i; j++)
                    {
                        parentName += catogories[j] + "/";
                    }
                }
                if (i == catogories.Length - 1)
                {
                    object sendData = data;
                    _tree.AddNode(catogories[i], sendData, parentName, true);
                }
                else
                {
                    _tree.AddNode(catogories[i], null, parentName);
                }
            }
        }

        public static void Rename(string path, string newName)
        {
            _tree.Rename(FilterIse(path), newName);
        }

        public static IEnumerable<string> SortByValue(string path)
        {
            IEnumerable<string> children = GetChildren(path);
            //BUG:not sort yet
            return children;
        }

        /// <summary>
        /// 判断状态路径是否存在
        /// </summary>
        /// <param name="path">状态路径</param>
        /// <returns>是否存在</returns>
        public static bool IsExist(string path)
        {
            path = FilterIse(path);
            MultiTree.Node node = FindNode(path);
            return node != null;
        }

        /// <summary>
        /// 删除某状态
        /// </summary>
        /// <param name="path">被删除的状态路径</param>
        public static void Delete(string path)
        {
            path = FilterIse(path);
            MultiTree.Node node = FindNode(path);
            if (node != null && node.Data != null)
            {
                Appends.Remove(path);
                node.Parent.Children.Remove(node);
            }
        }

        /// <summary>
        /// 获取状态路径的子路径
        /// </summary>
        /// <returns>返回状态子路径集合</returns>
        public static IEnumerable<string> GetChildren(string path)
        {
            Logger.Info("Get children from path:" + path);
            path = FilterIse(path);
            MultiTree.Node node = FindNode(path);
            if (node != null)
            {
                return node.Children.Select(o => "ISE://" + o.Path).ToList();
            }
            return null;
        }


        /// <summary>
        /// 根据路径获取状态信息
        /// </summary>
        /// <param name="path">状态路径</param>
        /// <returns>返回状态值</returns>
        public static T Get<T>(string path)
        {
            Logger.Info("Get data from path:" + path);
            path = FilterIse(path);
            MultiTree.Node node = FindNode(path);
            if (node != null)
            {
                return (T)Convert.ChangeType(node.Data, typeof(T));
            }
            return default(T);
        }

        /// <summary>
        /// 获取所有状态信息
        /// </summary>
        /// <returns></returns>
        public static string GetAllState()
        {
            StringBuilder builder = new StringBuilder();
            TravelSelf(ref builder, _tree.Root);
            return builder.ToString();
        }

        /// <summary>
        /// 清除所有状态
        /// </summary>
        public static void CleanAll()
        {
            _tree = new MultiTree();
        }

        /// <summary>
        /// 备份所有状态
        /// </summary>
        public static void Backup()
        {
            Serializer.SerializeToFile(_tree, new IniHelper().ReadValue("StateCenter", "State.Serialize.File.Name", "Iveely.State.Center.ser"));
        }

        /// <summary>
        /// 还原状态信息
        /// </summary>
        public static void Restore()
        {
            string bakupFile = new IniHelper().ReadValue("StateCenter", "State.Serialize.File.Name", "Iveely.State.Center.ser");
            if (File.Exists(bakupFile))
            {
                _tree = Serializer.DeserializeFromFile<MultiTree>(bakupFile);
            }
        }

        /// <summary>
        /// 遍历整个状态中心
        /// </summary>
        /// <param name="builder">信息存储</param>
        /// <param name="node">访问结点</param>
        private static void TravelSelf(ref StringBuilder builder, MultiTree.Node node)
        {
            if (node.Data != null)
            {
                builder.AppendLine("ISE://" + node.Path + "=>" + node.Data);
            }
            foreach (MultiTree.Node childNode in node.Children)
            {
                TravelSelf(ref builder, childNode);
            }
        }


        /// <summary>
        /// 查找状态结点
        /// </summary>
        /// <param name="path">结点的路径</param>
        /// <returns></returns>
        private static MultiTree.Node FindNode(string path)
        {
            path = FilterIse(path);
            string[] categories = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            MultiTree.Node node = _tree.Root;

            int flag = categories.Length;
            for (int i = 0; i < categories.Length && node != null; i++)
            {
                flag--;
                node = node.FindChild(categories[i]);
            }
            if (flag == 0)
            {
                return node;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string FilterIse(string path)
        {
            if (path.ToLower().StartsWith("ise://"))
            {
                return path.Substring(6, path.Length - 6);
            }
            return path;
        }

#if DEBUG

        [TestMethod]
        public void TestPutGet()
        {
            CleanAll();
            string path = "ISE://mypath/app1";
            string data = "this is my application named 'app1'.";
            Put(path, data);
            Assert.AreEqual(data, Get<string>(path));

            string repeatePathData = "this is repeate data";
            Put(path, repeatePathData);
            Assert.AreEqual(repeatePathData, Get<string>(path));

            CleanAll();
            string bytePath = "ISE://mypath/app1";
            byte[] bytes = { 1, 2 };
            Put(path, bytes);
            Assert.AreEqual(bytes, Get<byte[]>(bytePath));
        }

        [TestMethod]
        public void TestIsExist()
        {
            CleanAll();
            string pathA = "ISE://mypath/pathA";
            string pathB = "ISE://mypath/pathB";
            Put(pathA, "A");
            Put(pathB, "B");
            Assert.IsTrue(IsExist(pathA));
            Assert.IsTrue(IsExist(pathB));
            Delete(pathA);
            Assert.IsFalse(IsExist(pathA));
            Assert.IsTrue(IsExist(pathB));
        }

        [TestMethod]
        public void TestGetChildren()
        {
            CleanAll();
            string pathA = "ISE://mypath/pathA";
            string pathB = "ISE://mypath/pathB";
            string pathC = "ISE://yourpath/pathc";
            Put(pathA, "A");
            Put(pathB, "B");
            Put(pathC, "C");
            List<string> children = new List<string>(GetChildren("ISE://mypath"));
            Assert.IsTrue(children.Count() == 2);
            Assert.IsTrue(children.Contains(pathA));
            Assert.IsTrue(children.Contains(pathB));
        }

        [TestMethod]
        public void TestCleanRestoreBackup()
        {
            string bakupFile = new IniHelper().ReadValue("StateCenter", "State.Serialize.File.Name", "Iveely.State.Center.ser");
            if (File.Exists(bakupFile))
            {
                File.Delete(bakupFile);
            }
            CleanAll();
            const string pathA = "ISE://mypath/pathA";
            Put(pathA, "A");
            Assert.IsTrue(IsExist(pathA));
            Backup();
            CleanAll();
            Assert.IsFalse(IsExist(pathA));
            Restore();
            Assert.IsTrue(IsExist(pathA));
        }

        [TestMethod]
        public void Test_State_Rename()
        {
            CleanAll();
            string path = "ISE://mypath/pathA";
            string newPathName = "PA";
            Put(path, "A");
            Rename(path, newPathName);
            Assert.IsTrue(IsExist("ISE://mypath/PA"));
            Assert.IsFalse(IsExist("ISE://mypath/pathA"));
            string val = Get<string>("ISE://mypath/PA");
            Assert.AreEqual(val, "A");
        }

#endif

    }
}
