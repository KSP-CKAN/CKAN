using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace CKAN
{
    [JsonObject(MemberSerialization.OptIn)]
    public class InstalledModuleFile
    {
        [JsonConstructor]
        public InstalledModuleFile()
        {
        }
    }

    /// <summary>
    /// A simple class that represents an installed module. Includes the time of installation,
    /// the module itself, and a list of files installed with it.
    ///
    /// Primarily used by the Registry class.
    /// </summary>

    [JsonObject(MemberSerialization.OptIn)]
    public class InstalledModule
    {
        #region Fields and Properties

        [JsonProperty]
        private readonly DateTime install_time;

        [JsonProperty]
        private readonly CkanModule source_module;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        private bool auto_installed;

        // TODO: Our InstalledModuleFile already knows its path, so this could just
        // be a list. However we've left it as a dictionary for now to maintain
        // registry format compatibility.
        [JsonProperty]
        private Dictionary<string, InstalledModuleFile> installed_files;

        public IEnumerable<string> Files       => installed_files.Keys;
        public string              identifier  => source_module.identifier;
        public CkanModule          Module      => source_module;
        public DateTime            InstallTime => install_time;

        public bool AutoInstalled
        {
            get => auto_installed;
            set {
                if (Module.IsDLC)
                {
                    throw new ModuleIsDLCKraken(Module);
                }
                auto_installed = value;
            }
        }

        #endregion

        #region Constructors

        public InstalledModule(GameInstance ksp, CkanModule module, IEnumerable<string> relative_files, bool autoInstalled)
        {
            install_time = DateTime.Now;
            source_module = module;
            // We need case insensitive path matching on Windows
            installed_files = new Dictionary<string, InstalledModuleFile>(Platform.PathComparer);
            auto_installed = autoInstalled;

            if (ksp != null)
            {
                foreach (string file in relative_files)
                {
                    if (Path.IsPathRooted(file))
                    {
                        throw new PathErrorKraken(file, "InstalledModule *must* have relative paths");
                    }

                    // IMF needs a KSP object so it can compute the SHA1.
                    installed_files[file] = new InstalledModuleFile();
                }
            }
        }

        // If we're being deserialised from our JSON file, we don't need to
        // do any special construction.
        [JsonConstructor]
        private InstalledModule()
        {
        }

        #endregion

        #region Serialisation Fixes

        [OnDeserialized]
        private void DeSerialisationFixes(StreamingContext context)
        {
            if (installed_files == null)
            {
                installed_files = new Dictionary<string, InstalledModuleFile>();
            }
            if (Platform.IsWindows)
            {
                // We need case insensitive path matching on Windows
                installed_files = new Dictionary<string, InstalledModuleFile>(installed_files,
                                                                              Platform.PathComparer);
            }
        }

        /// <summary>
        /// Ensures all files for this module have relative paths.
        /// Called when upgrading registry versions. Should be a no-op
        /// if called on newer registries.
        /// </summary>
        public void Renormalise(GameInstance ksp)
        {
            // We need case insensitive path matching on Windows
            var normalised_installed_files = new Dictionary<string, InstalledModuleFile>(Platform.PathComparer);

            foreach (var tuple in installed_files)
            {
                string path = CKANPathUtils.NormalizePath(tuple.Key);

                if (Path.IsPathRooted(path))
                {
                    path = ksp.ToRelativeGameDir(path);
                }

                normalised_installed_files[path] = tuple.Value;
            }

            installed_files = normalised_installed_files;
        }

        #endregion

        public override string ToString()
            => string.Format(AutoInstalled ? Properties.Resources.InstalledModuleToStringAutoInstalled
                                           : Properties.Resources.InstalledModuleToString,
                             Module,
                             InstallTime);
    }
}
