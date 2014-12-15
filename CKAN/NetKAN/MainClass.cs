// KerbalStuff to CKAN generator.

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CommandLine;
using ICSharpCode.SharpZipLib.Zip;
using log4net;
using log4net.Config;
using log4net.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN
{
    internal class MainClass
    {
        private const int EXIT_OK = 0;
        private const int EXIT_BADOPT = 1;
        private const int EXIT_ERROR = 2;

        private static readonly ILog log = LogManager.GetLogger(typeof (MainClass));
        private const string expand_token = "$kref"; // See #70 for naming reasons
        private const string version_token = "$vref"; // It'd be nice if we could have repeated $krefs.

        public static int Main(string[] args)
        {
            BasicConfigurator.Configure();
            LogManager.GetRepository().Threshold = Level.Error;
            var user = new ConsoleUser();

            var options = new CmdLineOptions();
            Parser.Default.ParseArgumentsStrict(args, options);

            if (options.Verbose)
            {
                LogManager.GetRepository().Threshold = Level.Info;
            }
            else if (options.Debug)
            {
                LogManager.GetRepository().Threshold = Level.Debug;
            }

            if (options.File == null)
            {
                log.Fatal("Usage: netkan [--verbose|--debug] [--outputdir=...] <filename>");
                return EXIT_BADOPT;
            }

            NetFileCache cache = FindCache(options, new KSPManager(user), user);

            log.InfoFormat("Processing {0}", options.File);

            JObject json = JsonFromFile(options.File);
            NetKanRemote remote = FindRemote(json);

            JObject metadata;
            switch (remote.source)
            {
                case "kerbalstuff":
                    metadata = KerbalStuff(json, remote.id, cache);
                    break;
                case "github":
                    if (options.GitHubToken != null)
                    {
                        GithubAPI.SetCredentials(options.GitHubToken);
                    }

                    metadata = GitHub(json, remote.id, cache);
                    break;
                default:
                    log.FatalFormat("Unknown remote source: {0}", remote.source);
                    return EXIT_ERROR;
            }

            if (metadata == null)
            {
                log.Error("There was an error, aborting");
                return EXIT_ERROR;
            }

            // Make sure that at the very least this validates against our own
            // internal model.

            CkanModule mod = CkanModule.FromJson(metadata.ToString());

            // Make sure our identifiers match.
            if (mod.identifier != (string)json["identifier"])
            {
                log.FatalFormat("Error: Have metadata for {0}, but wanted {1}", mod.identifier, json["identifier"]);
                return EXIT_ERROR;
            }

            // Find our cached file, we'll need it later.
            string file = cache.GetCachedZip(mod.download);
            if (file == null)
            {
                log.FatalFormat("Error: Unable to find {0} in the cache", mod.identifier);
                return EXIT_ERROR;
            }

            // Make sure this would actually generate an install
            try
            {
                ModuleInstaller.FindInstallableFiles(mod, file, null);
            }
            catch (BadMetadataKraken kraken)
            {
                log.FatalFormat("Error: Bad metadata for {0}: {1}", kraken.module, kraken.Message);
                return EXIT_ERROR;
            }

            // Now inflate our original metadata from AVC, if we have it.
            // The reason we inflate our metadata and not the module is that in the module we can't
            // tell if there's been an override of the version fields or not.

            // TODO: Remove magic string "#/ckan/ksp-avc"
            if (metadata[version_token] != null && (string) metadata[version_token] == "#/ckan/ksp-avc")
            {
                metadata.Remove(version_token);

                try {
                    AVC avc = AVC.FromZipFile(mod, file);
                    avc.InflateMetadata(metadata, null, null);
                }
                catch (JsonReaderException)
                {                    
                    user.RaiseMessage("Bad embedded KSP-AVC file for {0}, halting.", mod);
                    return EXIT_ERROR;
                }

                // If we've done this, we need to re-inflate our mod, too.
                mod = CkanModule.FromJson(metadata.ToString());
            }

            // All done! Write it out!

            string final_path = Path.Combine(options.OutputDir, String.Format ("{0}-{1}.ckan", mod.identifier, mod.version));

            log.InfoFormat("Writing final metadata to {0}", final_path);
            File.WriteAllText(final_path, metadata.ToString());

            return EXIT_OK;
        }

        /// <summary>
        /// Fetch le things from le KerbalStuff.
        /// Returns a JObject that should be a fully formed CKAN file.
        /// </summary>
        internal static JObject KerbalStuff(JObject orig_metadata, string remote_id, NetFileCache cache)
        {
            // Look up our mod on KS by its ID.
            KSMod ks = KSAPI.Mod(Convert.ToInt32(remote_id));

            KSVersion latest = ks.Latest();

            Version version = latest.friendly_version;

            log.DebugFormat("Mod: {0} {1}", ks.name, version);

            // Find the latest download.
            string filename = latest.Download((string) orig_metadata["identifier"], cache);

            JObject metadata = MetadataFromFileOrDefault(filename, orig_metadata);

            // Check if we should auto-inflate.
            string kref = (string)metadata[expand_token];

            if (kref == (string)orig_metadata[expand_token] || kref == "#/ckan/kerbalstuff")
            {
                log.InfoFormat("Inflating from KerbalStuff... {0}", metadata[expand_token]);
                ks.InflateMetadata(metadata, filename, latest);
                metadata.Remove(expand_token);
            }
            else
            {
                log.WarnFormat("Not inflating metadata for {0}", orig_metadata["identifier"]);
            }

            return metadata;
        }

        /// <summary>
        /// Fetch things from Github, returning a complete CkanModule document.
        /// </summary>
        private static JObject GitHub(JObject orig_metadata, string repo, NetFileCache cache)
        {
            // Find the release on github and download.
            GithubRelease release = GithubAPI.GetLatestRelease(repo);

            if (release == null)
            {
                log.Error("Downloaded releases for " + repo + " but there were none");
                return null;
            }
            string filename = release.Download((string) orig_metadata["identifier"], cache);

            // Extract embedded metadata, or use what we have.
            JObject metadata = MetadataFromFileOrDefault(filename, orig_metadata);

            // Check if we should auto-inflate.

            string kref = (string)metadata[expand_token];

            if (kref == (string)orig_metadata[expand_token] || kref == "#/ckan/github")
            {
                log.InfoFormat("Inflating from github metadata... {0}", metadata[expand_token]);
                release.InflateMetadata(metadata, filename, repo);
                metadata.Remove(expand_token);
            }
            else
            {
                log.WarnFormat("Not inflating metadata for {0}", orig_metadata["identifier"]);
            }

            return metadata;

        }

        /// <summary>
        /// Returns the CKAN metadata found in the file provided, or returns the
        /// second parameter (the default) if none found.</summary>
        internal static JObject MetadataFromFileOrDefault(string filename, JObject default_json)
        {
            try
            {
                return ExtractCkanInfo(filename);
            }
            catch (MetadataNotFoundKraken)
            {
                return default_json;
            }
        }

        /// <summary>
        /// Return a parsed JObject from a stream.
        /// </summary>

        // Courtesy https://stackoverflow.com/questions/8157636/can-json-net-serialize-deserialize-to-from-a-stream/17788118#17788118 
        private static JObject DeserializeFromStream(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    return (JObject) JToken.ReadFrom(jsonTextReader);
                }
            }
        }

        /// <summary>
        /// Given a zip filename, finds and returns the first embedded .ckan file it finds.
        /// Throws a MetadataNotFoundKraken if there is none.
        /// </summary>
        private static JObject ExtractCkanInfo(string filename)
        {
            using (var zipfile = new ZipFile(filename))
            {
                // Skip everything but embedded .ckan files.
                var entries = zipfile.Cast<ZipEntry>().Where(entry => Regex.IsMatch(entry.Name, ".CKAN$", RegexOptions.IgnoreCase));
                foreach (ZipEntry entry in entries)
                {
                    log.DebugFormat("Reading {0}", entry.Name);

                    using (Stream zipStream = zipfile.GetInputStream(entry))
                    {
                        JObject meta_ckan = DeserializeFromStream(zipStream);
                        zipStream.Close();
                        return meta_ckan;
                    }
                }
            }

            // No metadata found? Uh oh!
            throw new MetadataNotFoundKraken(filename);
        }

        /// <summary>
        /// Find a remote source and remote id from a JObject by parsing its expand_token
        /// </summary>
        internal static NetKanRemote FindRemote(JObject json)
        {
            string kref = (string) json[expand_token];
            Match match = Regex.Match(kref, @"^#/ckan/([^/]+)/(.+)");

            if (! match.Success)
            {
                // TODO: Have a proper kraken class!
                throw new Kraken("Cannot find remote and ID in kref: " + kref);
            }

            return new NetKanRemote(source: match.Groups[1].ToString(), id: match.Groups[2].ToString());
        }

        /// <summary>
        /// Reads JSON from a file and returns a JObject representation.
        /// </summary>
        internal static JObject JsonFromFile(string filename)
        {
            return JObject.Parse(File.ReadAllText(filename));
        }

        internal static NetFileCache FindCache(CmdLineOptions options, KSPManager ksp_manager, IUser user)
        {
            if (options.CacheDir != null)
            {
                log.InfoFormat("Using user-supplied cache at {0}", options.CacheDir);
                return new NetFileCache(options.CacheDir);
            }

            try
            {
                KSP ksp = ksp_manager.GetPreferredInstance();
                log.InfoFormat("Using CKAN cache at {0}",ksp.Cache.GetCachePath());
                return ksp.Cache;
            }
            catch
            {
                // Meh, can't find KSP. 'Scool, bro.
            }

            string tempdir = Path.GetTempPath();
            log.InfoFormat("Using tempdir for cache: {0}", tempdir);

            return new NetFileCache(tempdir);
        }
    }

    internal class NetKanRemote
    {
        public string source; // EG: "kerbalstuff" or "github"
        public string id;     // EG: "269" or "pjf/DogeCoinFlag"

        internal NetKanRemote(string source, string id)
        {
            this.source = source;
            this.id = id;
        }
    }

    internal class MetadataNotFoundKraken : Exception
    {
        public string filename;

        public MetadataNotFoundKraken(string filename)
        {
            this.filename = filename; // Is there a better way?
        }
    }
}