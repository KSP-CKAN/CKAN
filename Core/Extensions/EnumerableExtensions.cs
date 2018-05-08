using System;
using System.Collections.Generic;
using System.Linq;

namespace CKAN.Extensions
{
    internal static class EnumerableExtensions
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
    }
}
