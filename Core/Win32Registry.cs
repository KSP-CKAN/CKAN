using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

namespace CKAN
{
    public interface IWin32Registry
    {
        string AutoStartInstance { get; set; }
        void SetRegistryToInstances(SortedList<string, KSP> instances, string autoStartInstance);
        IEnumerable<Tuple<string, string>> GetInstances();
        string GetKSPBuilds();
        void SetKSPBuilds(string buildMap);
    }

    public class Win32Registry : IWin32Registry
    {
        private const           string CKAN_KEY           = @"HKEY_CURRENT_USER\Software\CKAN";
        private static readonly string CKAN_KEY_NO_PREFIX = StripPrefixKey(CKAN_KEY);

        private const           string authTokenKey         = CKAN_KEY + @"\AuthTokens";
        private static readonly string authTokenKeyNoPrefix = StripPrefixKey(authTokenKey);

        public Win32Registry()
        {
            ConstructKey(CKAN_KEY_NO_PREFIX);
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

        public void SetRegistryToInstances(SortedList<string, KSP> instances, string autoStartInstance)
        {
            SetAutoStartInstance(autoStartInstance ?? string.Empty);
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

        /// <summary>
        /// Look for an auth token in the registry.
        /// </summary>
        /// <param name="host">Host for which to find a token</param>
        /// <param name="token">Value of the token returned in parameter</param>
        /// <returns>
        /// True if found, false otherwise
        /// </returns>
        public static bool TryGetAuthToken(string host, out string token)
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

        /// <summary>
        /// Get the hosts that have auth tokens stored in the registry
        /// </summary>
        /// <returns>
        /// Strings that are values of the auth token registry key
        /// </returns>
        public static IEnumerable<string> GetAuthTokenHosts()
        {
            RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(authTokenKeyNoPrefix);
            return key?.GetValueNames() ?? new string[0];
        }

        /// <summary>
        /// Set an auth token in the registry
        /// </summary>
        /// <param name="host">Host for which to set the token</param>
        /// <param name="token">Token to set, or null to delete</param>
        public static void SetAuthToken(string host, string token)
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
    }
}
