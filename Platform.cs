using System;

/// <summary>
/// Platform class to detect if we're running on Linux, Windows, or Mac.
/// Any code for checking for libraries being present also goes here.
/// 
/// This uses the modern technique of checking PlatformID enums, rather than
/// magic numbers like we used to.
/// </summary>
namespace CKAN
{
    public static class Platform
    {
        /// <summary>
        /// Are we on a Unix (including Linux, but *not* Mac) system.
        /// </summary>
        /// <value><c>true</c> if is unix; otherwise, <c>false</c>.</value>
        public static bool IsUnix
        {
            get
            {
                return Environment.OSVersion.Platform == PlatformID.Unix;
            }
        }

        /// <summary>
        /// Are we on a Mac?
        /// </summary>
        /// <value><c>true</c> if is mac; otherwise, <c>false</c>.</value>
        public static bool IsMac
        {
            get
            {
                return Environment.OSVersion.Platform == PlatformID.MacOSX;
            }
        }

        /// <summary>
        /// Are we on a flavour of Windows? This is implemented internally by checking
        /// if we're not on Unix or Mac.
        /// </summary>
        /// <value><c>true</c> if is windows; otherwise, <c>false</c>.</value>
        public static bool IsWindows
        {
            get
            {
                return !(IsUnix || IsMac);
            }
        }

    }
}

