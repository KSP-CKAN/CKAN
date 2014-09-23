using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;

/// <summary>
/// Describes a CKAN module (ie, what's in the CKAN.schema file).
/// 
/// A lot of heavy lifting is done here; including fetching and installing.
///
/// Using Mono, and getting certificate errors? Populate the certificate store with:
/// `mozroots --import --ask-remove`
/// 
/// </summary>
/// 
// TODO: It would be *awesome* if the schema could generate this for us.

namespace CKAN {			

	[JsonObject(MemberSerialization.OptIn)]
	public class Module {

		[JsonProperty("name", Required = Required.Always)]
		public string name;

		[JsonProperty("identifier", Required = Required.Always)]
		public string identifier; // TODO: Strong type

		// TODO: Change spec: abstract -> description
		[JsonProperty("abstract", Required = Required.Always)]
		public string description;

		[JsonProperty("comment")]
		public string comment;

		[JsonProperty("author")]
		public string[] author;

		[JsonProperty("download", Required = Required.Always)]        
		public Uri    download;

		[JsonProperty("license", Required= Required.Always)]
		public dynamic license; // TODO: Strong type

		[JsonProperty("version", Required = Required.Always)]
		public string version; // TODO: Strong type

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

		[JsonProperty("resourcs")]
		public dynamic[] resources;

		[JsonProperty("install", Required = Required.Always)]
		public dynamic[] install;

		[JsonProperty("bundles")]
		public dynamic[] bundles;

		/// <summary> Generates a CKAN.Meta object given a filename</summary>
		public static Module from_file(string filename) {
			string json = System.IO.File.ReadAllText (filename);

			return Module.from_string (json);
		}

		/// <summary> Generates a CKAN.META object from a string </summary>
		public static Module from_string(string json) {
			return JsonConvert.DeserializeObject<Module> (json);
		}

		/// <summary>
		/// Fetch the given mod. Returns the filename it was saved to.
		/// 
		/// If no filename is provided, the standard_name() will be used.
		/// 
		/// </summary>
		/// <param name="filename">Filename.</param>
		public string fetch(string filename = null) {

			// Generate a temporary file if none is provided.
			if (filename == null) {
				filename = standard_name();
			}

			WebClient agent = new WebClient ();

			Console.WriteLine (download);
			agent.DownloadFile (download, filename);

			return filename;
		}

		/// <summary>
		/// Returns a standardised name for this module, in the form
		/// "identifier-version.zip". For example, `RealSolarSystem-7.3.zip`
		/// </summary>
		public string standard_name() {
			return identifier + "-" + version + ".zip";
		}
	}
}

