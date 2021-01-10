using System;
using System.Net;
using Newtonsoft.Json;
using CKAN.Games;

namespace CKAN
{
    public class Repository
    {
        [JsonIgnore] public static readonly string default_ckan_repo_name = "default";

        public string  name;
        public Uri     uri;
        public string  last_server_etag;
        public int     priority = 0;
        public Boolean ckan_mirror = false;

        public Repository()
        {
        }

        public Repository(string name, string uri)
        {
            this.name = name;
            this.uri = new Uri(uri);
        }

        public Repository(string name, string uri, int priority)
        {
            this.name = name;
            this.uri = new Uri(uri);
            this.priority = priority;
        }

        public Repository(string name, Uri uri)
        {
            this.name = name;
            this.uri = uri;
        }

        public override string ToString()
        {
            return String.Format("{0} ({1}, {2})", name, priority, uri);
        }
    }

    public struct RepositoryList
    {
        public Repository[] repositories;

        public static RepositoryList DefaultRepositories(IGame game)
        {
            try
            {
                return JsonConvert.DeserializeObject<RepositoryList>(
                    Net.DownloadText(game.RepositoryListURL)
                );
            }
            catch
            {
                return default(RepositoryList);
            }
        }

    }

}
