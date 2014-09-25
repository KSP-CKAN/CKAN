using System;
using System.Collections.Generic;

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

		public Registry (int version, Dictionary<string, InstalledModule> mods)
		{
			/* TODO: support more than just the latest version */
			if (version != LATEST_REGISTRY_VERSION) {
				throw new RegistryVersionNotSupportedException (version);
			}

			installed_modules = mods;
		}

		public static Registry empty ()
		{
			return new Registry (LATEST_REGISTRY_VERSION, new Dictionary<string, InstalledModule> ());
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
	}
}