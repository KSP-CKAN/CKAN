using System.IO;

namespace CKAN
{
    public class KSPPathConstants
    {
        public const string CKAN_KEY = @"HKEY_CURRENT_USER\Software\CKAN";
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

    }
}

