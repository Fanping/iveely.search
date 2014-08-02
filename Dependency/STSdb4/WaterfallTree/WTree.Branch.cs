using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Iveely.STSdb4.Database;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.IO;

namespace Iveely.STSdb4.WaterfallTree
{
    public partial class WTree
    {
        private partial class Branch
        {
            public readonly WTree Tree;
            public BranchCache Cache = new BranchCache();

            /// <summary>
            /// on load
            /// </summary>
            public Branch(WTree tree, NodeType nodeType, long nodeHandle)
            {
                Tree = tree;
                NodeType = nodeType;
                NodeHandle = nodeHandle;
            }

            /// <summary>
            /// on brand new branch
            /// </summary>
            public Branch(WTree tree, NodeType nodeType)
                : this(tree, nodeType, tree.heap.ObtainNewHandle())
            {
                node = WTree.Node.Create(this);
            }

            public override string ToString()
            {
                return String.Format("NodeType = {0}, Handle = {1}, IsNodeLoaded = {2}, Cache.OperationCount = {3}", NodeType, NodeHandle, IsNodeLoaded, Cache.OperationCount);
            }

            #region Node

            public NodeType NodeType;

            /// <summary>
            /// permanent and unique node handle 
            /// </summary>
            public long NodeHandle { get; set; }
            
            public volatile NodeState NodeState;

            public bool IsNodeLoaded
            {
                get { return node != null; }
            }

            private Node node;

            public Node Node
            {
                get
                {
                    if (node != null)
                        return node;

                    node = Tree.Retrieve(NodeHandle);

                    if (node != null)
                    {
                        node.Branch.WaitFall();
                        node.Branch = this;
                        Tree.Packet(NodeHandle, node);
                    }
                    else
                    {
                        node = WTree.Node.Create(this);
                        node.Load();
                    }

                    return node;
                }
                set
                {
                    node = value;
                }
            }

            #endregion
        }
    }
}