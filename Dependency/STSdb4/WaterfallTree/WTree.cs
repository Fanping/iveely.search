using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
using System.Diagnostics;
using Iveely.General.Threading;
using Iveely.Data;
using Iveely.Database;
using Iveely.General.Collections;

namespace Iveely.WaterfallTree
{
    public partial class WTree : IDisposable
    {
        public int INTERNAL_NODE_MIN_BRANCHES = 2; //default values
        public int INTERNAL_NODE_MAX_BRANCHES = 4;
        public int INTERNAL_NODE_MAX_OPERATIONS_IN_ROOT = 8 * 1024;
        public int INTERNAL_NODE_MIN_OPERATIONS = 64 * 1024;
        public int INTERNAL_NODE_MAX_OPERATIONS = 128 * 1024;
        public int LEAF_NODE_MIN_RECORDS = 16 * 1024;
        public int LEAF_NODE_MAX_RECORDS = 128 * 1024;

        //reserved handles
        private const long HANDLE_SETTINGS = 0;
        private const long HANDLE_SCHEME = 1;
        private const long HANDLE_ROOT = 2;
        private const long HANDLE_RESERVED = 3;

        private readonly Countdown WorkingFallCount = new Countdown();
        private readonly Branch RootBranch;
        private bool isRootCacheLoaded;

        private volatile bool disposed = false;
        private volatile bool Shutdown = false;
        private int Depth = 1;

        private long globalVersion;

        public long GlobalVersion
        {
            get { return Interlocked.Read(ref globalVersion); }
            set { Interlocked.Exchange(ref globalVersion, value); }
        }

        private readonly Scheme scheme;
        public readonly IHeap heap;

        public WTree(IHeap heap)
        {
            if (heap == null)
                throw new NullReferenceException("heap");

            this.heap = heap;

            if (heap.Exists(HANDLE_SETTINGS))
            {
                //create root branch with dummy handle
                RootBranch = new Branch(this, NodeType.Leaf, 0);

                //read settings - settings will set the RootBranch.NodeHandle
                using (MemoryStream ms = new MemoryStream(heap.Read(HANDLE_SETTINGS)))
                    Settings.Deserialize(this, ms);

                //read scheme
                using (MemoryStream ms = new MemoryStream(heap.Read(HANDLE_SCHEME)))
                    scheme = Scheme.Deserialize(new BinaryReader(ms));

                ////load branch cache
                //using (MemoryStream ms = new MemoryStream(Heap.Read(HANDLE_ROOT)))
                //    RootBranch.Cache.Load(this, new BinaryReader(ms));
                isRootCacheLoaded = false;
            }
            else
            {
                //obtain reserved handles
                var handle = heap.ObtainNewHandle();
                if (handle != HANDLE_SETTINGS)
                    throw new Exception("Logical error.");

                scheme = new WaterfallTree.Scheme();
                handle = heap.ObtainNewHandle();
                if (handle != HANDLE_SCHEME)
                    throw new Exception("Logical error.");

                handle = heap.ObtainNewHandle();
                if (handle != HANDLE_ROOT)
                    throw new Exception("Logical error.");

                handle = heap.ObtainNewHandle();
                if (handle != HANDLE_RESERVED)
                    throw new Exception("Logical error.");

                RootBranch = new Branch(this, NodeType.Leaf); //the constructor will invoke Heap.ObtainHandle()

                isRootCacheLoaded = true;
            }

            CacheThread = new Thread(DoCache);
            CacheThread.Start();
        }

        private void LoadRootCache()
        {
            using (MemoryStream ms = new MemoryStream(heap.Read(HANDLE_ROOT)))
                RootBranch.Cache.Load(this, new BinaryReader(ms));

            isRootCacheLoaded = true;
        }

