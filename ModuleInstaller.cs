using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.Text.RegularExpressions;

namespace CKAN
{
	public class ModuleInstaller
	{
		RegistryManager registry_manager = new RegistryManager("/tmp/ksp_registry");

		public ModuleInstaller ()
		{
		}

		/// <summary>
		/// Download the given mod. Returns the filename it was saved to.
		///
		/// If no filename is provided, the standard_name() will be used.
		///
		/// </summary>
		/// <param name="filename">Filename.</param>
		public string download (Module module, string filename = null)
		{

			// Generate a temporary file if none is provided.
			if (filename == null) {
				filename = module.standard_name ();
			}

			Console.WriteLine ("    * Downloading " + filename + "...");

			WebClient agent = new WebClient ();
			agent.DownloadFile (module.download, filename);

			return filename;
		}

		/// <summary>
		/// Install our mod from the filename supplied.
		/// If no file is supplied, we will fetch() it first.
		/// </summary>

		public void install (Module module, string filename = null)
		{
			List<InstalledModuleFile> module_files = new List<InstalledModuleFile> ();

			Console.WriteLine (module.identifier + ":\n");

			// Fetch our file if we don't already have it.
			if (filename == null) {
				filename = download (module);
			}

			// Open our zip file for processing
			ZipFile zipfile = new ZipFile (File.OpenRead (filename));

			// Walk through our install instructions.
			foreach (dynamic stanza in module.install) {
				install_component (stanza, zipfile, module_files);

				// TODO: We should just *serialise* our current state, not
				// copy the original file, because we can't always guarantee
				// there will be an original file.

				// TODO: This will just throw them in GameData.
				// We need a way to convert stanzas to install locations.
				// We really should make Stanza its own class.

				File.WriteAllText (KSP.gameData () + module.identifier + ".ckan", module.serialise());
			}

			// Handle bundled mods, if we have them.
			if (module.bundles != null) {

				foreach (dynamic stanza in module.bundles) {

					// TODO: Check versions, so we don't double install.

					install_component (stanza, zipfile, module_files);

					// TODO: Generate CKAN metadata for the bundled component.
				}
			}

			Registry registry = registry_manager.load_or_create ();
			registry_manager.save(
				registry.append (new InstalledModule (module_files.ToArray(), module, DateTime.Now)));

			return;

		}

		string sha1_sum (string path)
		{
			SHA1 hasher = new SHA1CryptoServiceProvider();

			try {
				return BitConverter.ToString(hasher.ComputeHash (File.OpenRead (path)));
			}
			catch {
				return null;
			};
		}

		void install_component (dynamic stanza, ZipFile zipfile, List<InstalledModuleFile> module_files)
		{
			string fileToInstall = stanza.file;

			Console.WriteLine ("    * Installing " + fileToInstall);

			string[] path = fileToInstall.Split ('/');

			// TODO: This will depend upon the `install_to` in the JSON file
			string installDir = KSP.gameData ();

			// This is what we strip off paths
			string stripDir = String.Join ("/", path.Take (path.Count () - 1)) + "/";

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

				module_files.Add (new InstalledModuleFile {
					sha1_sum = sha1_sum (fullPath),
					name = outputName,
				});
			}

			return;
		}
		// TODO: Test that this actually throws exceptions if it can't do its job.
		void copyZipEntry (ZipFile zipfile, ZipEntry entry, string fullPath)
		{

			if (entry.IsDirectory) {
				// Console.WriteLine ("Making directory " + fullPath);
				Directory.CreateDirectory (fullPath);
			} else {
				// Console.WriteLine ("Writing file " + fullPath);

				// It's a file! Prepare the streams
				Stream zipStream = zipfile.GetInputStream (entry);
				FileStream output = File.Create (fullPath);

				// Copy
				zipStream.CopyTo (output);

				// Tidy up.
				zipStream.Close ();
				output.Close ();
			}

			return;

		}
	}
}