using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// Find all the modules we have installed.
// This may be from a local cache, or by crawling GameData itself.

namespace CKAN {

	/// <summary>
	/// Utility class for holding dictionaries of modules.
	/// </summary>

	public class ModuleDict : Dictionary<String, Module> {

		public void scanGameData() {
			// TODO: It would be great to optimise this to skip .git directories and the like.
			// Yes, I keep my GameData in git.

			// TODO: Optimisation: Do all the below in a single pass, rather than walking
			// GameData twice.

			// Console.WriteLine ("In ModuleDict");

			RegistryManager registry_manager = new RegistryManager("/tmp/ksp_registry");

			Registry registry = registry_manager.load_or_create ();

			string gameData = KSP.gameData ();

			// Find all the DLLs. ModuleManager assumes that if a DLL exists, then a mod
			// is installed by the same name (after clipping off versions).

			// Console.WriteLine ("Finding DLLs");

			// TODO: We should *empty* the dll container first, so if things have been
			// uninstalled, we no longer have record of them.

			string[] dllFiles = Directory.GetFiles (gameData, "*.dll", SearchOption.AllDirectories);

			// TODO: Check how ModuleManager transforms DLL names into mod names, and make
			// sure we do things the same way.

			foreach (string file in dllFiles) {

				registry.register_dll (file);

				// We're going to clip at the first period to get the ModuleName.
				// We'll get some false positives from bundled support libraries, but nobody's
				// going to depend upon those or treat them as proper mods (I hope!)

				string module = Regex.Replace (file, "^.*/([^.]+).*", "$1");

				this [module] = new Module ();
				this [module].version = "0";     // We can say it exists, but have no idea of other info.
				this [module].identifier = module;

				// Console.WriteLine (this[module]._identifier + " ( " + file + " ) ");
			}

			registry_manager.save (registry);

			// Search all directories, find .ckan files.
			// These will *overwrite* the results from above, and that's cool,
			// CKAN files give us much richer meta-info than dlls.

			// Find all the CKAN files. Presumably, these are actually representative
			// of what we have installed.

			// Console.WriteLine ("Finding CKAN files");

			string[] ckanFiles = Directory.GetFiles(gameData, "*.ckan", SearchOption.AllDirectories);

			foreach (string file in ckanFiles) {
				Module module = Module.from_file (file);

				this [module.identifier] = module;

				// Console.WriteLine (module._identifier + " " + module._version + " ( " + file + " ) ");

			}

		}

		public void showInstalled() {
			foreach(KeyValuePair<string, Module> entry in this) {
				Console.WriteLine (entry.Value.identifier + " " + entry.Value.version);
			}

			return;
		}
	}
}

