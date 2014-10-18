using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace CKAN {

    using System;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using log4net;

    /// <summary>
    /// Describes a CKAN module (ie, what's in the CKAN.schema file).
    /// </summary>

    // Base class for both modules (installed via the CKAN) and bundled
    // modules (which are more lightweight)

    [JsonObject(MemberSerialization.OptIn)]
    public class Module {

        private static readonly ILog log = LogManager.GetLogger (typeof(Module));

        // identifier, license, and version are always required, so we know
        // what we've got.

        [JsonProperty("identifier", Required = Required.Always)]
        public string identifier;

        [JsonProperty("license", Required = Required.Always)]
        public dynamic license; // TODO: Strong type

        [JsonProperty("version", Required = Required.Always)]
        public Version version;

        // We also have lots of optional attributes.

        [JsonProperty("name")]
        public string name;

        [JsonProperty("abstract")]
        public string @abstract;

        [JsonProperty("comment")]
        public string comment;

        [JsonProperty("author")]
        public string[] author;

        [JsonProperty("download")]        
        public Uri    download;

        [JsonProperty("release_status")]
        public string release_status; // TODO: Strong type

        [JsonProperty("ksp_version")]
        public KSPVersion ksp_version;

        [JsonProperty("ksp_version_min")]
        public KSPVersion ksp_version_min;

        [JsonProperty("ksp_version_max")]
        public KSPVersion ksp_version_max;

        [JsonProperty("provides")]
        public string[] provides;

        [JsonProperty("pre_depends")]
        public dynamic[] pre_depends;

        [JsonProperty("depends")]
        public dynamic[] depends;

        [JsonProperty("recommends")]
        public dynamic[] recommends;

        [JsonProperty("suggests")]
        public dynamic[] suggests;

        [JsonProperty("conflicts")]
        public dynamic[] conflicts;

        [JsonProperty("resources")]
        public dynamic resources;

        public string serialise () {
            return JsonConvert.SerializeObject (this);
        }

        public override string ToString () {
            return string.Format ("{0} {1}",identifier, version);
        }

        [OnDeserialized]
        private void DeSerialisationFixes(StreamingContext like_i_could_care) {

            // Make sure our version fields are populated.
            // TODO: There's got to be a better way of doing this, right?

            if (ksp_version_min == null) {
                ksp_version_min = new KSPVersion (null);
            } else {
                ksp_version_min.ToLongMin ();
            }

            if (ksp_version_max == null) {
                ksp_version_max = new KSPVersion (null);
            } else {
                ksp_version_max.ToLongMax ();
            }

            if (ksp_version == null) {
                ksp_version = new KSPVersion (null);
            }

            // Now see if we've got version with version min/max.
            if (ksp_version.IsNotAny() && (ksp_version_max.IsNotAny() || ksp_version_min.IsNotAny())) {
                // KSP version mixed with min/max.
                throw new InvalidModuleAttributesException ("ksp_version mixed wtih ksp_version_(min|max)", this);
            }
        }

        /// <summary>
        /// Returns true if our mod is compatible with the KSP version specified.
        /// </summary>

        public bool IsCompatibleKSP(string version) {
            return IsCompatibleKSP (new KSPVersion (version));
        }

        public bool IsCompatibleKSP(KSPVersion version) {

            log.DebugFormat ("Testing if {0} is compatible with KSP {1}", this, version);

            // Check the min and max versions.

            if (ksp_version_min.IsNotAny() && version < ksp_version_min ) {
                return false;
            }

            if (ksp_version_max.IsNotAny() && version > ksp_version_max) {
                return false;
            }

            // We didn't hit the min/max guards. They may not have existed.

            // Note that since ksp_version is "any" if not specified, this
            // will work fine if there's no target, or if there were min/max
            // fields and we passed them successfully.

            return ksp_version.Targets (version);
        }
    }
    
    public class BundledModule : Module {

        public BundledModule(dynamic stanza) {
            // For now, we just copy across the fields from our stanza.
            JValue jVersion = (JValue) stanza.version;
            version = new Version((string)jVersion.Value);
            identifier = stanza.identifier;
            license    = stanza.license;
        }
    }

    public class CkanInvalidMetadataJson : Exception { }

    public class CkanModule : Module {

        private static string[] required_fields = {
            "spec_version",
            "name",
            "abstract",
            "identifier",
            "download",
            "license",
            "version"
        };

        // Only CKAN modules can have install and bundle instructions.

        [JsonProperty("install")]
        public dynamic[] install;

        [JsonProperty("bundles")]
        public dynamic[] bundles;

        [JsonProperty("spec_version")]
        public string spec_version;

        private static readonly ILog log = LogManager.GetLogger (typeof(CkanModule));

        private static JsonSchema metadata_schema = null;
        private static string metadata_schema_path = "CKAN.schema";
        private static bool metadata_schema_missing_warning_fired = false;

        private static bool validate_json_against_schema(string json)
        {
            return true;
            // due to Newtonsoft Json not supporting v4 of the standard, we can't actually do this :(

            if (metadata_schema == null) {
                string schema = "";

                try {
                    schema = System.IO.File.ReadAllText(metadata_schema_path);
                }
                catch (Exception) {
                    if (!metadata_schema_missing_warning_fired) {
                        User.Error("Couldn't open metadata schema at \"{0}\", will not validate metadata files", metadata_schema_path);
                        metadata_schema_missing_warning_fired = true;
                    }

                    return true;
                }

                metadata_schema = JsonSchema.Parse(schema);
            }

            JObject obj = JObject.Parse(json);
            return obj.IsValid(metadata_schema);
        }

        /// <summary> Generates a CKAN.Meta object given a filename</summary>
        public static CkanModule from_file(string filename) {
            string json = System.IO.File.ReadAllText (filename);
            return CkanModule.from_string (json);
        }

        /// <summary> Generates a CKAN.META object from a string.
        /// Also validates that all required fields are present.
        /// </summary>
        public static CkanModule from_string(string json) {
            if (!validate_json_against_schema(json))
            {
                throw new CkanInvalidMetadataJson();
            }

            CkanModule newModule = JsonConvert.DeserializeObject<CkanModule> (json);

            // Check everything in the spec if defined.
            // TODO: It would be great if this could be done with attributes.

            foreach (string field in required_fields) {
                object value = newModule.GetType ().GetField (field).GetValue (newModule);

                if (value == null) {
                    log.ErrorFormat ("Module {0} missing required field: {1}", newModule.identifier, field);
                    throw new MissingFieldException (); // Is there a better exception choice?
                }
            }

            // All good! Return module
            return newModule;
        }

        /// <summary>
        /// Returns a standardised name for this module, in the form
        /// "identifier-version.zip". For example, `RealSolarSystem-7.3.zip`
        /// </summary>
        public string StandardName ()
        {
            return identifier + "-" + version + ".zip";
        }

    }

    public class InvalidModuleAttributesException : Exception {
        private string why;
        private Module module;

        public InvalidModuleAttributesException(string why, Module module = null) {
            this.why = why;
            this.module = module;
        }

        public override string ToString () {
            string modname = "unknown";

            if (module != null) {
                modname = module.identifier;
            }

            return string.Format ("[InvalidModuleAttributesException] {0} in {1}", why, modname);
        }
    }
}