using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace CKAN
{
    [JsonObject(MemberSerialization.OptIn)]
    public class InstalledModuleFile
    {

        // TODO: This class should also record file paths as well.
        // It's just sha1 now for registry compatibility.

        [JsonProperty("sha1_sum", NullValueHandling = NullValueHandling.Ignore)]
        private string sha1_sum;

        public string Sha1
        {
            get
            {
                return sha1_sum;
            }
        }

        public InstalledModuleFile(string path, GameInstance ksp)
        {
            string absolute_path = ksp.ToAbsoluteGameDir(path);
            sha1_sum = Sha1Sum(absolute_path);
        }

        // We need this because otherwise JSON.net tries to pass in
        // our sha1's as paths, and things go wrong.
        [JsonConstructor]
        private InstalledModuleFile()
        {
        }

        /// <summary>
        /// Returns the sha1 sum of the given filename.
        /// Returns null if passed a directory.
        /// Throws an exception on failure to access the file.
        /// </summary>
        private static string Sha1Sum(string path)
        {
            if (Directory.Exists(path))
            {
                return null;
            }

            SHA1 hasher = new SHA1CryptoServiceProvider();

            // Even if we throw an exception, the using block here makes sure
            // we close our file.
            using (var fh = File.OpenRead(path))
            {
                string sha1 = BitConverter.ToString(hasher.ComputeHash(fh));
                fh.Close();
                return sha1;
            }
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

        [JsonProperty] private DateTime install_time;

        [JsonProperty] private CkanModule source_module;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        private bool auto_installed;

        // TODO: Our InstalledModuleFile already knows its path, so this could just
        // be a list. However we've left it as a dictionary for now to maintain
        // registry format compatibility.
        [JsonProperty] private Dictionary<string, InstalledModuleFile> installed_files;

        public IEnumerable<string> Files
        {
            get { return installed_files.Keys; }
        }

        public string identifier
        {
            get { return source_module.identifier; }
        }

        public CkanModule Module
        {
            get { return source_module; }
        }

        public DateTime InstallTime
        {
            get { return install_time; }
        }

        public bool AutoInstalled
        {
            get { return auto_installed; }
            set
            {
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
            installed_files = Platform.IsWindows
                ? new Dictionary<string, InstalledModuleFile>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, InstalledModuleFile>();
            auto_installed = autoInstalled;

            foreach (string file in relative_files)
            {
                if (Path.IsPathRooted(file))
                {
                    throw new PathErrorKraken(file, "InstalledModule *must* have relative paths");
                }

                // IMF needs a KSP object so it can compute the SHA1.
                installed_files[file] = new InstalledModuleFile(file, ksp);
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
            if (Platform.IsWindows)
            {
                // We need case insensitive path matching on Windows
                installed_files = new Dictionary<string, InstalledModuleFile>(installed_files, StringComparer.OrdinalIgnoreCase);
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
            var normalised_installed_files = Platform.IsWindows
                ? new Dictionary<string, InstalledModuleFile>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, InstalledModuleFile>();

            foreach (KeyValuePair<string, InstalledModuleFile> tuple in installed_files)
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
    }
}
