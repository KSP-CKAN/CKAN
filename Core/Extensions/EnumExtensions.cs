using System;
using System.Linq;

namespace CKAN.Extensions
{
    public static class EnumExtensions
    {

        public static bool HasAnyFlag(this Enum val, params Enum[] flags)
            => flags.Any(f => val.HasFlag(f));

    }
}
