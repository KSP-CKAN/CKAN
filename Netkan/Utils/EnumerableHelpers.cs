using System.Collections.Generic;

namespace CKAN.NetKAN.Utils
{
    internal static class EnumerableHelpers
    {
        public static IEnumerable<T> Once<T>(T value)
        {
            return new T[] { value };
        }
    }
}