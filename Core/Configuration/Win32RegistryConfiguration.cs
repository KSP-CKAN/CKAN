using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Win32;
using log4net;
using Newtonsoft.Json;

namespace CKAN.Configuration
{
    // DEPRECATED: We now use a JSON configuration file. This still exists to facillitate migration.
    //
    // N.B., you can resume using this version by changing the instance created in ServiceLocator.
    public class Win32RegistryConfiguration : IConfiguration
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Win32RegistryConfiguration));

        private const           string CKAN_KEY           = @"HKEY_CURRENT_USER\Software\CKAN";
        private static readonly string CKAN_KEY_NO_PREFIX = StripPrefixKey(CKAN_KEY);

        private const           string authTokenKey         = CKAN_KEY + @"\AuthTokens";
        private static readonly string authTokenKeyNoPrefix = StripPrefixKey(authTokenKey);

        private static readonly string defaultDownloadCacheDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CKAN",
            "downloads"
        );

        public string DownloadCacheDir
        {
            get { return GetRegistryValue(@"DownloadCacheDir", defaultDownloadCacheDir); }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    DeleteRegistryValue(@"DownloadCacheDir");
                }
                else
                {
                    if (!Path.IsPathRooted(value))
                    {
                        value = Path.GetFullPath(value);
                    }
                    SetRegistryValue(@"DownloadCacheDir", value);
                }
            }
        }

        public long? CacheSizeLimit
        {
            get
            {
                string val = GetRegistryValue<string>(@"CacheSizeLimit", null);
                return string.IsNullOrEmpty(val) ? null : (long?)Convert.ToInt64(val);
            }
            set
            {
                if (!value.HasValue)
                {
                    DeleteRegistryValue(@"CacheSizeLimit");
                }
                else
                {
                    SetRegistryValue(@"CacheSizeLimit", value.Value);
                }
            }
        }

        public int RefreshRate
        {
            get { return GetRegistryValue(@"RefreshRate", 0); }
            set
            {
                if (value <= 0)
                {
                    DeleteRegistryValue(@"RefreshRate");
                }
                else
                {
                    SetRegistryValue(@"RefreshRate", value);
                }
            }
        }

        private int InstanceCount
        {
            get { return GetRegistryValue(@"KSPInstanceCount", 0); }
        }

        public string AutoStartInstance
        {
            get { return GetRegistryValue(@"KSPAutoStartInstance", ""); }
            set { SetAutoStartInstance(value??String.Empty); }
        }

        public string Language
        {
            get { return GetRegistryValue<string>("Language", null); }
            set
            {
                if (Utilities.AvailableLanguages.Contains(value))
                    SetRegistryValue("Language", value);
            }
        }

        public Win32RegistryConfiguration()
        {
            ConstructKey(CKAN_KEY_NO_PREFIX);
        }

        private Tuple<string, string, string> GetInstance(int i)
        {
            return new Tuple<string, string, string>(
                GetRegistryValue("KSPInstanceName_" + i, string.Empty),
                GetRegistryValue("KSPInstancePath_" + i, string.Empty),
                GetRegistryValue("KSPInstanceGame_" + i, string.Empty)
            );
        }

        public void SetRegistryToInstances(SortedList<string, GameInstance> instances)
        {
            SetNumberOfInstances(instances.Count);

            foreach (var instance in instances.Select((instance,i)=>
                new {number=i,name=instance.Key,path=instance.Value}))
            {
                SetInstanceKeysTo(instance.number, instance.name, instance.path);
            }
        }

        public IEnumerable<Tuple<string, string, string>> GetInstances()
        {
            return Enumerable.Range(0, InstanceCount).Select(GetInstance).ToList();
        }

        public bool TryGetAuthToken(string host, out string token)
        {
            try
            {
                token = Microsoft.Win32.Registry.GetValue(authTokenKey, host, null) as string;
                return !string.IsNullOrEmpty(token);
            }
            catch
            {
                // If GetValue threw SecurityException, IOException, or ArgumentException,
                // just report failure.
                token = "";
                return false;
            }
        }

        public IEnumerable<string> GetAuthTokenHosts()
        {
            RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(authTokenKeyNoPrefix);
            return key?.GetValueNames() ?? new string[0];
        }

        public void SetAuthToken(string host, string token)
        {
            ConstructKey(authTokenKeyNoPrefix);
            if (!string.IsNullOrEmpty(host))
            {
                if (string.IsNullOrEmpty(token))
                {
                    RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(authTokenKeyNoPrefix, true);
                    key.DeleteValue(host);
                }
                else
                {
                    Microsoft.Win32.Registry.SetValue(authTokenKey, host, token);
                }
            }
        }

        public static bool DoesRegistryConfigurationExist()
        {
            RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(CKAN_KEY_NO_PREFIX);
            return key != null;
        }

        public static void DeleteAllKeys()
        {
            // This can fail if the key doesn't exist, but we don't really care...
            try {
                Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(CKAN_KEY_NO_PREFIX);
            } catch { }
        }

        private static string StripPrefixKey(string keyname)
        {
            int firstBackslash = keyname.IndexOf(@"\");
            return firstBackslash < 0
                ? keyname
                : keyname.Substring(1 + firstBackslash);
        }

        private static void ConstructKey(string whichKey)
        {
            RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(whichKey);
            if (key == null)
            {
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey(whichKey);
            }
        }

        private void SetAutoStartInstance(string instanceName)
        {
            SetRegistryValue(@"KSPAutoStartInstance", instanceName ?? string.Empty);
        }

        private void SetNumberOfInstances(int count)
        {
            SetRegistryValue(@"KSPInstanceCount", count);
        }

        private void SetInstanceKeysTo(int instanceIndex, string name, GameInstance ksp)
        {
            SetRegistryValue(@"KSPInstanceName_" + instanceIndex, name);
            SetRegistryValue(@"KSPInstancePath_" + instanceIndex, ksp.GameDir());
        }

        private static void SetRegistryValue<T>(string key, T value)
        {
            Microsoft.Win32.Registry.SetValue(CKAN_KEY, key, value);
        }

        private static T GetRegistryValue<T>(string key, T defaultValue)
        {
            return (T)Microsoft.Win32.Registry.GetValue(CKAN_KEY, key, defaultValue);
        }

        private static void DeleteRegistryValue(string name)
        {
            RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(CKAN_KEY_NO_PREFIX, true);
            key.DeleteValue(name, false);
        }
    }
}
