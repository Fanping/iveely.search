using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Iveely.Database;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using Iveely.Data;

namespace Iveely.WaterfallTree
{
    public partial class WTree
    {
        private partial class Branch
        {
            public volatile Task FallTask;

            private void DoFall(object state)
            {
                var tuple = (Tuple<Branch, BranchCache, int, Token, Params>)state;
                var branch = tuple.Item1;
                var cache = tuple.Item2;
                var level = tuple.Item3;
                var token = tuple.Item4;
                var param = tuple.Item5;
                if (!branch.IsNodeLoaded)
                    token.Semaphore.Wait();
                var node = branch.Node;
                Debug.Assert(branch == node.Branch);
//#if DEBUG
//                Debug.Assert(node.TaskID == 0);
//                node.TaskID = Task.CurrentId.Value;
//#endif

                //if (param.WalkAction != WalkAction.CacheFlush)
                    node.Touch(level);

                //1. Apply cache
                if (cache != null)
                {
                    if (cache.Count == 1 || node.Type == WTree.NodeType.Leaf)
                    {
                        foreach (var kv in cache)
                        {
                            //compact operations
                            kv.Key.Apply.Internal(kv.Value);

                            //apply
                            if (kv.Value.Count > 0)
                                node.Apply(kv.Value);
                        }
                    }
                    else
                    {
                        Parallel.ForEach(cache, (kv) =>
                        {
                            //compact operations
                            kv.Key.Apply.Internal(kv.Value);

                            //apply
                            if (kv.Value.Count > 0)
                                node.Apply(kv.Value);
                        });
                    }

                    cache.Clear();
                }
                
                //2. Maintenance
                if (node.Type == WTree.NodeType.Internal)
                    ((InternalNode)node).Maintenance(level, token);
                branch.NodeState = node.State;

                if (node.IsExpiredFromCache && (param.WalkAction & WalkAction.CacheFlush) == WalkAction.CacheFlush)
                    param = new Params(param.WalkMethod, WalkAction.Store | WalkAction.Unload, param.WalkParams, param.Sink);

                if (node.Type == WTree.NodeType.Internal)
                {
                    if (param.WalkMethod != WalkMethod.Current)
                    {
                        //broadcast
                        ((InternalNode)node).BroadcastFall(level, token, param);
                    }
                }

                if ((param.WalkAction & WalkAction.Store) == WTree.WalkAction.Store)
                {
                    if (node.IsModified)
                        node.Store();
                }

                if ((param.WalkAction & WalkAction.Unload) == WTree.WalkAction.Unload)
                {
                    //node.IsExpiredFromCache = false;
                    branch.Node = null;
                    Tree.Exclude(branch.NodeHandle);
                }

                if (token != null)
                    token.CountdownEvent.Signal();
//#if DEBUG
//                node.TaskID = 0;
//#endif
                branch.FallTask = null;

                Tree.WorkingFallCount.Decrement();
            }

            public bool Fall(int level, Token token, Params param, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
            {
                lock (this)
                {
                    WaitFall();

                    if (token != null)
                    {
                        if (token.Cancellation.IsCancellationRequested)
                            return false;

                        token.CountdownEvent.AddCount(1);
                    }

                    bool haveSink = false;
                    BranchCache cache = null;
                    if (param.Sink)
                    {
                        if (Cache.OperationCount > 0)
                        {
                            if (param.IsTotal)
                            {
                                cache = Cache;
                                Cache = new BranchCache();
                                haveSink = true;
                            }
                            else //no matter IsOverall or IsPoint, we exclude all the operations for the path
                            {
                                IOperationCollection operationCollection = Cache.Exclude(param.Path);
                                if (operationCollection != null)
                                {
                                    cache = new BranchCache(/*param.Path,*/ operationCollection);
                                    haveSink = true;
                                }
                            }
                        }
                    }

                    Tree.WorkingFallCount.Increment();
                    FallTask = Task.Factory.StartNew(DoFall, new Tuple<Branch, BranchCache, int, Token, Params>(this, cache, level - 1, token, param), taskCreationOptions);

                    return haveSink;
                }
            }

            public void WaitFall()
            {
                lock (this)
                {
                    Task task = FallTask;
                    if (task != null)
                        task.Wait();
                }
            }

            public void ApplyToCache(Locator locator, IOperation operation)
            {
                lock (this)
                    Cache.Apply(locator, operation);
            }

            public void ApplyToCache(IOperationCollection operations)
            {
                lock (this)
                    Cache.Apply(operations);
            }

            public void MaintenanceRoot(Token token)
            {
                if (node.IsOverflow)
                {
                    Branch newBranch = new Branch(Tree, NodeType, NodeHandle);
                    newBranch.Node = Node;
                    newBranch.Node.Branch = newBranch;
                    newBranch.NodeState = newBranch.Node.State;

                    NodeType = WTree.NodeType.Internal;
                    //NodeHandle = Tree.Repository.Reserve();
                    NodeHandle = Tree.heap.ObtainNewHandle();
                    Node = WTree.Node.Create(this);
                    NodeState = WTree.NodeState.None;

                    Tree.Depth++;

                    InternalNode rootNode = (InternalNode)Node;
                    rootNode.Branches.Add(new FullKey(Tree.MinLocator, null), newBranch);
                    //rootNode.Branches.Add(newBranch.Node.FirstKey, newBranch);
                    rootNode.HaveChildrenForMaintenance = true;
                    rootNode.Maintenance(Tree.Depth + 1, token);
                }
                else if (node.IsUnderflow)
                {
                    //TODO: also to release handle
                    //Debug.Assert(node.Type == NodeType.Internal);

                    //Branch branch = ((InternalNode)Node).Branches[0].Value;

                    //NodeType = branch.NodeType;
                    //NodeHandle = branch.NodeHandle;
                    //Node = branch.node;
                    //NodeState = branch.NodeState;

                    //Tree.Depth--;
                }

                token.CountdownEvent.Signal();
            }
        }

