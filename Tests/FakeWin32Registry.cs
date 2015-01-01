using System;
using System.Collections.Generic;
using System.Linq;
using CKAN;

namespace Tests
{
    internal class FakeWin32Registry : IWin32Registry
    {
        public FakeWin32Registry(KSP instance, string autostart)
        {
            Instances = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("test", instance.GameDir())
            };
            AutoStartInstance = autostart;
        }

        public FakeWin32Registry(KSP instance) : this(instance, "test")
        {

        }


        public FakeWin32Registry(List<Tuple<string, string>> instances, string auto_start_instance = null)
        {
            Instances = instances;
            AutoStartInstance = auto_start_instance;
        }

        public List<Tuple<string, string>> Instances { get; set; }

        public int InstanceCount
        {
            get { return Instances.Count; }
        }

        public string AutoStartInstance { get; set; }

        public Tuple<string, string> GetInstance(int i)
        {
            return Instances[i];
        }

        public void SetRegistryToInstances(Dictionary<string, KSP> instances, string auto_start_instance)
        {
            Instances =
                instances.Select(kvpair => new Tuple<string, string>(kvpair.Key, kvpair.Value.GameDir())).ToList();
            AutoStartInstance = auto_start_instance;
        }

        public IEnumerable<Tuple<string, string>> GetInstances()
        {
            return Instances;
        }
    }
}
