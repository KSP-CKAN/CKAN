using System;
using System.IO;

/// <summary>
/// Everything for dealing with KSP itself.
/// </summary>

namespace CKAN {
	public class KSP {
		// TODO: Have this *actually* find our GameData directory!
		public static string gameData() {
			return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.Personal),
				".steam", "steam", "SteamApps", "common", "Kerbal Space Program", "GameData"
			);
		}

		public static void scanGameData() {

			// TODO: Get rid of magic paths!
			RegistryManager registry_manager = new RegistryManager("/tmp/ksp_registry");

			Registry registry = registry_manager.load_or_create ();

			// Forget that we've seen any DLLs, as we're going to refresh them all.
			registry.clear_dlls ();

			// TODO: It would be great to optimise this to skip .git directories and the like.
			// Yes, I keep my GameData in git.

			string[] dllFiles = Directory.GetFiles (gameData(), "*.dll", SearchOption.AllDirectories);

			foreach (string file in dllFiles) {
				// register_dll does the heavy lifting of turning it into a modname
				registry.register_dll (file);
			}

			registry_manager.save (registry);
		}
	}
}