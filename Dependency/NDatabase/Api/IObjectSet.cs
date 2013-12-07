using System.Collections.Generic;

namespace NDatabase.Api
{
    /// <summary>
    /// query resultset.
    /// 
    /// The <code>ObjectSet</code> interface serves as a cursor to
    /// iterate through a set of objects retrieved by a query.
    /// </summary>
    public interface IObjectSet<TItem> : ICollection<TItem>
    {
        /// <summary>
        /// returns <code>true</code> if the <code>ObjectSet</code> has more elements.
        /// </summary>
        /// <returns><code>true</code> if the <code>ObjectSet</code> has more elements</returns>
        bool HasNext();

        /// <summary>
        /// returns the next object in the <code>ObjectSet</code>.
        /// </summary>
        /// <returns>the next object in the <code>ObjectSet</code>.</returns>C
        TItem Next();

        /// <summary>
        ///   Return the first object of the collection, if exist
        /// </summary>
        /// <returns> </returns>
        TItem GetFirst();

        /// <summary>
        /// resets the <code>ObjectSet</code> cursor before the first element. 
        /// 
        /// A subsequent call to <code>next()</code> will return the first element.
        /// </summary>
        void Reset();
    }
}
