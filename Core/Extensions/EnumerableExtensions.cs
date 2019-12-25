using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CKAN.Extensions
{
    public static class EnumerableExtensions
    {
        public static ICollection<T> AsCollection<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source is ICollection<T> collection ? collection : source.ToArray();
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new HashSet<T>(source);
        }

        public static IEnumerable<T> Memoize<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            else
            {
                return new Memoized<T>(source);
            }
        }
    }

    /// <summary>
    /// Memoized lazy evaluation in C#!
    /// From https://stackoverflow.com/a/12428250/2422988
    /// </summary>
    public class Memoized<T> : IEnumerable<T>
    {
        public Memoized(IEnumerable<T> source)
        {
            this.source = source;
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (gate)
            {
                if (isCacheComplete)
                {
                    return cache.GetEnumerator();
                }
                else if (enumerator == null)
                {
                    enumerator = source.GetEnumerator();
                }
            }
            return GetMemoizingEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerator<T> GetMemoizingEnumerator()
        {
            for (Int32 index = 0; TryGetItem(index, out T item); ++index)
            {
                yield return item;
            }
        }

        private bool TryGetItem(Int32 index, out T item)
        {
            lock (gate)
            {
                if (!IsItemInCache(index))
                {
                    // The iteration may have completed while waiting for the lock
                    if (isCacheComplete)
                    {
                        item = default(T);
                        return false;
                    }
                    if (!enumerator.MoveNext())
                    {
                        item = default(T);
                        isCacheComplete = true;
                        enumerator.Dispose();
                        return false;
                    }
                    cache.Add(enumerator.Current);
                }
                item = cache[index];
                return true;
            }
        }

        private bool IsItemInCache(Int32 index)
        {
            return index < cache.Count;
        }

        private readonly IEnumerable<T> source;
        private          IEnumerator<T> enumerator;
        private readonly List<T>        cache = new List<T>();
        private          bool           isCacheComplete;
        private readonly object         gate = new object();
    }
}
