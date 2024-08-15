using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;
using ChinhDo.Transactions.FileManager;
using log4net;

using CKAN.Extensions;
using CKAN.Games;

namespace CKAN
{
    /// <summary>
    /// Retrieves data from repositories and provides access to it.
    /// Data is cached in memory and on disk to minimize reloading.
    /// </summary>
    public class RepositoryDataManager
    {
        /// <summary>
        /// Instantiate a repo data manager
        /// </summary>
        /// <param name="path">Directory to use as cache, defaults to APPDATA/CKAN/repos if null</param>
        public RepositoryDataManager(string path = null)
        {
            reposDir = path ?? defaultReposDir;
            Directory.CreateDirectory(reposDir);
            loadETags();
        }

        #region Provide access to the data

        /// <summary>
        /// Return the cached available modules from a given set of repositories
        /// for a given identifier
        /// </summary>
        /// <param name="repos">The repositories we want to use</param>
        /// <param name="identifier">The identifier to look up</param>
        /// <returns>Sequence of available modules, if any</returns>
        public IEnumerable<AvailableModule> GetAvailableModules(IEnumerable<Repository> repos,
                                                                string identifier)
            => GetRepoDatas(repos)
                .Where(data => data.AvailableModules != null)
                .Select(data => data.AvailableModules.TryGetValue(identifier, out AvailableModule am)
                                    ? am : null)
                .Where(am => am != null);

        /// <summary>
        /// Return the cached available module dictionaries for a given set of repositories.
        /// That's a bit low-level for a public function, but the CompatibilitySorter
        /// makes some complex use of these dictionaries.
        /// </summary>
        /// <param name="repos">The repositories we want to use</param>
        /// <returns>Sequence of available module dictionaries</returns>
        public IEnumerable<Dictionary<string, AvailableModule>> GetAllAvailDicts(IEnumerable<Repository> repos)
            => GetRepoDatas(repos).Select(data => data.AvailableModules)
                                  .Where(availMods => availMods != null
                                                      && availMods.Count > 0);

        /// <summary>
        /// Return the cached AvailableModule objects from the given repositories.
        /// This should not hit the network; only Update() should do that.
        /// </summary>
        /// <param name="repos">Sequence of repositories to get modules from</param>
        /// <returns>Sequence of available modules</returns>
        public IEnumerable<AvailableModule> GetAllAvailableModules(IEnumerable<Repository> repos)
            => GetAllAvailDicts(repos).SelectMany(d => d.Values);

        /// <summary>
        /// Get the cached download count for a given identifier
        /// </summary>
        /// <param name="repos">The repositories from which to get download count data</param>
        /// <param name="identifier">The identifier to look up</param>
        /// <returns>Number if found, else null</returns>
        public int? GetDownloadCount(IEnumerable<Repository> repos, string identifier)
            => GetRepoDatas(repos)
                .Select(data => data.DownloadCounts.TryGetValue(identifier, out int count)
                                    ? (int?)count : null)
                .FirstOrDefault(count => count != null);

        #endregion

        #region Manage the repo cache and files

        /// <summary>
        /// Load the cached data for the given repos, WITHOUT any network calls
        /// </summary>
        /// <param name="repos">Repositories for which to load data</param>
        /// <param name="progress">Progress object for reporting percentage complete</param>
        public void Prepopulate(List<Repository> repos, IProgress<int> percentProgress)
        {
            // Look up the sizes of repos that have uncached files
            var reposAndSizes = repos.Where(r => r.uri != null && !repositoriesData.ContainsKey(r))
                                     .Select(r => new Tuple<Repository, string>(r, GetRepoDataPath(r)))
                                     .Where(tuple => File.Exists(tuple.Item2))
                                     .Select(tuple => new Tuple<Repository, long>(tuple.Item1,
                                                                                  new FileInfo(tuple.Item2).Length))
                                     .ToList();
            // Translate from file group offsets to percent
            var progress = new ProgressScalePercentsByFileSizes(
                percentProgress, reposAndSizes.Select(tuple => tuple.Item2));
            foreach (var repo in reposAndSizes.Select(tuple => tuple.Item1))
            {
                LoadRepoData(repo, progress);
                progress.NextFile();
            }
        }

