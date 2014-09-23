using System;
using System.IO;
using System.Net;
using System.Linq;

using Newtonsoft.Json;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

using System.Text.RegularExpressions;

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

// TODO: It would be *awesome* if the schema could generate this for us.
// TODO: Code style: Should we have Module.meta.attribute paths, so we can
//       get exactly what we found from the JSON?

// TODO: Currently we have all attributes from JSON starting with an underscore,
//       that's super-ugly. What's the preferred C# way of doing this?

namespace CKAN {			

	[JsonObject(MemberSerialization.OptIn)]
	public class Module {

		[JsonProperty("name", Required = Required.Always)]
		public string _name;

		[JsonProperty("identifier", Required = Required.Always)]
		public string _identifier; // TODO: Strong type

		// TODO: Change spec: abstract -> description
		[JsonProperty("abstract", Required = Required.Always)]
		public string _abstract;

		[JsonProperty("comment")]
		public string _comment;

		[JsonProperty("author")]
		public string[] _author;

		[JsonProperty("download", Required = Required.Always)]        
		public Uri    _download;

		[JsonProperty("license", Required= Required.Always)]
		public dynamic _license; // TODO: Strong type

		[JsonProperty("version", Required = Required.Always)]
		public string _version; // TODO: Strong type

		[JsonProperty("release_status")]
		public string _release_status; // TODO: Strong type

		[JsonProperty("min_ksp")]
		public string _min_ksp; // TODO: Type

		[JsonProperty("max_ksp")]
		public string _max_ksp; // TODO: Type

		[JsonProperty("requires")]
		public dynamic[] _requires;

		[JsonProperty("recommends")]
		public dynamic[] _recommends;

		[JsonProperty("conflicts")]
		public dynamic[] _conflicts;

		[JsonProperty("resourcs")]
		public dynamic[] _resources;

		[JsonProperty("install", Required = Required.Always)]
		public dynamic[] _install;

		[JsonProperty("bundles")]
		public dynamic[] _bundles;

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
		/// Download the given mod. Returns the filename it was saved to.
		/// 
		/// If no filename is provided, the standard_name() will be used.
		/// 
		/// </summary>
		/// <param name="filename">Filename.</param>
		public string download(string filename = null) {

			// Generate a temporary file if none is provided.
			if (filename == null) {
				filename = standard_name();
			}

			WebClient agent = new WebClient ();
			agent.DownloadFile (_download, filename);

			return filename;
		}

		/// <summary>
		/// Returns a standardised name for this module, in the form
		/// "identifier-version.zip". For example, `RealSolarSystem-7.3.zip`
		/// </summary>
		public string standard_name() {
			return _identifier + "-" + _version + ".zip";
		}

		/// <summary>
		/// Install our mod from the filename supplied.
		/// If no file is supplied, we will fetch() it first.
		/// </summary>

		public void install(string filename = null) {

			// Fetch our file if we don't already have it.
			if (filename == null) {
				filename = download ();
			}

			// FastZip lets us extract the bits we need.
			// FastZip zipfile = new FastZip ();

			// TODO: Actually figure out where KSP is installed, rather than
			// assuming it's here.

			string gameData = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.Personal),
				".steam", "steam", "SteamApps", "common", "Kerbal Space Program", "GameData"
			);

			Console.WriteLine (gameData);

			ZipFile zipfile = new ZipFile (File.OpenRead (filename));

			// And walk through our install instructions.
			foreach (dynamic stanza in _install) {

				string fileToInstall = stanza.file;

				string[] path = fileToInstall.Split('/');

				string installDir = gameData;
				string stripDir   = String.Join("/", path.Take(path.Count() - 1)) + "/";

				Console.WriteLine("InstallDir is "+installDir);
				Console.WriteLine ("StripDir is " + stripDir);
				
				// Ugh. This is awful. There's got to be a better way to extract a tree?
				string filter = "^" + stanza.file + "(/|$)";

				// Ugh. O(N^2) solution. Surely there's a better way...
				foreach (ZipEntry entry in zipfile) {
					if ( Regex.IsMatch( entry.Name, filter ) ) {
						// Hooray! A file we want!

						// Get the full name of the file.
						string outputName = entry.Name;

						// Strip off the prefix (often GameData/)
						// TODO: The C# equivalent of "\Q stripDir \E" so we can't be caught by metacharacters.
						outputName = Regex.Replace (outputName, @"^" + stripDir, "");

						// Console.WriteLine(outputName);

						// Aww hell yes, let's write this file out!

						string fullPath = Path.Combine (installDir, outputName);
						// Console.WriteLine (fullPath);

						if (entry.IsDirectory) {
							Console.WriteLine ("Making directory " + fullPath);
							Directory.CreateDirectory (fullPath);
						}
						else {
							Console.WriteLine ("Writing file " + fullPath);

							// It's a file! Prepare the streams
							Stream zipStream = zipfile.GetInputStream(entry);
							FileStream output = File.Create (fullPath);

							// Copy
							zipStream.CopyTo (output);

							// Tidy up.
							zipStream.Close();
							output.Close();
						}
					}
				}


				// zipfile.NameTransform = new StripLeadingDir (filename);

				// zipfile.ExtractZip (filename, destination, filter);
			}



			// And (for now) display the contents!
			// foreach (ZipEntry entry in zipfile) {
			//	Console.WriteLine(entry.Name);
			// }

			// Can we do that twice?

		}
	}
}

