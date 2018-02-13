using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;

namespace CKAN
{
    /// <summary>
    /// Manage multiple KSP installs.
    /// </summary>
    public class KSPManager
    {
        public IUser User { get; set; }
        public IWin32Registry Win32Registry { get; set; }
        public KSP CurrentInstance { get; set; }


        private static readonly ILog log = LogManager.GetLogger(typeof (KSPManager));

        private readonly SortedList<string, KSP> instances = new SortedList<string, KSP>();

        public string AutoStartInstance
        {
            get { return Win32Registry.AutoStartInstance; }
            private set
            {
                if (!String.IsNullOrEmpty(value) && !HasInstance(value))
                {
                    throw new InvalidKSPInstanceKraken(value);
                }
                Win32Registry.AutoStartInstance = value;
            }
        }

        public SortedList<string, KSP> Instances
        {
            get { return new SortedList<string, KSP>(instances); }
        }

        public KSPManager(IUser user, IWin32Registry win32_registry = null)
        {
            User = user;
            Win32Registry = win32_registry ?? new Win32Registry();
            LoadInstancesFromRegistry();
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
        public KSP GetPreferredInstance()
        {
            CurrentInstance = _GetPreferredInstance();
            return CurrentInstance;
        }

        // Actual worker for GetPreferredInstance()
        internal KSP _GetPreferredInstance()
        {
            // First check if we're part of a portable install
            // Note that this *does not* register in the registry.
            string path = KSP.PortableDir();

            if (path != null)
            {
                return new KSP(path, "portable", User);
            }

            // If we only know of a single instance, return that.
            if (instances.Count == 1 && instances.First().Value.Valid)
            {
                return instances.First().Value;
            }

            // Return the autostart, if we can find it.
            // We check both null and "" as we can't write NULL to the registry, so we write an empty string instead
            // This is necessary so we can indicate that the user wants to reset the current AutoStartInstance without clearing the windows registry keys!
            if (!string.IsNullOrEmpty(AutoStartInstance)
                    && HasInstance(AutoStartInstance)
                    && instances[AutoStartInstance].Valid)
            {
                return instances[AutoStartInstance];
            }

            // If we know of no instances, try to find one.
            // Otherwise, we know of too many instances!
            // We don't know which one to pick, so we return null.
            return !instances.Any() ? FindAndRegisterDefaultInstance() : null;
        }

        /// <summary>
        /// Find and register a default instance by running
        /// game autodetection code.
        ///
        /// Returns the resulting KSP object if found.
        /// </summary>
        public KSP FindAndRegisterDefaultInstance()
        {
            if (instances.Any())
                throw new KSPManagerKraken("Attempted to scan for defaults with instances in registry");

            try
            {
                string gamedir = KSP.FindGameDir();
                return AddInstance(new KSP(gamedir, "auto", User));
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }
            catch (NotKSPDirKraken)
            {
                return null;
            }
        }

        /// <summary>
        /// Adds a KSP instance to registry.
        /// Returns the resulting KSP object.
        /// </summary>
        public KSP AddInstance(KSP ksp_instance)
        {
            if (ksp_instance.Valid)
            {
                string name = ksp_instance.Name;
                instances.Add(name, ksp_instance);
                Win32Registry.SetRegistryToInstances(instances, AutoStartInstance);
            }
            else
            {
                throw new NotKSPDirKraken(ksp_instance.GameDir());
            }
            return ksp_instance;
        }

        /// <summary>
        /// Given a string returns a unused valid instance name by postfixing the string
        /// </summary>
        /// <returns> A unused valid instance name.</returns>
        /// <param name="name">The name to use as a base.</param>
        /// <exception cref="CKAN.Kraken">Could not find a valid name.</exception>
        public string GetNextValidInstanceName(string name)
        {
            // Check if the current name is valid.
            if (InstanceNameIsValid(name))
            {
                return name;
            }

            // Try appending a number to the name.
            var validName = Enumerable.Repeat(name, 1000)
                .Select((s, i) => s + " (" + i + ")")
                .FirstOrDefault(InstanceNameIsValid);
            if (validName != null)
            {
                return validName;
            }

            // Check if a name with the current timestamp is valid.
            validName = name + " (" + DateTime.Now + ")";

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
        private bool InstanceNameIsValid(string name)
        {
            // Discard null, empty strings and white space only strings.
            // Look for the current name in the list of loaded instances.
            return !String.IsNullOrWhiteSpace(name) && !HasInstance(name);
        }

        /// <summary>
        /// Removes the instance from the registry and saves.
        /// </summary>
        public void RemoveInstance(string name)
        {
            instances.Remove(name);
            Win32Registry.SetRegistryToInstances(instances, AutoStartInstance);
        }

        /// <summary>
        /// Renames an instance in the registry and saves.
        /// </summary>
        public void RenameInstance(string from, string to)
        {
            // TODO: What should we do if our target name already exists?
            KSP ksp = instances[from];
            instances.Remove(from);
            ksp.Name = to;
            instances.Add(to, ksp);
            Win32Registry.SetRegistryToInstances(instances, AutoStartInstance);
        }

        /// <summary>
        /// Sets the current instance.
        /// Throws an InvalidKSPInstanceKraken if not found.
        /// </summary>
        public void SetCurrentInstance(string name)
        {
            if (!HasInstance(name))
            {
                throw new InvalidKSPInstanceKraken(name);
            }
            else if (!instances[name].Valid)
            {
                throw new NotKSPDirKraken(instances[name].GameDir());
            }

            // Don't try to Dispose a null CurrentInstance.
            if (CurrentInstance != null)
            {
                // Dispose of the old registry manager, to release the registry.
                var manager = RegistryManager.Instance(CurrentInstance);
                if (manager != null)
                {
                    manager.Dispose();
                }
            }

            CurrentInstance = instances[name];
        }

        public void SetCurrentInstanceByPath(string path)
        {
            KSP ksp = new KSP(path, "custom", User);
            if (ksp.Valid)
            {
                CurrentInstance = ksp;
            }
            else
            {
                throw new NotKSPDirKraken(ksp.GameDir());
            }
        }

        /// <summary>
        /// Sets the autostart instance in the registry and saves it.
        /// </summary>
        public void SetAutoStart(string name)
        {
            if (!HasInstance(name))
            {
                throw new InvalidKSPInstanceKraken(name);
            }
            else if (!instances[name].Valid)
            {
                throw new NotKSPDirKraken(instances[name].GameDir());
            }
            AutoStartInstance = name;
        }

        public bool HasInstance(string name)
        {
            return instances.ContainsKey(name);
        }

        public void ClearAutoStart()
        {
            Win32Registry.AutoStartInstance = null;
        }

        public void LoadInstancesFromRegistry()
        {
            log.Info("Loading KSP instances from registry");

            instances.Clear();

            foreach (Tuple<string, string> instance in Win32Registry.GetInstances())
            {
                var name = instance.Item1;
                var path = instance.Item2;
                log.DebugFormat("Loading {0} from {1}", name, path);
                // Add unconditionally, sort out invalid instances downstream
                instances.Add(name, new KSP(path, name, User));
            }

            try
            {
                AutoStartInstance = Win32Registry.AutoStartInstance;
            }
            catch (InvalidKSPInstanceKraken e)
            {
                log.WarnFormat("Auto-start instance was invalid: {0}", e.Message);
                AutoStartInstance = null;
            }
        }

    }

    public class KSPManagerKraken : Kraken
    {
        public KSPManagerKraken(string reason = null, Exception innerException = null)
            : base(reason, innerException)
        {
        }
    }

    public class InvalidKSPInstanceKraken : Exception
    {
        public string instance;

        public InvalidKSPInstanceKraken(string instance, string reason = null, Exception innerException = null)
            : base(reason, innerException)
        {
            this.instance = instance;
        }
    }
}
