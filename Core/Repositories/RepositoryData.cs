using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ChinhDo.Transactions.FileManager;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using log4net;

using CKAN.Versioning;
using CKAN.Games;

namespace CKAN
{
    /// <summary>
    /// Represents everything we retrieve from one metadata repository
    /// </summary>
    public class RepositoryData
    {
        /// <summary>
        /// The available modules from this repository
        /// </summary>
        [JsonProperty("available_modules", NullValueHandling = NullValueHandling.Ignore)]
        public readonly Dictionary<string, AvailableModule> AvailableModules;

        /// <summary>
        /// The download counts from this repository's download_counts.json
        /// </summary>
        [JsonProperty("download_counts", NullValueHandling = NullValueHandling.Ignore)]
        public readonly SortedDictionary<string, int> DownloadCounts;

        /// <summary>
        /// The game versions from this repository's builds.json
        /// Currently not used, maybe in the future
        /// </summary>
        [JsonProperty("known_game_versions", NullValueHandling = NullValueHandling.Ignore)]
        public readonly GameVersion[] KnownGameVersions;

        /// <summary>
        /// The other repositories listed in this repo's repositories.json
        /// Currently not used, maybe in the future
        /// </summary>
        [JsonProperty("repositories", NullValueHandling = NullValueHandling.Ignore)]
        public readonly Repository[] Repositories;

        /// <summary>
        /// Instantiate a repo data object
        /// </summary>
        /// <param name="modules">The available modules contained in this repo</param>
        /// <param name="counts">Download counts from this repo</param>
        /// <param name="versions">Game versions in this repo</param>
        /// <param name="repos">Contents of repositories.json in this repo</param>
        public RepositoryData(IEnumerable<CkanModule>       modules,
                              SortedDictionary<string, int> counts,
                              IEnumerable<GameVersion>      versions,
                              IEnumerable<Repository>       repos)
        {
            AvailableModules  = modules?.GroupBy(m => m.identifier)
                                        .ToDictionary(grp => grp.Key,
                                                      grp => new AvailableModule(grp.Key, grp));
            DownloadCounts    = counts ?? new SortedDictionary<string, int>();
            KnownGameVersions = (versions ?? Enumerable.Empty<GameVersion>()).ToArray();
            Repositories      = (repos ?? Enumerable.Empty<Repository>()).ToArray();
        }

