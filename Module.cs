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

// TODO: It would be *awesome* if the schema could generate all the JsonProperty
//       things for us.

// TODO: Currently we have all attributes from JSON starting with an underscore,
//       that's kinda ugly. What's the preferred C# way of doing this? Do we
//       just want Module.meta.whatever ?

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

		// Private record of which file we came from.
		string origCkanFile;

		/// <summary> Generates a CKAN.Meta object given a filename</summary>
		public static Module from_file(string filename) {
			string json = System.IO.File.ReadAllText (filename);

			Module built = Module.from_string (json);

			// Attach which file this came from.
			built.origCkanFile = filename;

			return built;
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

			// Open our zip file for processing
			ZipFile zipfile = new ZipFile (File.OpenRead (filename));

			// Walk through our install instructions.
			foreach (dynamic stanza in _install) {
				install_component (stanza, zipfile);

				// TODO: We should just *serialise* our current state, not
				// copy the original file, because we can't always guarantee
				// there will be an original file.

				// XXX: This will just throw them in GameData.
				// We need a way to convert stanzas to install locations.
				// We really should make Stanza its own class.
				File.Copy (origCkanFile, gameData() + "/" + _identifier + ".ckan" );
			}

			// Finish now if we have no bundled mods.
			if (_bundles == null) { return; }

			// Do the same with our bundled mods.
			foreach (dynamic stanza in _bundles) {

				// TODO: Check versions, so we don't double install.

				install_component (stanza, zipfile);

				// TODO: Generate CKAN metadata for the bundled component.
			}

			return;

		}

		void install_component(dynamic stanza, ZipFile zipfile) {
			string fileToInstall = stanza.file;

			Console.WriteLine ("Installing " + fileToInstall);

			string[] path = fileToInstall.Split('/');

			// TODO: This will depend upon the `install_to` in the JSON file
			string installDir = gameData ();

			// This is what we strip off paths
			string stripDir   = String.Join("/", path.Take(path.Count() - 1)) + "/";

			// Console.WriteLine("InstallDir is "+installDir);
			// Console.WriteLine ("StripDir is " + stripDir);

			// This is awful. There's got to be a better way to extract a tree?
			string filter = "^" + stanza.file + "(/|$)";

			// O(N^2) solution. Surely there's a better way...
			foreach (ZipEntry entry in zipfile) {

				// Skip things we don't want.
				if (! Regex.IsMatch (entry.Name, filter)) {
					continue;
				}

				// Get the full name of the file.
				string outputName = entry.Name;

				// Strip off the prefix (often GameData/)
				// TODO: The C# equivalent of "\Q stripDir \E" so we can't be caught by metacharacters.
				outputName = Regex.Replace (outputName, @"^" + stripDir, "");

				// Aww hell yes, let's write this file out!

				string fullPath = Path.Combine (installDir, outputName);
				// Console.WriteLine (fullPath);

				copyZipEntry (zipfile, entry, fullPath);
			}

			return;
		}

		// TODO: Have this *actually* find our GameData directory!
		public static string gameData() {
			return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.Personal),
				".steam", "steam", "SteamApps", "common", "Kerbal Space Program", "GameData"
			);
		}

		// TODO: Test that this actually throws exceptions if it can't do its job.
		void copyZipEntry(ZipFile zipfile, 	ZipEntry entry, string fullPath) {
			
			if (entry.IsDirectory) {
				// Console.WriteLine ("Making directory " + fullPath);
				Directory.CreateDirectory (fullPath);
			}
			else {
				// Console.WriteLine ("Writing file " + fullPath);

				// It's a file! Prepare the streams
				Stream zipStream = zipfile.GetInputStream(entry);
				FileStream output = File.Create (fullPath);

				// Copy
				zipStream.CopyTo (output);

				// Tidy up.
				zipStream.Close();
				output.Close();
			}

			return;

		}
	}
}

