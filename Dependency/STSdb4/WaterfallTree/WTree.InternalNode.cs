using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Iveely.STSdb4.Database;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Iveely.STSdb4.General.Compression;
using Iveely.STSdb4.General.Extensions;
using Iveely.STSdb4.Data;

namespace Iveely.STSdb4.WaterfallTree
{
    public partial class WTree
    {
        private sealed partial class InternalNode : Node
        {
            public const byte VERSION = 40;

            private readonly BranchesOptimizator Optimizator = new BranchesOptimizator();

            public readonly BranchCollection Branches;

            public InternalNode(Branch branch)
                : this(branch, new BranchCollection(), false)
            {
            }

            public InternalNode(Branch branch, BranchCollection branches, bool isModified)
                : base(branch)
            {
                Debug.Assert(branch.NodeType == NodeType.Internal);

                Branches = branches;
                IsModified = isModified;
            }

            private void SequentialApply(IOperationCollection operations)
            {
                Locator locator = operations.Locator;

                var last = Branches[Branches.Count - 1];
                if (Object.ReferenceEquals(last.Key.Locator, locator) && locator.KeyComparer.Compare(last.Key.Key, operations[0].FromKey) <= 0)
                {
                    Branch branch = last.Value;
                    branch.ApplyToCache(operations);
                    if (branch.NodeState != NodeState.None)
                        HaveChildrenForMaintenance = true;

                    return;
                }

                Range range = Optimizator.FindRange(locator);

                if (!range.IsBaseLocator)
                {
                    Branch branch = Branches[range.LastIndex].Value;
                    branch.ApplyToCache(operations);
                    if (branch.NodeState != NodeState.None)
                        HaveChildrenForMaintenance = true;

                    return;
                }

                int index = 0;

                for (int i = range.FirstIndex; i <= range.LastIndex; i++)
                {
                    var key = Branches[i].Key.Key;

                    int idx = operations.BinarySearch(key, index, operations.Count - index);
                    if (idx < 0)
                        idx = ~idx;
                    idx--;

                    int count = idx - index + 1;
                    if (count > 0)
                    {
                        var oprs = count < operations.Count ? operations.Midlle(index, count) : operations;
                        var branch = Branches[i - 1].Value;

                        branch.ApplyToCache(oprs);
                        if (branch.NodeState != NodeState.None)
                            HaveChildrenForMaintenance = true;

                        index += count;
                    }
                }

                if (operations.Count - index > 0)
                {
                    var oprs = index > 0 ? operations.Midlle(index, operations.Count - index) : operations;
                    var branch = Branches[range.LastIndex].Value;

                    Debug.Assert(Branches[range.LastIndex].Key.Locator.Equals(oprs.Locator));
                    Debug.Assert(oprs.Locator.KeyComparer.Compare(Branches[range.LastIndex].Key.Key, oprs[0].FromKey) <= 0);

                    branch.ApplyToCache(oprs);
                    if (branch.NodeState != NodeState.None)
                        HaveChildrenForMaintenance = true;
                }
            }

            public override void Apply(IOperationCollection operations)
            {
                Debug.Assert(operations.Count > 0);

                if (operations.AreAllMonotoneAndPoint)
                {
                    SequentialApply(operations); //sequential mode optimization
                    IsModified = true;
                    return;
                }

                Locator locator = operations.Locator;
                Range range = Optimizator.FindRange(locator);

                foreach (var operation in operations)
                {
                    int firstIndex, lastIndex;

                    switch (operation.Scope)
                    {
                        case OperationScope.Point:
                            {
                                firstIndex = lastIndex = Optimizator.FindIndex(range, locator, operation.FromKey);
                                Debug.Assert(firstIndex >= 0);
                            }
                            break;
                        case OperationScope.Range:
                            {
                                firstIndex = Optimizator.FindIndex(range, locator, operation.FromKey);
                                if (firstIndex < 0)
                                    firstIndex = 0;
                                lastIndex = Optimizator.FindIndex(range, locator, operation.ToKey);
                            }
                            break;
                        case OperationScope.Overall:
                            {
                                firstIndex = range.FirstIndex;
                                if (range.IsBaseLocator && range.FirstIndex > 0)
                                    firstIndex--;

                                lastIndex = range.LastIndex;
                            }
                            break;
                        default:
                            throw new NotSupportedException(operation.Scope.ToString());
                    }

                    for (int i = firstIndex; i <= lastIndex; i++)
                    {
                        Branch branch = Branches[i].Value;

                        branch.ApplyToCache(locator, operation);

                        if (branch.NodeState != NodeState.None)
                            HaveChildrenForMaintenance = true;
                    }
                }

                IsModified = true;
            }

