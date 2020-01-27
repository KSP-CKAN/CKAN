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
using CKAN.Extensions;
using CKAN.Versioning;

namespace CKAN
{
    public abstract class RelationshipDescriptor : IEquatable<RelationshipDescriptor>
    {
        public abstract bool MatchesAny(
            IEnumerable<CkanModule> modules,
            HashSet<string> dlls,
            IDictionary<string, UnmanagedModuleVersion> dlc
        );

        public abstract bool WithinBounds(CkanModule otherModule);

        public abstract List<CkanModule> LatestAvailableWithProvides(IRegistryQuerier registry, KspVersionCriteria crit, IEnumerable<CkanModule> toInstall = null);

        public abstract bool Equals(RelationshipDescriptor other);

        public abstract bool ContainsAny(IEnumerable<string> identifiers);

        public abstract bool StartsWith(string prefix);

        // virtual ToString() already present in 'object'
    }

    public class ModuleRelationshipDescriptor : RelationshipDescriptor
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ModuleVersion max_version;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ModuleVersion min_version;
        //Why is the identifier called name?
        public /* required */ string name;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ModuleVersion version;


        public override bool WithinBounds(CkanModule otherModule)
        {
            return otherModule.ProvidesList.Contains(name)
                && WithinBounds(otherModule.version);
        }

        /// <summary>
        /// Returns if the other version satisfies this RelationshipDescriptor.
        /// If the RelationshipDescriptor has version set it compares against that.
        /// Else it uses the {min,max}_version fields treating nulls as unbounded.
        /// Note: Uses inclusive inequalities.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>True if other_version is within the bounds</returns>
        public bool WithinBounds(ModuleVersion other)
        {
            // UnmanagedModuleVersions with unknown versions always satisfy the bound
            if (other is UnmanagedModuleVersion unmanagedModuleVersion && unmanagedModuleVersion.IsUnknownVersion)
                return true;

            if (version == null)
            {
                if (max_version == null && min_version == null)
                    return true;

                var minSat = min_version == null || min_version <= other;
                var maxSat = max_version == null || max_version >= other;

                if (minSat && maxSat)
                    return true;
            }
            else
            {
                if (version.Equals(other))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Check whether any of the modules in a given list match this descriptor.
        /// NOTE: Only proper modules and DLC can be checked for versions!
        ///       DLLs match all versions, as do "provides" clauses.
        /// </summary>
        /// <param name="modules">Sequence of modules to consider</param>
        /// <param name="dlls">Sequence of DLLs to consider</param>
        /// <param name="dlc">DLC to consider</param>
        /// <returns>
        /// true if any of the modules match this descriptor, false otherwise.
        /// </returns>
        public override bool MatchesAny(
            IEnumerable<CkanModule> modules,
            HashSet<string> dlls,
            IDictionary<string, UnmanagedModuleVersion> dlc
        )
        {
            modules = modules?.AsCollection();

            // DLLs are considered to match any version
            if (dlls != null && dlls.Contains(name))
            {
                return true;
            }

            if (modules != null)
            {
                // See if anyone else "provides" the target name
                // Note that versions can't be checked for "provides" clauses
                if (modules.Any(m => m.identifier != name && m.provides != null && m.provides.Contains(name)))
                {
                    return true;
                }

                // See if the real thing is there
                foreach (var m in modules.Where(m => m.identifier == name))
                {
                    if (WithinBounds(m))
                    {
                        return true;
                    }
                }
            }

            if (dlc != null)
            {
                foreach (var d in dlc.Where(i => i.Key == name))
                {
                    if (WithinBounds(d.Value))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override List<CkanModule> LatestAvailableWithProvides(IRegistryQuerier registry, KspVersionCriteria crit, IEnumerable<CkanModule> toInstall = null)
        {
            return registry.LatestAvailableWithProvides(name, crit, this, toInstall);
        }

        public override bool Equals(RelationshipDescriptor other)
        {
            ModuleRelationshipDescriptor modRel = other as ModuleRelationshipDescriptor;
            return modRel != null
                && name        == modRel.name
                && version     == modRel.version
                && min_version == modRel.min_version
                && max_version == modRel.max_version;
        }

        public override bool ContainsAny(IEnumerable<string> identifiers)
        {
            return identifiers.Contains(name);
        }

        public override bool StartsWith(string prefix)
        {
            return name.IndexOf(prefix, StringComparison.CurrentCultureIgnoreCase) == 0;
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
                    min_version != null ? min_version.ToString() : "any version",
                    max_version != null ? max_version.ToString() : "any version");
            }
        }

        /// <summary>
        /// Generate a user readable description of the relationship
        /// </summary>
        /// <returns>
        /// Depending on the version properties, one of:
        /// name
        /// name version
        /// name min_version -- max_version
        /// name min_version or later
        /// name max_version or earlier
        /// </returns>
        public override string ToString()
        {
            return
                  version     != null                        ? $"{name} {version}"
                : min_version != null && max_version != null ? $"{name} {min_version} -- {max_version}"
                : min_version != null                        ? $"{name} {min_version} or later"
                : max_version != null                        ? $"{name} {max_version} or earlier"
                : name;
        }

    }

    public class AnyOfRelationshipDescriptor : RelationshipDescriptor
    {
        [JsonProperty("any_of", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonRelationshipConverter))]
        public List<RelationshipDescriptor> any_of;

        public override bool WithinBounds(CkanModule otherModule)
        {
            return any_of?.Any(r => r.WithinBounds(otherModule))
                ?? false;
        }

        public override bool MatchesAny(
            IEnumerable<CkanModule> modules,
            HashSet<string> dlls,
            IDictionary<string, UnmanagedModuleVersion> dlc
        )
        {
            return any_of?.Any(r => r.MatchesAny(modules, dlls, dlc))
                ?? false;
        }

        public override List<CkanModule> LatestAvailableWithProvides(IRegistryQuerier registry, KspVersionCriteria crit, IEnumerable<CkanModule> toInstall = null)
        {
            return any_of?.SelectMany(r => r.LatestAvailableWithProvides(registry, crit, toInstall)).ToList();
        }

        public override bool Equals(RelationshipDescriptor other)
        {
            AnyOfRelationshipDescriptor anyRel = other as AnyOfRelationshipDescriptor;
            return anyRel != null
                && (any_of?.SequenceEqual(anyRel.any_of) ?? anyRel.any_of == null);
        }

        public override bool ContainsAny(IEnumerable<string> identifiers)
        {
            return any_of?.Any(r => r.ContainsAny(identifiers))
                ?? false;
        }

        public override bool StartsWith(string prefix)
        {
            return any_of?.Any(r => r.StartsWith(prefix))
                ?? false;
        }

        public override string ToString()
        {
            return any_of?.Select(r => r.ToString())
                .Aggregate((a, b) => $"{a} OR {b}");
        }
    }

    public class ModuleReplacement
    {
        public CkanModule ToReplace;
        public CkanModule ReplaceWith;
    }

    public class ResourcesDescriptor
    {
        [JsonProperty("homepage", Order = 1, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonIgnoreBadUrlConverter))]
        public Uri homepage;

        [JsonProperty("spacedock", Order = 2, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonOldResourceUrlConverter))]
        public Uri spacedock;

