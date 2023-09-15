using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using log4net;

using CKAN;
using CKAN.Games.KerbalSpaceProgram;

namespace Tests.Data
{
    /// <summary>
    /// Provides a RepositoryDataManager given some repositories,
    /// with automatic cleanup of the cache dir
    /// </summary>
    public class TemporaryRepositoryData : IDisposable
    {
        public TemporaryRepositoryData(IUser user, params Repository[] repos)
        {
            repoDataDir = TestData.NewTempDir();
            Manager = new RepositoryDataManager(repoDataDir);
            if (repos.Length > 0)
            {
                Manager.Update(repos, new KerbalSpaceProgram(),
                               false, new NetAsyncDownloader(user), user);
            }
        }

        public TemporaryRepositoryData(IUser user, IEnumerable<Repository> repos)
            : this(user, repos.ToArray())
        {
        }

        public TemporaryRepositoryData(IUser user, Dictionary<Repository, RepositoryData> reposAndData)
            : this(user)
        {
            foreach (var kvp in reposAndData)
            {
                var repo = kvp.Key;
                var data = kvp.Value;

                log.DebugFormat("Saving data to {0}", Path.Combine(repoDataDir, $"{NetFileCache.CreateURLHash(repo.uri)}.json"));
                data.SaveTo(Path.Combine(repoDataDir, $"{NetFileCache.CreateURLHash(repo.uri)}.json"));
            }
        }

        public void Dispose()
        {
            Directory.Delete(repoDataDir, true);
        }

        public readonly RepositoryDataManager Manager;

        private readonly string repoDataDir;

        private static readonly ILog log = LogManager.GetLogger(typeof(TemporaryRepositoryData));
    }
}
