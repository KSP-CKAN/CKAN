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
        /// and other decorations as filled in by our build system.
        /// Eg: v1.3.5-12-g055d7c3 (beta) or "development (unstable)"
        /// </summary>
        public static string Version()
        {
            string version = BuildVersion();

            #if (STABLE)
            version += " (stable)";
            #else
            version += " (beta)";
            #endif

            return version;
        }

        /// <summary>
        /// Returns only the build info, with no decorations, or "development" if
        /// unknown.
        /// </summary>
        public static string BuildVersion()
        {
            return BUILD_VERSION ?? Development;
        }

        /// <summary>
        /// Returns just our release number (eg: 1.0.3), or null for a dev build.
        /// </summary>
        public static Version ReleaseNumber()
        {
            string build_version = BuildVersion();

            if (build_version == Development)
            {
                return null;
            }

            string short_version = Regex.Match(build_version, @"^(.*)-\d+-.*$").Result("$1");

            return new Version(short_version);
        }

        /// <summary>
        /// Returns true if this is a 'stable' build, false otherwise.
        /// </summary>
        public static bool IsStable()
        {
            #if (STABLE)
            return true;
            #else
            return false;
            #endif
        }
    }
}
