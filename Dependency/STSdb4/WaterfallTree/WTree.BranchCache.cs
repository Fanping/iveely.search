using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Diagnostics;
using Iveely.STSdb4.Database;

namespace Iveely.STSdb4.WaterfallTree
{
    public partial class WTree
    {
        private class BranchCache : IEnumerable<KeyValuePair<Locator, IOperationCollection>>
        {
            private Dictionary<Locator, IOperationCollection> cache;
            private IOperationCollection Operations;

            /// <summary>
            /// Number of all operations in cache
            /// </summary>
            public int OperationCount { get; private set; }

            public int Count { get; private set; }

            public BranchCache()
            {
            }

            public BranchCache(IOperationCollection operations)
            {
                Operations = operations;
                Count = 1;
                OperationCount = operations.Count;
            }

            private IOperationCollection Obtain(Locator locator)
            {
                if (Count == 0)
                {
                    Operations = locator.OperationCollectionFactory.Create(0);
                    Debug.Assert(cache == null);
                    Count++;
                }
                else
                {
                    if (!Operations.Locator.Equals(locator))
                    {
                        if (cache == null)
                        {
                            cache = new Dictionary<Locator, IOperationCollection>();
                            cache[Operations.Locator] = Operations;
                        }

                        if (!cache.TryGetValue(locator, out Operations))
                        {
                            cache[locator] = Operations = locator.OperationCollectionFactory.Create(0);
                            Count++;
                        }
                    }
                }

                return Operations;
            }

            public void Apply(Locator locator, IOperation operation)
            {
                var operations = Obtain(locator);

                operations.Add(operation);
                OperationCount++;
            }

            public void Apply(IOperationCollection oprs)
            {
                var operations = Obtain(oprs.Locator);

                operations.AddRange(oprs);
                OperationCount += oprs.Count;
            }

            public void Clear()
            {
                cache = null;
                Operations = null;
                Count = 0;
                OperationCount = 0;
            }

            public bool Contains(Locator locator)
            {
                if (Count == 0)
                    return false;

                if (Count == 1)
                    return Operations.Locator.Equals(locator);

                if (cache != null)
                    return cache.ContainsKey(locator);

                return false;
            }
                        
            public IOperationCollection Exclude(Locator locator)
            {
                if (Count == 0)
                    return null;

                IOperationCollection operations;

                if (!Operations.Locator.Equals(locator))
                {
                    if (cache == null || !cache.TryGetValue(locator, out operations))
                        return null;

                    cache.Remove(locator);
                    if (cache.Count == 1)
                        cache = null;
                }
                else
                {
                    operations = Operations;

                    if (Count == 1)
                        Operations = null;
                    else
                    {
                        cache.Remove(locator);
                        Operations = cache.First().Value;
                        if (cache.Count == 1)
                            cache = null;
                    }
                }

                Count--;
                OperationCount -= operations.Count;

                return operations;
            }

            public IEnumerator<KeyValuePair<Locator, IOperationCollection>> GetEnumerator()
            {
                IEnumerable<KeyValuePair<Locator, IOperationCollection>> enumerable;

                if (Count == 0)
                    enumerable = System.Linq.Enumerable.Empty<KeyValuePair<Locator, IOperationCollection>>();
                else if (Count == 1)
                    enumerable = new KeyValuePair<Locator, IOperationCollection>[] { new KeyValuePair<Locator, IOperationCollection>(Operations.Locator, Operations) };
                else
                    enumerable = cache;

                return enumerable.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Store(WTree tree, BinaryWriter writer)
            {
                writer.Write(Count);
                if (Count == 0)
                    return;

                //write cache
                foreach (var kv in this)
                {
                    var locator = kv.Key;
                    var operations = kv.Value;

                    //write locator
                    tree.SerializeLocator(writer, locator);

                    //write operations
                    locator.OperationsPersist.Write(writer, operations);
                }
            }

            public void Load(WTree tree, BinaryReader reader)
            {
                int count = reader.ReadInt32();
                if (count == 0)
                    return;

                for (int i = 0; i < count; i++)
                {
                    //read locator
                    var locator = tree.DeserializeLocator(reader);

                    //read operations
                    var operations = locator.OperationsPersist.Read(reader);

                    Add(locator, operations);
                }
            }

            private void Add(Locator locator, IOperationCollection operations)
            {
                if (Count > 0)
                {
                    if (cache == null)
                    {
                        cache = new Dictionary<Locator, IOperationCollection>();
                        cache[Operations.Locator] = Operations;
                    }

                    cache.Add(locator, operations);
                }

                Operations = operations;

                OperationCount += operations.Count;
                Count++;
            }
        }
    }
}
