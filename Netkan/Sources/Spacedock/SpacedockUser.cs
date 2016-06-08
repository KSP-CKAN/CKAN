using Newtonsoft.Json;

namespace CKAN.NetKAN.Sources.Spacedock
{
    public sealed class SpacedockUser
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("forumUsername")]
        public string FormUsername { get; set; }

        [JsonProperty("ircNick")]
        public string IrcNick { get; set; }

        [JsonProperty("redditUsername")]
        public string RedditUsername { get; set; }

        [JsonProperty("twitterUsername")]
        public string TwitterUsername { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }
    }
}
