using log4net;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CKAN
{
    public interface IWin32Registry : IDisposable
    {
        string FindSteamPath();
        string AutoStartInstance { get; set; }
        void SetRegistryToInstances(SortedList<string, KSP> instances, string autoStartInstance);
        IEnumerable<Tuple<string, string>> GetInstances();
        string GetKSPBuilds();
        void SetKSPBuilds(string buildMap);
    }

    public class Win32Registry : IWin32Registry
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Win32Registry));

        /// <summary>
        /// The location in the registry used by CKAN to store data.
        /// </summary>
        private static readonly string CKANRegistryPath = @"Software\CKAN";

        /// <summary>
        /// The location in the registry pointing to a Steam install path.
        /// </summary>
        private static readonly string SteamRegistryPath = @"Software\Valve\Steam";

        private readonly RegistryKey _currentUser;

        public Win32Registry()
        {
            // second arg here is so that any legacy code won't accidentally select the 64-bit view
            _currentUser = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);
        }

        public void Dispose()
        {
            if (_currentUser != null)
            {
                _currentUser.Dispose();
            }
        }

        private int InstanceCount
        {
            get { return GetRegistryValue(@"KSPInstanceCount", 0); }
        }

        public string FindSteamPath()
        {
            try
            {
                Log.DebugFormat("Checking for Steam registry key {0}", SteamRegistryPath);

                var result = GetRegistryValue<string>(SteamRegistryPath, String.Empty);
                if (!String.IsNullOrEmpty(result)) {
                    return result;
                }

                Log.Debug("Couldn't find Steam via registry key, trying other locations...");

                // Not in the registry, or missing file, but that's cool. This should find it on Linux
                var steamPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    ".local",
                    "share",
                    "Steam"
                );

                Log.DebugFormat("Looking for Steam in {0}", steamPath);

                if (Directory.Exists(steamPath))
                {
                    Log.InfoFormat("Found Steam at {0}", steamPath);
                    return steamPath;
                }

                // Try an alternative path.
                steamPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    ".steam",
                    "steam"
                );

                Log.DebugFormat("Looking for Steam in {0}", steamPath);

                if (Directory.Exists(steamPath))
                {
                    Log.InfoFormat("Found Steam at {0}", steamPath);
                    return steamPath;
                }

                // Ok - Perhaps we're running OSX?
                steamPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    Path.Combine("Library", "Application Support", "Steam")
                    );

                Log.DebugFormat("Looking for Steam in {0}", steamPath);

                if (Directory.Exists(steamPath))
                {
                    Log.InfoFormat("Found Steam at {0}", steamPath);
                    return steamPath;
                }

                Log.Info("Steam not found on this system.");
            }
            catch (Exception e)
            {
                Log.Error("An error occured while trying to search the registry for a Steam install.", e);
            }

            return null;
        }

        public string AutoStartInstance
        {
            get { return GetRegistryValue(@"KSPAutoStartInstance", String.Empty); }
            set { SetAutoStartInstance(value ?? String.Empty); }
        }

        private Tuple<string, string> GetInstance(int i)
        {
            return new Tuple<string, string>(GetRegistryValue("KSPInstanceName_" + i, String.Empty),
                GetRegistryValue("KSPInstancePath_" + i, String.Empty));
        }

        public void SetRegistryToInstances(SortedList<string, KSP> instances, string autoStartInstance)
        {
            SetAutoStartInstance(autoStartInstance ?? String.Empty);
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
            return GetRegistryValue<string>("KSPBuilds", null);
        }

        public void SetKSPBuilds(string buildMap)
        {
            SetRegistryValue("KSPBuilds", buildMap);
        }

        private void SetAutoStartInstance(string instanceName)
        {
            SetRegistryValue("KSPAutoStartInstance", instanceName ?? String.Empty);
        }

        private void SetNumberOfInstances(int count)
        {
            SetRegistryValue("KSPInstanceCount", count);
        }

        private void SetInstanceKeysTo(int instance_number, string name, KSP ksp)
        {            
            SetRegistryValue("KSPInstanceName_" + instance_number, name);
            SetRegistryValue("KSPInstancePath_" + instance_number, ksp.GameDir());
        }        

        private void SetRegistryValue<T>(string key, T value)
        {
            Microsoft.Win32.Registry.SetValue(CKANRegistryPath, key, value);
        }

        private T GetRegistryValue<T>(string key, T defaultValue)
        {
            if (_currentUser == null)
            {
                return defaultValue;
            }

            return (T)_currentUser.GetValue(CKANRegistryPath + '\\' + key, defaultValue);
        }
    }
}