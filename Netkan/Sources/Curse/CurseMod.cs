using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace CKAN.NetKAN.Sources.Curse
{
    internal class CurseMod
    {
        [JsonProperty] public string license;
        [JsonProperty] public string title;
        [JsonProperty] public string description;
        [JsonProperty] public List<CurseFile> files;
        [JsonProperty] public string thumbnail;
        [JsonProperty] public int id;
        [JsonProperty] public string game;
        [JsonProperty] public List<CurseModMember> members;

        public string[] authors => members.Select(m => m.username).ToArray();

        private string _pageUrl;
        private string _name;

        public CurseFile Latest()
        {
            return files.First();
        }

        public IEnumerable<CurseFile> All()
        {
            return files;
        }

        /// <summary>
        /// Returns the static Url of the project
        /// </summary>
        /// <returns>The home</returns>
        public string GetProjectUrl()
        {
            return "https://kerbal.curseforge.com/projects/" + id;
        }

        /// <summary>
        /// Returns the direct path to the mod's current home on Curse
        /// </summary>
        /// <returns>The home</returns>
        public string GetPageUrl()
        {
            if (_pageUrl == null)
            {
                var url = new Uri(GetProjectUrl());
                var resolved = Net.ResolveRedirect(url);
                if (resolved == null)
                {
                    throw new Kraken($"Too many redirects resolving: {url}");
                }
                _pageUrl = new Uri(Regex.Replace(resolved.ToString(), "\\?.*$", "")).ToString();
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
                if (match.Groups.Count > 1)
                {
                    _name = match.Groups[1].Value;
                }
                else
                {
                    _name = "title";
                };
            }
            return _name;
        }

        public override string ToString()
        {
            return string.Format("{0}", title);
        }

        public static CurseMod FromJson(string json)
        {
            CurseMod mod = JsonConvert.DeserializeObject<CurseMod>(json);
            if (mod != null)
            {
                foreach (CurseFile file in mod.files)
                {
                    file.ModPageUrl = mod.GetPageUrl();
                }
            }
            return mod;
        }
    }

    internal class CurseModMember
    {

        [JsonProperty] public string title;
        [JsonProperty] public string username;

    }
}