            public override Node Split()
            {
                Branch rightBranch = new Branch(Branch.Tree, NodeType.Internal);
                InternalNode rightNode = (InternalNode)rightBranch.Node;

                int leftCount = 0;
                int leftBranchesCount = Branches.Count / 2;

                for (int i = 0; i < leftBranchesCount; i++)
                    leftCount += Branches[i].Value.Cache.OperationCount;

                rightNode.Branches.AddRange(Branches, leftBranchesCount, Branches.Count - leftBranchesCount);
                Branches.RemoveRange(leftBranchesCount, Branches.Count - leftBranchesCount);

                RebuildOptimizator();
                rightNode.RebuildOptimizator();

                rightNode.TouchID = TouchID;

                IsModified = true;

                return rightNode;
            }

            public override void Merge(Node node)
            {
                InternalNode rightNode = (InternalNode)node;
                Branches.AddRange(rightNode.Branches);

                RebuildOptimizator();
                rightNode.RebuildOptimizator();

                if (TouchID < node.TouchID)
                    TouchID = node.TouchID;

                IsModified = true;
            }

            public void BroadcastFall(int level, Token token, Params param)
            {
                int firstIndex, lastIndex;
                if (param.IsTotal)
                {
                    firstIndex = 0;
                    lastIndex = Branches.Count - 1;
                }
                else
                {
                    Range range = Optimizator.FindRange(param.Path);
                    if (param.IsPoint)
                    {
                        firstIndex = lastIndex = Optimizator.FindIndex(range, param.Path, param.FromKey);
                        Debug.Assert(firstIndex >= 0);
                    }
                    else if (param.IsOverall)
                    {
                        firstIndex = range.FirstIndex;
                        if (range.IsBaseLocator && range.FirstIndex > 0)
                            firstIndex--;

                        lastIndex = range.LastIndex;
                    }
                    else
                    {
                        firstIndex = Optimizator.FindIndex(range, param.Path, param.FromKey);
                        if (firstIndex < 0)
                            firstIndex = 0;
                        lastIndex = Optimizator.FindIndex(range, param.Path, param.ToKey);
                    }
                }

                IEnumerable<KeyValuePair<FullKey, Branch>> branches;
                switch (param.WalkMethod)
                {
                    case WalkMethod.CascadeFirst:
                        branches = Branches.Range(firstIndex, firstIndex);
                        break;
                    case WalkMethod.CascadeLast:
                        branches = Branches.Range(lastIndex, lastIndex);
                        break;
                    case WalkMethod.Cascade:
                        branches = Branches.Range(firstIndex, lastIndex);
                        break;
                    case WalkMethod.CascadeButOnlyLoaded:
                        branches = Branches.Range(firstIndex, lastIndex);
                        break;
                    default:
                        throw new NotSupportedException(param.WalkMethod.ToString());
                }

                TaskCreationOptions taskCreationOptions = TaskCreationOptions.None;
                //if ((param.WalkAction & WalkAction.Store) == WTree<TPath>.WalkAction.Store)
                  //  taskCreationOptions = TaskCreationOptions.AttachedToParent;

                Parallel.ForEach(branches, (branch) =>
                    {
                        if (param.WalkMethod == WalkMethod.CascadeButOnlyLoaded)
                        {
                            branch.Value.WaitFall();
                            if (!branch.Value.IsNodeLoaded)
                                return;
                        }

                        if (branch.Value.Fall(level, token, param, taskCreationOptions))
                            IsModified = true;

                        //if ((param.WalkAction & WalkAction.Store) == WTree<TPath>.WalkAction.Store)
                        //    branch.Value.WaitFall();
                    });
            }

