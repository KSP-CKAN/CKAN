using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Win32;

namespace CKAN.Win32Registry
{

    public class Win32RegistryReal : IWin32Registry
    {
        private const           string CKAN_KEY           = @"HKEY_CURRENT_USER\Software\CKAN";
        private static readonly string CKAN_KEY_NO_PREFIX = StripPrefixKey(CKAN_KEY);

        private const           string authTokenKey         = CKAN_KEY + @"\AuthTokens";
        private static readonly string authTokenKeyNoPrefix = StripPrefixKey(authTokenKey);

        static Win32RegistryReal()
        {
            ConstructKey(CKAN_KEY_NO_PREFIX);
        }

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

        private Tuple<string, string> GetInstance(int i)
        {
            return new Tuple<string, string>(GetRegistryValue("KSPInstanceName_" + i, string.Empty),
                GetRegistryValue("KSPInstancePath_" + i, string.Empty));
        }

        public void SetRegistryToInstances(SortedList<string, KSP> instances)
        {
            SetNumberOfInstances(instances.Count);

            foreach (var instance in instances.Select((instance,i)=>
                new {number=i,name=instance.Key,path=instance.Value}))
            {
                SetInstanceKeysTo(instance.number, instance.name, instance.path);
            }
        }

        public IEnumerable<Tuple<string, string>> GetInstances()
        {
            return Enumerable.Range(0, InstanceCount).Select(GetInstance).ToList();
        }

        public string GetKSPBuilds()
        {
            return GetRegistryValue("KSPBuilds", null as string);
        }

        public void SetKSPBuilds(string buildMap)
        {
            SetRegistryValue(@"KSPBuilds", buildMap);
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

        private void SetInstanceKeysTo(int instanceIndex, string name, KSP ksp)
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
