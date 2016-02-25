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
        //[JsonProperty] public string website;
        //[JsonProperty] public string source_code;
        //[JsonProperty] public int default_version_id;
        [JsonProperty] public string thumbnail;

        private string _pageUrl;
        private string _name;

        public CurseFile Latest()
        {
            return files.Values.First();
        }

        /// <summary>
        /// Returns the direct path to the mod's current home on Curse
        /// </summary>
        /// <returns>The home</returns>
        public string GetPageUrl()
        {
            if (_pageUrl == null)
            {
                _pageUrl = new Uri(Regex.Replace(CurseApi.ResolveRedirect(new Uri(project_url)).ToString(), "\\?.*$", "")).ToString();
            }
            return _pageUrl;
        }

        /// <summary>
        /// Returns the name of the mod
        /// </summary>
        /// <returns>The name</returns>
        public string GetName()
        {
            if (_name == null)
            {
                // Matches the longest sequence of letters and spaces ending in two letters from the beggining of the string
                // This is to filter out version information
                Match match = Regex.Match(title, "^([A-Za-z ]*[A-Za-z][A-Za-z])");
                if (match.Groups.Count > 1) _name = match.Groups[1].Value;
                else _name = "title"; ;
            }
            return _name;
        }

        public override string ToString()
        {
            //return string.Format("{0}", name);
            return string.Format("{0}", title);
        }

        public static CurseMod FromJson(string json)
        {
            CurseMod mod = JsonConvert.DeserializeObject<CurseMod>(json);
            foreach (CurseFile file in mod.files.Values)
            {
                file.Mod = mod;
            }
            return mod;
        }
    }
}