using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.IO.Compression;
using Iveely.General.Compression;

namespace Iveely.WaterfallTree
{
    public partial class WTree
    {
        private abstract class Node
        {
            public bool IsModified { get; protected set; }
            public Branch Branch;
            public volatile bool IsExpiredFromCache;
#if DEBUG
            public volatile int TaskID;
#endif
            private static long globalTouchID = 0;
            private long touchID;

            public long TouchID
            {
                get { return Interlocked.Read(ref touchID); }
                set { Interlocked.Exchange(ref touchID, value); }
            }

            public Node(Branch branch)
            {
                Branch = branch;
            }

            public abstract void Apply(IOperationCollection operations);
            public abstract Node Split();
            public abstract void Merge(Node node);
            public abstract bool IsOverflow { get; }
            public abstract bool IsUnderflow { get; }
            public abstract FullKey FirstKey { get; }

            public abstract void Store(Stream stream);
            public abstract void Load(Stream stream);

            public void Touch(long count)
            {
                Debug.Assert(count > 0);
                touchID = Interlocked.Add(ref globalTouchID, count);

                //IsExpiredFromCache = false;
            }

            //only for speed reason
            public NodeType Type
            {
                get { return Branch.NodeType; }
            }

            public bool IsRoot
            {
                get { return Object.ReferenceEquals(Branch.Tree.RootBranch, Branch); }
            }

            public NodeState State
            {
                get
                {
                    if (IsOverflow)
                        return NodeState.Overflow;

                    if (IsUnderflow)
                        return NodeState.Underflow;

                    return NodeState.None;
                }
            }

            public void Store()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    Store(stream);

                    //int recordCount = 0;
                    //string type = "";
                    //if (this is InternalNode)
                    //{
                    //    recordCount = ((InternalNode)this).Branch.Cache.OperationCount;
                    //    type = "Internal";
                    //}
                    //else
                    //{
                    //    recordCount = ((LeafNode)this).RecordCount;
                    //    type = "Leaf";
                    //}
                    //double sizeInMB = Math.Round(stream.Length / (1024.0 * 1024), 2);
                    //Console.WriteLine("{0} {1}, Records {2}, Size {3} MB", type, Branch.NodeHandle, recordCount, sizeInMB);

                    Branch.Tree.heap.Write(Branch.NodeHandle, stream.GetBuffer(), 0, (int)stream.Length);
                }
            }

            public void Load()
            {
                var heap = Branch.Tree.heap;
                byte[] buffer = heap.Read(Branch.NodeHandle);
                Load(new MemoryStream(buffer));
            }

            public static Node Create(Branch branch)
            {
                Node node;
                switch (branch.NodeType)
                {
                    case WTree.NodeType.Leaf:
                        node = new LeafNode(branch, true);
                        break;
                    case WTree.NodeType.Internal:
                        node = new InternalNode(branch, new BranchCollection(), true);
                        break;
                    default:
                        throw new NotSupportedException();
                }

                branch.Tree.Packet(node.Branch.NodeHandle, node);
                return node;
            }
        }

        public enum NodeState
        {
            None,
            Overflow,
            Underflow
        }

        protected enum NodeType : byte
        {
            Leaf,
            Internal
        }
    }
}
