using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CKAN
{
	class RegistryVersionNotSupportedException : Exception
	{
		public int requested_version;

		public RegistryVersionNotSupportedException (int v)
		{
			requested_version = v;
		}
	}

	public class Registry
	{
		const int LATEST_REGISTRY_VERSION = 0;
		public int registry_version;
		public Dictionary<string, InstalledModule> installed_modules;
		public Dictionary<string, string> installed_dlls;

		public Registry (int version, Dictionary<string, InstalledModule> mods, Dictionary<string, string> dlls)
		{
			/* TODO: support more than just the latest version */
			if (version != LATEST_REGISTRY_VERSION) {
				throw new RegistryVersionNotSupportedException (version);
			}

			installed_modules = mods;
			installed_dlls    = dlls;
		}

		public static Registry empty ()
		{
			return new Registry (LATEST_REGISTRY_VERSION, new Dictionary<string, InstalledModule> (), new Dictionary<string,string> () );
		}

		/// <summary>
		/// Register the supplied module as having been installed, thereby keeping
		/// track of its metadata and files.
		/// </summary>
		/// <param name="mod">Mod.</param>
		public void register_module (InstalledModule mod)
		{
			installed_modules.Add (mod.source_module.identifier, mod);
		}

		public void register_dll (string path)
		{
			// Oh my, does .NET support extended regexps (like Perl?), we could use them right now.
			Match match = Regex.Match (path, @".*?(?:^|/)GameData/((?:.*/|)([^.]+).*dll)");

			string relPath = match.Groups[1].Value;
			string modName = match.Groups[2].Value;

			Console.WriteLine ("Registering {0} -> {1}", modName, relPath);

			// We're fine if we overwrite an existing key.
			installed_dlls[modName] = relPath;
		}

		public void clear_dlls() {
			installed_dlls = new Dictionary<string,string> ();
		}

		/// <summary>
		/// Returns the installed version of a given mod.
		/// If the mod was autodetected (but present), a "0" is returned.
		/// If the mod is not found, a null will be returned.
		/// </summary>
		/// <returns>The version.</returns>
		/// <param name="modName">Mod name.</param>

		public string installedVersion(string modName) {
			if (installed_modules.ContainsKey(modName)) {
				return installed_modules [modName].source_module.version;
			}
			else if (installed_dlls.ContainsKey(modName)) {
				return "0";	// We probably want a better way to signal auto-detected modules.
			}

			return null;
		}

		/// <summary>
		/// Check if a mod is installed (either via CKAN, or a DLL detected)
		/// </summary>
		/// <returns><c>true</c>, if installed<c>false</c> otherwise.</returns>
		public bool isInstalled(string modName) {
			if (installedVersion (modName) == null) {
				return false;
			}
			return true;
		}
	}
}