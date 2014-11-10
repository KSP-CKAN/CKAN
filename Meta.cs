using System.Reflection;
using System.Text.RegularExpressions;

namespace CKAN
{
    public static class Meta
    {
        public readonly static string Development = "development";

        /// <summary>
        /// Returns the version of the CKAN.dll used, complete with git info
        /// filled in by our build system. Eg: v1.3.5-12-g055d7c3
        /// </summary>
        public static string Version()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            // SeriouslyLongestClassNamesEverThanksMicrosoft
            var attr =
                (AssemblyInformationalVersionAttribute[])
                    assembly.GetCustomAttributes(typeof (AssemblyInformationalVersionAttribute), false);

            if (attr.Length == 0 || attr[0].InformationalVersion == null)
            {
                // Dunno the version. Some dev probably built it. 
                return Development;
            }
            return attr[0].InformationalVersion;
        }

        /// <summary>
        /// Returns just our release number (eg: 1.0.3), or null for a dev build.
        /// </summary>
        public static Version ReleaseNumber()
        {
            string long_version = Version();

            if (long_version == Development)
            {
                return null;
            }

            string short_version = Regex.Match(long_version, @"^(.*)-\d+-.*$").Result("$1");

            return new CKAN.Version(short_version);
        }
    }
}