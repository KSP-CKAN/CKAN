using System;
using System.Linq;
using System.Reflection;

namespace CKAN
{
    public static class Meta
    {
        /// <summary>
        /// Programmatically generate the string "CKAN" from the assembly info attributes,
        /// so we don't have to embed that string in many places
        /// </summary>
        /// <returns>"CKAN"</returns>
        public static string GetProductName()
            => Assembly.GetExecutingAssembly()
                       .GetAssemblyAttribute<AssemblyProductAttribute>()
                       .Product;

        public static string GetVersion(VersionFormat format = VersionFormat.Normal)
        {
            var version = Assembly
                .GetExecutingAssembly()
                .GetAssemblyAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;

            switch (format)
            {
                case VersionFormat.Short:
                    return $"v{version.UpToCharacters(shortDelimiters)}";
                case VersionFormat.Normal:
                    return $"v{version.UpToCharacter('+')}";
                case VersionFormat.Full:
                    return $"v{version}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }

        private static readonly char[] shortDelimiters = new char[] { '-', '+' };

        private static string UpToCharacter(this string orig, char what)
            => orig.UpToIndex(orig.IndexOf(what));

        private static string UpToCharacters(this string orig, char[] what)
            => orig.UpToIndex(orig.IndexOfAny(what));

        private static string UpToIndex(this string orig, int index)
            => index == -1 ? orig
                           : orig.Substring(0, index);

        private static T GetAssemblyAttribute<T>(this Assembly assembly)
            => (T)assembly.GetCustomAttributes(typeof(T), false)
                          .First();
    }
}
