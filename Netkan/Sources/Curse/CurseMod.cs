using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace CKAN.NetKAN.Sources.Curse
{
    internal class CurseMod
    {
        [JsonProperty] public string project_url; // CID
        [JsonProperty] public string license;
        [JsonProperty] public string title;
        //[JsonProperty] public string short_description;
        [JsonProperty] public string[] authors;
        [JsonProperty] public Dictionary<int, CurseFile> files;
        //[JsonProperty] public Uri website;
        //[JsonProperty] public Uri source_code;
        //[JsonProperty] public int default_version_id;
        [JsonProperty] public string thumbnail;

        public CurseFile Latest()
        {
            return files.Values.First();
        }

        /// <summary>
        /// Returns the direct path to the mod's current home on Curse
        /// </summary>
        /// <returns>The home.</returns>
        public Uri GetPageUrl()
        {
            return new Uri(Regex.Replace(CurseApi.ResolveRedirect(new Uri(project_url)).ToString(), "\\?.*$", ""));
        }

        /// <summary>
        /// Returns the direct path to the file
        /// </summary>
        /// <returns>The home.</returns>
        public Uri GetDownloadUrl(CurseFile file)
        {
            return CurseApi.ResolveRedirect(new Uri(GetPageUrl().ToString() + "/files/" + file.id + "/download"));
        }

        public override string ToString()
        {
            //return string.Format("{0}", name);
            return string.Format("{0}", title);
        }

        public static CurseMod FromJson(string json)
        {
            return JsonConvert.DeserializeObject<CurseMod>(json);
        }
    }
}