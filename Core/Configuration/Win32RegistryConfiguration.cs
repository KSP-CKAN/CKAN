#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.Configuration
{
    using WinReg = Microsoft.Win32.Registry;

    /// <summary>
    /// Originally, CKAN's system level configuration was stored in the
    /// Windows registry (and Mono's emulation of it in ~/.mono/registry/).
    /// netstandard2.0 does not provide the Microsoft.Win32 namespace,
    /// so this was replaced by a config.json file in the app data folder.
    /// This class now exists solely to purge the old registry data.
    /// </summary>
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    internal class Win32RegistryConfiguration
    {
        public static bool DoesRegistryConfigurationExist()
            => WinReg.CurrentUser.OpenSubKey(CKAN_KEY_NO_PREFIX) != null;

        public static void DeleteAllKeys()
        {
            try
            {
                WinReg.CurrentUser.DeleteSubKeyTree(CKAN_KEY_NO_PREFIX);
            }
            catch
            {
                // This can fail if the key doesn't exist, but we don't really care...
            }
        }

        private static string StripPrefixKey(string keyname)
            => keyname.IndexOf(@"\") switch
               {
                   < 0                => keyname,
                   var firstBackslash => keyname[(1 + firstBackslash)..],
               };

        private const  string CKAN_KEY           =  @"HKEY_CURRENT_USER\Software\CKAN";
        private static string CKAN_KEY_NO_PREFIX => StripPrefixKey(CKAN_KEY);
    }
}
