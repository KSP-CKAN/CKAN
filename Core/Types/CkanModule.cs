using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Autofac;
using log4net;
using Newtonsoft.Json;
using CKAN.Versioning;

namespace CKAN
{
    public class ModuleReplacement
    {
        public CkanModule ToReplace;
        public CkanModule ReplaceWith;
    }

    public class DownloadHashesDescriptor
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

        private static readonly Dictionary<string, string[]> required_fields =
            new Dictionary<string, string[]>()
            {
                {
                    "package", new string[]
                    {
                        "spec_version",
                        "name",
                        "abstract",
                        "identifier",
                        "download",
                        "license",
                        "version"
                    }
                },
                {
                    "metapackage", new string[]
                    {
                        "spec_version",
                        "name",
                        "abstract",
                        "identifier",
                        "license",
                        "version"
                    }
                },
                {
                    "dlc", new string[]
                    {
                        "spec_version",
                        "name",
                        "abstract",
                        "identifier",
                        "license",
                        "version"
                    }
                },
            };

        // identifier, license, and version are always required, so we know
        // what we've got.

        [JsonProperty("abstract", Order = 5)]
        public string @abstract;

        [JsonProperty("description", Order = 6, NullValueHandling = NullValueHandling.Ignore)]
        public string description;

        // Package type: in spec v1.6 can be either "package" or "metapackage"
        // In spec v1.28, "dlc"
        [JsonProperty("kind", Order = 29, NullValueHandling = NullValueHandling.Ignore)]
        public string kind;