        public TimeSpan LastUpdate(IEnumerable<Repository> repos)
            => repos.Distinct().Where(r => repoDataStale(r))
                               .Select(r => RepoUpdateTimestamp(r))
                               .OfType<DateTime>()
                               .Select(dt => DateTime.Now - dt)
                               .DefaultIfEmpty(TimeSpan.Zero)
                               .Min();

        public static readonly TimeSpan TimeTillStale     = TimeSpan.FromDays(3);
        public static readonly TimeSpan TimeTillVeryStale = TimeSpan.FromDays(14);

        /// <summary>
        /// Values to describe the result of an attempted repository update.
        /// Failure is actually handled by throwing exceptions, so I'm not sure we need that.
        /// </summary>
        public enum UpdateResult
        {
            Failed,
            Updated,
            NoChanges,
            OutdatedClient,
        }

        /// <summary>
        /// Retrieve repository data from the network and store it in the cache
        /// </summary>
        /// <param name="repos">Repositories for which we want to retrieve data</param>
        /// <param name="game">The game for which these repo has data, used to get the default URL and for parsing the game versions because the format can vary</param>
        /// <param name="skipETags">True to force downloading regardless of the etags, false to skip if no changes on remote</param>
        /// <param name="downloader">The object that will do the actual downloading for us</param>
        /// <param name="user">Object for reporting messages and progress to the UI</param>
        /// <returns>Updated if we changed any of the available modules, NoChanges if already up to date</returns>
        public UpdateResult Update(Repository[]       repos,
                                   IGame              game,
                                   bool               skipETags,
                                   NetAsyncDownloader downloader,
                                   IUser              user)
        {
            // Get latest copy of the game versions data (remote build map)
            user.RaiseMessage(Properties.Resources.NetRepoUpdatingBuildMap);
            game.RefreshVersions();

            // Check if any ETags have changed, quit if not
            user.RaiseProgress(Properties.Resources.NetRepoCheckingForUpdates, 0);
            var toUpdate = repos.DefaultIfEmpty(Repository.DefaultGameRepo(game))
                                .DistinctBy(r => r.uri)
                                .Where(r => r.uri != null
                                            && (r.uri.IsFile
                                                || skipETags
                                                || repoDataStale(r)))
                                .ToArray();
            if (toUpdate.Length < 1)
            {
                // Update timestamp for already up to date repos
                foreach (var f in repos.Select(GetRepoDataPath))
                {
                    File.SetLastWriteTimeUtc(f, DateTime.UtcNow);
                }
                user.RaiseProgress(Properties.Resources.NetRepoAlreadyUpToDate, 100);
                user.RaiseMessage(Properties.Resources.NetRepoNoChanges);
                return UpdateResult.NoChanges;
            }

            downloader.onOneCompleted += setETag;
            try
            {
                // Download metadata
                var targets = toUpdate.Select(r => new NetAsyncDownloader.DownloadTargetStream(r.uri))
                                      .ToArray();
                downloader.DownloadAndWait(targets);

                // If we get to this point, the downloads were successful
                // Load them
                string msg = "";
                var progress = new ProgressFilesOffsetsToPercent(
                    new Progress<int>(p => user.RaiseProgress(msg, p)),
                    targets.Select(t => t.size));
                foreach ((var repo, var target) in toUpdate.Zip(targets))
                {
                    msg = string.Format(Properties.Resources.NetRepoLoadingModulesFromRepo,
                                        repo.name);
                    log.InfoFormat("Loading repo stream...");
                    try
                    {
                        using (target)
                        {
                            // Load the stream, save to in memory cache
                            var repoData = repositoriesData[repo] =
                                RepositoryData.FromStream(target.contents, game, progress);
                            // Save parsed data to disk
                            log.DebugFormat("Saving data for {0} repo...", repo.name);
                            repoData.SaveTo(GetRepoDataPath(repo));
                        }
                    }
                    catch (UnsupportedKraken kraken)
                    {
                        // Show parsing errors in the Downloads Failed popup
                        throw new DownloadErrorsKraken(Array.IndexOf(toUpdate, repo), kraken);
                    }
                    progress.NextFile();
                }
                // Commit these etags to disk
                saveETags();

                // Fire an event so affected registry objects can clear their caches
                Updated?.Invoke(toUpdate);
            }
            catch (DownloadErrorsKraken exc)
            {
                loadETags();
                throw new DownloadErrorsKraken(
                    // Renumber the exceptions based on the original repo list
                    exc.Exceptions.Select(kvp => new KeyValuePair<int, Exception>(
                                                     Array.IndexOf(repos, toUpdate[kvp.Key]),
                                                     kvp.Value))
                                  .ToList());
            }
            catch (Exception exc)
            {
                foreach (var e in exc.TraverseNodes(ex => ex.InnerException)
                                     .Reverse())
                {
                    log.Error("Repository update failed", e);
                }
                // Reset etags on errors
                loadETags();
                throw;
            }
            finally
            {
                // Teardown event handler with or without an exception
                downloader.onOneCompleted -= setETag;
            }

            return repositoriesData.Values.Any(repoData => repoData.UnsupportedSpec)
                ? UpdateResult.OutdatedClient
                : UpdateResult.Updated;
        }

