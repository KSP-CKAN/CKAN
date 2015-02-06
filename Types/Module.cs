using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using log4net;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace CKAN
{
    public class RelationshipDescriptor
    {
        public string max_version;
        public string min_version;
        //Why is the identifier called name? 
        public /* required */ string name;
        public string version;
    }

    public class ResourcesDescriptor
    {
        public Uri repository;
        public Uri homepage;
        public Uri bugtracker;

        [JsonConverter(typeof(JsonOldResourceUrlConverter))]
        public Uri kerbalstuff;
    }

    /// <summary>
    ///     Describes a CKAN module (ie, what's in the CKAN.schema file).
    /// </summary>

    // Base class for both modules (installed via the CKAN) and bundled
    // modules (which are more lightweight)
    [JsonObject(MemberSerialization.OptIn)]
    public class Module
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Module));

        // identifier, license, and version are always required, so we know
        // what we've got.

        [JsonProperty("abstract")]
        public string @abstract;
        [JsonProperty("description")]
        public string description;

        // Package type: in spec v1.6 can be either "package" or "metapackage"
        [JsonProperty("kind")]
        public string kind;

        [JsonProperty("author")]
        [JsonConverter(typeof(JsonSingleOrArrayConverter<string>))]
        public List<string> author;

        [JsonProperty("comment")]
        public string comment;

        [JsonProperty("conflicts")]
        public List<RelationshipDescriptor> conflicts;
        [JsonProperty("depends")]
        public List<RelationshipDescriptor> depends;

        [JsonProperty("download")]
        public Uri download;
        [JsonProperty("download_size")]
        public long download_size;
        [JsonProperty("identifier", Required = Required.Always)]
        public string identifier;

        [JsonProperty("ksp_version")]
        public KSPVersion ksp_version;

        [JsonProperty("ksp_version_max")]
        public KSPVersion ksp_version_max;
        [JsonProperty("ksp_version_min")]
        public KSPVersion ksp_version_min;

        [JsonProperty("license", Required = Required.Always)]
        public License license;

        [JsonProperty("name")]
        public string name;

        [JsonProperty("provides")]
        public List<string> provides;

        [JsonProperty("recommends")]
        public List<RelationshipDescriptor> recommends;
        [JsonProperty("release_status")]
        public ReleaseStatus release_status;

        [JsonProperty("resources")]
        public ResourcesDescriptor resources;
        [JsonProperty("suggests")]
        public List<RelationshipDescriptor> suggests;
        [JsonProperty("version", Required = Required.Always)]
        public Version version;

        [JsonProperty("supports")]
        public List<RelationshipDescriptor> supports;

        // A list of eveything this mod provides.
        public List<string> ProvidesList
        {
            // TODO: Consider caching this, but not in a way that the serialiser will try and
            // serialise it.
            get
            {
                var provides = new List<string> {identifier};

                if (this.provides != null)
                {
                    provides.AddRange(this.provides);
                }

                return provides;
            }
        }

        public string serialise()
        {
            return JsonConvert.SerializeObject(this);
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", identifier, version);
        }

        [OnDeserialized]
        private void DeSerialisationFixes(StreamingContext like_i_could_care)
        {
            // Make sure our version fields are populated.
            // TODO: There's got to be a better way of doing this, right?

            if (ksp_version_min == null)
            {
                ksp_version_min = new KSPVersion(null);
            }
            else
            {
                ksp_version_min.ToLongMin();
            }

            if (ksp_version_max == null)
            {
                ksp_version_max = new KSPVersion(null);
            }
            else
            {
                ksp_version_max.ToLongMax();
            }

            if (ksp_version == null)
            {
                ksp_version = new KSPVersion(null);
            }

            // Now see if we've got version with version min/max.
            if (ksp_version.IsNotAny() && (ksp_version_max.IsNotAny() || ksp_version_min.IsNotAny()))
            {
                // KSP version mixed with min/max.
                throw new InvalidModuleAttributesException("ksp_version mixed wtih ksp_version_(min|max)", this);
            }
        }

        /// <summary>
        /// Returns true if we conflict with the given module.
        /// </summary>
        public bool ConflictsWith(Module module)
        {
            return UniConflicts(this, module) || UniConflicts(module, this);
        }

        /// <summary>
        /// Checks if A conflicts with B, but not if B conflicts with A.
        /// Used by ConflictsWith.
        /// </summary>
        internal static bool UniConflicts(Module mod1, Module mod2)
        {
            if (mod1.conflicts == null)
            {
                return false;
            }
            return mod1.conflicts.Any(conflict => mod2.ProvidesList.Contains(conflict.name));
        }

        /// <summary>
        ///     Returns true if our mod is compatible with the KSP version specified.
        /// </summary>
        public bool IsCompatibleKSP(string version)
        {
            return IsCompatibleKSP(new KSPVersion(version));
        }

        public bool IsCompatibleKSP(KSPVersion version)
        {
            log.DebugFormat("Testing if {0} is compatible with KSP {1}", this, version);

            // Check the min and max versions.

            if (ksp_version_min.IsNotAny() && version < ksp_version_min)
            {
                return false;
            }

            if (ksp_version_max.IsNotAny() && version > ksp_version_max)
            {
                return false;
            }

            // We didn't hit the min/max guards. They may not have existed.

            // Note that since ksp_version is "any" if not specified, this
            // will work fine if there's no target, or if there were min/max
            // fields and we passed them successfully.

            return ksp_version.Targets(version);
        }

        /// <summary>
        /// Returns true if this module provides the functionality requested.
        /// </summary>
        public bool DoesProvide(string identifier)
        {
            return this.identifier == identifier || provides.Contains(identifier);
        }

        public bool IsMetapackage
        {
            get
            {
                return (!string.IsNullOrEmpty(this.kind) && this.kind == "metapackage");
            }
        }

    }

    public class CkanModule : Module
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CkanModule));
        private static readonly string[] required_fields =
        {
            "spec_version",
            "name",
            "abstract",
            "identifier",
            "download",
            "license",
            "version"
        };
        // Only CKAN modules can have install and bundle instructions.

        [JsonProperty("install")] public ModuleInstallDescriptor[] install;
        [JsonProperty("spec_version", Required = Required.Always)] public Version spec_version;

        private static bool validate_json_against_schema(string json)
        {

            log.Debug("In-client JSON schema validation unimplemented.");
            return true;
            // due to Newtonsoft Json not supporting v4 of the standard, we can't actually do this :(

            //            if (metadata_schema == null)
            //            {
            //                string schema = "";
            //
            //                try
            //                {
            //                    schema = File.ReadAllText(metadata_schema_path);
            //                }
            //                catch (Exception)
            //                {
            //                    if (!metadata_schema_missing_warning_fired)
            //                    {
            //                        User.Error("Couldn't open metadata schema at \"{0}\", will not validate metadata files",
            //                            metadata_schema_path);
            //                        metadata_schema_missing_warning_fired = true;
            //                    }
            //
            //                    return true;
            //                }
            //
            //                metadata_schema = JsonSchema.Parse(schema);
            //            }
            //
            //            JObject obj = JObject.Parse(json);
            //            return obj.IsValid(metadata_schema);
        }

        /// <summary>
        /// Tries to parse an identifier in the format Modname=version
        /// If the module cannot be found in the registry, throws a ModuleNotFoundKraken.
        /// </summary>
        public static CkanModule FromIDandVersion(Registry registry, string mod, KSPVersion ksp_version)
        {
            CkanModule module;

            Match match = Regex.Match(mod, @"^(?<mod>[^=]*)=(?<version>.*)$");

            if (match.Success)
            {
                string ident = match.Groups["mod"].Value;
                string version = match.Groups["version"].Value;

                module = registry.GetModuleByVersion(ident, version);

                if (module == null)
                    throw new ModuleNotFoundKraken(ident, version, string.Format("Cannot install {0}, version {1} not available", ident, version));
            }
            else
                module = registry.LatestAvailable(mod, ksp_version);

            if (module == null)
                throw new ModuleNotFoundKraken(mod, null, string.Format("Cannot install {0}, module not available", mod));
            else
                return module;
        }

        /// <summary> Generates a CKAN.Meta object given a filename</summary>
        public static CkanModule FromFile(string filename)
        {
            string json = File.ReadAllText(filename);
            return FromJson(json);
        }

        public static void ToFile(CkanModule module, string filename)
        {
            var json = ToJson(module);
            File.WriteAllText(filename, json);
        }

        /// <summary>
        /// Generates a CKAN.META object from a string.
        /// Also validates that all required fields are present.
        /// Throws a BadMetaDataKraken if any fields are missing.
        /// </summary>
        public static CkanModule FromJson(string json)
        {
            if (!validate_json_against_schema(json))
            {
                throw new BadMetadataKraken(null, "Validation against spec failed");
            }

            CkanModule newModule;

            try
            {
                newModule = JsonConvert.DeserializeObject<CkanModule>(json);
            }
            catch (JsonException ex)
            {
                throw new BadMetadataKraken(null, "JSON deserialization error", ex);
            }

            // NOTE: Many of these tests may be better inour Deserialisation handler.
            if (!newModule.IsSpecSupported())
            {
                throw new UnsupportedKraken(
                    String.Format(
                        "{0} requires CKAN {1}, we can't read it.",
                        newModule,
                        newModule.spec_version
                    )
                );
            }

            // Check everything in the spec if defined.
            // TODO: This *can* and *should* be done with JSON attributes!

            foreach (string field in required_fields)
            {
                object value = newModule.GetType().GetField(field).GetValue(newModule);

                if (value == null)
                {
                    // Metapackages are allowed to have no download field
                    if (field == "download" && newModule.IsMetapackage) continue;

                    string error = String.Format("{0} missing required field {1}", newModule.identifier, field);

                    log.Error(error);
                    throw new BadMetadataKraken(null, error);
                }
            }
            // All good! Return module
            return newModule;
        }            

        public static string ToJson(CkanModule module)
        {
            return JsonConvert.SerializeObject(module);
        }

        /// <summary>
        /// Returns true if we support at least spec_version of the CKAN spec.
        /// </summary>
        internal static bool IsSpecSupported(Version spec_vesion)
        {
            // This could be a read-only state variable; do we have those in C#?
            Version release = Meta.ReleaseNumber();

            return release == null || release.IsGreaterThan(spec_vesion);
        }

        /// <summary>
        /// Returns true if we support the CKAN spec used by this module.
        /// </summary>
        private bool IsSpecSupported()
        {
            return IsSpecSupported(spec_version);
        }

        /// <summary>
        ///     Returns a standardised name for this module, in the form
        ///     "identifier-version.zip". For example, `RealSolarSystem-7.3.zip`
        /// </summary>
        public string StandardName()
        {
            return StandardName(identifier, version);
        }

        public static string StandardName(string identifier, Version version)
        {
            return identifier + "-" + version + ".zip";
        }
    }

    public class InvalidModuleAttributesException : Exception
    {
        private readonly Module module;
        private readonly string why;

        public InvalidModuleAttributesException(string why, Module module = null)
        {
            this.why = why;
            this.module = module;
        }

        public override string ToString()
        {
            string modname = "unknown";

            if (module != null)
            {
                modname = module.identifier;
            }

            return string.Format("[InvalidModuleAttributesException] {0} in {1}", why, modname);
        }
    }
}
