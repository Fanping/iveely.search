using System;
using System.Collections.Generic;
using System.Linq;

namespace Iveely.Framework.DataStructure
{
    /// <summary>
    /// Multi-Tree
    /// </summary>
    [Serializable]
    public class MultiTree
    {
        /// <summary>
        /// The node of multi-tree
        /// </summary>
        [Serializable]
        public class Node
        {
            /// <summary>
            /// The name of the node (relative path)
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The deep of the node
            /// </summary>
            public int Deep { get; set; }

            /// <summary>
            /// The full path of node (absolute path)
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// The data of node
            /// </summary>
            public object Data { get; set; }

            /// <summary>
            /// The parent node
            /// </summary>
            public Node Parent { get; set; }

            /// <summary>
            /// The nodes of children
            /// </summary>
            public List<Node> Children { get; set; }

            /// <summary>
            /// Create node
            /// </summary>
            /// <param name="name">name of node</param>
            public Node(string name)
            {
                Path = name;
                Name = name;
                Deep = 0;
                Parent = null;
                Children = new List<Node>();
            }

            private Node()
            {
            }

            /// <summary>
            /// Create node with parent
            /// </summary>
            /// <param name="name">name of node</param>
            /// <param name="data"></param>
            /// <param name="parent">parent of node</param>
            public Node(string name, object data, Node parent)
            {
                if (parent.Path.Length > 1)
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
            /// Add a child
            /// </summary>
            /// <param name="name">name of the child</param>
            /// <param name="data">data of the child</param>
            /// <param name="parent">the parent of the child</param>
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
            /// Find a child by name
            /// </summary>
            /// <param name="name">name of the child</param>
            /// <returns>return the node of the child,null is not found.</returns>
            public Node FindChild(string name)
            {
                //if (!name.StartsWith("/"))
                //{
                //    name = "/" + name;
                //}
                foreach (Node node in Children)
                {
                    if (node.Name.Equals(name))
                    {
                        return node;
                    }
                }
                return null;
            }

            public void Clean()
            {
                Children = new List<Node>();
            }

            /// <summary>
            /// Get all full path on nodes of children
            /// </summary>
            /// <returns>list of the node path</returns>
            public IEnumerable<string> GetChildrenPath()
            {
                IEnumerable<string> pathes = Children.Select(o => o.Path);
                return pathes;
            }

            /// <summary>
            /// Get the number of children
            /// </summary>
            /// <returns>return the number</returns>
            public int GetChildrenNum()
            {
                IEnumerable<string> pathes = GetChildrenPath();
                return pathes != null ? pathes.Count() : 0;
            }
        }

        /// <summary>
        /// The root node
        /// </summary>
        public Node Root { get; set; }

        public MultiTree()
        {
            Root = new Node("/");
        }

        /// <summary>
        /// Get a node by name
        /// </summary>
        /// <param name="name">name of the node</param>
        /// <returns>the node has been found</returns>
        public Node GetNodeByName(string name)
        {
            string[] catelogs = name.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            Node node = Root;
            foreach (string catelog in catelogs)
            {
                string relativePath = catelog;
                //if (!relativePath.StartsWith("/"))
                //{
                //    relativePath = "/" + relativePath;
                //}
                node = FindNode(node, relativePath);
            }
            return node;
        }

        public void Rename(string path, string newName)
        {
            Node node = GetNodeByName(path);
            if (node != null)
            {
                node.Name = newName;
                node.Path =
                    node.Path.Substring(0, node.Path.LastIndexOf('/') + 1) +
                    newName;
            }

        }

        /// <summary>
        /// Add a node under anther node
        /// </summary>
        /// <param name="name">name of node which want to add</param>
        /// <param name="data"></param>
        /// <param name="parentName">parent name of the node</param>
        /// <param name="lastNode"></param>
        public void AddNode(string name, object data, string parentName, bool lastNode = false)
        {
            Node parent;
            if (parentName == string.Empty || parentName.ToLower() == "ise:/")
            {
                parent = Root;
            }
            else
            {
                parent = GetNodeByName(parentName);
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
        /// Add a node under root node
        /// </summary>
        /// <param name="name">name of the node</param>
        /// <param name="data"></param>
        public void AddNode(string name, object data)
        {
            AddNode(name, data, string.Empty);
        }

        /// <summary>
        /// Clean all data
        /// </summary>
        public void Clean()
        {
            Root.Children = null;
        }

        /// <summary>
        /// Find a node by name
        /// </summary>
        /// <param name="node">the node that start to find</param>
        /// <param name="name">the name of node which want to find</param>
        /// <returns>return the node has been found</returns>
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
                    //if (!name.StartsWith("/"))
                    //{
                    //    name = "/" + name;
                    //}

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
        /// Check the node whether exist on the node
        /// </summary>
        /// <param name="name">the name of the node</param>
        /// <param name="node">the node to check</param>
        /// <returns></returns>
        private bool IsExist(string name, Node node)
        {
            return FindNode(node, name) != null;
        }
    }
}