        /// <summary>
        /// Fired when repository data changes so registries can invalidate their
        /// caches of available module data
        /// </summary>
        public event Action<Repository[]> Updated;

        #region ETags

        private void loadETags()
        {
            try
            {
                etags = JsonConvert.DeserializeObject<Dictionary<Uri, string>>(File.ReadAllText(etagsPath))
                        // An empty or all-null file can deserialize as null
                        ?? new Dictionary<Uri, string>();
            }
            catch
            {
                // We set etags to an empty dictionary at startup, so it won't be null
            }
        }

        private void saveETags()
        {
            TxFileManager file_transaction = new TxFileManager();
            file_transaction.WriteAllText(etagsPath, JsonConvert.SerializeObject(etags, Formatting.Indented));
        }

        private void setETag(NetAsyncDownloader.DownloadTarget target, Exception error, string etag)
        {
            var url = target.urls.First();
            if (etag != null)
            {
                etags[url] = etag;
            }
            else if (etags.ContainsKey(url))
            {
                etags.Remove(url);
            }
        }

        private bool repoDataStale(Repository r)
            // No ETag on file
            => !etags.TryGetValue(r.uri, out string etag)
               // No data on disk
               || !File.Exists(GetRepoDataPath(r))
               // Current ETag doesn't match
               || etag != Net.CurrentETag(r.uri);

        #endregion

        private RepositoryData GetRepoData(Repository repo)
            => repositoriesData.TryGetValue(repo, out RepositoryData data)
                ? data
                : LoadRepoData(repo, null);

        private RepositoryData LoadRepoData(Repository repo, IProgress<int> progress)
        {
            var path = GetRepoDataPath(repo);
            log.DebugFormat("Looking for data in {0}", path);
            var data = RepositoryData.FromJson(path, progress);
            if (data != null)
            {
                log.Debug("Found it! Adding...");
                repositoriesData.Add(repo, data);
            }
            return data;
        }

        private IEnumerable<RepositoryData> GetRepoDatas(IEnumerable<Repository> repos)
            => repos?.OrderBy(repo => repo.priority)
                     .ThenBy(repo => repo.name)
                     .Select(repo => GetRepoData(repo))
                     .Where(data => data != null)
               ?? Enumerable.Empty<RepositoryData>();

        private DateTime? RepoUpdateTimestamp(Repository repo)
            => FileTimestamp(GetRepoDataPath(repo));

        private static DateTime? FileTimestamp(string path)
            => File.Exists(path) ? (DateTime?)File.GetLastWriteTime(path)
                                 : null;

        private string etagsPath => Path.Combine(reposDir, "etags.json");
        private Dictionary<Uri, string> etags = new Dictionary<Uri, string>();

        private readonly Dictionary<Repository, RepositoryData> repositoriesData =
            new Dictionary<Repository, RepositoryData>();

        private string GetRepoDataPath(Repository repo)
            => GetRepoDataPath(repo, NetFileCache.CreateURLHash(repo?.uri));

        private string GetRepoDataPath(Repository repo, string hash)
            => Directory.EnumerateFiles(reposDir)
                        .Where(path => Path.GetFileName(path).StartsWith(hash))
                        .DefaultIfEmpty(Path.Combine(reposDir, $"{hash}-{repo.name}.json"))
                        .First();

        private readonly string reposDir;
        private static readonly string defaultReposDir = Path.Combine(CKANPathUtils.AppDataPath, "repos");

        #endregion

        private static readonly ILog log = LogManager.GetLogger(typeof(RepositoryDataManager));
    }
}
