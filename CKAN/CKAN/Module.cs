using System;
using Newtonsoft.Json;

/// <summary>
/// Describes a CKAN module (ie, what's in the CKAN.schema file).
/// 
/// 
/// A lot of heavy lifting is done here; including fetching and installing.
///
/// Using Mono, and getting certificate errors? Populate the certificate store with:
/// `mozroots --import --ask-remove`
/// 
/// </summary>
///

namespace CKAN {

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

		[JsonProperty("min_ksp")]
		public string min_ksp; // TODO: Type

		[JsonProperty("max_ksp")]
		public string max_ksp; // TODO: Type

		[JsonProperty("requires")]
		public dynamic[] requires;

		[JsonProperty("recommends")]
		public dynamic[] recommends;

		[JsonProperty("conflicts")]
		public dynamic[] conflicts;

		[JsonProperty("resources")]
		public dynamic resources;

		public string serialise ()
		{
			return JsonConvert.SerializeObject (this);
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
					Console.WriteLine ("Missing required field: {0}", field);
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
		public string standard_name ()
		{
			return identifier + "-" + version + ".zip";
		}

	}
}