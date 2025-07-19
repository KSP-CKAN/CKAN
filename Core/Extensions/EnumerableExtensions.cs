using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace CKAN.Extensions
{
    public static class EnumerableExtensions
    {
        public static ConcurrentDictionary<K, V> ToConcurrentDictionary<K, V>(this IEnumerable<KeyValuePair<K, V>> pairs) where K: class
            => new ConcurrentDictionary<K, V>(pairs);

        public static IEnumerable<T> AsParallelIf<T>(this IEnumerable<T> source,
                                                     bool                parallel)
            => parallel ? source.AsParallel()
                        : source;

        // https://stackoverflow.com/a/55591477/2422988
        public static ParallelQuery<T> WithProgress<T>(this ParallelQuery<T> source,
                                                       long                  totalCount,
                                                       IProgress<int>?       progress)
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
            => source is Memoized<T>
                   // Already memoized, don't wrap another layer
                   ? source
                   : new Memoized<T>(source);

        public static void RemoveWhere<K, V>(this Dictionary<K, V> source,
                                             Func<KeyValuePair<K, V>, bool> predicate) where K: class
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

        /// <summary>
        /// Zip a sequence with a sequence generated from the first sequence using the given function
        /// </summary>
        /// <typeparam name="T">Source sequence type</typeparam>
        /// <typeparam name="V">Type of elements returned by func</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="func">Function to generate values of second sequence given source</param>
        /// <returns>Sequence of tuples containing pairs from each sequence</returns>
        public static IEnumerable<(T First, V Second)> ZipBy<T, V>(this IEnumerable<T> source, Func<IEnumerable<T>, IEnumerable<V>> func)
            => source.ToArray() is T[] array
                   ? array.Zip(func(array))
                   : Enumerable.Empty<(T First, V Second)>();

        /// <summary>
        /// Insert new elements between consecutive pairs of existing elements,
        /// preserving the original elements in order, and using null to
        /// represent the elements before the beginning and after the end.
        /// </summary>
        /// <param name="source">Sequence into which to inject</param>
        /// <param name="inBetween">Function to generate the new elements</param>
        /// <returns>Sequence with new elements in it</returns>
        public static IEnumerable<T> Inject<T>(this IEnumerable<T> source,
                                               Func<T?, T?, T>     inBetween)
            where T : class
        {
            using (var e = source.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    yield return inBetween(null, e.Current);
                    yield return e.Current;
                    var prev = e.Current;
                    while (e.MoveNext())
                    {
                        yield return inBetween(prev, e.Current);
                        yield return e.Current;
                        prev = e.Current;
                    }
                    yield return inBetween(prev, null);
                }
                else
                {
                    yield return inBetween(null, null);
                }
            }
        }

        /// <summary>
        /// Poor man's PLINQ, a trivially parallelized SelectMany that
        /// runs one process per item in the source sequence.
        /// For short sequences and long-running functions,
        /// when you don't feel like fighting with Partitioner.Create
        /// over how many items should be in each partition.
        /// </summary>
        /// <param name="source">The sequence to process</param>
        /// <param name="func">The function to apply to each item in the sequence</param>
        /// <returns>Sequence of all values from the function</returns>
        public static IEnumerable<V> SelectManyTasks<T, V>(this ICollection<T>     source,
                                                           Func<T, IEnumerable<V>> func)
        {
            if (source.Count <= 1)
            {
                return source.SelectMany(func);
            }
            else
            {
                var tasks = source.Select(item => Task.Run(() => func(item).ToArray()))
                                  // Force non-lazy creation of tasks
                                  .ToArray();
                return Utilities.WithRethrowInner(() =>
                {
                    // Without this, later tasks don't finish if an earlier one throws
                    Task.WaitAll(tasks);
                    return tasks.SelectMany(task => task.Result);
                });
            }
        }

        /// <summary>
        /// Generate a sequence from a linked list
        /// </summary>
        /// <param name="start">The first node</param>
        /// <param name="getNext">Function to go from one node to the next</param>
        /// <returns>All the nodes in the list as a sequence</returns>
        public static IEnumerable<T> TraverseNodes<T>(this T start, Func<T, T?> getNext)
            where T : class
        {
            for (T? t = start; t != null; t = Utilities.DefaultIfThrows(() => getNext(t)))
            {
                yield return t;
            }
        }

        /// <summary>
        /// Try matching a regex against a series of strings and return the Match objects
        /// </summary>
        /// <param name="source">Sequence of strings to scan</param>
        /// <param name="pattern">Pattern to match</param>
        /// <returns>Sequence of Match objects</returns>
        public static IEnumerable<Match> WithMatches(this IEnumerable<string> source, Regex pattern)
            => source.Select(val => pattern.TryMatch(val, out Match? match) ? match : null)
                     .OfType<Match>();

        /// <summary>
        /// Apply a function to a sequence and handle any exceptions that are thrown
        /// </summary>
        /// <typeparam name="TSrc">Type of source sequence</typeparam>
        /// <typeparam name="TDest">Type of destination sequence</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="func">Function to apply to each item</param>
        /// <param name="onThrow">Function to call if there's an exception</param>
        /// <returns>Sequence of return values of given function</returns>
        public static IEnumerable<TDest?> SelectWithCatch<TSrc, TDest>(this IEnumerable<TSrc>       source,
                                                                       Func<TSrc, TDest>             func,
                                                                       Func<TSrc, Exception, TDest?> onThrow)
                where TDest : class
            => source.Select(item => Utilities.DefaultIfThrows(()  => func(item),
                                                               exc => onThrow(item, exc)));

        /// <summary>
        /// Apply a sequence-generating function to a sequence and combine the subsequences,
        /// skipping elements in the input sequence that throw exceptions.
        /// </summary>
        /// <param name="source">The sequence to process</param>
        /// <param name="func">The function that generates more subsequences</param>
        /// <returns>Sequence of values</returns>
        public static IEnumerable<V> SelectManyWithCatch<T, V>(this IEnumerable<T>     source,
                                                               Func<T, IEnumerable<V>> func)
            => source.SelectMany(elt => Utilities.DefaultIfThrows(() => func(elt))
                                        ?? Enumerable.Empty<V>());

        /// <summary>
        /// Get a hash code for a sequence with a variable number of elements
        /// </summary>
        /// <typeparam name="T">Type of the elements in the sequence</typeparam>
        /// <param name="source">The sequence</param>
        /// <returns></returns>
        public static int ToSequenceHashCode<T>(this IEnumerable<T> source)
            => source.Aggregate(new HashCode(),
                                (hc, item) =>
                                {
                                    hc.Add(item);
                                    return hc;
                                },
                                hc => hc.ToHashCode());

        /// <summary>
        /// Accumulate a sequence of values based on a seed value and a function,
        /// similar to Aggregate but including intermediate values
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source">Input sequence</param>
        /// <param name="seed">First intermediate value, not included in return sequence</param>
        /// <param name="func">Function to transform a previous result and a next input sequence element into the next result</param>
        /// <returns></returns>
        public static IEnumerable<TResult> Accumulate<TSource, TResult>(this IEnumerable<TSource>       source,
                                                                        TResult                         seed,
                                                                        Func<TResult, TSource, TResult> func)
        {
            var result = seed;
            foreach (var item in source)
            {
                result = func(result, item);
                yield return result;


            }
        }

        public static IEnumerable<string> ExceptContainsAny(this   IEnumerable<string> source,
                                                            params string[]            strings)
            => source.Where(elt => !strings.Any(s => elt.Contains(s)));
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

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public IEnumerator<T> GetEnumerator()
        {
            lock (gate)
            {
                if (isCacheComplete)
                {
                    return cache.GetEnumerator();
                }
                else
                {
                    enumerator ??= source.GetEnumerator();
                }
            }
            return GetMemoizingEnumerator();
        }

        private IEnumerator<T> GetMemoizingEnumerator()
        {
            for (int index = 0; TryGetItem(index, out T? item); ++index)
            {
                yield return item;
            }
        }

        private bool TryGetItem(int index,
                                out T item)
        {
            lock (gate)
            {
                if (enumerator is not null && !IsItemInCache(index))
                {
                    // The iteration may have completed while waiting for the lock
                    #nullable disable
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
                    #nullable enable
                    cache.Add(enumerator.Current);
                }
                item = cache[index];
                return true;
            }
        }

        private bool IsItemInCache(int index)
            => index < cache.Count;

        private readonly IEnumerable<T>  source;
        private          IEnumerator<T>? enumerator;
        private readonly List<T>         cache = new List<T>();
        private          bool            isCacheComplete;
        private readonly object          gate = new object();
    }
}
