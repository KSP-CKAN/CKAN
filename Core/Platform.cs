﻿using System;
using System.Reflection;
using System.Runtime.InteropServices;
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
        public static readonly bool IsMac = IsRunningOnMac();

        /// <summary>
        /// Are we on a Unix (including Linux, but *not* Mac) system.
        /// Note that Mono thinks Mac is Unix! So we need to negate Mac explicitly.
        /// </summary>
        /// <value><c>true</c> if is unix; otherwise, <c>false</c>.</value>
        public static readonly bool IsUnix = Environment.OSVersion.Platform == PlatformID.Unix && !IsMac;

        /// <summary>
        /// Are we on a flavour of Windows? This is implemented internally by checking
        /// if we're not on Unix or Mac.
        /// </summary>
        /// <value><c>true</c> if is windows; otherwise, <c>false</c>.</value>
        public static readonly bool IsWindows = !IsUnix && !IsMac;

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
        public static readonly bool IsX11 = IsUnix && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY"));


        // From https://github.com/mono/monodevelop/blob/master/main/src/core/Mono.Texteditor/Mono.TextEditor/Platform.cs
        // Environment.OSVersion.Platform returns Unix for Mac OS X as of Mono v4.0.0:
        // https://bugzilla.xamarin.com/show_bug.cgi?id=13345
        // So we have to detect it another way
        private static bool IsRunningOnMac()
        {
            IntPtr buf = IntPtr.Zero;
            try
            {
                buf = Marshal.AllocHGlobal(8192);
                // This is a hacktastic way of getting sysname from uname ()
                if (uname(buf) == 0)
                {
                    string os = Marshal.PtrToStringAnsi(buf);
                    if (os == "Darwin")
                        return true;
                }
            }
            catch
            {
            }
            finally
            {
                if (buf != IntPtr.Zero)
                    Marshal.FreeHGlobal(buf);
            }
            return false;
        }

        [DllImport("libc")]
        private static extern int uname(IntPtr buf);

        private static bool IsOnMonoFourOrLater()
        {
            if (!IsMono)
                return false;

            // Get Mono's display name and parse the version
            Type type = Type.GetType("Mono.Runtime");
            string display_name =
                (string)type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);

            var match = versionMatcher.Match(display_name);
            if (match.Success)
            {
                int majorVersion = Int32.Parse(match.Groups["majorVersion"].Value);
                return majorVersion >= 4;
            }
            else
            {
                return false;
            }
        }

        private static readonly Regex versionMatcher = new Regex(
            "^\\s*(?<majorVersion>\\d+)\\.\\d+\\.\\d+\\s*\\(",
            RegexOptions.Compiled
        );
    }
}
