using System;
using Newtonsoft.Json;

namespace CKAN
{
    public class Repository
    {
        [JsonIgnore] public static readonly string default_ckan_repo_name = "default";
        [JsonIgnore] public static readonly Uri default_ckan_repo_uri = new Uri("https://github.com/KSP-CKAN/CKAN-meta/archive/master.tar.gz");
        [JsonIgnore] public static readonly Uri default_repo_master_list = new Uri("https://raw.githubusercontent.com/KSP-CKAN/CKAN-meta/master/repositories.json");

        public string name;
        public Uri uri;
        public int priority = 0;
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

}
