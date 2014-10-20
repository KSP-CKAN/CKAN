// KerbalStuff to CKAN generator.

using System;
using System.IO;
using System.Text.RegularExpressions;
using log4net;
using log4net.Config;
using log4net.Core;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CKAN.KerbalStuff
{
    internal class MainClass
    {
        private static readonly int EXIT_OK = 0;
        private static readonly int EXIT_BADOPT = 1;
        private static readonly int EXIT_ERROR = 2;

        private static readonly ILog log = LogManager.GetLogger(typeof (MainClass));
        private static readonly string expand_token = "$kref"; // See #70 for naming reasons
        private static readonly string ks_expand_path = "#/ckan/kerbalstuff";
        private static readonly string gh_expand_path = "#/ckan/github";

        public static int Main(string[] args)
        {
            BasicConfigurator.Configure();
            LogManager.GetRepository().Threshold = Level.Debug;
            log.Debug("KerbalStuff2CKAN started");

            if (args.Length < 4)
            {
                User.WriteLine("Usage: ext2ckan ks identifier ksid outputdir  ## KerbalStuff");
                User.WriteLine("Usage: ext2ckan gh identifier repo outputdir  ## GitHub");
                return EXIT_BADOPT;
            }

            string source = args[0];
            string identifier = args[1];
            string remote_id = args[2];
            string output_path = args[3];

            log.InfoFormat("Processing {0} {1} {2}", source, identifier, remote_id);

            JObject metadata = null;
            if (source == "ks")
            {
                metadata = KerbalStuff(identifier, remote_id);
            }
            else if (source == "gh")
            {
                metadata = GitHub(identifier, remote_id);
            }

            // Make sure that at the very least this validates against our own
            // internal model.

            CkanModule mod = CkanModule.from_string(metadata.ToString());

            // Make sure our identifiers match.

            if (mod.identifier != identifier)
            {
                log.FatalFormat("Error: Mod ident {0} does not match expected {1}", mod.identifier, identifier);
                return EXIT_ERROR;
            }

            // All done! Write it out!

            string final_path = Path.Combine(output_path, String.Format ("{0}-{1}.ckan", mod.identifier, mod.version));

            File.WriteAllText(final_path, metadata.ToString());

            return EXIT_OK;
        }

        /// <summary>
        /// Fetch le things from le KerbalStuff.
        /// Returns a JObject that should be a fully formed CKAN file.
        /// </summary>
        private static JObject KerbalStuff(string identifier, string remote_id)
        {
            KSMod ks = KSAPI.Mod(Convert.ToInt32(remote_id));
            Version version = ks.versions[0].friendly_version;

            log.DebugFormat("Mod: {0} {1}", ks.name, version);

            KSVersion latest = ks.versions[0];
            string filename = latest.Download(identifier);

            JObject metadata = ExtractOrBootKAN(identifier, filename);

            // Check if we should auto-inflate!
            if ((string) metadata[expand_token] == ks_expand_path)
            {
                log.InfoFormat("Inflating...");
                ks.InflateMetadata(metadata, filename, latest);
                metadata.Remove(expand_token);
            }

            return metadata;
        }

        private static JObject GitHub(string identifier, string repo)
        {
            GithubRelease release = GithubAPI.GetLatestRelease(repo);
            string filename = release.Download(identifier);

            JObject metadata = ExtractOrBootKAN(identifier, filename);

            if ((string)metadata[expand_token] == gh_expand_path)
            {
                log.InfoFormat("Inflating from github metadata...");
                release.InflateMetadata(metadata, filename, repo);
                metadata.Remove(expand_token);
            }

            return metadata;

        }

        internal static JObject ExtractOrBootKAN(string identifier, string filename)
        {
            JObject metadata = null;
            try
            {
                metadata = ExtractCkanInfo(filename);
            }
            catch (MetadataNotFoundKraken)
            {
                log.WarnFormat ("Reading BootKAN metadata for {0}", filename);
                metadata = BootKAN (identifier);
            }
            return metadata;
        }


        // Courtesy https://stackoverflow.com/questions/8157636/can-json-net-serialize-deserialize-to-from-a-stream/17788118#17788118 
        private static JObject DeserializeFromStream(Stream stream)
        {
            // var serializer = new JsonSerializer();

            using (var sr = new StreamReader(stream))
            {
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    return (JObject) JToken.ReadFrom(jsonTextReader);
                }
            }
        }

        /// <summary>
        ///     Given a zip filename, finds and returns the first embedded .ckan file it finds.
        /// </summary>
        private static JObject ExtractCkanInfo(string filename)
        {
            var zipfile = new ZipFile(File.OpenRead(filename));

            foreach (ZipEntry entry in zipfile)
            {
                // Skip everything but embedded .ckan files.
                if (! Regex.IsMatch(entry.Name, ".CKAN$", RegexOptions.IgnoreCase))
                {
                    continue;
                }

                log.DebugFormat("Reading {0}", entry.Name);

                Stream zipStream = zipfile.GetInputStream(entry);

                JObject meta_ckan = DeserializeFromStream(zipStream);

                return meta_ckan;
            }

            throw new MetadataNotFoundKraken(filename);
        }

        /// <summary>
        /// Returns the metadata fragment found in the BootKAN/ directory for
        /// this identifier, if it exists.
        /// </summary>
        internal static JObject BootKAN(string identifier)
        {
            string fragment = File.ReadAllText(Path.Combine ("BootKAN", identifier + ".ckan"));
            return JObject.Parse(fragment);
        }
    }

    internal class MetadataNotFoundKraken : Exception
    {
        public string filename = null;

        public MetadataNotFoundKraken(string filename)
        {
            this.filename = filename; // Is there a better way?
        }
    }
}