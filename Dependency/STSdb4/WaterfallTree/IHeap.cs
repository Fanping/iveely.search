using Iveely.STSdb4.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iveely.STSdb4.WaterfallTree
{
    /// <summary>
    /// Gives opportunity to write and read blocks referenced by logical keys (handles). The heap implementations must provide atomic commit of all writes (all or nothing) and must be thread-safe.
    /// The heap implementation can rely on the fact that the majority of the created by the engine blocks are with relatively large size (> 2MB).
    /// </summary>
    public interface IHeap
    {
        /// <summary>
        /// Register new handle. The returned handle must be always unique.
        /// </summary>
        long ObtainNewHandle();

        /// <summary>
        /// Release the allocated space behind the handle.
        /// </summary>
        void Release(long handle);

        /// <summary>
        /// Is there such handle in the heap
        /// </summary>
        bool Exists(long handle);

        /// <summary>
        /// Write data with the specified handle
        /// </summary>
        void Write(long handle, byte[] buffer, int index, int count);

        /// <summary>
        /// Read the current data behind the handle
        /// </summary>
        byte[] Read(long handle);

        /// <summary>
        /// Atomic commit ALL changes in the heap (all or nothing).
        /// </summary>
        void Commit();

        /// <summary>
        /// Close the heap and release any resources
        /// </summary>
        void Close();

        /// <summary>
        /// Small user data (usually less than one physical sector), atomic written with the Commit()
        /// </summary>
        byte[] Tag { get; set; }

        /// <summary>
        /// Total size in bytes of the user data
        /// </summary>
        long DataSize { get; }

        /// <summary>
        /// Total size in bytes of the heap.
        /// </summary>
        long Size { get; }
    }
}
