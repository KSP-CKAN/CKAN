using System;
using System.Collections.Generic;
using System.Linq;

namespace CKAN
{
    public interface IWin32Registry
    {

        string AutoStartInstance { get; set; }
        void SetRegistryToInstances(SortedList<string, KSP> instances, string auto_start_instance);
        IEnumerable<Tuple<string, string>> GetInstances();
        string GetKSPBuilds();
        void SetKSPBuilds(string buildMap);
    }

    public class Win32Registry : IWin32Registry
    {
        private static readonly string CKAN_KEY = @"HKEY_CURRENT_USER\Software\CKAN";

        public Win32Registry()
        {
            ConstructKey();
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
            return new Tuple<string, string>(GetRegistryValue("KSPInstanceName_" + i, ""),
                GetRegistryValue("KSPInstancePath_" + i, ""));
        }

        public void SetRegistryToInstances(SortedList<string, KSP> instances, string auto_start_instance)
        {
            SetAutoStartInstance(auto_start_instance ?? "");
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
            return GetRegistryValue(@"KSPBuilds", (string)null);
        }

        public void SetKSPBuilds(string buildMap)
        {
            SetRegistryValue(@"KSPBuilds", buildMap);
        }

        private void ConstructKey()
        {
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\CKAN");
            if (key == null)
            {
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\CKAN");
            }
        }

        private void SetAutoStartInstance(string instance_name)
        {
            SetRegistryValue(@"KSPAutoStartInstance", instance_name ?? String.Empty);
        }

        private void SetNumberOfInstances(int count)
        {
            SetRegistryValue(@"KSPInstanceCount", count);
        }

        private void SetInstanceKeysTo(int instance_number, string name, KSP ksp)
        {            
            SetRegistryValue(@"KSPInstanceName_" + instance_number, name);
            SetRegistryValue(@"KSPInstancePath_" + instance_number, ksp.GameDir());
        }        

        private void SetRegistryValue<T>(string key, T value)
        {
            Microsoft.Win32.Registry.SetValue(CKAN_KEY, key, value);
        }

        private T GetRegistryValue<T>(string key, T default_value)
        {
            return (T)Microsoft.Win32.Registry.GetValue(CKAN_KEY, key, default_value);
        }
    }
}