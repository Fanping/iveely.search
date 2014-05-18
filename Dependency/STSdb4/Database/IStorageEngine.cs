using Iveely.Data;
using Iveely.WaterfallTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Iveely.Database
{
    public interface IStorageEngine : IEnumerable<IDescriptor>, IDisposable
    {
        /// <summary>
        /// Works with anonymous types.
        /// </summary>
        ITable<IData, IData> OpenXTablePortable(string name, DataType keyDataType, DataType recordDataType);

        /// <summary>
        /// Works with portable types via custom transformers.
        /// </summary>
        ITable<TKey, TRecord> OpenXTablePortable<TKey, TRecord>(string name, DataType keyDataType, DataType recordDataType, ITransformer<TKey, IData> keyTransformer, ITransformer<TRecord, IData> recordTransformer);

        /// <summary>
        /// Works with anonymous types via default transformers.
        /// </summary>
        ITable<TKey, TRecord> OpenXTablePortable<TKey, TRecord>(string name);

        /// <summary>
        /// Works with the user types directly.
        /// </summary>
        ITable<TKey, TRecord> OpenXTable<TKey, TRecord>(string name);

        /// <summary>
        /// 
        /// </summary>
        XFile OpenXFile(string name);

        IDescriptor this[string name] { get; }
        IDescriptor Find(long id);

        void Delete(string name);
        void Rename(string name, string newName);
        bool Exists(string name);

        /// <summary>
        /// The number of tables & virtual files into the storage engine.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// The number of nodes that are kept in memory.
        /// </summary>
        int CacheSize { get; set; }

        /// <summary>
        /// Heap assigned to the StorageEngine instance.
        /// </summary>
        IHeap Heap { get; }

        void Commit();
        void Close();
    }
}
