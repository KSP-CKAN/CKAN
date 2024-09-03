using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CKAN.NetKAN.Sources.Spacedock
{
    internal class SpacedockMod
    {
        [JsonProperty] public int              id; // SDID
        [JsonProperty] public string?          license;
        [JsonProperty] public string?          name;
        [JsonProperty] public string?          short_description;
        [JsonProperty] public string?          author;
        [JsonProperty] public SDVersion[]?     versions;
        [JsonProperty] public string?          website;
        [JsonProperty] public string?          source_code;
        [JsonProperty] public int              default_version_id;
        [JsonProperty] public SpacedockUser[]? shared_authors;
        [JsonProperty] public Uri?             background;

        public SDVersion Latest()
        {
            // The version we want is specified by `default_version_id`, it's not just
            // the latest. See GH #214. Thanks to @Starstrider42 for spotting this.

            var latest =
                from release in versions
                where release.id == default_version_id
                select release
            ;

            // There should only ever be one.
            return latest.First();
        }

        public IEnumerable<SDVersion> All()
            => versions ?? Enumerable.Empty<SDVersion>();

        /// <summary>
        /// Returns the path to the mod's home on SpaceDock
        /// </summary>
        /// <returns>The home.</returns>
        public Uri GetPageUrl()
            => new Uri(name != null
                           ? $"{SpacedockApi.SpacedockBase}mod/{id}/{name.Replace(" ", "%20")}"
                           : $"{SpacedockApi.SpacedockBase}mod/{id}");

        public override string ToString()
            => string.Format("{0}", name);

        public static SpacedockMod? FromJson(string json)
            => JsonConvert.DeserializeObject<SpacedockMod>(json);
    }
}
