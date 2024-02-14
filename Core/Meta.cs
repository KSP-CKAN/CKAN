using System;
using System.Linq;
using System.Reflection;

using CKAN.Versioning;

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

        public static readonly ModuleVersion ReleaseVersion = new ModuleVersion(GetVersion());

        public static string GetVersion(VersionFormat format = VersionFormat.Normal)
        {
            var version = Assembly
                .GetExecutingAssembly()
                .GetAssemblyAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;

            switch (format)
            {
                case VersionFormat.Normal:
                    return "v" + Assembly.GetExecutingAssembly()
                                         .GetAssemblyAttribute<AssemblyFileVersionAttribute>()
                                         .Version;
                case VersionFormat.Full:
                    return $"v{version}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }

        private static T GetAssemblyAttribute<T>(this Assembly assembly)
            => (T)assembly.GetCustomAttributes(typeof(T), false)
                          .First();
    }
}