            public override bool IsOverflow
            {
                get { return Branches.Count > Branch.Tree.INTERNAL_NODE_MAX_BRANCHES; }
            }

            public override bool IsUnderflow
            {
                get
                {
                    if (IsRoot)
                        return Branches.Count < 2;

                    return Branches.Count < Branch.Tree.INTERNAL_NODE_MIN_BRANCHES;
                }
            }

            public override FullKey FirstKey
            {
                get { return Branches[0].Key; }
            }

            public override void Store(Stream stream)
            {
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(VERSION);

                CountCompression.Serialize(writer, checked((ulong)Branch.NodeHandle));

                writer.Write(HaveChildrenForMaintenance);
                Branches.Store(Branch.Tree, writer);

                IsModified = false;
            }

            public override void Load(Stream stream)
            {
                BinaryReader reader = new BinaryReader(stream);
                if (reader.ReadByte() != VERSION)
                    throw new Exception("Invalid InternalNode version.");

                long id = (long)CountCompression.Deserialize(reader);
                if (id != Branch.NodeHandle)
                    throw new Exception("Wtree logical error.");

                HaveChildrenForMaintenance = reader.ReadBoolean();
                Branches.Load(Branch.Tree, reader);

                RebuildOptimizator();

                IsModified = false;
            }

            private KeyValuePair<FullKey, Branch> FindFirstBranch(Range range, ref FullKey nearFullKey, ref bool hasNearFullKey)
            {
                int idx;
                if (!range.IsBaseLocator)
                    idx = range.LastIndex;
                else
                {
                    idx = range.FirstIndex;
                    if (idx > 0)
                        idx--;
                }

                if (idx + 1 < Branches.Count)
                {
                    hasNearFullKey = true;
                    nearFullKey = Branches[idx + 1].Key;
                }

                return Branches[idx]; 
            }

            private KeyValuePair<FullKey, Branch> FindLastBranch(Range range, ref FullKey nearFullKey, ref bool hasNearFullKey)
            {
                int idx = range.LastIndex; //no matter of range.IsBaseLocator

                if (idx > 0)
                {
                    hasNearFullKey = true;
                    nearFullKey = Branches[idx - 1].Key;
                }

                return Branches[idx]; 
            }

            public Branch FindLastBranch(Locator locator, ref FullKey nearFullKey, ref bool hasNearFullKey)
            {
                Range range = Optimizator.FindRange(locator);
                int idx = range.LastIndex; //no matter of range.IsBaseLocator

                if (idx > 0)
                {
                    hasNearFullKey = true;
                    nearFullKey = Branches[idx - 1].Key;
                }

                return Branches[idx].Value;
            }

            /// <summary>
            /// The hook.
            /// </summary>
            public KeyValuePair<FullKey, Branch> FindBranch(Locator locator, IData key, Direction direction, ref FullKey nearFullKey, ref bool hasNearFullKey)
            {
                Range range = Optimizator.FindRange(locator);
                
                if (key == null)
                {
                    switch (direction)
                    {
                        case Direction.Forward:
                            return FindFirstBranch(range, ref nearFullKey, ref hasNearFullKey);
                        case Direction.Backward:
                            return FindLastBranch(range, ref nearFullKey, ref hasNearFullKey);
                        default:
                            throw new NotSupportedException(direction.ToString());
                    }
                }

                int idx = Optimizator.FindIndex(range, locator, key);
                Debug.Assert(idx >= 0);

                switch (direction)
                {
                    case Direction.Backward:
                        {
                            if (idx > 0)
                            {
                                nearFullKey = Branches[idx - 1].Key;
                                hasNearFullKey = true;
                            }
                        }
                        break;
                    case Direction.Forward:
                        {
                            if (idx < Branches.Count - 1)
                            {
                                hasNearFullKey = true;
                                nearFullKey = Branches[idx + 1].Key;
                            }
                        }
                        break;
                }

                return Branches[idx];
            }

            public void RebuildOptimizator()
            {
                Optimizator.Rebuild(Branches);
            }
        }
    }
}