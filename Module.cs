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

        // identifier, license, and version are always required, so we know
        // what we've got.

        [JsonProperty("identifier", Required = Required.Always)]
        public string identifier;

        [JsonProperty("license", Required = Required.Always)]
        public dynamic license; // TODO: Strong type

        [JsonProperty("version", Required = Required.Always)]
        public string version; // TODO: Strong type

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

        [OnDeserialized]
        private void DeSerialisationFixes(StreamingContext like_i_could_care) {

            if (ksp_version != null && (ksp_version_max != null || ksp_version_min != null)) {
                // KSP version mixed with min/max.
                throw new InvalidModuleAttributesException ("ksp_version mixed wtih ksp_version_(min|max)", this);
            }

            // Make sure our version fields are populated.

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

        }

        /// <summary>
        /// Returns true if our mod is compatible with the KSP version specified.
        /// </summary>

        public bool IsCompatibleKSP(string v) {

            KSPVersion version = new KSPVersion (v);

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
            version    = stanza.version;
            identifier = stanza.identifier;
            license    = stanza.license;
        }
    }

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

        /// <summary> Generates a CKAN.Meta object given a filename</summary>
        public static CkanModule from_file(string filename) {
            string json = System.IO.File.ReadAllText (filename);
            return CkanModule.from_string (json);
        }

        /// <summary> Generates a CKAN.META object from a string.
        /// Also validates that all required fields are present.
        /// </summary>
        public static CkanModule from_string(string json) {
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