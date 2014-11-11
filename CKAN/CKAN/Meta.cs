using System.Text.RegularExpressions;

namespace CKAN
{
    public static class Meta
    {
        public readonly static string Development = "development";

        // Do *not* change the following line, BUILD_VERSION is
        // replaced by our build system with our actual version.

        private readonly static string BUILD_VERSION = null;

        /// <summary>
        /// Returns the version of the CKAN.dll used, complete with git info
        /// filled in by our build system. Eg: v1.3.5-12-g055d7c3
        /// </summary>
        public static string Version()
        {

            if (BUILD_VERSION == null)
            {
                // Dunno the version. Some dev probably built it. 
                return Development;
            }

            return BUILD_VERSION;
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