using System;
using System.Linq;
using System.Collections.Generic;

namespace CKAN.Extensions
{
    public static class DictionaryExtensions
    {
        public static bool DictionaryEquals<K, V>(this IDictionary<K, V>? a,
                                                  IDictionary<K, V>?      b)
            => a == null ? b == null
                         : b != null && a.Count == b.Count
                           && a.Keys.All(b.ContainsKey)
                           && b.Keys.All(k => a.ContainsKey(k)
                                              && EqualityComparer<V>.Default.Equals(a[k], b[k]));

        public static IEnumerable<Tuple<K, V1, V2>> KeyZip<K, V1, V2>(this IDictionary<K, V1> source,
                                                                      IDictionary<K, V2>      other)
            where K : notnull
            => source.Select(kvp => other.TryGetValue(kvp.Key, out V2? val2)
                                        ? Tuple.Create(kvp.Key, kvp.Value, val2)
                                        : null)
                     .OfType<Tuple<K, V1, V2>>();

    }
}
