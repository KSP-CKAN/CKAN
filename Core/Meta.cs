using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using CKAN.Extensions;
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
        public static readonly string ProductName =
            Assembly.GetExecutingAssembly()
                    .GetAssemblyAttribute<AssemblyProductAttribute>()
                    .Product;

        public static readonly ModuleVersion ReleaseVersion = new ModuleVersion(GetVersion());

        public static readonly ModuleVersion SpecVersion =
            // A dev build always has an odd patch version, and
            // an odd number always ends with an odd digit in base 10
            new Regex(@"^(?<prefix>v\d+\.)(?<minor>\d+)\.\d*[13579]\.")
                .TryMatch(ReleaseVersion.ToString(), out Match? m)
                && int.TryParse(m.Groups["minor"].Value, out int minor)
                ? new ModuleVersion($"{m.Groups["prefix"]}{minor + 1}.999")
                : ReleaseVersion;

        public static readonly bool IsNetKAN =
            Assembly.GetExecutingAssembly()
                    .GetAssemblyAttribute<AssemblyTitleAttribute>()
                    .Title.Contains("NetKAN");

        public static string GetVersion(VersionFormat format = VersionFormat.Normal)
            => "v" + (format switch
            {
                VersionFormat.Full =>
                    Assembly.GetExecutingAssembly()
                            .GetAssemblyAttribute<AssemblyInformationalVersionAttribute>()
                            .InformationalVersion,

                VersionFormat.Normal or _ =>
                    Assembly.GetExecutingAssembly()
                            .GetAssemblyAttribute<AssemblyFileVersionAttribute>()
                            .Version,
            });

        private static T GetAssemblyAttribute<T>(this Assembly assembly)
            => (T)assembly.GetCustomAttributes(typeof(T), false)
                          .First();
    }
}