        private void Sink()
        {
            RootBranch.WaitFall();

            if (RootBranch.NodeState != NodeState.None)
            {
                Token token = new Token(CacheSemaphore, new CancellationTokenSource().Token);
                RootBranch.MaintenanceRoot(token);
                RootBranch.Node.Touch(Depth + 1);
                token.CountdownEvent.Wait();
            }

            RootBranch.Fall(Depth + 1, new Token(CacheSemaphore, CancellationToken.None), new Params(WalkMethod.Current, WalkAction.None, null, true));
        }

        public void Execute(IOperationCollection operations)
        {
            if (disposed)
                throw new ObjectDisposedException("WTree");

            lock (RootBranch)
            {
                if (!isRootCacheLoaded)
                    LoadRootCache();
                
                RootBranch.ApplyToCache(operations);

                if (RootBranch.Cache.OperationCount > INTERNAL_NODE_MAX_OPERATIONS_IN_ROOT)
                    Sink();
            }
        }

        public void Execute(Locator locator, IOperation operation)
        {
            if (disposed)
                throw new ObjectDisposedException("WTree");

            lock (RootBranch)
            {
                if (!isRootCacheLoaded)
                    LoadRootCache();
                
                RootBranch.ApplyToCache(locator, operation);

                if (RootBranch.Cache.OperationCount > INTERNAL_NODE_MAX_OPERATIONS_IN_ROOT)
                    Sink();
            }
        }

        /// <summary>
        /// The hook.
        /// </summary>
        public IOrderedSet<IData, IData> FindData(Locator originalLocator, Locator locator, IData key, Direction direction, out FullKey nearFullKey, out bool hasNearFullKey, ref FullKey lastVisitedFullKey)
        {
            if (disposed)
                throw new ObjectDisposedException("WTree");

            nearFullKey = default(FullKey);
            hasNearFullKey = false;

            var branch = RootBranch;
            Monitor.Enter(branch);

            if (!isRootCacheLoaded)
                LoadRootCache();

            Params param;
            if (key != null)
                param = new Params(WalkMethod.Cascade, WalkAction.None, null, true, locator, key);
            else
            {
                switch (direction)
                {
                    case Direction.Forward:
                        param = new Params(WalkMethod.CascadeFirst, WalkAction.None, null, true, locator);
                        break;
                    case Direction.Backward:
                        param = new Params(WalkMethod.CascadeLast, WalkAction.None, null, true, locator);
                        break;
                    default:
                        throw new NotSupportedException(direction.ToString());
                }
            }

            branch.Fall(Depth + 1, new Token(CacheSemaphore, CancellationToken.None), param);
            branch.WaitFall();

            switch (direction)
            {
                case Direction.Forward:
                    {
                        while (branch.NodeType == NodeType.Internal)
                        {
                            KeyValuePair<FullKey, Branch> newBranch = ((InternalNode)branch.Node).FindBranch(locator, key, direction, ref nearFullKey, ref hasNearFullKey);

                            Monitor.Enter(newBranch.Value);
                            newBranch.Value.WaitFall();
                            Debug.Assert(!newBranch.Value.Cache.Contains(originalLocator));
                            Monitor.Exit(branch);

                            branch = newBranch.Value;
                        }
                    }
                    break;
                case Direction.Backward:
                    {
                        int depth = Depth;
                        KeyValuePair<FullKey, Branch> newBranch = default(KeyValuePair<FullKey, Branch>);
                        while (branch.NodeType == NodeType.Internal)
                        {
                            InternalNode node = (InternalNode)branch.Node;
                            newBranch = node.Branches[node.Branches.Count - 1];

                            int cmp = newBranch.Key.Locator.CompareTo(lastVisitedFullKey.Locator);
                            if (cmp == 0)
                            {
                                if (lastVisitedFullKey.Key == null)
                                    cmp = -1;
                                else
                                    cmp = newBranch.Key.Locator.KeyComparer.Compare(newBranch.Key.Key, lastVisitedFullKey.Key);
                            }
                            //else
                            //{
                            //    Debug.WriteLine("");
                            //}

                            //newBranch.Key.CompareTo(lastVisitedFullKey) >= 0
                            if (cmp >= 0)
                                newBranch = node.FindBranch(locator, key, direction, ref nearFullKey, ref hasNearFullKey);
                            else
                            {
                                if (node.Branches.Count >= 2)
                                {
                                    hasNearFullKey = true;
                                    nearFullKey = node.Branches[node.Branches.Count - 2].Key;
                                }
                            }
                            
                            Monitor.Enter(newBranch.Value);
                            depth--;
                            newBranch.Value.WaitFall();
                            if (newBranch.Value.Cache.Contains(originalLocator))
                            {
                                newBranch.Value.Fall(depth + 1, new Token(CacheSemaphore, CancellationToken.None), new Params(WalkMethod.Current, WalkAction.None, null, true, originalLocator));
                                newBranch.Value.WaitFall();
                            }
                            Debug.Assert(!newBranch.Value.Cache.Contains(originalLocator));
                            Monitor.Exit(branch);

                            branch = newBranch.Value;
                        }

                        //if (lastVisitedFullKey.Locator.Equals(newBranch.Key.Locator) &&
                        //    (lastVisitedFullKey.Key != null && lastVisitedFullKey.Locator.KeyEqualityComparer.Equals(lastVisitedFullKey.Key, newBranch.Key.Key)))
                        //{
                        //    Monitor.Exit(branch);
                        //    return null;
                        //}

                        lastVisitedFullKey = newBranch.Key;
                    }
                    break;
                default:
                    throw new NotSupportedException(direction.ToString());
            }

            IOrderedSet<IData, IData> data = ((LeafNode)branch.Node).FindData(originalLocator, direction, ref nearFullKey, ref hasNearFullKey);

            Monitor.Exit(branch);

            return data;
        }

