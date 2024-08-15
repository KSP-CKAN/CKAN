using System;
using System.ComponentModel;

using Newtonsoft.Json;

using CKAN.Games;

namespace CKAN
{
    public class Repository : IEquatable<Repository>
    {
        [JsonIgnore]
        public static string default_ckan_repo_name => Properties.Resources.RepositoryDefaultName;

        public string name;
        public Uri    uri;
        public int    priority = 0;

        // These are only sourced from repositories.json

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        public bool   x_mirror;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string x_comment;

        public Repository()
        { }

        public Repository(string name, Uri uri)
        {
            this.name = name;
            this.uri  = uri;
        }

        public Repository(string name, string uri)
            : this(name, new Uri(uri))
        { }

        public Repository(string name, string uri, int priority)
            : this(name, uri)
        {
            this.priority = priority;
        }

        public override bool Equals(object other)
            => Equals(other as Repository);

        public bool Equals(Repository other)
            => other != null && uri == other.uri;

        public override int GetHashCode()
            => uri?.GetHashCode() ?? 0;

        public override string ToString()
            => string.Format("{0} ({1}, {2})", name, priority, uri);

        public static Repository DefaultGameRepo(IGame game)
            => new Repository($"{game.ShortName}-{default_ckan_repo_name}",
                              game.DefaultRepositoryURL);
    }
}
