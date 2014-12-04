using System;
using System.IO;

namespace CKAN
{
    public class KSPPathConstants
    {
        public const string CKAN_SUBKEY = @"Software\CKAN";
        public const string CKAN_KEY = @"HKEY_CURRENT_USER\" + CKAN_SUBKEY;
        public const string CKAN_GAMEDIR_VALUE = @"GameDir";
        public const string CKAN_INSTANCES_COUNT_VALUE = @"InstancesCount";
        public static readonly string steamKSP = Path.Combine("SteamApps", "common", "Kerbal Space Program");

        public static void SetRegistryValue<T>(string key, T value)
        {
            Microsoft.Win32.Registry.SetValue(KSPPathConstants.CKAN_KEY, key, value);
        }

        public static T GetRegistryValue<T>(string key, T defaultValue)
        {
            return (T)Microsoft.Win32.Registry.GetValue(KSPPathConstants.CKAN_KEY, key, defaultValue);
        }

        public static bool RemoveRegistryKey(string key)
        {
            // Check input.
            if (key == null)
            {
                return false;
            }

            var _key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(KSPPathConstants.CKAN_SUBKEY, true);

            if (_key == null)
            {
                return false;
            }
            else
            {
                _key.DeleteValue(key);
            }

            // Write the changes.
            _key.Close();

            return true;
        }
    }
}

