using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iveely.General.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> Merge<T>(this IEnumerable<T> primary, IEnumerable<T> secondary, IComparer<T> comparer)
        {
            //return Merge(primary, secondary, (p, s) => { return new KeyValuePair<bool, T>(true, p); }, comparer);
            
            var enumerator1 = primary.GetEnumerator();
            var enumerator2 = secondary.GetEnumerator();

            bool haveNext1 = enumerator1.MoveNext();
            bool haveNext2 = enumerator2.MoveNext();

            if (haveNext1 && haveNext2)
            {
                var item1 = enumerator1.Current;
                var item2 = enumerator2.Current;

                while (true)
                {
                    int cmp = comparer.Compare(item1, item2);
                    if (cmp < 0)
                    {
                        yield return item1;

                        haveNext1 = enumerator1.MoveNext();
                        if (!haveNext1)
                            break;

                        item1 = enumerator1.Current;
                    }
                    else if (cmp > 0)
                    {
                        yield return item2;

                        haveNext2 = enumerator2.MoveNext();
                        if (!haveNext2)
                            break;

                        item2 = enumerator2.Current;
                    }
                    else
                    {
                        yield return item1;

                        haveNext1 = enumerator1.MoveNext();
                        haveNext2 = enumerator2.MoveNext();
                        if (!haveNext1 || !haveNext2)
                            break;

                        item1 = enumerator1.Current;
                        item2 = enumerator2.Current;
                    }
                }
            }

            while (haveNext1)
            {
                yield return enumerator1.Current;
                haveNext1 = enumerator1.MoveNext();
            }

            while (haveNext2)
            {
                yield return enumerator2.Current;
                haveNext2 = enumerator2.MoveNext();
            }
        }

        public static IEnumerable<T> Merge<T>(this IEnumerable<T> primary, IEnumerable<T> secondary)
        {
            return Merge<T>(primary, secondary, Comparer<T>.Default);
        }

        public static IEnumerable<T> Merge<T>(this IEnumerable<T> collection1, IEnumerable<T> collection2, Func<T, T, KeyValuePair<bool, T>> onConflict, IComparer<T> comparer)
        {
            var enumerator1 = collection1.GetEnumerator();
            var enumerator2 = collection2.GetEnumerator();

            bool haveNext1 = enumerator1.MoveNext();
            bool haveNext2 = enumerator2.MoveNext();

            if (haveNext1 && haveNext2)
            {
                var item1 = enumerator1.Current;
                var item2 = enumerator2.Current;

                while (true)
                {
                    int cmp = comparer.Compare(item1, item2);
                    if (cmp < 0)
                    {
                        yield return item1;

                        haveNext1 = enumerator1.MoveNext();
                        if (!haveNext1)
                            break;

                        item1 = enumerator1.Current;
                    }
                    else if (cmp > 0)
                    {
                        yield return item2;

                        haveNext2 = enumerator2.MoveNext();
                        if (!haveNext2)
                            break;

                        item2 = enumerator2.Current;
                    }
                    else
                    {
                        var kv = onConflict(item1, item2);
                        if (kv.Key)
                            yield return kv.Value;

                        haveNext1 = enumerator1.MoveNext();
                        haveNext2 = enumerator2.MoveNext();
                        if (!haveNext1 || !haveNext2)
                            break;

                        item1 = enumerator1.Current;
                        item2 = enumerator2.Current;
                    }
                }
            }

            while (haveNext1)
            {
                yield return enumerator1.Current;
                haveNext1 = enumerator1.MoveNext();
            }

            while (haveNext2)
            {
                yield return enumerator2.Current;
                haveNext2 = enumerator2.MoveNext();
            }
        }

        public static IEnumerable<T> Merge<T>(this IEnumerable<T> collection1, IEnumerable<T> collection2, Func<T, T, KeyValuePair<bool, T>> onConflict)
        {
            return Merge(collection1, collection2, onConflict, Comparer<T>.Default);
        }

        public static IEnumerable<T> Apply<T>(this IEnumerable<T> collection, Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            foreach (var item in collection)
            {
                action(item);

                yield return item;
            }
        }

        public static bool IsOrdered<T>(this IEnumerable<T> collection, IComparer<T> comparer, bool strictMonotone)
        {
            var enumerator = collection.GetEnumerator();
            if (!enumerator.MoveNext())
                return true;

            int limit = strictMonotone ? 0 : -1;
            var item = enumerator.Current;

            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                if (comparer.Compare(item, current) > limit)
                    return false;

                item = current;
            }

            return true;
        }

        public static bool IsOrdered<T>(this IEnumerable<T> collection, bool strictMonotone = false)
        {
            return collection.IsOrdered(Comparer<T>.Default, strictMonotone);
        }
    }
}
