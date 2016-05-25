using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using log4net;
using Newtonsoft.Json;
using System.Transactions;
using Autofac;

namespace CKAN
{
    public class RelationshipDescriptor
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Version max_version;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Version min_version;
        //Why is the identifier called name?
        public /* required */ string name;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Version version;

        /// <summary>
        /// Returns if the other version satisfies this RelationshipDescriptor.
        /// If the RelationshipDescriptor has version set it compares against that.
        /// Else it uses the {min,max}_version fields treating nulls as unbounded.
        /// Note: Uses inclusive inequalities.
        /// </summary>
        /// <param name="other_version"></param>
        /// <returns>True if other_version is within the bounds</returns>
        public bool version_within_bounds(Version other_version)
        {
            // DLL versions (aka autodetected mods) satisfy *all* relationships
            if (other_version is DllVersion)
                return true;

            if (version == null)
            {
                if (max_version == null && min_version == null)
                    return true;
                bool min_sat = min_version == null || min_version <= other_version;
                bool max_sat = max_version == null || max_version >= other_version;
                if (min_sat && max_sat) return true;
            }
            else
            {
                if (version.Equals(other_version))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// A user friendly message for what versions satisfies this descriptor.
        /// </summary>
        [JsonIgnore]
        public string RequiredVersion
        {
            get
            {
                if (version != null)
                    return version.ToString();
                return string.Format("between {0} and {1} inclusive.",
                    min_version != null ?min_version.ToString() : "any version",
                    max_version != null ? max_version.ToString() : "any version");
            }
        }

    }

    public class ResourcesDescriptor
    {
        [JsonConverter(typeof(JsonIgnoreBadUrlConverter))]
        public Uri repository;

        [JsonConverter(typeof(JsonIgnoreBadUrlConverter))]
        public Uri homepage;

        [JsonConverter(typeof(JsonIgnoreBadUrlConverter))]
        public Uri bugtracker;

        [JsonConverter(typeof(JsonOldResourceUrlConverter))]
        public Uri spacedock;
    }

    public class DownloadHashsDescriptor
    {
        [JsonProperty("sha1")]
        public string sha1;

        [JsonProperty("sha256")]
        public string sha256;
    }

    public class NameComparer : IEqualityComparer<CkanModule>
    {
        public bool Equals(CkanModule x, CkanModule y)
        {
            return x.identifier.Equals(y.identifier);
        }

        public int GetHashCode(CkanModule obj)
        {
            return obj.identifier.GetHashCode();
        }
    }

    /// <summary>
    ///     Describes a CKAN module (ie, what's in the CKAN.schema file).
    /// </summary>
    
    // Base class for both modules (installed via the CKAN) and bundled
    // modules (which are more lightweight)
    [JsonObject(MemberSerialization.OptIn)]
    public class CkanModule : IEquatable<CkanModule>
    {

        #region Fields

        private static readonly ILog log = LogManager.GetLogger(typeof (CkanModule));

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

        [JsonProperty("download_hash")]
        public DownloadHashsDescriptor download_hash;

        [JsonProperty("identifier", Required = Required.Always)]
        public string identifier;

        [JsonProperty("ksp_version")]
        public KSPVersion ksp_version;

        [JsonProperty("ksp_version_max")]
        public KSPVersion ksp_version_max;

        [JsonProperty("ksp_version_min")]
        public KSPVersion ksp_version_min;

        [JsonProperty("ksp_version_strict")]
        public bool ksp_version_strict = false;

        [JsonProperty("license")]
        [JsonConverter(typeof(JsonSingleOrArrayConverter<License>))]
        public List<License> license;

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

        [JsonProperty("install")]
        public ModuleInstallDescriptor[] install;

        // Used to see if we're compatible with a given game/KSP version or not.
        private readonly IGameComparator _comparator;

        [JsonIgnore]
        [JsonProperty("specVersion", Required = Required.Default)]
        private Version specVersion;
        // We integrated the Module and CkanModule into one class
        // Since spec_version was only required for CkanModule before
        // this change, we now need to make sure the user is converted
        // and has the spec_version's in his installed_modules section
        // We should return this to a simple Required.Always field some time in the future
        // ~ Postremus, 03.09.2015
        [JsonProperty("spec_version")]
        public Version spec_version
        {
            get
            {
                if (specVersion == null)
                    specVersion = new Version("1");
                return specVersion;
            }
            set
            {
                if (value == null)
                    specVersion = new Version("1");
                else
                    specVersion = value;
            }
        }

        // A list of eveything this mod provides.
        public List<string> ProvidesList
        {
            // TODO: Consider caching this, but not in a way that the serialiser will try and
            // serialise it.
            get
            {
                var provides = new List<string> { identifier };

                if (this.provides != null)
                {
                    provides.AddRange(this.provides);
                }

                return provides;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// To be used by test cases and cascaded json deserialisation only,
        /// and even then I'm not sure this is a great idea.
        /// </summary>
        [JsonConstructor]
        internal CkanModule()
        {
            // We don't have this passed in, so we'll ask the service locator
            // directly. Yuck.
            _comparator = ServiceLocator.Container.Resolve<IGameComparator>();
        }

        /// <summary>
        /// Inflates a CKAN object from a JSON string.
        /// </summary>
        public CkanModule(string json, IGameComparator comparator)
        {
            _comparator = comparator;

            if (!validate_json_against_schema(json))
            {
                throw new BadMetadataKraken(null, "Validation against spec failed");
            }

            try
            {
                // Use the json string to populate our object
                JsonConvert.PopulateObject(json, this);
            }
            catch (JsonException ex)
            {
                throw new BadMetadataKraken(null, "JSON deserialization error", ex);
            }

            // NOTE: Many of these tests may be better in our Deserialisation handler.
            if (!IsSpecSupported())
            {
                throw new UnsupportedKraken(
                    String.Format(
                        "{0} requires CKAN {1}, we can't read it.",
                        this,
                        spec_version
                    )
                );
            }

            // Check everything in the spec if defined.
            // TODO: This *can* and *should* be done with JSON attributes!

            foreach (string field in required_fields)
            {
                object value = null;
                if (GetType().GetField(field) != null)
                {
                    value = typeof(CkanModule).GetField(field).GetValue(this);
                }
                else
                {
                    // uh, maybe it is not a field, but a property?
                    value = typeof(CkanModule).GetProperty(field).GetValue(this, null);
                }

                if (value == null)
                {
                    // Metapackages are allowed to have no download field
                    if (field == "download" && IsMetapackage) continue;

                    string error = String.Format("{0} missing required field {1}", identifier, field);

                    log.Error(error);
                    throw new BadMetadataKraken(null, error);
                }
            }
        }

        public string serialise()
        {
            return JsonConvert.SerializeObject(this);
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
                ksp_version_min = ksp_version_min.ToLongMin();
            }

            if (ksp_version_max == null)
            {
                ksp_version_max = new KSPVersion(null);
            }
            else
            {
                ksp_version_max = ksp_version_max.ToLongMax();
            }

            if (ksp_version == null)
            {
                ksp_version = new KSPVersion(null);
            }

            // Now see if we've got version with version min/max.
            if (ksp_version.IsNotAny() && (ksp_version_max.IsNotAny() || ksp_version_min.IsNotAny()))
            {
                // KSP version mixed with min/max.
                throw new InvalidModuleAttributesException("ksp_version mixed with ksp_version_(min|max)", this);
            }

            if (license == null)
            {
                license = new List<License> { new License("unknown") };
            }

            if (@abstract == null)
            {
                @abstract = "";
            }

            if (name == null)
            {
                name = "";
            }
        }

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
        public static CkanModule FromIDandVersion(IRegistryQuerier registry, string mod, KSPVersion ksp_version)
        {
            CkanModule module;

            Match match = Regex.Match(mod, @"^(?<mod>[^=]*)=(?<version>.*)$");

            if (match.Success)
            {
                string ident = match.Groups["mod"].Value;
                string version = match.Groups["version"].Value;

                module = registry.GetModuleByVersion(ident, version);

                if (module == null)
                        throw new ModuleNotFoundKraken(ident, version,
                            string.Format("Cannot install {0}, version {1} not available", ident, version));
            }
            else
                module = registry.LatestAvailable(mod, ksp_version);

            if (module == null)
                    throw new ModuleNotFoundKraken(mod, null,
                        string.Format("Cannot install {0}, module not available", mod));
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
            log.Debug("Inflating comparator object");
            IGameComparator comparator = ServiceLocator.Container.Resolve<IGameComparator>();

            log.Debug("Building CkanModule");
            return new CkanModule(json, comparator);
        }

        #endregion

        /// <summary>
        /// Returns true if we conflict with the given module.
        /// </summary>
        public bool ConflictsWith(CkanModule module)
        {
            // We never conflict with ourselves, since we can't be installed at
            // the same time as another version of ourselves.
            if (module.identifier == this.identifier) return false;

            return UniConflicts(this, module) || UniConflicts(module, this);
        }

        /// <summary>
        /// Checks if A conflicts with B, but not if B conflicts with A.
        /// Used by ConflictsWith.
        /// </summary>
        internal static bool UniConflicts(CkanModule mod1, CkanModule mod2)
        {
            if (mod1.conflicts == null)
            {
                return false;
            }
            return
                mod1.conflicts.Any(
                    conflict =>
                        mod2.ProvidesList.Contains(conflict.name) && conflict.version_within_bounds(mod2.version));
        }

        /// <summary>
        /// Returns true if our mod is compatible with the KSP version specified.
        /// </summary>
        public bool IsCompatibleKSP(string version)
        {
            return IsCompatibleKSP(new KSPVersion(version));
        }

        /// <summary>
        /// Returns true if our mod is compatible with the KSP version specified.
        /// </summary>
        public bool IsCompatibleKSP(KSPVersion version)
        {
            log.DebugFormat("Testing if {0} is compatible with KSP {1}", this, version);

            return _comparator.Compatible(version, this);
        }

        /// <summary>
        /// Returns a human readable string indicating the highest compatible
        /// version of KSP this module will run with. (Eg: 1.0.2, 1.0.2+,
        /// "All version", etc).
        /// 
        /// This is for *human consumption only*, as the strings may change in the
        /// future as we support additional locales.
        /// </summary>
        public string HighestCompatibleKSP()
        {
            // Find the highest compatible KSP version
            if (!String.IsNullOrEmpty(ksp_version_max.ToString()))
            {
                return ksp_version_max.ToLongMax().ToString();
            }
            else if (!String.IsNullOrEmpty(ksp_version.ToString()))
            {
                return ksp_version.ToLongMax().ToString();
            }
            else if (!String.IsNullOrEmpty(ksp_version_min.ToString()))
            {
                return ksp_version_min.ToLongMin().ToString() + "+";
            }

            return "All versions";
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
            get { return (!string.IsNullOrEmpty(this.kind) && this.kind == "metapackage"); }
        }

        protected bool Equals(CkanModule other)
        {
            return string.Equals(identifier, other.identifier) && version.Equals(other.version);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CkanModule)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (identifier.GetHashCode() * 397) ^ version.GetHashCode();
            }
        }

        bool IEquatable<CkanModule>.Equals(CkanModule other)
        {
            return Equals(other);
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
            // Versions can contain ALL SORTS OF WACKY THINGS! Colons, friggin newlines,
            // slashes, and heaven knows what use mod authors try to smoosh into them.
            // We'll reduce this down to "friendly" characters, replacing everything else with
            // dashes. This doesn't change look-ups, as we use the hash prefix for that.
            string version_string = Regex.Replace(version.ToString(), "[^A-Za-z0-9_.-]", "-");

            return identifier + "-" + version_string + ".zip";
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", identifier, version);
        }
    }

    public class InvalidModuleAttributesException : Exception
    {
        private readonly CkanModule module;
        private readonly string why;

        public InvalidModuleAttributesException(string why, CkanModule module = null)
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

