using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ChinhDo.Transactions.FileManager;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using log4net;

using CKAN.Versioning;
using CKAN.Games;
using CKAN.Extensions;

namespace CKAN
{
    using ArchiveEntry = Tuple<CkanModule,
                               SortedDictionary<string, int>,
                               GameVersion[],
                               Repository[],
                               long>;

    using ArchiveList = Tuple<List<CkanModule>,
                              SortedDictionary<string, int>,
                              GameVersion[],
                              Repository[],
                              bool>;

    /// <summary>
    /// Represents everything we retrieve from one metadata repository
    /// </summary>
    public class RepositoryData
    {
        /// <summary>
        /// The available modules from this repository
        /// </summary>
        [JsonProperty("available_modules", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonParallelDictionaryConverter<AvailableModule>))]
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
        /// true if any module we found requires a newer client version, false otherwise
        /// </summary>
        [JsonIgnore]
        public readonly bool UnsupportedSpec;

        private RepositoryData(Dictionary<string, AvailableModule> modules,
                               SortedDictionary<string, int>       counts,
                               GameVersion[]                       versions,
                               Repository[]                        repos)
        {
            AvailableModules  = modules;
            DownloadCounts    = counts;
            KnownGameVersions = versions;
            Repositories      = repos;
        }

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
                              IEnumerable<Repository>       repos,
                              bool                          unsupportedSpec)
            : this(modules?.GroupBy(m => m.identifier)
                           .ToDictionary(grp => grp.Key,
                                         grp => new AvailableModule(grp.Key, grp)),
                   counts ?? new SortedDictionary<string, int>(),
                   (versions ?? Enumerable.Empty<GameVersion>()).ToArray(),
                   (repos ?? Enumerable.Empty<Repository>()).ToArray())
        {
            UnsupportedSpec   = unsupportedSpec;
        }

        [JsonConstructor]
        private RepositoryData()
        {
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
        /// <param name="progress">Progress notifier to receive updates of percent completion of this file</param>
        /// <returns>A repo data object or null if loading fails</returns>
        public static RepositoryData FromJson(string path, IProgress<int> progress)
        {
            try
            {
                long fileSize = new FileInfo(path).Length;
                log.DebugFormat("Trying to load repository data from {0}", path);
                // Ain't OOP grand?!
                using (var stream = File.Open(path, FileMode.Open))
                using (var progressStream = new ReadProgressStream(
                    stream,
                    progress == null
                        ? null
                        // Treat JSON parsing as the first 50%
                        : new ProgressImmediate<long>(p => progress.Report((int)(50 * p / fileSize)))))
                using (var reader = new StreamReader(progressStream))
                using (var jStream = new JsonTextReader(reader))
                {
                    var settings = new JsonSerializerSettings()
                    {
                        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                        Context = new StreamingContext(
                            StreamingContextStates.Other,
                            progress == null
                                ? null
                                : new ProgressImmediate<int>(p =>
                                    // Treat CkanModule creation as the last 50%
                                    progress.Report(50 + (p / 2)))),
                    };
                    return JsonSerializer.Create(settings)
                                         .Deserialize<RepositoryData>(jStream);
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
                default: throw new UnsupportedKraken(string.Format(Properties.Resources.NetRepoNotATarGz, path));
            }
        }

        public static RepositoryData FromStream(Stream stream, IGame game, IProgress<long> progress)
        {
            switch (FileIdentifier.IdentifyFile(stream))
            {
                case FileType.TarGz: return FromTarGzStream(stream, game, progress);
                case FileType.Zip:   return FromZipStream(stream, game, progress);
                default: throw new UnsupportedKraken(Properties.Resources.NetRepoNotATarGzStream);
            }
        }

        private static RepositoryData FromTarGz(string path, IGame game, IProgress<long> progress)
        {
            using (var inputStream = File.OpenRead(path))
            {
                return FromTarGzStream(inputStream, game, progress);
            }
        }

        private static RepositoryData FromTarGzStream(Stream inputStream, IGame game, IProgress<long> progress)
        {
            inputStream.Seek(0, SeekOrigin.Begin);
            using (var progressStream = new ReadProgressStream(inputStream, progress))
            using (var gzipStream     = new GZipInputStream(progressStream))
            using (var tarStream      = new TarInputStream(gzipStream, Encoding.UTF8))
            {
                (List<CkanModule>              modules,
                 SortedDictionary<string, int> counts,
                 GameVersion[]                 versions,
                 Repository[]                  repos,
                 bool                          unsupSpec) = AggregateArchiveEntries(archiveEntriesFromTar(tarStream, game));
                return new RepositoryData(modules, counts, versions, repos, unsupSpec);
            }
        }

        private static ParallelQuery<ArchiveEntry> archiveEntriesFromTar(TarInputStream tarStream, IGame game)
            => Partitioner.Create(getTarEntries(tarStream))
                          .AsParallel()
                          .Select(tuple => getArchiveEntry(tuple.Item1.Name,
                                                           () => tuple.Item2,
                                                           game,
                                                           tarStream.Position));

        private static IEnumerable<Tuple<TarEntry, string>> getTarEntries(TarInputStream tarStream)
        {
            TarEntry entry;
            while ((entry = tarStream.GetNextEntry()) != null)
            {
                if (!entry.Name.EndsWith(".frozen"))
                {
                    yield return new Tuple<TarEntry, string>(entry, tarStreamString(tarStream, entry));
                }
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
            using (var inputStream = File.OpenRead(path))
            {
                return FromZipStream(inputStream, game, progress);
            }
        }

        private static RepositoryData FromZipStream(Stream inputStream, IGame game, IProgress<long> progress)
        {
            inputStream.Seek(0, SeekOrigin.Begin);
            using (var progressStream = new ReadProgressStream(inputStream, progress))
            using (var zipfile = new ZipFile(progressStream))
            {
                (List<CkanModule>              modules,
                 SortedDictionary<string, int> counts,
                 GameVersion[]                 versions,
                 Repository[]                  repos,
                 bool                          unsupSpec) = AggregateArchiveEntries(archiveEntriesFromZip(zipfile, game));
                zipfile.Close();
                return new RepositoryData(modules, counts, versions, repos, unsupSpec);
            }
        }

        private static ParallelQuery<ArchiveEntry> archiveEntriesFromZip(ZipFile zipfile, IGame game)
            => zipfile.Cast<ZipEntry>()
                      .ToArray()
                      .AsParallel()
                      .Select(entry => getArchiveEntry(
                                           entry.Name,
                                           () => new StreamReader(zipfile.GetInputStream(entry)).ReadToEnd(),
                                           game,
                                           entry.Offset));

        private static ArchiveList AggregateArchiveEntries(ParallelQuery<ArchiveEntry> entries)
            => entries.Aggregate(new ArchiveList(new List<CkanModule>(), null, null, null, false),
                                 (subtotal, item) =>
                                    item == null
                                         ? subtotal
                                         : new ArchiveList(
                                             item.Item1 == null
                                                 ? subtotal.Item1
                                                 : subtotal.Item1.Append(item.Item1).ToList(),
                                             subtotal.Item2 ?? item.Item2,
                                             subtotal.Item3 ?? item.Item3,
                                             subtotal.Item4 ?? item.Item4,
                                             subtotal.Item5 || (item.Item1 == null
                                                                && item.Item2 == null
                                                                && item.Item3 == null
                                                                && item.Item4 == null)),
                                 (total, subtotal)
                                     => new ArchiveList(total.Item1.Concat(subtotal.Item1).ToList(),
                                                        total.Item2 ?? subtotal.Item2,
                                                        total.Item3 ?? subtotal.Item3,
                                                        total.Item4 ?? subtotal.Item4,
                                                        total.Item5 || subtotal.Item5),
                                 total => total);

        private static ArchiveEntry getArchiveEntry(string       filename,
                                                    Func<string> getContents,
                                                    IGame        game,
                                                    long         position)
            => filename.EndsWith(".ckan")
                ? new ArchiveEntry(ProcessRegistryMetadataFromJSON(getContents(), filename),
                                   null,
                                   null,
                                   null,
                                   position)
            : filename.EndsWith("download_counts.json")
                ? new ArchiveEntry(null,
                                   JsonConvert.DeserializeObject<SortedDictionary<string, int>>(getContents()),
                                   null,
                                   null,
                                   position)
            : filename.EndsWith("builds.json")
                ? new ArchiveEntry(null,
                                   null,
                                   game.ParseBuildsJson(JToken.Parse(getContents())),
                                   null,
                                   position)
            : filename.EndsWith("repositories.json")
                ? new ArchiveEntry(null,
                                   null,
                                   null,
                                   JObject.Parse(getContents())["repositories"]
                                          .ToObject<Repository[]>(),
                                   position)
            : null;

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
                if (!handled)
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