        [JsonProperty("author", Order = 7, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonSingleOrArrayConverter<string>))]
        public List<string> author;

        [JsonProperty("comment", Order = 2, NullValueHandling = NullValueHandling.Ignore)]
        public string comment;

        [JsonProperty("conflicts", Order = 23, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonRelationshipConverter))]
        public List<RelationshipDescriptor> conflicts;

        [JsonProperty("depends", Order = 19, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonRelationshipConverter))]
        public List<RelationshipDescriptor> depends;

        [JsonProperty("replaced_by", NullValueHandling = NullValueHandling.Ignore)]
        public ModuleRelationshipDescriptor replaced_by;

        [JsonProperty("download", Order = 25, NullValueHandling = NullValueHandling.Ignore)]
        public Uri download;

        [JsonProperty("download_size", Order = 26, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(0)]
        public long download_size;

        [JsonProperty("download_hash", Order = 27, NullValueHandling = NullValueHandling.Ignore)]
        public DownloadHashesDescriptor download_hash;

        [JsonProperty("download_content_type", Order = 28, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue("application/zip")]
        public string download_content_type;

        [JsonProperty("identifier", Order = 3, Required = Required.Always)]
        public string identifier;

        [JsonProperty("ksp_version", Order = 9, NullValueHandling = NullValueHandling.Ignore)]
        public GameVersion ksp_version;

        [JsonProperty("ksp_version_max", Order = 11, NullValueHandling = NullValueHandling.Ignore)]
        public GameVersion ksp_version_max;

        [JsonProperty("ksp_version_min", Order = 10, NullValueHandling = NullValueHandling.Ignore)]
        public GameVersion ksp_version_min;

        [JsonProperty("ksp_version_strict", Order = 12, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        public bool ksp_version_strict = false;

        [JsonProperty("license", Order = 13)]
        [JsonConverter(typeof(JsonSingleOrArrayConverter<License>))]
        public List<License> license;

        [JsonProperty("name", Order = 4)]
        public string name;

        [JsonProperty("provides", Order = 18, NullValueHandling = NullValueHandling.Ignore)]
        public List<string> provides;

        [JsonProperty("recommends", Order = 20, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonRelationshipConverter))]
        public List<RelationshipDescriptor> recommends;

        [JsonProperty("release_status", Order = 14, NullValueHandling = NullValueHandling.Ignore)]
        public ReleaseStatus release_status;

        [JsonProperty("resources", Order = 15, NullValueHandling = NullValueHandling.Ignore)]
        public ResourcesDescriptor resources;

        [JsonProperty("suggests", Order = 21, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonRelationshipConverter))]
        public List<RelationshipDescriptor> suggests;

        [JsonProperty("version", Order = 8, Required = Required.Always)]
        public ModuleVersion version;

        [JsonProperty("supports", Order = 22, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonRelationshipConverter))]
        public List<RelationshipDescriptor> supports;

        [JsonProperty("install", Order = 24, NullValueHandling = NullValueHandling.Ignore)]
        public ModuleInstallDescriptor[] install;

        [JsonProperty("localizations", Order = 17, NullValueHandling = NullValueHandling.Ignore)]
        public string[] localizations;

        // Used to see if we're compatible with a given game/KSP version or not.
        private readonly IGameComparator _comparator;

        [JsonIgnore]
        [JsonProperty("specVersion", Required = Required.Default)]
        private ModuleVersion specVersion;
        // We integrated the Module and CkanModule into one class
        // Since spec_version was only required for CkanModule before
        // this change, we now need to make sure the user is converted
        // and has the spec_version's in his installed_modules section
        // We should return this to a simple Required.Always field some time in the future
        // ~ Postremus, 03.09.2015
        [JsonProperty("spec_version", Order = 1)]
        public ModuleVersion spec_version
        {
            get
            {
                if (specVersion == null)
                    specVersion = new ModuleVersion("1");
                return specVersion;
            }
            set
            {
                if (value == null)
                    specVersion = new ModuleVersion("1");
                else
                    specVersion = value;
            }
        }

        [JsonProperty("tags", Order = 16, NullValueHandling = NullValueHandling.Ignore)]
        public HashSet<string> Tags;

        [JsonProperty("release_date", Order = 30, NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? release_date;

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

        // These are used to simplify the search by dropping special chars.
        [JsonIgnore]
        public string SearchableName;
        [JsonIgnore]
        public string SearchableIdentifier;
        [JsonIgnore]
        public string SearchableAbstract;
        [JsonIgnore]
        public string SearchableDescription;
        [JsonIgnore]
        public List<string> SearchableAuthors;
        // This regex finds all those special chars.
        [JsonIgnore]
        public static readonly Regex nonAlphaNums = new Regex("[^a-zA-Z0-9]", RegexOptions.Compiled);

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

            try
            {
                // Use the json string to populate our object
                JsonConvert.PopulateObject(json, this);
            }
            catch (JsonException ex)
            {
                throw new BadMetadataKraken(null, string.Format("JSON deserialization error: {0}", ex.Message), ex);
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

            // Check everything in the spec is defined.
            // TODO: This *can* and *should* be done with JSON attributes!
            foreach (string field in required_fields[kind ?? "package"])
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
                    string error = String.Format("{0} missing required field {1}", identifier, field);

                    throw new BadMetadataKraken(null, error);
                }
            }

            // Calculate the Searchables.
            CalculateSearchables();
        }

        /// <summary>
        /// Calculate the mod properties used for searching via Regex.
        /// </summary>
        public void CalculateSearchables()
        {
            SearchableIdentifier  = identifier  == null ? string.Empty : CkanModule.nonAlphaNums.Replace(identifier, "");
            SearchableName        = name        == null ? string.Empty : CkanModule.nonAlphaNums.Replace(name, "");
            SearchableAbstract    = @abstract   == null ? string.Empty : CkanModule.nonAlphaNums.Replace(@abstract, "");
            SearchableDescription = description == null ? string.Empty : CkanModule.nonAlphaNums.Replace(description, "");
            SearchableAuthors = new List<string>();

            if (author == null)
            {
                SearchableAuthors.Add(string.Empty);
            }
            else
            {
                foreach (string auth in author)
                {
                    SearchableAuthors.Add(CkanModule.nonAlphaNums.Replace(auth, ""));
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

            // Now see if we've got version with version min/max.
            if (ksp_version != null && (ksp_version_max != null || ksp_version_min != null))
            {
                // KSP version mixed with min/max.
                throw new InvalidModuleAttributesException("ksp_version mixed with ksp_version_(min|max)", this);
            }

            license = license ?? new List<License> { License.UnknownLicense };
            @abstract = @abstract ?? string.Empty;
            name = name ?? string.Empty;

            CalculateSearchables();
        }

        /// <summary>
        /// Tries to parse an identifier in the format identifier=version and returns a matching CkanModule from the registry.
        /// Returns the latest compatible or installed module if no version has been given.
        /// </summary>
        /// <param name="registry">CKAN registry object for current game instance</param>
        /// <param name="name">The identifier or identifier=version of the module</param>
        /// <param name="ksp_version">The current KSP version criteria to consider</param>
        /// <returns>A CkanModule</returns>
        /// <exception cref="ModuleNotFoundKraken">Thrown if no matching module could be found</exception>
        public static CkanModule FromIDandVersion(IRegistryQuerier registry, string mod, GameVersionCriteria ksp_version)
        {
            CkanModule module;

            Match match = idAndVersionMatcher.Match(mod);

            if (match.Success)
            {
                string ident   = match.Groups["mod"].Value;
                string version = match.Groups["version"].Value;

                module = registry.GetModuleByVersion(ident, version);

                if (module == null
                        || (ksp_version != null && !module.IsCompatibleKSP(ksp_version)))
                    throw new ModuleNotFoundKraken(ident, version,
                        string.Format("Module {0} version {1} not available", ident, version));
            }
            else
            {
                module = registry.LatestAvailable(mod, ksp_version)
                      ?? registry.InstalledModule(mod)?.Module;

                if (module == null)
                    throw new ModuleNotFoundKraken(mod, null,
                        string.Format("Module {0} not installed or available", mod));
            }
            return module;
        }

        public static readonly Regex idAndVersionMatcher = new Regex(
            @"^(?<mod>[^=]*)=(?<version>.*)$",
            RegexOptions.Compiled
        );

        /// <summary> Generates a CKAN.Meta object given a filename</summary>
        /// TODO: Catch and display errors
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

        public static string ToJson(CkanModule module)
        {
            var sw = new StringWriter(new StringBuilder());
            using (var writer = new JsonTextWriter(sw))
            {
                writer.Formatting  = Formatting.Indented;
                writer.Indentation = 4;
                writer.IndentChar  = ' ';
                new JsonSerializer().Serialize(writer, module);
            }
            return sw + Environment.NewLine;
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
            return mod1?.conflicts?.Any(
                conflict => conflict.MatchesAny(new CkanModule[] {mod2}, null, null)
            ) ?? false;
        }

        /// <summary>
        /// Returns true if our mod is compatible with the KSP version specified.
        /// </summary>
        public bool IsCompatibleKSP(GameVersionCriteria version)
        {
            log.DebugFormat("Testing if {0} is compatible with KSP {1}", this, version.ToString());


            return _comparator.Compatible(version, this);
        }

        /// <summary>
        /// Returns a human readable string indicating the highest compatible
        /// version of KSP this module will run with. (Eg: 1.0.2,
        /// "All versions", etc).
        ///
        /// This is for *human consumption only*, as the strings may change in the
        /// future as we support additional locales.
        /// </summary>
        public string HighestCompatibleKSP()
        {
            GameVersion v = LatestCompatibleKSP();
            if (v.IsAny)
                return "All versions";
            else
                return v.ToString();
        }

        /// <summary>
        /// Returns machine readable object indicating the highest compatible
        /// version of KSP this module will run with.
        /// </summary>
        public GameVersion LatestCompatibleKSP()
        {
            // Find the highest compatible KSP version
            if (ksp_version_max != null)
                return ksp_version_max;
            else if (ksp_version != null)
                return ksp_version;
            else
                // No upper limit.
                return GameVersion.Any;
        }

        /// <summary>
        /// Returns machine readable object indicating the lowest compatible
        /// version of KSP this module will run with.
        /// </summary>
        public GameVersion EarliestCompatibleKSP()
        {
            // Find the lowest compatible KSP version
            if (ksp_version_min != null)
                return ksp_version_min;
            else if (ksp_version != null)
                return ksp_version;
            else
                // No lower limit.
                return GameVersion.Any;
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
                return this.kind == "metapackage";
            }
        }

        public bool IsDLC
        {
            get
            {
                return this.kind == "dlc";
            }
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

        /// <summary>
        /// Returns true if we support at least spec_version of the CKAN spec.
        /// </summary>
        internal static bool IsSpecSupported(ModuleVersion spec_vesion)
        {
            // This could be a read-only state variable; do we have those in C#?
            ModuleVersion release = new ModuleVersion(Meta.GetVersion(VersionFormat.Short));

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

        public static string StandardName(string identifier, ModuleVersion version)
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

        public string DescribeInstallStanzas()
        {
            List<string> descriptions = new List<string>();
            if (install != null)
            {
                foreach (ModuleInstallDescriptor mid in install)
                {
                    descriptions.Add(mid.DescribeMatch());
                }
            }
            else
            {
                descriptions.Add(ModuleInstallDescriptor.DefaultInstallStanza(identifier).DescribeMatch());
            }
            return string.Join(", ", descriptions);
        }

        /// <summary>
        /// Return an archive.org URL for this download, or null if it's not there.
        /// The filenames look a lot like the filenames in Net.Cache, but don't be fooled!
        /// Here it's the first 8 characters of the SHA1 of the DOWNLOADED FILE, not the URL!
        /// </summary>
        public Uri InternetArchiveDownload
        {
            get
            {
                string verStr = version.ToString().Replace(':', '-');
                // Some alternate registry repositories don't set download_hash
                return (download_hash?.sha1 != null && license.All(l => l.Redistributable))
                    ? new Uri(
                        $"https://archive.org/download/{identifier}-{verStr}/{download_hash.sha1.Substring(0, 8)}-{identifier}-{verStr}.zip")
                    : null;
            }
        }

        /// <summary>
        /// Format a byte count into readable file size
        /// </summary>
        /// <param name="bytes">Number of bytes in a file</param>
        /// <returns>
        /// ### bytes or ### KB or ### MB or ### GB
        /// </returns>
        public static string FmtSize(long bytes)
        {
            const double K = 1024;
            if (bytes < K) {
                return $"{bytes} B";
            } else if (bytes < K * K) {
                return $"{bytes / K :N1} KiB";
            } else if (bytes < K * K * K) {
                return $"{bytes / K / K :N1} MiB";
            } else {
                return $"{bytes / K / K / K :N1} GiB";
            }
        }
    }

    public class InvalidModuleAttributesException : Exception
    {
        private readonly CkanModule module;
        private readonly string why;

        public InvalidModuleAttributesException(string why, CkanModule module = null)
            : base(why)
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
