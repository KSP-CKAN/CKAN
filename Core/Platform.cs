using System;
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
            IsMac = IsRunningOnMac();
            IsMono = IsOnMono();
            IsMonoFour = IsOnMonoFour();//Depends on IsMono
        }

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


        /// <summary>
        /// Are we on a Unix (including Linux, but *not* Mac) system.
        /// </summary>
        /// <value><c>true</c> if is unix; otherwise, <c>false</c>.</value>
        public static bool IsUnix
        {
            get { return Environment.OSVersion.Platform == PlatformID.Unix; }
        }

        /// <summary>
        /// Are we on a Mac?
        /// </summary>
        /// <value><c>true</c> if is mac; otherwise, <c>false</c>.</value>
        public static bool IsMac { get; private set; }


        /// <summary>
        /// Are we on a flavour of Windows? This is implemented internally by checking
        /// if we're not on Unix or Mac.
        /// </summary>
        /// <value><c>true</c> if is windows; otherwise, <c>false</c>.</value>
        public static bool IsWindows
        {
            get { return !(IsUnix || IsMac); }
        }

        /// <summary>
        /// Are we on Mono?
        /// </summary>
        public static bool IsMono { get; private set; }

        private static bool IsOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        /// <summary>
        /// Are we running on a mono with major version 4?
        /// </summary>
        public static bool IsMonoFour { get; private set; }

        private static bool IsOnMonoFour()
        {
            if (!IsMono) return false;
            Type type = Type.GetType("Mono.Runtime");
            string display_name =
                (string) type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
            return Regex.IsMatch(display_name, "^\\s*4\\.\\d+\\.\\d+\\s*\\(");
        }
    }
}