        private void Commit(CancellationToken cancellationToken, Locator locator = default(Locator), bool hasLocator = false, IData fromKey = null, IData toKey = null)
        {
            if (disposed)
                throw new ObjectDisposedException("WTree");

            Params param;
            if (!hasLocator)
                param = new Params(WalkMethod.CascadeButOnlyLoaded, WalkAction.Store, null, false);
            else
            {
                if (fromKey == null)
                    param = new Params(WalkMethod.CascadeButOnlyLoaded, WalkAction.Store, null, false, locator);
                else
                {
                    if (toKey == null)
                        param = new Params(WalkMethod.CascadeButOnlyLoaded, WalkAction.Store, null, false, locator, fromKey);
                    else
                        param = new Params(WalkMethod.CascadeButOnlyLoaded, WalkAction.Store, null, false, locator, fromKey, toKey);
                }
            }

            lock (RootBranch)
            {
                if (!isRootCacheLoaded)
                    LoadRootCache();
                
                Token token = new Token(CacheSemaphore, cancellationToken);
                RootBranch.Fall(Depth + 1, token, param);

                token.CountdownEvent.Signal();
                token.CountdownEvent.Wait();

                //write settings
                using (MemoryStream ms = new MemoryStream())
                {
                    Settings.Serialize(this, ms);
                    heap.Write(HANDLE_SETTINGS, ms.GetBuffer(), 0, (int)ms.Length);
                }

                //write scheme
                using (MemoryStream ms = new MemoryStream())
                {
                    scheme.Serialize(new BinaryWriter(ms));
                    heap.Write(HANDLE_SCHEME, ms.GetBuffer(), 0, (int)ms.Length);
                }

                //write root cache
                using (MemoryStream ms = new MemoryStream())
                {
                    RootBranch.Cache.Store(this, new BinaryWriter(ms));
                    heap.Write(HANDLE_ROOT, ms.GetBuffer(), 0, (int)ms.Length);
                }

                heap.Commit();
            }
        }

        public virtual void Commit()
        {
            Commit(CancellationToken.None);
        }

        public IHeap Heap
        {
            get { return heap; }
        }

        #region Locator

        private Locator MinLocator
        {
            get { return Locator.MIN; }
        }

