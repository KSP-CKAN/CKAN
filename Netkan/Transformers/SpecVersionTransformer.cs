using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;
using log4net;

using CKAN.Versioning;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Extensions;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that sets the spec version to match the metadata.
    /// </summary>
    internal sealed class SpecVersionTransformer : ITransformer
    {
        /// <summary>
        /// Defines the name of this transformer
        /// </summary>
        public string Name => "spec_version";

        public IEnumerable<Metadata> Transform(Metadata         metadata,
                                               TransformOptions opts)
        {
            var json       = metadata.Json();
            var minVersion = MinimumSpecVersion(json);
            if (metadata.SpecVersion == null || metadata.SpecVersion != minVersion)
            {
                log.InfoFormat("Setting spec version {0}", minVersion);
                json[Metadata.SpecVersionPropertyName] = minVersion.ToSpecVersionJson();
                yield return new Metadata(json);
            }
            else
            {
                yield return metadata;
            }
        }

        private static ModuleVersion MinimumSpecVersion(JObject json)
            // Add new stuff at the top, versions in this function should be in descending order
            => json["download_hash"] is JObject hashes
               && (!hashes.ContainsKey("sha256") || !hashes.ContainsKey("sha1")) ? v1p35

             : json["download"] is JArray ? v1p34

             : AllRelationships(json).Any(rel => rel.ContainsKey("any_of")
                                              && rel.ContainsKey("choice_help_text")) ? v1p31

             : HasLicense(json, "MPL-2.0") ? v1p30

             : (json["install"] as JArray)?.OfType<JObject>().Any(stanza =>
                 ((string)stanza["install_to"]).StartsWith("Ships/Script")
                 || ((string)stanza["install_to"] == "Ships" && (
                     // find: .../Script, install_to: Ships
                     ((string)stanza["find"])?.Split(new char[] {'/'})?.LastOrDefault() == "Script"
                     // file: .../Script, install_to: Ships
                     || ((string)stanza["file"])?.Split(new char[] {'/'})?.LastOrDefault() == "Script"
                     // install_to: Ships, as: Script
                     || (((string)stanza["as"])?.EndsWith("Script") ?? false)))) ?? false ? v1p29

             : (string)json["kind"] == "dlc" ? v1p28

             : json.ContainsKey("replaced_by") ? v1p26

             : AllRelationships(json).Any(rel => rel.ContainsKey("any_of")) ? v1p26

             : (json["install"] as JArray)?.OfType<JObject>()
                                           .Any(stanza => (string)stanza["install_to"] == "Missions") ?? false ? v1p25

             : (json["install"] as JArray)?.OfType<JObject>()
                                           .Any(stanza => stanza.ContainsKey("include_only")
                                                      || stanza.ContainsKey("include_only_regexp")) ?? false ? v1p24

             : HasLicense(json, "Unlicense") ? v1p18

             : (json["install"] as JArray)?.OfType<JObject>()
                                           .Any(stanza => stanza.ContainsKey("as")) ?? false ? v1p18

             : json.ContainsKey("ksp_version_strict") ? v1p16

             : (json["install"] as JArray)?.OfType<JObject>()
                                           .Any(stanza => ((string)stanza["install_to"]).StartsWith("Ships/@thumbs")) ?? false ? v1p16

             : (json["install"] as JArray)?.OfType<JObject>()
                                           .Any(stanza => stanza.ContainsKey("find_matches_files")) ?? false ? v1p16

             : (json["install"] as JArray)?.OfType<JObject>()
                                           .Any(stanza => (string)stanza["install_to"] == "Scenarios") ?? false ? v1p14

             : (json["install"] as JArray)?.OfType<JObject>()
                                           .Any(stanza => ((string)stanza["install_to"]).StartsWith("Ships/")) ?? false ? v1p12

             : (json["install"] as JArray)?.OfType<JObject>()
                                           .Any(stanza => stanza.ContainsKey("find_regexp")
                                                       || stanza.ContainsKey("filter_regexp")) ?? false ? v1p10

             : json["license"] is JArray ? v1p8

             : (string)json["kind"] == "metapackage" ? v1p6

             : (json["install"] as JArray)?.OfType<JObject>()
                                           .Any(stanza => stanza.ContainsKey("find")) ?? false ? v1p4

             : HasLicense(json, "WTFPL") ? v1p2

             : json.ContainsKey("supports") ? v1p2

             : (json["install"] as JArray)?.OfType<JObject>()
                                           .Any(stanza => ((string)stanza["install_to"]).StartsWith("GameData/")) ?? false ? v1p2

             : v1p0;

        private static bool HasLicense(JObject json,
                                       string  name)
            => json["license"] is JArray array ?  array.Contains(name)
             : json["license"] is JToken token && ((string)token) == name;

        private static IEnumerable<JObject> AllRelationships(JObject json)
            => relProps.SelectMany(p => json[p] is JArray array ? array.OfType<JObject>()
                                                                : Enumerable.Empty<JObject>());

        private static readonly string[] relProps = new string[]
        {
            "depends",
            "recommends",
            "suggests",
            "conflicts",
            "supports"
        };

        private static readonly ModuleVersion v1p0  = new ModuleVersion("v1.0");
        private static readonly ModuleVersion v1p2  = new ModuleVersion("v1.2");
        private static readonly ModuleVersion v1p4  = new ModuleVersion("v1.4");
        private static readonly ModuleVersion v1p6  = new ModuleVersion("v1.6");
        private static readonly ModuleVersion v1p8  = new ModuleVersion("v1.8");
        private static readonly ModuleVersion v1p10 = new ModuleVersion("v1.10");
        private static readonly ModuleVersion v1p12 = new ModuleVersion("v1.12");
        private static readonly ModuleVersion v1p14 = new ModuleVersion("v1.14");
        private static readonly ModuleVersion v1p16 = new ModuleVersion("v1.16");
        private static readonly ModuleVersion v1p18 = new ModuleVersion("v1.18");
        private static readonly ModuleVersion v1p24 = new ModuleVersion("v1.24");
        private static readonly ModuleVersion v1p25 = new ModuleVersion("v1.25");
        private static readonly ModuleVersion v1p26 = new ModuleVersion("v1.26");
        private static readonly ModuleVersion v1p28 = new ModuleVersion("v1.28");
        private static readonly ModuleVersion v1p29 = new ModuleVersion("v1.29");
        private static readonly ModuleVersion v1p30 = new ModuleVersion("v1.30");
        private static readonly ModuleVersion v1p31 = new ModuleVersion("v1.31");
        private static readonly ModuleVersion v1p34 = new ModuleVersion("v1.34");
        private static readonly ModuleVersion v1p35 = new ModuleVersion("v1.35");

        private static readonly ILog log = LogManager.GetLogger(typeof(SpecVersionTransformer));
    }
}
