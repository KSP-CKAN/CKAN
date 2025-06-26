#if !NET8_0_OR_GREATER
using System.Collections.Generic;

namespace System.Linq
{
    public static class LinqExtensions
    {
        #if NET45 || NETSTANDARD2_0

        #if NET45
        public
        #elif NETSTANDARD2_0
        internal
        #endif
        static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
            => new HashSet<T>(source);

        #if NET45
        public
        #elif NETSTANDARD2_0
        internal
        #endif
        static HashSet<T> ToHashSet<T>(this IEnumerable<T>  source,
                                       IEqualityComparer<T> comparer)
            => new HashSet<T>(source, comparer);

        #endif

        #if NET45

        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T next)
            => source.Concat(Enumerable.Repeat<T>(next, 1));

        #endif

        #if !NET8_0_OR_GREATER

        public static Dictionary<K, V> ToDictionary<K, V>(this IEnumerable<KeyValuePair<K, V>> pairs) where K: class
            => pairs.ToDictionary(kvp => kvp.Key,
                                  kvp => kvp.Value);

        public static Dictionary<K, V> ToDictionary<K, V>(this IEnumerable<KeyValuePair<K, V>> pairs,
                                                          IEqualityComparer<K>                 comparer)
            where K: class
            => pairs.ToDictionary(kvp => kvp.Key,
                                  kvp => kvp.Value,
                                  comparer);

        public static Dictionary<K, V> ToDictionary<K, V>(this ParallelQuery<KeyValuePair<K, V>> pairs) where K: class
            => pairs.ToDictionary(kvp => kvp.Key,
                                  kvp => kvp.Value);

        #endif

        #if NETFRAMEWORK || NETSTANDARD2_0

        /// <summary>
        /// Eliminate duplicate elements based on the value returned by a callback
        /// </summary>
        /// <param name="seq">Sequence of elements to check</param>
        /// <param name="func">Function to return unique value per element</param>
        /// <returns>Sequence where each element has a unique return value</returns>
        public static IEnumerable<T> DistinctBy<T, U>(this IEnumerable<T> seq, Func<T, U> func)
            => seq.GroupBy(func).Select(grp => grp.First());

        /// <summary>
        /// Make pairs out of the elements of two sequences
        /// </summary>
        /// <param name="seq1">The first sequence</param>
        /// <param name="seq2">The second sequence</param>
        /// <returns>Sequence of pairs of one element from seq1 and one from seq2</returns>
        public static IEnumerable<(T1 First, T2 Second)> Zip<T1, T2>(this IEnumerable<T1> seq1, IEnumerable<T2> seq2)
            => seq1.Zip(seq2, (First, Second) => (First, Second));

        #endif
    }
}

#endif

#if NETFRAMEWORK || NETSTANDARD2_0

namespace System.Collections.Generic
{
    public static class KeyValuePairDeconstructExtensions
    {
        /// <summary>
        /// Enable a `foreach` over a sequence of key value pairs
        /// </summary>
        /// <param name="kvp">A pair to deconstruct</param>
        /// <param name="key">Set to the key from the pair</param>
        /// <param name="val">Set to the value from the pair</param>
        public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> kvp,
                                               out  T1                   key,
                                               out  T2                   val)
        {
            key = kvp.Key;
            val = kvp.Value;
        }
    }
}

#endif

#if NET45

namespace System
{
    public static class TupleDeconstructExtensions
    {
        /// <summary>
        /// Enable a `foreach` over a sequence of tuples
        /// </summary>
        /// <param name="tuple">A tuple to deconstruct</param>
        /// <param name="item1">Set to the first item from the tuple</param>
        /// <param name="item2">Set to the second item from the tuple</param>
        public static void Deconstruct<T1, T2>(this Tuple<T1, T2> tuple,
                                               out  T1            item1,
                                               out  T2            item2)
        {
            item1 = tuple.Item1;
            item2 = tuple.Item2;
        }

        /// <summary>
        /// Enable a `foreach` over a sequence of tuples
        /// </summary>
        /// <param name="tuple">A tuple to deconstruct</param>
        /// <param name="item1">Set to the first item from the tuple</param>
        /// <param name="item2">Set to the second item from the tuple</param>
        /// <param name="item3">Set to the third item from the tuple</param>
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
        /// <param name="item1">Set to the first item from the tuple</param>
        /// <param name="item2">Set to the second item from the tuple</param>
        /// <param name="item3">Set to the third item from the tuple</param>
        /// <param name="item4">Set to the fourth item from the tuple</param>
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
    }
}

#endif