        [JsonProperty("curse", Order = 3, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonOldResourceUrlConverter))]
        public Uri curse;

        [JsonProperty("repository", Order = 4, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonIgnoreBadUrlConverter))]
        public Uri repository;

        [JsonProperty("bugtracker", Order = 5, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonIgnoreBadUrlConverter))]
        public Uri bugtracker;

        [JsonProperty("ci", Order = 6, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonIgnoreBadUrlConverter))]
        public Uri ci;

        [JsonProperty("license", Order = 7, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonIgnoreBadUrlConverter))]
        public Uri license;

        [JsonProperty("manual", Order = 8, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonIgnoreBadUrlConverter))]
        public Uri manual;

        [JsonProperty("metanetkan", Order = 9, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonOldResourceUrlConverter))]
        public Uri metanetkan;
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

        [JsonProperty("abstract", Order = 5)]
        public string @abstract;

        [JsonProperty("description", Order = 6, NullValueHandling = NullValueHandling.Ignore)]
        public string description;

        // Package type: in spec v1.6 can be either "package" or "metapackage"
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
        public KspVersion ksp_version;

        [JsonProperty("ksp_version_max", Order = 11, NullValueHandling = NullValueHandling.Ignore)]
        public KspVersion ksp_version_max;

        [JsonProperty("ksp_version_min", Order = 10, NullValueHandling = NullValueHandling.Ignore)]
        public KspVersion ksp_version_min;

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
        /// Tries to parse an identifier in the format Modname=version
        /// If the module cannot be found in the registry, throws a ModuleNotFoundKraken.
        /// </summary>
        public static CkanModule FromIDandVersion(IRegistryQuerier registry, string mod, KspVersionCriteria ksp_version)
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
        public bool IsCompatibleKSP(KspVersionCriteria version)
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
            KspVersion v = LatestCompatibleKSP();
            if (v.IsAny)
                return "All versions";
            else
                return v.ToString();
        }

        /// <summary>
        /// Returns machine readable object indicating the highest compatible
        /// version of KSP this module will run with.
        /// </summary>
        public KspVersion LatestCompatibleKSP()
        {
            // Find the highest compatible KSP version
            if (ksp_version_max != null)
                return ksp_version_max;
            else if (ksp_version != null)
                return ksp_version;
            else
                // No upper limit.
                return KspVersion.Any;
        }

        /// <summary>
        /// Returns machine readable object indicating the lowest compatible
        /// version of KSP this module will run with.
        /// </summary>
        public KspVersion EarliestCompatibleKSP()
        {
            // Find the lowest compatible KSP version
            if (ksp_version_min != null)
                return ksp_version_min;
            else if (ksp_version != null)
                return ksp_version;
            else
                // No lower limit.
                return KspVersion.Any;
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
            foreach (ModuleInstallDescriptor mid in install)
            {
                descriptions.Add(mid.DescribeMatch());
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
