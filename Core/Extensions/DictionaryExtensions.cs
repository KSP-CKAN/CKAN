using System.Linq;
using System.Collections.Generic;

namespace CKAN.Extensions
{
    public static class DictionaryExtensions
    {
        public static bool DictionaryEquals<K, V>(this IDictionary<K, V> a,
                                                  IDictionary<K, V> b)
            => a == null ? b == null
                         : b != null && a.Count == b.Count
                           && a.Keys.All(k => b.ContainsKey(k))
                           && b.Keys.All(k => a.ContainsKey(k)
                                              && EqualityComparer<V>.Default.Equals(a[k], b[k]));

        public static V GetOrDefault<K, V>(this Dictionary<K, V> dict, K key)
        {
            V val = default;
            if (key != null)
            {
                dict.TryGetValue(key, out val);
            }
            return val;
        }

    }
}
