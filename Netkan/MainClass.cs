// KerbalStuff / Github / Jenkins to CKAN generator.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            var user = new ConsoleUser(false);

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

            if (options.NetUserAgent != null)
            {
                Net.UserAgentString = options.NetUserAgent;
            }

            if (options.File == null)
            {
                log.Fatal("Usage: netkan [--verbose|--debug] [--prerelease] [--outputdir=...] <filename>");
                return EXIT_BADOPT;
            }

            NetFileCache cache = FindCache(options, new KSPManager(user), user);

            log.InfoFormat("Processing {0}", options.File);

            JObject json = JsonFromFile(options.File);
            NetKanRemote remote = FindRemoteKref(json,user);

            JObject metadata = json;
            if (remote == null)
            {
                log.WarnFormat("Not inflating metadata for {0}", json["identifier"]);
            }
            else
            {
                switch (remote.source)
                {
                    case "kerbalstuff":
                        metadata = KerbalStuff(json, remote.id, cache);
                        break;
                    case "jenkins":
                        metadata = Jenkins(json, remote.id, cache);
                        break;
                    case "github":
                        if (options.GitHubToken != null)
                        {
                            GithubAPI.SetCredentials(options.GitHubToken);
                        }

                        metadata = GitHub(json, remote.id, options.PreRelease, cache);
                        break;
                    case "http":
                        metadata = HTTP(json, remote.id, cache, user);
                        break;
                    default:
                        log.FatalFormat("Unknown remote source: {0}", remote.source);
                        return EXIT_ERROR;
                }
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
            if (mod.identifier != (string) json["identifier"])
            {
                log.FatalFormat("Error: Have metadata for {0}, but wanted {1}", mod.identifier, json["identifier"]);
                return EXIT_ERROR;
            }

            // Find our cached file, we'll need it later.
            string file = cache.GetCachedZip(mod.download);
            if (file == null)
            {
                //Last attempt. Should only be triggered by files without krefs
                try
                {
                    file = ModuleInstaller.CachedOrDownload(mod.identifier, mod.version, mod.download, cache);
                }
                catch (Exception)
                {
                    log.FatalFormat("Error: Unable to find {0} in the cache", mod.identifier);
                    return EXIT_ERROR;
                }
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
            foreach (string vref in vrefs(metadata))
            {
                log.DebugFormat("Expanding vref {0}", vref);

                if (vref.StartsWith("#/ckan/ksp-avc"))
                {
                    // HTTP has already included the KSP-AVC data as it needs it earlier in the process
                    if (remote.source != "http")
                    {
                        var version_remote = FindVersionRemote(metadata, vref);

                        try
                        {
                            AVC avc = AVC.FromZipFile(mod, file, version_remote.id);
                            // TODO check why the AVC file can be null...
                            if (avc != null)
                            {
                                avc.InflateMetadata(metadata, null, null);
                            }
                        }
                        catch (JsonReaderException)
                        {
                            user.RaiseMessage("Bad embedded KSP-AVC file for {0}, halting.", mod);
                            return EXIT_ERROR;
                        }
                    }
                }

                if (vref.StartsWith("#/ckan/kerbalstuff"))
                {
                    log.DebugFormat("Kerbalstuff vref: {0}", vref);
                    Match match = Regex.Match(vref, @"^#/ckan/([^/]+)/(.+)");

                    if (!match.Success)
                    {
                        // TODO: Have a proper kraken class!
                        user.RaiseMessage("Cannot find remote and ID in vref: {0}, halting.", vref);
                        return EXIT_ERROR;
                    }

                    string remote_id = match.Groups[2].ToString();
                    log.DebugFormat("Kerbalstuff id  : {0}", remote_id);
                    try
                    {
                        KSMod ks = KSAPI.Mod(Convert.ToInt32(remote_id));
                        if (ks != null)
                        {
                            KSVersion version = new KSVersion();
                            version.friendly_version = mod.version;
                            ks.InflateMetadata(metadata, file, version);
                        }
                    }
                    catch (JsonReaderException)
                    {
                        user.RaiseMessage("Cannot find remote and ID in vref: {0}, halting.", vref);
                        return EXIT_ERROR;
                    }
                }

            }

            // Fix our version string, if required.
            metadata = FixVersionStrings(metadata);

            // Re-inflate our mod, in case our vref or FixVersionString routines have
            // altered it at all.
            mod = CkanModule.FromJson(metadata.ToString());

            // All done! Write it out!

            var version_filename = mod.version.ToString().Replace(':', '-');
            string final_path = Path.Combine(options.OutputDir, string.Format("{0}-{1}.ckan", mod.identifier, version_filename));

            log.InfoFormat("Writing final metadata to {0}", final_path);

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonTextWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;
                writer.IndentChar = ' ';

                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, metadata);
            }

            File.WriteAllText(final_path, sw + Environment.NewLine);

            return EXIT_OK;
        }

        /// <summary>
        /// (safely) load all vrefs from file.
        /// Returns a list of vref strings.
        /// </summary>
        internal static List<string> vrefs(JObject orig_metadata)
        {
            List<string> result = new List<string>();

            JToken vref = orig_metadata[version_token];
            // none: null
            // single: Newtonsoft.Json.Linq.JValue
            // multiple: Newtonsoft.Json.Linq.JArray
            if (vref != null)
            {
                if (vref is JValue)
                {
                    result.Add(vref.ToString());
                }
                else if (vref is JArray)
                {
                    foreach (string innerVref in vref.ToArray())
                    {
                        result.Add(innerVref);
                    }
                }

                // Remove $vref from the resulting ckan
                orig_metadata.Remove(version_token);
            }

            return result;
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
            string kref = (string) metadata[expand_token];

            if (kref == (string) orig_metadata[expand_token] || kref == "#/ckan/kerbalstuff")
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
        /// Fetch Metadata from (successful) Jenkins builds
        /// Returns a JObject that should be a fully formed CKAN file.
        /// </summary>
        internal static JObject Jenkins(JObject orig_metadata, string remote_id, NetFileCache cache)
        {
            string versionBase = (string) orig_metadata["x_ci_version_base"];
            log.DebugFormat("versionBase: {0}", versionBase);

            JObject resources = (JObject) orig_metadata["resources"];
            log.DebugFormat("resources: {0}", resources);

            string baseUri = (string) resources["ci"];
            if (baseUri == null)
            {
                // Fallback, we don't have the defined resource 'ci' in the schema yet...
                baseUri = (string) resources["x_ci"];
            }

            log.DebugFormat("baseUri: {0}", baseUri);

            JenkinsBuild build = JenkinsAPI.GetLatestBuild(baseUri, versionBase, true);

            Version version = build.version;

            log.DebugFormat("Mod: {0} {1}", remote_id, version);

            // Find the latest download.
            string filename = build.Download((string) orig_metadata["identifier"], cache);

            JObject metadata = MetadataFromFileOrDefault(filename, orig_metadata);

            // Check if we should auto-inflate.
            string kref = (string) metadata[expand_token];

            if (kref == (string) orig_metadata[expand_token] || kref == "#/ckan/kerbalstuff")
            {
                log.InfoFormat("Inflating from Jenkins... {0}", metadata[expand_token]);
                build.InflateMetadata(metadata, filename, null);
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
        private static JObject GitHub(JObject orig_metadata, string repo, bool prerelease, NetFileCache cache)
        {
            // Find the release on github and download.
            GithubRelease release = GithubAPI.GetLatestRelease(repo, prerelease);

            if (release == null)
            {
                log.Error("Downloaded releases for " + repo + " but there were none");
                return null;
            }
            string filename = release.Download((string) orig_metadata["identifier"], cache);

            // Extract embedded metadata, or use what we have.
            JObject metadata = MetadataFromFileOrDefault(filename, orig_metadata);

            // Check if we should auto-inflate.

            string kref = (string) metadata[expand_token];

            if (kref == (string) orig_metadata[expand_token] || kref == "#/ckan/github")
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

        internal static JObject HTTP(JObject orig_metadata, string remote_id, NetFileCache cache, IUser user)
        {
            var metadata = orig_metadata;

            // Check if we should auto-inflate.
            string kref = (string) metadata[expand_token];

            if (kref == (string) orig_metadata[expand_token] || kref == "#/ckan/http")
            {
                log.InfoFormat("Inflating from HTTP download... {0}", metadata[expand_token]);
                metadata["download"] = remote_id;
                metadata["version"] = "0.0.0"; // add a dummy version that will be replaced by the KSP-AVC parser later
                metadata.Remove(expand_token);

                var remote_uri = new Uri(remote_id);
                var downloaded_file = Net.Download(remote_uri);
                var module = CkanModule.FromJson(metadata.ToString());

                // Check the type of the downloaded file.
                FileType downloaded_file_type = FileIdentifier.IdentifyFile(downloaded_file);

                if (downloaded_file_type != FileType.Zip)
                {
                    // We assume the downloaded file is a zip file later on.
                    string error_message = String.Format("Downloaded file not identified as a zip. Got {0} instead.", downloaded_file_type.ToString());
                    throw new Kraken(error_message);
                }

                if (metadata[version_token] != null && (metadata[version_token].ToString()).StartsWith("#/ckan/ksp-avc"))
                {
                    // TODO pass the correct vref here...
                    var versionRemote = FindVersionRemote(metadata, metadata[version_token].ToString());
                    metadata.Remove(version_token);

                    try
                    {
                        AVC avc = AVC.FromZipFile(module, downloaded_file, versionRemote.id);
                        avc.InflateMetadata(metadata, null, null);
                        metadata["version"] = avc.version.ToString();
                        module.version = avc.version;
                    }
                    catch (JsonReaderException)
                    {
                        user.RaiseMessage("Bad embedded KSP-AVC file for {0}, halting.", module);
                        return null;
                    }

                    // If we've done this, we need to re-inflate our mod, too.
                    module = CkanModule.FromJson(metadata.ToString());
                }
                else
                {
                    throw new Kraken("No $vref specified, $kref HTTP method requires it, bailing out..");
                }

                ModuleInstaller.CachedOrDownload(module.identifier, module.version, module.download, cache);
            }
            else
            {
                log.WarnFormat("Not inflating metadata for {0}", orig_metadata["identifier"]);
            }

            // metadata["download"] = metadata["download"].ToString() + '#' + metadata["version"].ToString();
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
        internal static NetKanRemote FindRemoteKref(JObject json, IUser user)
        {
            string kref = (string) json[expand_token];

            if (kref == null)
            {
                user.RaiseMessage("Warning. No kref found in netkan");
                return null;
            }

            Match match = Regex.Match(kref, @"^#/ckan/([^/]+)/(.+)");

            if (!match.Success)
            {
                // TODO: Have a proper kraken class!
                throw new Kraken("Cannot find remote and ID in kref: " + kref);
            }

            return new NetKanRemote(source: match.Groups[1].ToString(), id: match.Groups[2].ToString());
        }

        internal static NetKanRemote FindVersionRemote(JObject json, string vref)
        {
            if (vref == "#/ckan/ksp-avc")
            {
                return new NetKanRemote(source: "ksp-avc", id: "");
            }

            Match match = Regex.Match(vref, @"^#/ckan/([^/]+)/(.+)");

            if (!match.Success)
            {
                // TODO: Have a proper kraken class!
                throw new Kraken("Cannot find remote and ID in vref: " + vref);
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
                log.InfoFormat("Using CKAN cache at {0}", ksp.Cache.GetCachePath());
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

        /// <summary>
        /// Fixes version strings.
        /// This adds a 'v' if 'x_netkan_force_v' is set, and the version string does not
        /// already begin with a 'v'.
        /// This adds the epoch if 'x_netkan_epoch' is set, and contains a positive int
        /// </summary>
        internal static JObject FixVersionStrings(JObject metadata)
        {
            log.Debug("Fixing version strings (if required)...");

            JToken force_v;
            if (metadata.TryGetValue("x_netkan_force_v", out force_v) && (bool) force_v)
            {
                // Force a 'v' in front of the version string if it's not there
                // already.

                string version = (string) metadata.GetValue("version");

                if (!version.StartsWith("v"))
                {
                    log.DebugFormat("Force-adding 'v' to start of {0}", version);
                    version = "v" + version;
                    metadata["version"] = version;
                }
            }

            JToken epoch;
            if (metadata.TryGetValue("x_netkan_epoch", out epoch))
            {
                uint epoch_number;
                if (uint.TryParse(epoch.ToString(), out epoch_number))
                {
                    //Implicit if zero. No need to add
                    if (epoch_number != 0)
                        metadata["version"] = epoch_number + ":" + metadata["version"];
                }
                else
                {
                    log.Error("Invaild epoch: " + epoch);
                    throw new BadMetadataKraken(null, "Invaild epoch: " + epoch + "In " + metadata["identifier"]);
                }
            }

            return metadata;

        }


    }

    internal class NetKanRemote
    {
        public string source; // EG: "kerbalstuff" or "github"
        public string id; // EG: "269" or "pjf/DogeCoinFlag"

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