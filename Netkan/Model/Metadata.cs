using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YamlDotNet.RepresentationModel;

using CKAN.Versioning;
using CKAN.Extensions;
using CKAN.NetKAN.Transformers;

namespace CKAN.NetKAN.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class Metadata
    {
        public Metadata(JObject json)
        {
            AllJson = json;
            try
            {
                JsonSerializer.CreateDefault(UtcSettings)
                              .Populate(json.CreateReader(), this);
            }
            catch (TargetInvocationException exc)
            {
                throw exc.GetBaseException();
            }
            catch (JsonException jsonExc)
            {
                throw new BadMetadataKraken(null, jsonExc.Message, jsonExc);
            }
        }

        public Metadata(YamlMappingNode yaml)
            : this(yaml.ToJObject())
        {
        }

        [JsonProperty("identifier", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                                    NullValueHandling    = NullValueHandling.Ignore)]
        [DefaultValue("")]
        public readonly string Identifier = "";

        [JsonProperty("$kref", NullValueHandling = NullValueHandling.Ignore)]
        public readonly RemoteRef? Kref;

        [JsonProperty("$vref", NullValueHandling = NullValueHandling.Ignore)]
        public readonly RemoteRef? Vref;

        [JsonProperty("spec_version", NullValueHandling = NullValueHandling.Ignore)]
        public readonly ModuleVersion? SpecVersion;

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public readonly ModuleVersion? Version;

        [JsonProperty("x_netkan_epoch", NullValueHandling = NullValueHandling.Ignore)]
        public readonly uint? Epoch;

        [JsonProperty("license")]
        [JsonConverter(typeof(JsonSingleOrArrayConverter<License>))]
        public readonly List<License>? Licenses;

        public bool Redistributable => Licenses?.Any(lic => lic.Redistributable)
                                               ?? false;

        [JsonProperty("download", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonSingleOrArrayConverter<Uri>))]
        public readonly List<Uri>? Download;

        public Uri? FallbackDownload => Identifier != null
                                     && Redistributable
                                     && Version?.ToString().Replace(':', '-') is string verStr
                                     && (JObject?)AllJson["download_hash"] is JObject hashes
                                     && (string?)hashes["sha1"] is string sha1
                                         ? new Uri($"https://archive.org/download/{Identifier}-{verStr}/{sha1[..8]}-{Identifier}-{verStr}.zip")
                                         : null;

        public string[] Hosts => Download?.Select(u => u.Host)
                                          .ToArray()
                                         ?? Array.Empty<string>();

        [JsonProperty("release_date", NullValueHandling = NullValueHandling.Ignore)]
        public readonly DateTime? ReleaseDate;

        [JsonProperty("release_status", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                                        NullValueHandling    = NullValueHandling.Ignore)]
        [DefaultValue(CKAN.ReleaseStatus.stable)]
        public readonly ReleaseStatus? ReleaseStatus = CKAN.ReleaseStatus.stable;

        public bool Prerelease => ReleaseStatus is CKAN.ReleaseStatus.development
                                                or CKAN.ReleaseStatus.testing;

        [JsonProperty("resources", NullValueHandling = NullValueHandling.Ignore)]
        public readonly Dictionary<string, string>? Resources;

        [JsonProperty("x_netkan_staging", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                                          NullValueHandling    = NullValueHandling.Ignore)]
        [DefaultValue(false)]
        public readonly bool Staged = false;

        [JsonProperty("x_netkan_staging_reason", NullValueHandling = NullValueHandling.Ignore)]
        public readonly string? StagingReason;

        [JsonProperty("x_netkan_allow_out_of_order", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                                                     NullValueHandling    = NullValueHandling.Ignore)]
        [DefaultValue(false)]
        public readonly bool AllowOutOfOrder = false;

        [JsonProperty("x_netkan_trust_version_file", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                                                     NullValueHandling    = NullValueHandling.Ignore)]
        [DefaultValue(false)]
        public readonly bool TrustVersionFile = false;

        [JsonProperty("x_netkan_force_v", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                                          NullValueHandling    = NullValueHandling.Ignore)]
        [DefaultValue(false)]
        public readonly bool ForceV = false;

        [JsonProperty("x_netkan_license_ok", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                                             NullValueHandling    = NullValueHandling.Ignore)]
        [DefaultValue(false)]
        public readonly bool SkipLicenseValidation = false;

        [JsonProperty("x_netkan_optimus_prime", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                                                NullValueHandling    = NullValueHandling.Ignore)]
        [DefaultValue(false)]
        public readonly bool OptimusPrime = false;

        [JsonProperty("x_netkan_version_edit", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonVersionEditConverter))]
        public readonly VersionEditOptions? VersionEdit;

        [JsonProperty("x_netkan_override", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonSingleOrArrayConverter<OverrideOptions>))]
        public readonly List<OverrideOptions>? Overrides;

        [JsonProperty("x_netkan_jenkins", NullValueHandling = NullValueHandling.Ignore)]
        public readonly JenkinsOptions? Jenkins;

        [JsonProperty("x_netkan_github", NullValueHandling = NullValueHandling.Ignore)]
        public readonly GithubOptions? Github;

        [JsonProperty("x_netkan_gitlab", NullValueHandling = NullValueHandling.Ignore)]
        public readonly GitlabOptions? Gitlab;

        public readonly JObject AllJson;

        public JObject Json()
            => (JObject)AllJson.DeepClone();

        public static Metadata Merge(Metadata[] modules)
            => //modules is [var m]
               modules.Length == 1
               && modules[0] is var m
                   ? m
                   : PropertySortTransformer.SortProperties(
                       new Metadata(MergeJson(modules.Select(m => m.AllJson)
                                                     .ToArray())));

        public Metadata MergeFrom(JObject[] jsons)
        {
            var mergeSettings = new JsonMergeSettings()
            {
                MergeArrayHandling     = MergeArrayHandling.Replace,
                MergeNullValueHandling = MergeNullValueHandling.Merge,
            };
            var main = Json();
            foreach (var other in jsons)
            {
                main.Merge(other, mergeSettings);
            }
            return new Metadata(main);
        }

        private static JObject MergeJson(JObject[] jsons)
        {
            var downloads = jsons.SelectMany(json => json["download"] is JArray arr
                                                         ? arr.Children()
                                                         : Enumerable.Repeat(json["download"], 1))
                                 .Distinct()
                                 .ToArray();
            var first = jsons.First();
            foreach (var other in jsons.Skip(1))
            {
                if ((string?)first["download_size"] != (string?)other["download_size"]
                    || (string?)first["download_hash"]?["sha1"] != (string?)other["download_hash"]?["sha1"]
                    || (string?)first["download_hash"]?["sha256"] != (string?)other["download_hash"]?["sha256"])
                {
                    // Can't treat the URLs as equivalent if they're different files
                    throw new Kraken(string.Format(
                        "Download from {0} does not match download from {1}",
                        first["download"], other["download"]));
                }
                first.Merge(other, mergeSettings);
            }
            // Merge game version compatibility
            GameVersion.SetJsonCompatibility(first, null, null, null);
            // Only generate array if multiple URLs
            first["download"] = downloads.Length == 1
                ? downloads.First()
                : JArray.FromObject(downloads);
            return first;
        }

        private static readonly JsonSerializerSettings UtcSettings = new JsonSerializerSettings()
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
        };

        private static readonly JsonMergeSettings mergeSettings = new JsonMergeSettings()
        {
            MergeArrayHandling     = MergeArrayHandling.Replace,
            MergeNullValueHandling = MergeNullValueHandling.Merge,
        };
    }
}
