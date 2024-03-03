using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Text.RegularExpressions;

namespace CKAN.Extensions
{
    public static class EnumerableExtensions
    {
        public static ICollection<T> AsCollection<T>(this IEnumerable<T> source)
            => source == null
                ? throw new ArgumentNullException(nameof(source))
                : source is ICollection<T> collection ? collection : source.ToArray();

#if NET45 || NETSTANDARD2_0

        #if NET45
        public
        #elif NETSTANDARD2_0
        internal
        #endif
        static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return new HashSet<T>(source);
        }

        #if NET45
        public
        #elif NETSTANDARD2_0
        internal
        #endif
        static HashSet<T> ToHashSet<T>(this IEnumerable<T>  source,
                                              IEqualityComparer<T> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return new HashSet<T>(source, comparer);
        }

#endif

#if NET45

        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T next)
            => source.Concat(Enumerable.Repeat<T>(next, 1));

#endif

        public static Dictionary<K, V> ToDictionary<K, V>(this IEnumerable<KeyValuePair<K, V>> pairs)
            => pairs.ToDictionary(kvp => kvp.Key,
                                  kvp => kvp.Value);

        public static Dictionary<K, V> ToDictionary<K, V>(this ParallelQuery<KeyValuePair<K, V>> pairs)
            => pairs.ToDictionary(kvp => kvp.Key,
                                  kvp => kvp.Value);

        public static ConcurrentDictionary<K, V> ToConcurrentDictionary<K, V>(this IEnumerable<KeyValuePair<K, V>> pairs)
            => new ConcurrentDictionary<K, V>(pairs);

        public static IEnumerable<T> AsParallelIf<T>(this IEnumerable<T> source,
                                                     bool                parallel)
            => parallel ? source.AsParallel()
                        : source;

        // https://stackoverflow.com/a/55591477/2422988
        public static ParallelQuery<T> WithProgress<T>(this ParallelQuery<T> source,
                                                       long                  totalCount,
                                                       IProgress<int>        progress)
        {
            long count       = 0;
            int  prevPercent = -1;
            return progress == null
                ? source
                : source.Select(item =>
                {
                    var percent = (int)(100 * Interlocked.Increment(ref count) / totalCount);
                    if (percent > prevPercent)
                    {
                        progress.Report(percent);
                        prevPercent = percent;
                    }
                    return item;
                });
        }

        public static IEnumerable<T> Memoize<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            else if (source is Memoized<T>)
            {
                // Already memoized, don't wrap another layer
                return source;
            }
            else
            {
                return new Memoized<T>(source);
            }
        }

        public static void RemoveWhere<K, V>(
            this Dictionary<K, V> source,
            Func<KeyValuePair<K, V>, bool> predicate)
        {
            var pairs = source.ToList();
            foreach (var kvp in pairs)
            {
                if (predicate(kvp))
                {
                    source.Remove(kvp.Key);
                }
            }
        }

        /// <summary>
        /// Sum a sequence of TimeSpans.
        /// Mysteriously not defined standardly.
        /// </summary>
        /// <param name="source">Sequence of TimeSpans to sum</param>
        /// <returns>
        /// Sum of the TimeSpans
        /// </returns>
        public static TimeSpan Sum(this IEnumerable<TimeSpan> source)
            => source.Aggregate(TimeSpan.Zero,
                                (a, b) => a + b);

        /// <summary>
        /// Select : SelectMany :: Zip : ZipMany
        /// </summary>
        /// <param name="seq1">Sequence from which to get first values</param>
        /// <param name="seq2">Sequence from which to get second values</param>
        /// <param name="func">Function to transform a value from each input sequence into a sequence of multiple outputs</param>
        /// <returns>Flattened sequence of values from func applies to seq1 and seq2</returns>
        public static IEnumerable<V> ZipMany<T, U, V>(this IEnumerable<T> seq1, IEnumerable<U> seq2, Func<T, U, IEnumerable<V>> func)
            => seq1.Zip(seq2, func).SelectMany(seqs => seqs);

#if NETFRAMEWORK || NETSTANDARD2_0

        /// <summary>
        /// Eliminate duplicate elements based on the value returned by a callback
        /// </summary>
        /// <param name="seq">Sequence of elements to check</param>
        /// <param name="func">Function to return unique value per element</param>
        /// <returns>Sequence where each element has a unique return value</returns>
        public static IEnumerable<T> DistinctBy<T, U>(this IEnumerable<T> seq, Func<T, U> func)
            => seq.GroupBy(func).Select(grp => grp.First());

