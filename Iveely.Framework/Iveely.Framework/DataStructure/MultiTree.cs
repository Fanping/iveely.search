using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iveely.Framework.DataStructure
{
    /// <summary>
    /// 多叉树
    /// </summary>
    [Serializable]
#if DEBUG
    [TestClass]
#endif
    public class MultiTree
    {
        /// <summary>
        /// 结点
        /// </summary>
        [Serializable]
        public class Node
        {
            /// <summary>
            /// 结点名称(是一个相对路径，不包含完整路径)
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// 结点深度
            /// </summary>
            public int Deep { get; set; }

            /// <summary>
            /// 结点全路径
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// 结点值
            /// </summary>
            public object Data { get; set; }

            /// <summary>
            /// 父节点
            /// </summary>
            public Node Parent { get; set; }

            /// <summary>
            /// 结点的孩子结点集合
            /// </summary>
            public List<Node> Children { get; set; }

            /// <summary>
            /// 创建结点(一般创建根结点)
            /// </summary>
            /// <param name="name">结点名称</param>
            public Node(string name)
            {
                Path = name;
                Name = name;
                Deep = 0;
                Parent = null;
                Children = new List<Node>();
            }

            public Node()
            {
            }

            /// <summary>
            /// 创建结点(一般创建子结点)
            /// </summary>
            /// <param name="name">结点名称</param>
            /// <param name="data">结点值</param>
            /// <param name="parent">结点的父结点</param>
            public Node(string name, object data, Node parent)
            {
                if (parent.Path.Length >= 1)
                {
                    Path = parent.Path + "/" + name;
                }
                else
                {
                    Path = name;
                }
                Name = name;
                Data = data;
                Deep = parent.Deep + 1;
                Parent = parent;
                Children = new List<Node>();
            }

            /// <summary>
            /// 添加孩子结点
            /// </summary>
            /// <param name="name">孩子结点名称</param>
            /// <param name="data">孩子结点的值</param>
            /// <param name="parent">孩子结点的父节点</param>
            public void AddChild(string name, object data, Node parent)
            {
                var child = new Node(name, data, parent);
                if (Children == null)
                {
                    Children = new List<Node>();
                }
                Children.Add(child);
            }

            /// <summary>
            /// 根据结点名称查找结点
            /// </summary>
            /// <param name="name">被查找结点的名称</param>
            /// <returns>返回被找到的结点，未找到则为null</returns>
            public Node FindChild(string name)
            {
                return Children.FirstOrDefault(node => node.Name.Equals(name));
            }

            /// <summary>
            /// 清楚所有子结点
            /// </summary>
            public void Clean()
            {
                Children = new List<Node>();
            }

            /// <summary>
            /// 获取所有孩子的全路径
            /// </summary>
            /// <returns>全路径集合</returns>
            public IEnumerable<string> GetChildrenPath()
            {
                IEnumerable<string> pathes = Children.Select(o => o.Path);
                return pathes;
            }

            /// <summary>
            /// 获取孩子结点的数量
            /// </summary>
            /// <returns>返回结点数量</returns>
            public int GetChildrenNum()
            {
                IEnumerable<string> pathes = GetChildrenPath();
                return pathes != null ? pathes.Count() : 0;
            }
        }

        /// <summary>
        /// 多叉树的根结点
        /// </summary>
        public Node Root { get; set; }

        public MultiTree()
        {
            Root = new Node(string.Empty);
        }

        /// <summary>
        /// 根据全路径查找结点
        /// </summary>
        /// <param name="path">被查找结点的全路径</param>
        /// <returns>返回被查找到的结点</returns>
        public Node GetNodeByPath(string path)
        {
            string[] catelogs = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            return catelogs.Aggregate(Root, (current, relativePath) => FindNode(current, relativePath));
        }

        /// <summary>
        /// 重命名结点名称
        /// </summary>
        /// <param name="path">被重命名结点的全路径</param>
        /// <param name="newName">被重命名结点的新名称(非全路径)</param>
        public void Rename(string path, string newName)
        {
            Node node = GetNodeByPath(path);
            if (node != null)
            {
                node.Name = newName;
                node.Path =
                    node.Path.Substring(0, node.Path.LastIndexOf('/') + 1) +
                    newName;
            }

        }

        /// <summary>
        /// 添加结点
        /// </summary>
        /// <param name="name">被添加结点的名称</param>
        /// <param name="data">被添加结点的值</param>
        /// <param name="parentName">被添加结点的父节点名称</param>
        /// <param name="lastNode">是否是最后一个结点</param>
        public void AddNode(string name, object data, string parentName, bool lastNode = false)
        {
            Node parent;
            if (parentName == string.Empty || parentName.ToLower() == "ise:/")
            {
                parent = Root;
            }
            else
            {
                parent = GetNodeByPath(parentName);
            }

            if (parent != null)
            {
                // If not exist
                if (!IsExist(name, parent))
                {
                    parent.AddChild(name, data, parent);
                    // parent.Children.Remove(this.FindNode(parent, name));
                }
                //update
                else
                {
                    if (lastNode)
                    {
                        parent.FindChild(name).Data = data;
                    }
                }

            }
            else
            {
                throw new NullReferenceException();
            }
        }

        /// <summary>
        /// 添加根结点的孩子结点
        /// </summary>
        /// <param name="name">结点名称</param>
        /// <param name="data">结点值</param>
        public void AddNode(string name, object data)
        {
            AddNode(name, data, string.Empty);
        }

        /// <summary>
        /// 清除所有结点
        /// </summary>
        public void Clean()
        {
            Root.Children = null;
        }

        /// <summary>
        /// 根据名称查找结点值
        /// </summary>
        /// <param name="node">开始查找结点</param>
        /// <param name="name">被查找结点的名称</param>
        /// <returns>返回被查找的结点</returns>
        private Node FindNode(Node node, string name)
        {
            Node result = null;
            if (node != null && node.Name == name)
            {
                return node;
            }
            if (node != null && node.Children != null)
            {
                foreach (Node child in node.Children)
                {
                    if (child.Name == name)
                    {
                        return child;
                    }
                    if (child.Children != null && child.Children.Count > 0)
                    {
                        result = FindNode(child, name);
                        if (result != null)
                        {
                            break;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 检查结点是否存在
        /// </summary>
        /// <param name="name">被查找结点的名称</param>
        /// <param name="node">开始查找的结点</param>
        /// <returns>true为存在，false不存在</returns>
        private bool IsExist(string name, Node node)
        {
            return FindNode(node, name) != null;
        }

#if DEBUG

        [TestMethod]
        public void Test_MultiTree()
        {
            MultiTree multiTree = new MultiTree();
            multiTree.AddNode("1", 1);
            multiTree.AddNode("1.1", 1.1, "1");
            multiTree.AddNode("2", 2);
            multiTree.AddNode("2.1", 2.1, "2");
            multiTree.AddNode("2.2", 2.2, "2");
            multiTree.AddNode("3", 3);
            multiTree.AddNode("3.1", 3.1, "3");
            multiTree.AddNode("3.2", 3.2, "3");
            multiTree.AddNode("3.3", 3.3, "3");
            multiTree.AddNode("4", 4);
            multiTree.AddNode("4.1", 4.1, "4");
            multiTree.AddNode("4.2", 4.2, "4");
            multiTree.AddNode("4.3", 4.3, "4");
            multiTree.AddNode("4.4", 4.4, "4");

            Node node = multiTree.GetNodeByPath("1");
            Assert.IsTrue((int)node.Data == 1);
            Assert.IsTrue(node.GetChildrenNum() == 1);
            Assert.IsTrue(node.Deep == 1);
            Assert.IsTrue(node.GetChildrenPath().Count() == 1);
            Assert.IsTrue(node.Name == "1");
            Assert.IsTrue(node.Path == "1");

            node = multiTree.GetNodeByPath("1/1.1");
            Assert.IsTrue(node.Name == "1.1");
            Assert.IsTrue(node.Path == "1/1.1");

            node = multiTree.GetNodeByPath("4");
            Assert.IsTrue(node.Name == "4");
            Assert.IsTrue(node.Path == "4");
            Assert.IsTrue(node.GetChildrenNum() == 4);
            node.Clean();
            Assert.IsTrue(node.GetChildrenNum() == 0);

        }

#endif
    }
}