        private enum WalkMethod
        {
            Current,
            CascadeFirst,
            CascadeLast,
            Cascade,
            CascadeButOnlyLoaded,
        }

        [Flags]
        private enum WalkAction
        {
            None = 0,
            Store = 0x01,
            Unload = 0x02,
            CacheFlush = 0x04
        }

        private class WalkParams
        {
        }

        private class CacheWalkParams : WalkParams
        {
            public long TouchID;

            public CacheWalkParams(long touchID)
            {
                TouchID = touchID;
            }
        }

        private class Params
        {
            public readonly WalkMethod WalkMethod;
            public readonly WalkAction WalkAction;
            public readonly WalkParams WalkParams;

            #region param scope

            public readonly Locator Path;
            public readonly IData FromKey;
            public readonly IData ToKey;
            public readonly bool IsPoint;
            public readonly bool IsOverall;
            public readonly bool IsTotal;

            #endregion

            public readonly bool Sink;

            public Params(WalkMethod walkMethod, WalkAction walkAction, WalkParams walkParams, bool sink, Locator path, IData fromKey, IData toKey)
            {
                WalkMethod = walkMethod;
                WalkAction = walkAction;
                WalkParams = walkParams;

                Sink = sink;

                Path = path;
                FromKey = fromKey;
                ToKey = toKey;
                IsPoint = false;
                IsOverall = false;
                IsTotal = false;
            }

            public Params(WalkMethod walkMethod, WalkAction walkAction, WalkParams walkParams, bool sink, Locator path, IData key)
            {
                WalkMethod = walkMethod;
                WalkAction = walkAction;
                WalkParams = walkParams;

                Sink = sink;

                Path = path;
                FromKey = key;
                ToKey = key;
                IsPoint = true;
                IsOverall = false;
                IsTotal = false;
            }

            public Params(WalkMethod walkMethod, WalkAction walkAction, WalkParams walkParams, bool sink, Locator path)
            {
                WalkMethod = walkMethod;
                WalkAction = walkAction;
                WalkParams = walkParams;

                Sink = sink;

                Path = path;
                IsPoint = false;
                IsOverall = true;
                IsTotal = false;
            }

            public Params(WalkMethod walkMethod, WalkAction walkAction, WalkParams walkParams, bool sink)
            {
                WalkMethod = walkMethod;
                WalkAction = walkAction;
                WalkParams = walkParams;

                Sink = sink;

                IsPoint = false;
                IsOverall = false;
                IsTotal = true;
            }
        }

        private class Token
        {
            //private static long globalID = 0;

            //public readonly long ID;
            public readonly CountdownEvent CountdownEvent = new CountdownEvent(1);
            public readonly SemaphoreSlim Semaphore;
            public readonly CancellationToken Cancellation;

            [DebuggerStepThrough]
            public Token(SemaphoreSlim semaphore, CancellationToken cancellationToken)
            {
                Semaphore = semaphore;
                Cancellation = cancellationToken;

                //ID = Interlocked.Increment(ref globalID);
            }
        }
    }
}