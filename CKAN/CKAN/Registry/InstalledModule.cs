using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Runtime.Serialization;
using log4net;

namespace CKAN
{
    [JsonObject(MemberSerialization.OptIn)]
    public class InstalledModuleFile
    {

        // TODO: This class should also record file paths as well.
        // It's just sha1 now for registry compatibility.

        [JsonProperty] private string sha1_sum;

        public string Sha1
        {
            get
            {
                return sha1_sum;
            }
        }

        // TODO: Record our path (relative to KSP install)
        public InstalledModuleFile(string path)
        {
            this.sha1_sum = Sha1Sum(path);
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
    /// A simple clss that represents an installed module. Includes the time of installation,
    /// the module itself, and a list of files installed with it.
    /// 
    /// Primarily used by the Registry class.
    /// </summary>

    [JsonObject(MemberSerialization.OptIn)]
    public class InstalledModule
    {
        [JsonProperty] private DateTime install_time;
        [JsonProperty] private Module source_module;

        private static readonly ILog log = LogManager.GetLogger(typeof(InstalledModule));

        // TODO: Our InstalledModuleFile already knows its path, so this could just
        // be a list. However we've left it as a dictionary for now to maintain
        // registry format compatibility.
        [JsonProperty] private Dictionary<string, InstalledModuleFile> installed_files;

        public IEnumerable<string> Files 
        {
            get
            {
                return installed_files.Keys;
            }
        }

        public string identifier
        {
            get
            {
                return source_module.identifier;
            }
        }

        public Module Module
        {
            get
            {
                return source_module;
            }
        }

        public InstalledModule(KSP ksp, Module module, IEnumerable<string> files)
        {
            this.install_time = DateTime.Now;
            this.source_module = module;
            this.installed_files = new Dictionary<string, InstalledModuleFile>();

            foreach (string full_path in files)
            {
                string rel_path = MakeRelativeToKsp(full_path, ksp);

                // IMF stil needs the full path for now so it can compute the SHA1
                installed_files[rel_path] = new InstalledModuleFile(full_path);
            }
        }

        [JsonConstructor]
        private InstalledModule()
        {
        }

        /// <summary>
        /// If the given file has an absolute path, make it relative to KSP.GameDir
        /// Does nothing if the file is already relative.
        /// Throws a kraken if it's relative, but not in the KSP dir.
        /// </summary>
        private static string MakeRelativeToKsp(string file, KSP ksp)
        {
            // If we've got an absolute path, convert it to a relative one
            // from the GameRoot.
            if (Path.IsPathRooted(file))
            {
                if (file.StartsWith(ksp.GameDir()))
                {
                    // The +1 here is because GameDir will never have
                    // a trailing slash, but our path will.
                    file = file.Remove(0, ksp.GameDir().Length + 1);
                }
                else
                {
                    throw new Kraken(
                        String.Format(
                            "Oh snap. {0} isn't inside my KSP dir ({1})",
                            file, ksp.GameDir()
                        )
                    );
                }
            }
            return file;
        }
    }
}