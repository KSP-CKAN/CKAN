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

            var dashIndex = version.IndexOf('-');
            var plusIndex = version.IndexOf('+');

            switch (format)
            {
                case VersionFormat.Short:
                    if (dashIndex >= 0)
                        version = version.Substring(0, dashIndex);
                    else if (plusIndex >= 0)
                        version = version.Substring(0, plusIndex);

                    break;
                case VersionFormat.Normal:
                    if (plusIndex >= 0)
                        version = version.Substring(0, plusIndex);

                    break;
                case VersionFormat.Full:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }

            return "v" + version;
        }

        private static T GetAssemblyAttribute<T>(this Assembly assembly)
            => (T)assembly.GetCustomAttributes(typeof(T), false)
                          .First();
    }
}
