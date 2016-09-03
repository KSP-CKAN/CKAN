using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CKAN
{
    public static class Extensions
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }
    }
}
