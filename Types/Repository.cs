using System;
using Newtonsoft.Json;

namespace CKAN
{
    public class Repository
    {
        [JsonIgnore] public static readonly string default_ckan_repo_name = "default";
        [JsonIgnore] public static readonly Uri default_ckan_repo_uri = new Uri("https://github.com/KSP-CKAN/CKAN-meta/archive/master.zip");
        [JsonIgnore] public static readonly Uri default_repo_master_list = new Uri("http://api.ksp-ckan.org/mirrors");

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
