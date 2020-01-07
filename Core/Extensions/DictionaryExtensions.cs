using System;
using System.Collections.Generic;

namespace CKAN.Extensions
{
    public static class DictionaryExtensions
    {

        public static V GetOrDefault<K, V>(this Dictionary<K, V> dict, K key)
        {
            V val = default(V);
            if (key != null)
            {
                dict.TryGetValue(key, out val);
            }
            return val;
        }

    }
}
