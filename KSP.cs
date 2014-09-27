using System;
using System.IO;

/// <summary>
/// Everything for dealing with KSP itself.
/// </summary>

namespace CKAN {
	public class KSP {

		public static string gameDir() {

			// TODO: Have this *actually* find our GameData directory!

			return Path.Combine (
				Environment.GetFolderPath (Environment.SpecialFolder.Personal),
				".steam", "steam", "SteamApps", "common", "Kerbal Space Program"
			);
		}
	
		public static string gameData() {
			return Path.Combine (gameDir (), "GameData");
		}

		public static string ckanDir() {
			return Path.Combine (gameDir (), "CKAN");
		}

		public static string downloadCacheDir() {
			return Path.Combine (ckanDir (), "downloads");
		}

		/// <summary>
		/// Create the CKAN directory and any supporting files.
		/// </summary>
		public static void init() {
			if (! Directory.Exists (ckanDir ())) {
				Console.WriteLine ("Setting up CKAN for the first time...");
				Console.WriteLine ("Creating {0}", ckanDir ());
				Directory.CreateDirectory (ckanDir ());

				Console.WriteLine ("Scanning for installed mods...");
				scanGameData ();
			}

			if (! Directory.Exists( downloadCacheDir() )) {
				Console.WriteLine ("Creating {0}", downloadCacheDir ());
				Directory.CreateDirectory (downloadCacheDir ());
			}
		}

		public static void scanGameData() {

			// TODO: Get rid of magic paths!
			RegistryManager registry_manager = RegistryManager.Instance();
			Registry registry = registry_manager.registry;

			// Forget that we've seen any DLLs, as we're going to refresh them all.
			registry.clear_dlls ();

			// TODO: It would be great to optimise this to skip .git directories and the like.
			// Yes, I keep my GameData in git.

			string[] dllFiles = Directory.GetFiles (gameData(), "*.dll", SearchOption.AllDirectories);

			foreach (string file in dllFiles) {
				// register_dll does the heavy lifting of turning it into a modname
				registry.register_dll (file);
			}

			registry_manager.save();
		}
	}
}