using System;
using Newtonsoft.Json;

namespace CKAN
{
    public class Repository
    {
        [JsonIgnore] public static readonly Uri default_ckan_repo_name = new Uri("default");
        [JsonIgnore] public static readonly Uri default_ckan_repo_uri = new Uri("https://github.com/KSP-CKAN/CKAN-meta/archive/master.zip");

        public string name;
        public Uri uri;
        public Boolean ckan_mirror = false;

        public Repository()
        {
        }

        public override string ToString()
        {
            return String.Format("{0} ({1})", name, uri.DnsSafeHost);
        }
    }

}