#endif

        /// <summary>
        /// Generate a sequence from a linked list
        /// </summary>
        /// <param name="start">The first node</param>
        /// <param name="getNext">Function to go from one node to the next</param>
        /// <returns>All the nodes in the list as a sequence</returns>
        public static IEnumerable<T> TraverseNodes<T>(this T start, Func<T, T> getNext)
        {
            for (T t = start; t != null; t = getNext(t))
            {
                yield return t;
            }
        }

#if NETFRAMEWORK || NETSTANDARD2_0

        /// <summary>
        /// Make pairs out of the elements of two sequences
        /// </summary>
        /// <param name="seq1">The first sequence</param>
        /// <param name="seq2">The second sequence</param>
        /// <returns>Sequence of pairs of one element from seq1 and one from seq2</returns>
        public static IEnumerable<Tuple<T1, T2>> Zip<T1, T2>(this IEnumerable<T1> seq1, IEnumerable<T2> seq2)
            => seq1.Zip(seq2, (item1, item2) => new Tuple<T1, T2>(item1, item2));

#endif

#if NET45

        /// <summary>
        /// Enable a `foreach` over a sequence of tuples
        /// </summary>
        /// <param name="tuple">A tuple to deconstruct</param>
        /// <param name="item1">Set to the first value from the tuple</param>
        /// <param name="item2">Set to the second value from the tuple</param>
        public static void Deconstruct<T1, T2>(this Tuple<T1, T2> tuple, out T1 item1, out T2 item2)
        {
            item1 = tuple.Item1;
            item2 = tuple.Item2;
        }

        /// <summary>
        /// Enable a `foreach` over a sequence of tuples
        /// </summary>
        /// <param name="tuple">A tuple to deconstruct</param>
        /// <param name="item1">Set to the first value from the tuple</param>
        /// <param name="item2">Set to the second value from the tuple</param>
        public static void Deconstruct<T1, T2, T3>(this Tuple<T1, T2, T3> tuple,
                                                   out  T1                item1,
                                                   out  T2                item2,
                                                   out  T3                item3)
        {
            item1 = tuple.Item1;
            item2 = tuple.Item2;
            item3 = tuple.Item3;
        }

        /// <summary>
        /// Enable a `foreach` over a sequence of tuples
        /// </summary>
        /// <param name="tuple">A tuple to deconstruct</param>
        /// <param name="item1">Set to the first value from the tuple</param>
        /// <param name="item2">Set to the second value from the tuple</param>
        public static void Deconstruct<T1, T2, T3, T4>(this Tuple<T1, T2, T3, T4> tuple,
                                                       out  T1                    item1,
                                                       out  T2                    item2,
                                                       out  T3                    item3,
                                                       out  T4                    item4)
        {
            item1 = tuple.Item1;
            item2 = tuple.Item2;
            item3 = tuple.Item3;
            item4 = tuple.Item4;
        }

#endif

        /// <summary>
        /// Enable a `foreach` over a sequence of key value pairs
        /// </summary>
        /// <param name="tuple">A tuple to deconstruct</param>
        /// <param name="item1">Set to the first value from the tuple</param>
        /// <param name="item2">Set to the second value from the tuple</param>
        public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> kvp, out T1 key, out T2 val)
        {
            key = kvp.Key;
            val = kvp.Value;
        }

        /// <summary>
        /// Try matching a regex against a series of strings and return the Match objects
        /// </summary>
        /// <param name="source">Sequence of strings to scan</param>
        /// <param name="pattern">Pattern to match</param>
        /// <returns>Sequence of Match objects</returns>
        public static IEnumerable<Match> WithMatches(this IEnumerable<string> source, Regex pattern)
            => source.Select(val => pattern.TryMatch(val, out Match match) ? match : null)
                     .Where(m => m != null);

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
            => GetEnumerator();

        private IEnumerator<T> GetMemoizingEnumerator()
        {
            for (int index = 0; TryGetItem(index, out T item); ++index)
            {
                yield return item;
            }
        }

        private bool TryGetItem(int index, out T item)
        {
            lock (gate)
            {
                if (!IsItemInCache(index))
                {
                    // The iteration may have completed while waiting for the lock
                    if (isCacheComplete)
                    {
                        item = default;
                        return false;
                    }
                    if (!enumerator.MoveNext())
                    {
                        item = default;
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

        private bool IsItemInCache(int index)
            => index < cache.Count;

        private readonly IEnumerable<T> source;
        private          IEnumerator<T> enumerator;
        private readonly List<T>        cache = new List<T>();
        private          bool           isCacheComplete;
        private readonly object         gate = new object();
    }
}
