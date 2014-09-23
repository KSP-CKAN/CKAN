using System;
using System.IO;
using System.Collections.Generic;

// Find all the modules we have installed.
// This may be from a local cache, or by crawling GameData itself.

namespace CKAN {
	public class ModuleDict : Dictionary<String, Module> {
		public ModuleDict () {

			Console.WriteLine ("In ModuleDict");

			string gameData = CKAN.Module.gameData ();

			// Search all directories, find .ckan files.

			// TODO: It would be great to optimise this to skip .git directories and the like.
			// Yes, I keep my GameData in git.

			string[] files = Directory.GetFiles(gameData, "*.ckan", SearchOption.AllDirectories);

			foreach (string file in files) {
				Console.WriteLine (file);
			}
		}
	}
}

