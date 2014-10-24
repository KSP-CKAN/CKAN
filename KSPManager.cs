using System;
using System.Collections.Generic;
using log4net;

namespace CKAN
{
    /// <summary>
    /// Manage multiple KSP installs.
    /// </summary>
    public static class KSPManager
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(KSPManager));

        internal static bool instances_loaded = false;
        internal static Dictionary<string, KSP> _Instances = new Dictionary<string, KSP>();
        internal static KSP _CurrentInstance = null;
        internal static string AutoStartInstance = null;

        public static Dictionary<string,KSP> Instances
        {
            get
            {
                if (! instances_loaded)
                {
                    // This also sets instances_loaded to true.
                    LoadInstancesFromRegistry ();
                }
                return _Instances;
            }
        }

        public static KSP CurrentInstance
        {
            get
            {
                return _CurrentInstance;
            }
        }

        /// <summary>
        /// Returns the prefered KSP instance, or null if none can be found.
        /// 
        /// This works by checking to see if we're in a KSP dir first, then the
        /// registry for an autostart instance, then will try to auto-populate
        /// by scanning for the game.
        /// 
        /// This *will not* touch the registry if we find a portable install.
        /// 
        /// This *will* run KSP instance autodetection if the registry is empty.
        /// 
        /// This *will* set the current instance, or throw an exception if it's already set.
        /// 
        /// Returns null if we have multiple instances, but none of them are preferred.
        /// </summary>
 
        public static KSP GetPreferredInstance()
        {
            if (_CurrentInstance != null)
            {
                // TODO: Throw a better exception
                throw new KSPManagerKraken("Tried to set KSP instance twice!");
            }

            _CurrentInstance = _GetPreferredInstance();
            return _CurrentInstance;
        }

        // Actual worker for GetPreferredInstance()
        internal static KSP _GetPreferredInstance()
        {
            // First check if we're part of a portable install
            // Note that this *does not* register in the registry.
            string path = KSP.PortableDir();

            if (path != null)
            {
                return new KSP(path);
            }

            // Return the autostart, if we can find it.
            if (AutoStartInstance != null)
            {
                KSP instance = Instances[AutoStartInstance];
                if (instance == null)
                {
                    throw new KSPManagerKraken(String.Format("Auto-start instance {0} registered but not found", AutoStartInstance));
                }
                return instance;
            }

            // If we only know of a single instance, return that.
            if (Instances.Count == 1)
            {
                // Surely there's a better way to get the singleton value than this?
                var keys = new List<string>(Instances.Keys);
                return Instances[keys[0]];
            }

            // If we know of no instances, try to find one.
            if (Instances.Count == 0)
            {
                return FindAndRegisterDefaultInstance();
            }

            // Otherwise, we know of too many instances!
            // We don't know which one to pick, so we return null.

            return null;
        }

        /// <summary>
        /// Find and register a default instance by running
        /// game autodetection code.
        /// 
        /// Returns the resulting KSP object if found.
        /// </summary>
        public static KSP FindAndRegisterDefaultInstance()
        {
            if (Instances.Count == 0)
            {
                string gamedir = KSP.FindGameDir();
                return AddInstance ("Auto-detected instance", gamedir);
            }

            throw new KSPManagerKraken("Attempted to scan for defaults with instances in registry");
        }

        /// <summary>
        /// Adds a KSP instance to registry.
        /// Returns the resulting KSP object.
        /// </summary>
        public static KSP AddInstance(string name, string path)
        {
            var ksp = new KSP(path);
            Instances.Add(name, ksp);
            PopulateRegistryWithInstances();
            return ksp;
        }

        /// <summary>
        /// Removes the instance from the registry and saves.
        /// </summary>
        public static void RemoveInstance(string name)
        {
            Instances.Remove(name);
            PopulateRegistryWithInstances();
        }

        /// <summary>
        /// Renames an instance in the registry and saves.
        /// </summary>
        /// 
        // TODO: What should we do if our target name already exists?
        public static void RenameInstance(string from, string to)
        {
            var ksp = Instances[from];
            Instances.Remove(from);
            Instances.Add(to, ksp);
            PopulateRegistryWithInstances();
        }

        /// <summary>
        /// Sets the current instance.
        /// Throws an InvalidKSPInstanceKraken if not found.
        /// </summary>
        public static void SetCurrentInstance(string name)
        {
            // TODO: Should we disallow this if _CurrentInstance is already set?

            if (!Instances.ContainsKey(name))
            {
                throw new InvalidKSPInstanceKraken(name);
            }

            _CurrentInstance = Instances[name];
        }

        public static void SetCurrentInstanceByPath(string name)
        {
            // TODO: Should we disallow this if _CurrentInstance is already set?
            _CurrentInstance = new KSP(name);
        }

        /// <summary>
        /// Sets the autostart instance in the registry and saves it.
        /// </summary>
        public static void SetAutoStart(string name)
        {
            if (!Instances.ContainsKey(name))
            {
                throw new InvalidKSPInstanceKraken(name);
            }

            AutoStartInstance = name;
            PopulateRegistryWithInstances();
        }

        public static void ClearAutoStart()
        {
            AutoStartInstance = null;
            PopulateRegistryWithInstances();
        }

        public static void LoadInstancesFromRegistry()
        {
            log.Debug("Loading KSP instances from registry");

            _Instances.Clear();

            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\CKAN");
            if (key == null)
            {
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\CKAN");
            }

            AutoStartInstance = KSPPathConstants.GetRegistryValue(@"KSPAutoStartInstance", "");
            var instanceCount = KSPPathConstants.GetRegistryValue(@"KSPInstanceCount", 0);

            for (int i = 0; i < instanceCount; i++)
            {
                var name = KSPPathConstants.GetRegistryValue(@"KSPInstanceName_" + i, "");
                var path = KSPPathConstants.GetRegistryValue(@"KSPInstancePath_" + i, "");

                log.DebugFormat("Loading {0} from {1}", name, path);

                var ksp = new KSP(path);
                _Instances.Add(name, ksp);

                log.DebugFormat("Added {0} at {1}", name, path);
            }

            instances_loaded = true;
        }

        public static void PopulateRegistryWithInstances()
        {
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\CKAN");
            if (key == null)
            {
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\CKAN");
            }

            KSPPathConstants.SetRegistryValue(@"KSPAutoStartInstance", AutoStartInstance == null ? "" : AutoStartInstance);
            KSPPathConstants.SetRegistryValue(@"KSPInstanceCount", _Instances.Count);

            int i = 0;
            foreach (var instance in _Instances)
            {
                var name = instance.Key;
                var ksp = instance.Value;

                KSPPathConstants.SetRegistryValue(@"KSPInstanceName_" + i, name);
                KSPPathConstants.SetRegistryValue(@"KSPInstancePath_" + i, ksp.GameDir());

                i++;
            }
        }
    }

    public class KSPManagerKraken : Kraken {
        public KSPManagerKraken(string reason = null, Exception inner_exception = null) :base(reason, inner_exception)
        {
        }
    }

    public class InvalidKSPInstanceKraken : Exception
    {
        public string instance;
        public InvalidKSPInstanceKraken(string instance, string reason = null, Exception inner_exception = null)
            :base(reason, inner_exception)
        {
            this.instance = instance;
        }


    }
}

