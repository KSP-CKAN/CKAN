using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CKAN.KerbalStuff
{
    public class GithubRelease
    {
        public Version version;
        public Uri download;
        public long size;
        public string author;

        public GithubRelease (JObject parsed_json)
        {
            version  = new Version( parsed_json["tag_name"].ToString() );
            author   = parsed_json["author"]["login"].ToString();
            size     = (long) parsed_json["assets"][0]["size"];
            download = new Uri( parsed_json["assets"][0]["browser_download_url"].ToString() ); 
        }
    }
}

