using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;

using Autofac;
using log4net;
using Newtonsoft.Json;

using CKAN.Versioning;
using CKAN.Games;
#if NETSTANDARD2_0
using CKAN.Extensions;
#endif

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
            => x.identifier.Equals(y.identifier);

        public int GetHashCode(CkanModule obj)
            => obj.identifier.GetHashCode();
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
        [JsonProperty("kind", Order = 31, NullValueHandling = NullValueHandling.Ignore)]
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
        [JsonConverter(typeof(JsonSingleOrArrayConverter<Uri>))]
        public List<Uri> download;

        [JsonProperty("download_size", Order = 26, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(0)]
        public long download_size;

        [JsonProperty("download_hash", Order = 27, NullValueHandling = NullValueHandling.Ignore)]
        public DownloadHashesDescriptor download_hash;

        [JsonProperty("download_content_type", Order = 28, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue("application/zip")]
        public string download_content_type;

        [JsonProperty("install_size", Order = 29, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(0)]
        public long install_size;

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
                {
                    specVersion = new ModuleVersion("1");
                }

                return specVersion;
            }
            #pragma warning disable IDE0027
            set
            {
                specVersion = value ?? new ModuleVersion("1");
            }
            #pragma warning restore IDE0027
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
            download_content_type = typeof(CkanModule).GetTypeInfo()
                                                      .GetDeclaredField("download_content_type")
                                                      .GetCustomAttribute<DefaultValueAttribute>()
                                                      .Value
                                                      .ToString();
        }

        /// <summary>
        /// Initialize a CkanModule
        /// </summary>
        /// <param name="spec_version">The version of the spec obeyed by this module</param>
        /// <param name="identifier">This module's machine-readable identifier</param>
        /// <param name="name">This module's user-visible display name</param>
        /// <param name="@abstract">Short description of this module</param>
        /// <param name="description">Long description of this module</param>
        /// <param name="author">Authors of this module</param>
        /// <param name="license">Licenses of this module</param>
        /// <param name="version">Version number of this release</param>
        /// <param name="download">Where to download this module</param>
        /// <param name="kind">package, metapackage, or dlc</param>
        /// <param name="comparator">Object used for checking compatibility of this module</param>
        public CkanModule(
            ModuleVersion spec_version,
            string        identifier,
            string        name,
            string        @abstract,
            string        description,
            List<string>  author,
            List<License> license,
            ModuleVersion version,
            Uri           download,
            string        kind = null,
            IGameComparator comparator = null
        )
        {
            this.spec_version = spec_version;
            this.identifier   = identifier;
            this.name         = name;
            this.@abstract    = @abstract;
            this.description  = description;
            this.author       = author;
            this.license      = license;
            this.version      = version;
            this.download     = new List<Uri> { download };
            this.kind         = kind;
            _comparator  = comparator ?? ServiceLocator.Container.Resolve<IGameComparator>();
            CheckHealth();
            CalculateSearchables();
        }

        /// <summary>
        /// Inflates a CKAN object from a JSON string.
        /// </summary>
        public CkanModule(string json, IGameComparator comparator = null)
        {
            try
            {
                // Use the json string to populate our object
                JsonConvert.PopulateObject(json, this, new JsonSerializerSettings
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                });
            }
            catch (JsonException ex)
            {
                throw new BadMetadataKraken(null,
                    string.Format(Properties.Resources.CkanModuleDeserialisationError, ex.Message),
                    ex);
            }
            _comparator = comparator ?? ServiceLocator.Container.Resolve<IGameComparator>();
            CheckHealth();
            CalculateSearchables();
        }

        /// <summary>
        /// Throw an exception if there's anything wrong with this module
        /// </summary>
        private void CheckHealth()
        {
            if (!IsSpecSupported())
            {
                throw new UnsupportedKraken(string.Format(
                    Properties.Resources.CkanModuleUnsupportedSpec, this, spec_version));
            }

            // Check everything in the spec is defined.
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
                    throw new BadMetadataKraken(null, string.Format(
                        Properties.Resources.CkanModuleMissingRequired, identifier, field));
                }
            }
        }

        /// <summary>
        /// Calculate the mod properties used for searching via Regex.
        /// </summary>
        private void CalculateSearchables()
        {
            SearchableIdentifier  = identifier  == null ? string.Empty : nonAlphaNums.Replace(identifier, "");
            SearchableName        = name        == null ? string.Empty : nonAlphaNums.Replace(name, "");
            SearchableAbstract    = @abstract   == null ? string.Empty : nonAlphaNums.Replace(@abstract, "");
            SearchableDescription = description == null ? string.Empty : nonAlphaNums.Replace(description, "");
            SearchableAuthors = new List<string>();

            if (author == null)
            {
                SearchableAuthors.Add(string.Empty);
            }
            else
            {
                foreach (string auth in author)
                {
                    SearchableAuthors.Add(nonAlphaNums.Replace(auth, ""));
                }
            }
        }

        public string serialise()
            => JsonConvert.SerializeObject(this);

        [OnDeserialized]
        private void DeSerialisationFixes(StreamingContext like_i_could_care)
        {
            // Make sure our version fields are populated.
            // TODO: There's got to be a better way of doing this, right?

            // Now see if we've got version with version min/max.
            if (ksp_version != null && (ksp_version_max != null || ksp_version_min != null))
            {
                // KSP version mixed with min/max.
                throw new InvalidModuleAttributesException(Properties.Resources.CkanModuleKspVersionMixed, this);
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
                        || (ksp_version != null && !module.IsCompatible(ksp_version)))
                {
                    throw new ModuleNotFoundKraken(ident, version,
                        string.Format(Properties.Resources.CkanModuleNotAvailable, ident, version));
                }
                return module;
            }
            return null;
        }

        public static readonly Regex idAndVersionMatcher =
            new Regex(@"^(?<mod>[^=]*)=(?<version>.*)$",
                      RegexOptions.Compiled);

        /// <summary> Generates a CKAN.Meta object given a filename</summary>
        public static CkanModule FromFile(string filename)
            => FromJson(File.ReadAllText(filename));

        public static void ToFile(CkanModule module, string filename)
        {
            File.WriteAllText(filename, ToJson(module));
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
        /// Generates a CkanModule object from a string.
        /// Also validates that all required fields are present.
        /// Throws a BadMetaDataKraken if any fields are missing.
        /// </summary>
        public static CkanModule FromJson(string json)
            => new CkanModule(json);

        #endregion

        /// <summary>
        /// Returns true if we conflict with the given module.
        /// </summary>
        public bool ConflictsWith(CkanModule module)
            // We never conflict with ourselves, since we can't be installed at
            // the same time as another version of ourselves.
            => module.identifier != identifier
                && (UniConflicts(this, module) || UniConflicts(module, this));

        /// <summary>
        /// Checks if A conflicts with B, but not if B conflicts with A.
        /// Used by ConflictsWith.
        /// </summary>
        internal static bool UniConflicts(CkanModule mod1, CkanModule mod2)
            => mod1?.conflicts?.Any(
                   conflict => conflict.MatchesAny(new CkanModule[] {mod2}, null, null))
               ?? false;

        /// <summary>
        /// Returns true if our mod is compatible with the KSP version specified.
        /// </summary>
        public bool IsCompatible(GameVersionCriteria version)
        {
            var compat = _comparator.Compatible(version, this);
            log.DebugFormat("Checking compat of {0} with game versions {1}: {2}",
                this, version.ToString(), compat ? "Compatible": "Incompatible");
            return compat;
        }

        /// <summary>
        /// Returns machine readable object indicating the highest compatible
        /// version of KSP this module will run with.
        /// </summary>
        public GameVersion LatestCompatibleGameVersion()
            // Find the highest compatible KSP version
            => ksp_version_max ?? ksp_version
               // No upper limit.
               ?? GameVersion.Any;

        /// <summary>
        /// Returns machine readable object indicating the lowest compatible
        /// version of KSP this module will run with.
        /// </summary>
        public GameVersion EarliestCompatibleGameVersion()
            // Find the lowest compatible KSP version
            => ksp_version_min ?? ksp_version
               // No lower limit.
               ?? GameVersion.Any;

        /// <summary>
        /// Return the latest game version from the given list that is
        /// compatible with this module, without the build number.
        /// </summary>
        /// <param name="realVers">Game versions to test, sorted from earliest to latest</param>
        /// <returns>The latest game version if any, else null</returns>
        public GameVersion LatestCompatibleRealGameVersion(List<GameVersion> realVers)
            => LatestCompatibleRealGameVersion(new GameVersionRange(EarliestCompatibleGameVersion(),
                                                                    LatestCompatibleGameVersion()),
                                               realVers);

        private GameVersion LatestCompatibleRealGameVersion(GameVersionRange range,
                                                            List<GameVersion> realVers)
            => (realVers?.LastOrDefault(v => range.Contains(v))
                        ?? LatestCompatibleGameVersion());

        public bool IsMetapackage => kind == "metapackage";

        public bool IsDLC => kind == "dlc";

        protected bool Equals(CkanModule other)
            => string.Equals(identifier, other.identifier) && version.Equals(other.version);

        public override bool Equals(object obj)
            => !(obj is null)
                && (ReferenceEquals(this, obj)
                    || (obj.GetType() == GetType() && Equals((CkanModule)obj)));

        public bool MetadataEquals(CkanModule other)
        {
            if ((install == null) != (other.install == null)
                    || (install != null
                        && install.Length != other.install.Length))
            {
                return false;
            }
            else if (install != null)
            {
                for (int i = 0; i < install.Length; i++)
                {
                    if (!install[i].Equals(other.install[i]))
                    {
                        return false;
                    }
                }
            }
            if (install_size != other.install_size)
            {
                return false;
            }

            if (!RelationshipsAreEquivalent(conflicts,  other.conflicts))
            {
                return false;
            }

            if (!RelationshipsAreEquivalent(depends,    other.depends))
            {
                return false;
            }

            if (!RelationshipsAreEquivalent(recommends, other.recommends))
            {
                return false;
            }

            if (replaced_by == null ? other.replaced_by != null
                                    : !replaced_by.Equals(other.replaced_by))
            {
                return false;
            }

            if (provides != other.provides)
            {
                if (provides == null || other.provides == null)
                {
                    return false;
                }
                else if (!provides.OrderBy(i => i).SequenceEqual(other.provides.OrderBy(i => i)))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool RelationshipsAreEquivalent(List<RelationshipDescriptor> a, List<RelationshipDescriptor> b)
        {
            if (a == b)
            {
                // If they're the same exact object they must be equivalent
                return true;
            }

            if (a == null || b == null)
            {
                // If they're not the same exact object and either is null then must not be equivalent
                return false;
            }

            if (a.Count != b.Count)
            {
                // If their counts different they must not be equivalent
                return false;
            }

            // Sort the lists so we can compare each relationship
            var aSorted = a.OrderBy(i => i.ToString()).ToList();
            var bSorted = b.OrderBy(i => i.ToString()).ToList();

            for (var i = 0; i < a.Count; i++)
            {
                var aRel = aSorted[i];
                var bRel = bSorted[i];

                if (!aRel.Equals(bRel))
                {
                    return false;
                }
            }

            // If we couldn't find any differences they must be equivalent
            return true;
        }

        public override int GetHashCode()
            => (identifier, version).GetHashCode();

        bool IEquatable<CkanModule>.Equals(CkanModule other)
            => Equals(other);

        /// <summary>
        /// Returns true if we support at least spec_version of the CKAN spec.
        /// </summary>
        internal static bool IsSpecSupported(ModuleVersion spec_version)
            => Meta.ReleaseVersion.IsGreaterThan(spec_version);

        /// <summary>
        /// Returns true if we support the CKAN spec used by this module.
        /// </summary>
        private bool IsSpecSupported()
            => IsSpecSupported(spec_version);

        /// <summary>
        ///     Returns a standardised name for this module, in the form
        ///     "identifier-version.zip". For example, `RealSolarSystem-7.3.zip`
        /// </summary>
        public string StandardName()
            => StandardName(identifier, version);

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
            => string.Format("{0} {1}", identifier, version);

        public string DescribeInstallStanzas(IGame game)
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
                descriptions.Add(ModuleInstallDescriptor.DefaultInstallStanza(game, identifier).DescribeMatch());
            }
            return string.Join(", ", descriptions);
        }

        /// <summary>
        /// Return an archive.org URL for this download, or null if it's not there.
        /// The filenames look a lot like the filenames in Net.Cache, but don't be fooled!
        /// Here it's the first 8 characters of the SHA1 of the DOWNLOADED FILE, not the URL!
        /// </summary>
        public Uri InternetArchiveDownload
            => !license.Any(l => l.Redistributable)
                ? null
                : InternetArchiveURL(
                    Truncate(bucketExcludePattern.Replace(identifier + "-"
                                                                  + version.ToString()
                                                                           .Replace(' ', '_')
                                                                           .Replace(':', '-'),
                                                                  ""),
                             100),
                    // Some alternate registry repositories don't set download_hash
                    download_hash?.sha1);

        private static string Truncate(string s, int len)
            => s.Length <= len ? s
                               : s.Substring(0, len);

        private static Uri InternetArchiveURL(string bucket, string sha1)
            => string.IsNullOrEmpty(sha1)
                ? null
                : new Uri($"https://archive.org/download/{bucket}/{sha1.Substring(0, 8)}-{bucket}.zip");

        // InternetArchive says:
        // Bucket names should be valid archive identifiers;
        // try someting matching this regular expression:
        // ^[a-zA-Z0-9][a-zA-Z0-9_.-]{4,100}$
        // (We enforce everything except the minimum of 4 characters)
        private static readonly Regex bucketExcludePattern = new Regex(@"^[^a-zA-Z0-9]+|[^a-zA-Z0-9._-]",
                                                                       RegexOptions.Compiled);

        private const double K = 1024;

        /// <summary>
        /// Format a byte count into readable file size
        /// </summary>
        /// <param name="bytes">Number of bytes in a file</param>
        /// <returns>
        /// ### bytes or ### KiB or ### MiB or ### GiB or ### TiB
        /// </returns>
        public static string FmtSize(long bytes)
            => bytes < K       ? $"{bytes} B"
             : bytes < K*K     ? $"{bytes /K :N1} KiB"
             : bytes < K*K*K   ? $"{bytes /K/K :N1} MiB"
             : bytes < K*K*K*K ? $"{bytes /K/K/K :N1} GiB"
             :                   $"{bytes /K/K/K/K :N1} TiB";

        public HashSet<CkanModule> GetDownloadsGroup(IEnumerable<CkanModule> modules)
            => OneDownloadGroupingPass(modules.ToHashSet(), this);

        public static List<HashSet<CkanModule>> GroupByDownloads(IEnumerable<CkanModule> modules)
        {
            // Each module is a vertex, each download URL is an edge
            // We want to group the vertices by transitive connectedness
            // We can go breadth first or depth first
            // Once we encounter a mod, we never have to look at it again
            var unsearched = modules.ToHashSet();
            var groups = new List<HashSet<CkanModule>>();
            while (unsearched.Count > 0)
            {
                groups.Add(OneDownloadGroupingPass(unsearched, unsearched.First()));
            }
            return groups;
        }

        private static HashSet<CkanModule> OneDownloadGroupingPass(HashSet<CkanModule> unsearched,
                                                                   CkanModule firstModule)
        {
            var searching = new List<CkanModule> { firstModule };
            unsearched.ExceptWith(searching);
            var found = searching.ToHashSet();
            // Breadth first search to find all modules with any URLs in common, transitively
            while (searching.Count > 0)
            {
                var origin = searching.First();
                searching.Remove(origin);
                var neighbors = origin.download
                    .SelectMany(dlUri => unsearched.Where(other => other.download.Contains(dlUri)))
                    .ToHashSet();
                unsearched.ExceptWith(neighbors);
                searching.AddRange(neighbors);
                found.UnionWith(neighbors);
            }
            return found;
        }

        /// <summary>
        /// Find the minimum and maximum mod versions and compatible game versions
        /// for a list of modules (presumably different versions of the same mod).
        /// </summary>
        /// <param name="modVersions">The modules to inspect</param>
        /// <param name="minMod">Return parameter for the lowest  mod  version</param>
        /// <param name="maxMod">Return parameter for the highest mod  version</param>
        /// <param name="minGame">Return parameter for the lowest  game version</param>
        /// <param name="maxGame">Return parameter for the highest game version</param>
        public static void GetMinMaxVersions(IEnumerable<CkanModule> modVersions,
                out ModuleVersion minMod,  out ModuleVersion maxMod,
                out GameVersion   minGame, out GameVersion   maxGame)
        {
            minMod  = maxMod  = null;
            minGame = maxGame = null;
            foreach (CkanModule rel in modVersions.Where(v => v != null))
            {
                if (minMod == null || minMod > rel.version)
                {
                    minMod = rel.version;
                }
                if (maxMod == null || maxMod < rel.version)
                {
                    maxMod = rel.version;
                }
                GameVersion relMin = rel.EarliestCompatibleGameVersion();
                GameVersion relMax = rel.LatestCompatibleGameVersion();
                if (minGame == null || (!minGame.IsAny && (minGame > relMin || relMin.IsAny)))
                {
                    minGame = relMin;
                }
                if (maxGame == null || (!maxGame.IsAny && (maxGame < relMax || relMax.IsAny)))
                {
                    maxGame = relMax;
                }
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
