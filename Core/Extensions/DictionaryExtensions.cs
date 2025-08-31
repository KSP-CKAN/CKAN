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
    }
}