        /// <summary>
        /// Save this repo data object to a JSON file
        /// </summary>
        /// <param name="path">Filename of the JSON file to create or overwrite</param>
        public void SaveTo(string path)
        {
            StringWriter sw = new StringWriter(new StringBuilder());
            using (JsonTextWriter writer = new JsonTextWriter(sw)
            {
                Formatting  = Formatting.Indented,
                Indentation = 0,
            })
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, this);
            }
            TxFileManager file_transaction = new TxFileManager();
            file_transaction.WriteAllText(path, sw + Environment.NewLine);
        }

        /// <summary>
        /// Load a previously cached repo data object from JSON on disk
        /// </summary>
        /// <param name="path">Filename of the JSON file to load</param>
        /// <returns>A repo data object or null if loading fails</returns>
        public static RepositoryData FromJson(string path, IProgress<long> progress)
        {
            try
            {
                log.DebugFormat("Trying to load repository data from {0}", path);
                // Ain't OOP grand?!
                using (var stream = File.Open(path, FileMode.Open))
                using (var progressStream = new ReadProgressStream(stream, progress))
                using (var reader = new StreamReader(progressStream))
                using (var jStream = new JsonTextReader(reader))
                {
                    return new JsonSerializer().Deserialize<RepositoryData>(jStream);
                }
            }
            catch (Exception exc)
            {
                log.DebugFormat("Valid repository data not found at {0}: {1}", path, exc.Message);
                return null;
            }
        }

        /// <summary>
        /// Load a repo data object from a downloaded .zip or .tar.gz file
        /// downloaded from the repo
        /// </summary>
        /// <param name="path">Filename of the archive to load</param>
        /// <param name="game">The game for which this repo has data, used for parsing the game versions because the format can vary</param>
        /// <param name="progress">Object for reporting progress in loading, since it can take a while</param>
        /// <returns>A repo data object</returns>
        public static RepositoryData FromDownload(string path, IGame game, IProgress<long> progress)
        {
            switch (FileIdentifier.IdentifyFile(path))
            {
                case FileType.TarGz: return FromTarGz(path, game, progress);
                case FileType.Zip:   return FromZip(path, game, progress);
                default: throw new UnsupportedKraken($"Not a .tar.gz or .zip, cannot process: {path}");
            }
        }

        private static RepositoryData FromTarGz(string path, IGame game, IProgress<long> progress)
        {
            using (var inputStream = File.OpenRead(path))
            using (var gzipStream  = new GZipInputStream(inputStream))
            using (var tarStream   = new TarInputStream(gzipStream, Encoding.UTF8))
            {
                var modules = new List<CkanModule>();
                SortedDictionary<string, int> counts = null;
                GameVersion[] versions = null;
                Repository[] repos = null;

                TarEntry entry;
                while ((entry = tarStream.GetNextEntry()) != null)
                {
                    ProcessFileEntry(entry.Name, () => tarStreamString(tarStream, entry),
                                     game, inputStream.Position, progress,
                                     ref modules, ref counts, ref versions, ref repos);
                }
                return new RepositoryData(modules, counts, versions, repos);
            }
        }

        private static string tarStreamString(TarInputStream stream, TarEntry entry)
        {
            // Read each file into a buffer.
            int buffer_size;

            try
            {
                buffer_size = Convert.ToInt32(entry.Size);
            }
            catch (OverflowException)
            {
                log.ErrorFormat("Error processing {0}: Metadata size too large.", entry.Name);
                return null;
            }

            byte[] buffer = new byte[buffer_size];

            stream.Read(buffer, 0, buffer_size);

            // Convert the buffer data to a string.
            return Encoding.UTF8.GetString(buffer);
        }

        private static RepositoryData FromZip(string path, IGame game, IProgress<long> progress)
        {
            using (var zipfile = new ZipFile(path))
            {
                var modules = new List<CkanModule>();
                SortedDictionary<string, int> counts = null;
                GameVersion[] versions = null;
                Repository[] repos = null;

                foreach (ZipEntry entry in zipfile)
                {
                    ProcessFileEntry(entry.Name, () => new StreamReader(zipfile.GetInputStream(entry)).ReadToEnd(),
                                     game, entry.Offset, progress,
                                     ref modules, ref counts, ref versions, ref repos);
                }
                zipfile.Close();
                return new RepositoryData(modules, counts, versions, repos);
            }
        }

        private static void ProcessFileEntry(string                            filename,
                                             Func<string>                      getContents,
                                             IGame                             game,
                                             long                              position,
                                             IProgress<long>                   progress,
                                             ref List<CkanModule>              modules,
                                             ref SortedDictionary<string, int> counts,
                                             ref GameVersion[]                 versions,
                                             ref Repository[]                  repos)
        {
            if (filename.EndsWith("download_counts.json"))
            {
                counts = JsonConvert.DeserializeObject<SortedDictionary<string, int>>(getContents());
            }
            else if (filename.EndsWith("builds.json"))
            {
                versions = game.ParseBuildsJson(JToken.Parse(getContents()));
            }
            else if (filename.EndsWith("repositories.json"))
            {
                repos = JObject.Parse(getContents())["repositories"]
                               .ToObject<Repository[]>();
            }
            else if (filename.EndsWith(".ckan"))
            {
                log.DebugFormat("Reading CKAN data from {0}", filename);
                CkanModule module = ProcessRegistryMetadataFromJSON(getContents(), filename);
                if (module != null)
                {
                    modules.Add(module);
                }
            }
            else
            {
                // Skip things we don't want
                log.DebugFormat("Skipping archive entry {0}", filename);
            }
            progress?.Report(position);
        }

        private static CkanModule ProcessRegistryMetadataFromJSON(string metadata, string filename)
        {
            try
            {
                CkanModule module = CkanModule.FromJson(metadata);
                // FromJson can return null for the empty string
                if (module != null)
                {
                    log.DebugFormat("Module parsed: {0}", module.ToString());
                }
                return module;
            }
            catch (Exception exception)
            {
                // Alas, we can get exceptions which *wrap* our exceptions,
                // because json.net seems to enjoy wrapping rather than propagating.
                // See KSP-CKAN/CKAN-meta#182 as to why we need to walk the whole
                // exception stack.

                bool handled = false;

                while (exception != null)
                {
                    if (exception is UnsupportedKraken || exception is BadMetadataKraken)
                    {
                        // Either of these can be caused by data meant for future
                        // clients, so they're not really warnings, they're just
                        // informational.

                        log.InfoFormat("Skipping {0}: {1}", filename, exception.Message);

                        // I'd *love a way to "return" from the catch block.
                        handled = true;
                        break;
                    }

                    // Look further down the stack.
                    exception = exception.InnerException;
                }

                // If we haven't handled our exception, then it really was exceptional.
                if (handled == false)
                {
                    if (exception == null)
                    {
                        // Had exception, walked exception tree, reached leaf, got stuck.
                        log.ErrorFormat("Error processing {0} (exception tree leaf)", filename);
                    }
                    else
                    {
                        // In case whatever's calling us is lazy in error reporting, we'll
                        // report that we've got an issue here.
                        log.ErrorFormat("Error processing {0}: {1}", filename, exception.Message);
                    }

                    throw;
                }
                return null;
            }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(RepositoryData));
    }
}
