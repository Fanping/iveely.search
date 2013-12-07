using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NDatabase.Api.Query
{
    /// <summary>
    /// IOrderedQueryable&lt;T&gt; derived interface, Linq
    /// </summary>
    public interface ILinqQueryable<out TElement> : IOrderedQueryable<TElement>, ILinqQueryable
    {
    }

    /// <summary>
    /// IOrderedQueryable derived interface, Linq
    /// </summary>
    public interface ILinqQueryable : IOrderedQueryable
    {
        /// <summary>
        /// Get underliying linq query
        /// </summary>
        /// <returns>Linq query</returns>
        ILinqQuery GetQuery();
    }

    /// <summary>
    /// NDatabase Linq Query generic interface
    /// </summary>
    public interface ILinqQuery<out T> : ILinqQuery, IEnumerable<T>
    {
    }

    /// <summary>
    /// NDatabase Linq Query interface
    /// </summary>
    public interface ILinqQuery : IEnumerable
    {
    }
}