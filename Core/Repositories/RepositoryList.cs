using Newtonsoft.Json;

using CKAN.Games;

namespace CKAN
{
    public class RepositoryList
    {
        public Repository[]     repositories = new Repository[] {};
        public BlacklistEntry[] blacklist    = new BlacklistEntry[] {};

        public static RepositoryList? DefaultRepositories(IGame game, string? userAgent)
        {
            try
            {
                return JsonConvert.DeserializeObject<RepositoryList>(
                    Net.DownloadText(game.RepositoryListURL, userAgent) ?? "");
            }
            catch
            {
                return default;
            }
        }
    }
}
