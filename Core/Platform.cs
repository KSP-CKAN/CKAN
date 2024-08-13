using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
#if NET6_0_OR_GREATER
using System.Runtime.Versioning;
#endif
using System.Text.RegularExpressions;
using System.Security.Principal;

using CKAN.Extensions;

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
        /// Are we on a flavour of Windows?
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
        /// Are we running in an X11 environment?
        /// </summary>
        public static readonly bool IsX11 =
            IsUnix
            && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY"));

        public static readonly StringComparer PathComparer =
            IsWindows ? StringComparer.OrdinalIgnoreCase
                      : StringComparer.Ordinal;

        public static readonly StringComparison PathComparison =
            IsWindows ? StringComparison.OrdinalIgnoreCase
                      : StringComparison.Ordinal;

        public static string FormatPath(string p)
            => p.Replace('/', Path.DirectorySeparatorChar);

        public static bool IsAdministrator()
        {
            if (File.Exists("/.dockerenv"))
            {
                // Treat as non-admin in a docker container, regardless of platform
                return false;
            }
            if (IsWindows)
            {
                // On Windows, check if we have administrator or system roles
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator)
                        || principal.IsInRole(WindowsBuiltInRole.SystemOperator);
                }
            }
            // Otherwise Unix-like; are we root?
            return getuid() == 0;
        }

        [DllImport("libc")]
        private static extern uint getuid();

        private static readonly Regex versionMatcher =
            new Regex("^\\s*(?<major>\\d+)\\.(?<minor>\\d+)\\.(?<patch>\\d+)\\s*\\(",
                      RegexOptions.Compiled);

        public static readonly Version MonoVersion
            = versionMatcher.TryMatch((string)Type.GetType("Mono.Runtime")
                                                  ?.GetMethod("GetDisplayName",
                                                              BindingFlags.NonPublic
                                                              | BindingFlags.Static)
                                                  ?.Invoke(null, null),
                                      out Match match)
                ? new Version(int.Parse(match.Groups["major"].Value),
                              int.Parse(match.Groups["minor"].Value),
                              int.Parse(match.Groups["patch"].Value))
                : null;

        public static readonly Version RecommendedMonoVersion = new Version(5, 0, 0);
    }
}
