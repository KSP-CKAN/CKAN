using System;
using System.Collections.Generic;
using log4net;
using System.IO;

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
        internal static string _AutoStartInstance = null;

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

        public static string AutoStartInstance
        {
            get
            {
                if (!instances_loaded)
                {
                    // This also sets instances_loaded to true.
                    LoadInstancesFromRegistry();
                }
                return _AutoStartInstance;
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
                // We check both null and "" as we can't write NULL to the registry, so we write an empty string instead
                // This is neccessary so we can indicate that the user wants to reset the current AutoStartInstance without clearing the windows registry keys!
                if (AutoStartInstance == "")
                {
                    return null;
                }

                if (Instances.ContainsKey(AutoStartInstance))
                {
                    return Instances[AutoStartInstance];
                }
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
                string gamedir;
                try
                {
                    gamedir = KSP.FindGameDir();
                }
                catch (DirectoryNotFoundException)
                {
                    return null;
                }
                 
                return AddInstance ("auto", gamedir);
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
        /// Gets the name of the next valid instance. Will attempt to append a number or the current (local) time to find the next valid solution.
        /// </summary>
        /// <returns>The next valid instance name.</returns>
        /// <param name="name">The name to check.</param>
        /// <exception cref="CKAN.Kraken">Could not find a valid name.</exception>
        public static string GetNextValidInstanceName(string name)
        {
            // Check if the current name is valid.
            if (InstanceNameIsValid(name))
            {
                return name;
            }

            string validName;

            // Try appending a number to the name.
            for (int i = 1; i < 1000; i++)
            {
                validName = name + " (" + i.ToString() + ")";
                if (InstanceNameIsValid(validName))
                {
                    return validName;
                }
            }

            // Check if a name with the current timestamp is valid.
            validName = name + " (" + DateTime.Now.ToString() + ")";

            if (InstanceNameIsValid(validName))
            {
                return validName;
            }

            // Give up.
            throw new Kraken("Could not return a valid name for the new instance.");
        }

        /// <summary>
        /// Check if the instance name is valid.
        /// </summary>
        /// <returns><c>true</c>, if name is valid, <c>false</c> otherwise.</returns>
        /// <param name="name">Name to check.</param>
        public static bool InstanceNameIsValid(string name)
        {
            // Discard null, empty strings and white space only strings.
            if (String.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            // Look for the current name in the list of loaded instances.
            if (Instances.ContainsKey(name))
            {
                return false;
            }

            return true;
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
        /// Removes the invalid KSP instance keys in the registry.
        /// </summary>
        public static void RemoveMissing()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Attempts to remove the requested registry key.
        /// </summary>
        /// <returns><c>true</c>, if registry key was removed, <c>false</c> otherwise.</returns>
        /// <param name="key">Key to remove.</param>
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

            _AutoStartInstance = name;
            PopulateRegistryWithInstances();
        }

        public static void ClearAutoStart()
        {
            _AutoStartInstance = null;
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

            _AutoStartInstance = KSPPathConstants.GetRegistryValue(@"KSPAutoStartInstance", "");
            var instanceCount = KSPPathConstants.GetRegistryValue(@"KSPInstanceCount", 0);

            for (int i = 0; i < instanceCount; i++)
            {
                var name = KSPPathConstants.GetRegistryValue(@"KSPInstanceName_" + i, "");
                var path = KSPPathConstants.GetRegistryValue(@"KSPInstancePath_" + i, "");

                log.DebugFormat("Loading {0} from {1}", name, path);

                try
                {
                    var ksp = new KSP(path);
                    _Instances.Add(name, ksp);
                }
                catch (NotKSPDirKraken)
                {
                    // The current instance is not a valid KSP directory. Warn the user.
                    log.WarnFormat("Instance {0} is not valid", name);

                    // Make sure the instance is not in the list and continue to the next key.
                    _Instances.Remove(name);
                    log.DebugFormat("Skipped {0}", name);
                    continue;
                }

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

            KSPPathConstants.SetRegistryValue(@"KSPAutoStartInstance", _AutoStartInstance == null ? "" : _AutoStartInstance);
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

