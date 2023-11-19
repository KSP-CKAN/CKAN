using System;
using System.Reflection;
using System.Runtime.InteropServices;
#if NET6_0_OR_GREATER
using System.Runtime.Versioning;
#endif
using System.Text.RegularExpressions;

namespace CKAN
{
    /// <summary>
    /// Platform class to detect if we're running on Linux, Windows, or Mac.
    /// Any code for checking for libraries being present also goes here.
    ///
    /// This uses the modern technique of checking PlatformID enums, rather than
    /// magic numbers like we used to.
    /// </summary>
    public static class Platform
    {
        static Platform()
        {
            // This call throws if we try to do it as a static initializer.
            IsMonoFourOrLater = IsOnMonoFourOrLater();
        }

        /// <summary>
        /// Are we on a Mac?
        /// </summary>
        /// <value><c>true</c> if is mac; otherwise, <c>false</c>.</value>
        #if NET6_0_OR_GREATER
        [SupportedOSPlatformGuard("macos")]
        #endif
        public static readonly bool IsMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        /// <summary>
        /// Are we on a Unix (including Linux, but *not* Mac) system.
        /// </summary>
        /// <value><c>true</c> if is unix; otherwise, <c>false</c>.</value>
        #if NET6_0_OR_GREATER
        [SupportedOSPlatformGuard("linux")]
        #endif
        public static readonly bool IsUnix = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        /// <summary>
        /// Are we on a flavour of Windows? This is implemented internally by checking
        /// if we're not on Unix or Mac.
        /// </summary>
        /// <value><c>true</c> if is windows; otherwise, <c>false</c>.</value>
        #if NET6_0_OR_GREATER
        [SupportedOSPlatformGuard("windows")]
        #endif
        public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        /// Are we on Mono?
        /// </summary>
        public static readonly bool IsMono = Type.GetType("Mono.Runtime") != null;

        /// <summary>
        /// Are we running on a Mono with major version 4 or later?
        /// </summary>
        public static readonly bool IsMonoFourOrLater;

        /// <summary>
        /// Are we running in an X11 environment?
        /// </summary>
        public static readonly bool IsX11 =
            IsUnix
            && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY"));

        private static bool IsOnMonoFourOrLater()
        {
            if (!IsMono)
            {
                return false;
            }

            // Get Mono's display name and parse the version
            string display_name =
                (string)Type.GetType("Mono.Runtime")
                            .GetMethod("GetDisplayName",
                                       BindingFlags.NonPublic | BindingFlags.Static)
                            .Invoke(null, null);

            var match = versionMatcher.Match(display_name);
            return match.Success
                   && int.Parse(match.Groups["majorVersion"].Value) >= 4;
        }

        private static readonly Regex versionMatcher =
            new Regex("^\\s*(?<majorVersion>\\d+)\\.\\d+\\.\\d+\\s*\\(",
                      RegexOptions.Compiled);
    }
}