        protected Locator CreateLocator(string name, int structureType, DataType keyDataType, DataType recordDataType, Type keyType, Type recordType)
        {
            return scheme.Create(name, structureType, keyDataType, recordDataType, keyType, recordType);
        }

        protected Locator GetLocator(long id)
        {
            return scheme[id];
        }

        protected IEnumerable<Locator> GetAllLocators()
        {
            return scheme.Select(kv => kv.Value);
        }

        private void SerializeLocator(BinaryWriter writer, Locator locator)
        {
            writer.Write(locator.ID);
        }

        private Locator DeserializeLocator(BinaryReader reader)
        {
            long id = reader.ReadInt64();
            if (id == Locator.MIN.ID)
                return Locator.MIN;

            Locator locator = scheme[id];

            if (!locator.IsReady)
                locator.Prepare();

            if (locator == null)
                throw new Exception("Logical error");

            return locator;
        }

        #endregion

        #region Cache

        /// <summary>
        /// Branch.NodeID -> node
        /// </summary>
        private readonly ConcurrentDictionary<long, Node> Cache = new ConcurrentDictionary<long, Node>();
        private Thread CacheThread;

        private SemaphoreSlim CacheSemaphore = new SemaphoreSlim(int.MaxValue, int.MaxValue);

        private int cacheSize = 32;

        public int CacheSize
        {
            get { return cacheSize; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Cache size is invalid.");

                cacheSize = value;

                if (Cache.Count > CacheSize * 1.1)
                {
                    lock (Cache)
                        Monitor.Pulse(Cache);
                }
            }
        }

        private void Packet(long id, Node node)
        {
            Debug.Assert(!Cache.ContainsKey(id));
            Cache[id] = node;

            if (Cache.Count > CacheSize * 1.1)
            {
                lock (Cache)
                    Monitor.Pulse(Cache);
            }
        }

        private Node Retrieve(long id)
        {
            Node node;
            Cache.TryGetValue(id, out node);

            return node;
        }

        private Node Exclude(long id)
        {
            Node node;
            Cache.TryRemove(id, out node);
            //Debug.Assert(node != null);

            int delta = (int)(CacheSize * 1.1 - Cache.Count);
            if (delta > 0)
                CacheSemaphore.Release(delta);

            return node;
        }

        private void DoCache()
        {
            while (!Shutdown)
            {
                while (Cache.Count > CacheSize * 1.1)
                {
                    KeyValuePair<long, Node>[] kvs = Cache.ToArray();

                    foreach (var kv in kvs.Where(x => !x.Value.IsRoot).OrderBy(x => x.Value.TouchID).Take(Cache.Count - CacheSize))
                        kv.Value.IsExpiredFromCache = true;
                    //Debug.WriteLine(Cache.Count);
                    Token token;
                    lock (RootBranch)
                    {
                        token = new Token(CacheSemaphore, CancellationToken.None);
                        CacheSemaphore = new SemaphoreSlim(0, int.MaxValue);
                        var param = new Params(WalkMethod.CascadeButOnlyLoaded, WalkAction.CacheFlush, null, false);
                        RootBranch.Fall(Depth + 1, token, param);
                    }

                    token.CountdownEvent.Signal();
                    token.CountdownEvent.Wait();
                    CacheSemaphore.Release(int.MaxValue / 2);
                }

                lock (Cache)
                {
                    if (Cache.Count <= CacheSize * 1.1)
                        Monitor.Wait(Cache, 1);
                }
            }
        }

        #endregion

        #region IDisposable Members

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    Shutdown = true;
                    if (CacheThread != null)
                        CacheThread.Join();

                    WorkingFallCount.Wait();

                    heap.Close();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            
            GC.SuppressFinalize(this);
        }

        ~WTree()
        {
            Dispose(false);
        }

        public virtual void Close()
        {
            Dispose();
        }

        #endregion
    }

    public enum Direction
    {
        Backward = -1,
        None = 0,
        Forward = 1
    }
}
