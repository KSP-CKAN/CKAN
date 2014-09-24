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
	}
}