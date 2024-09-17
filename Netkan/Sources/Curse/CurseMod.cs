using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace CKAN.NetKAN.Sources.Curse
{
    internal class CurseMod
    {
        [JsonProperty] public string? license;
        [JsonProperty] public string? title;
        [JsonProperty] public string? description;
        [JsonProperty] public List<CurseFile>? files;
        [JsonProperty] public string? thumbnail;
        [JsonProperty] public int id;
        [JsonProperty] public string? game;
        [JsonProperty] public List<CurseModMember>? members;

        public string[] authors => members?.Select(m => m.username)
                                           .OfType<string>()
                                           .ToArray()
                                          ?? Array.Empty<string>();

        private string? _pageUrl;
        private string? _name;

        public CurseFile? Latest()
            => files?.First();

        public IEnumerable<CurseFile> All()
            => files ?? Enumerable.Empty<CurseFile>();

        /// <summary>
        /// Returns the static Url of the project
        /// </summary>
        /// <returns>The home</returns>
        public string GetProjectUrl()
            => "https://kerbal.curseforge.com/projects/" + id;

        /// <summary>
        /// Returns the direct path to the mod's current home on Curse
        /// </summary>
        /// <returns>The home</returns>
        public string GetPageUrl(string? userAgent)
        {
            if (_pageUrl == null)
            {
                var url = new Uri(GetProjectUrl());
                var resolved = Net.ResolveRedirect(url, userAgent);
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
                Match match = Regex.Match(title ?? "", "^([A-Za-z ]*[A-Za-z][A-Za-z])");
                if (//match.Groups is [_, var grp, ..]
                    match.Groups.Count > 1
                    && match.Groups[1] is var grp)
                {
                    _name = grp.Value;
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

        public static CurseMod? FromJson(string json, string? userAgent)
        {
            var mod = JsonConvert.DeserializeObject<CurseMod>(json);
            if (mod != null)
            {
                foreach (CurseFile file in mod.All())
                {
                    file.ModPageUrl = mod.GetPageUrl(userAgent);
                }
            }
            return mod;
        }
    }

    internal class CurseModMember
    {
        [JsonProperty] public string? title;
        [JsonProperty] public string? username;
    }
}
