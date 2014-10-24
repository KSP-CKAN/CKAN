using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace CKAN
{
    public class RelationshipDescriptor
    {
        public string max_version;
        public string min_version;
        public /* required */ string name;
        public string version;
    }

    public abstract class InstallableDescriptor
    {
        public /* required */ string file;
        public /* required */ string install_to;
    }

    public class BundledModuleDescriptor : InstallableDescriptor
    {
        public /* required */ string identifier;
        public /* required */ string license;
        public /* required */ bool required;
        public /* required */ string version;
    }

    public class GitHubResourceDescriptor
    {
        public bool releases;
        public string url;
    }

    public class KerbalStuffResourceDescriptor
    {
        public /* optional */ string url;
    }

    public class ResourcesDescriptor
    {
        public GitHubResourceDescriptor github;
        public string homepage;

        public KerbalStuffResourceDescriptor kerbalstuff;
    }

    public class ModuleInstallDescriptor : InstallableDescriptor
    {
        public string description;
        public bool optional;
        public bool overwrite;
        public string requires;
    }

    public enum License
    {
        public_domain,
        Apache,
        Apache_1_0,
        Apache_2_0,
        Artistic,
        Artistic_1_0,
        Artistic_2_0,
        BSD_2_clause,
        BSD_3_clause,
        BSD_4_clause,
        ISC,
        CC_BY,
        CC_BY_1_0,
        CC_BY_2_0,
        CC_BY_2_5,
        CC_BY_3_0,
        CC_BY_4_0,
        CC_BY_SA,
        CC_BY_SA_1_0,
        CC_BY_SA_2_0,
        CC_BY_SA_2_5,
        CC_BY_SA_3_0,
        CC_BY_SA_4_0,
        CC_BY_NC,
        CC_BY_NC_1_0,
        CC_BY_NC_2_0,
        CC_BY_NC_2_5,
        CC_BY_NC_3_0,
        CC_BY_NC_4_0,
        CC_BY_NC_SA,
        CC_BY_NC_SA_1_0,
        CC_BY_NC_SA_2_0,
        CC_BY_NC_SA_2_5,
        CC_BY_NC_SA_3_0,
        CC_BY_NC_SA_4_0,
        CC_BY_NC_ND,
        CC_BY_NC_ND_1_0,
        CC_BY_NC_ND_2_0,
        CC_BY_NC_ND_2_5,
        CC_BY_NC_ND_3_0,
        CC_BY_NC_ND_4_0,
        CC0,
        CDDL,
        CPL,
        EFL_1_0,
        EFL_2_0,
        Expat,
        MIT,
        GPL_1_0,
        GPL_2_0,
        GPL_3_0,
        LGPL_2_0,
        LGPL_2_1,
        LGPL_3_0,
        GFDL_1_0,
        GFDL_1_1,
        GFDL_1_2,
        GFDL_1_3,
        GFDL_NIV_1_0,
        GFDL_NIV_1_1,
        GFDL_NIV_1_2,
        GFDL_NIV_1_3,
        LPPL_1_0,
        LPPL_1_1,
        LPPL_1_2,
        LPPL_1_3c,
        MPL_1_1,
        Perl,
        Python_2_0,
        QPL_1_0,
        W3C,
        Zlib,
        Zope,
        open_source,
        restricted,
        unrestricted,
        unknown
    }

    /// <summary>
    ///     Describes a CKAN module (ie, what's in the CKAN.schema file).
    /// </summary>

    // Base class for both modules (installed via the CKAN) and bundled
    // modules (which are more lightweight)
    [JsonObject(MemberSerialization.OptIn)]
    public class Module
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (Module));

        // identifier, license, and version are always required, so we know
        // what we've got.

        [JsonProperty("abstract")] public string @abstract;

        [JsonProperty("author")] [JsonConverter(typeof (JsonSingleOrArrayConverter<string>))] public List<string> author;

        [JsonProperty("comment")] public string comment;
        [JsonProperty("conflicts")] public RelationshipDescriptor[] conflicts;
        [JsonProperty("depends")] public RelationshipDescriptor[] depends;

        [JsonProperty("download")] public Uri download;
        [JsonProperty("download_size")] public long download_size;
        [JsonProperty("identifier", Required = Required.Always)] public string identifier;

        [JsonProperty("ksp_version")] public KSPVersion ksp_version;

        [JsonProperty("ksp_version_max")] public KSPVersion ksp_version_max;
        [JsonProperty("ksp_version_min")] public KSPVersion ksp_version_min;
        [JsonProperty("license", Required = Required.Always)] public string license; // TODO: Strong type

        [JsonProperty("name")] public string name;

        [JsonProperty("pre_depends")] public RelationshipDescriptor[] pre_depends;
        [JsonProperty("provides")] public string[] provides;

        [JsonProperty("recommends")] public RelationshipDescriptor[] recommends;
        [JsonProperty("release_status")] public string release_status; // TODO: Strong type

        [JsonProperty("resources")] public ResourcesDescriptor resources;
        [JsonProperty("suggests")] public RelationshipDescriptor[] suggests;
        [JsonProperty("version", Required = Required.Always)] public Version version;

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
    }

    public class BundledModule : Module
    {
        public BundledModule(BundledModuleDescriptor stanza)
        {
            // For now, we just copy across the fields from our stanza.
            version = new Version(stanza.version);
            identifier = stanza.identifier;
            license = stanza.license;
        }
    }

    public class CkanInvalidMetadataJson : Exception
    {
    }

    public class CkanModule : Module
    {
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

        private static readonly ILog log = LogManager.GetLogger(typeof (CkanModule));

//      private static JsonSchema metadata_schema;
//      private static string metadata_schema_path = "CKAN.schema";
//      private static bool metadata_schema_missing_warning_fired;
        [JsonProperty("bundles")] public BundledModuleDescriptor[] bundles;
        [JsonProperty("install")] public ModuleInstallDescriptor[] install;
        [JsonProperty("spec_version")] public string spec_version;

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
        ///     Generates a CKAN.META object from a string.
        ///     Also validates that all required fields are present.
        /// </summary>
        public static CkanModule FromJson(string json)
        {
            if (!validate_json_against_schema(json))
            {
                throw new CkanInvalidMetadataJson();
            }

            var newModule = JsonConvert.DeserializeObject<CkanModule>(json);

            // Check everything in the spec if defined.
            // TODO: It would be great if this could be done with attributes.

            foreach (string field in required_fields)
            {
                object value = newModule.GetType().GetField(field).GetValue(newModule);

                if (value == null)
                {
                    log.ErrorFormat("Module {0} missing required field: {1}", newModule.identifier, field);
                    throw new MissingFieldException(); // Is there a better exception choice?
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
