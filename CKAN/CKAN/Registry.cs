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
		public InstalledModule[] installed_modules;

		public Registry (int version, InstalledModule[] mods)
		{
			/* TODO: support more than just the latest version */
			if (version != LATEST_REGISTRY_VERSION) {
				throw new RegistryVersionNotSupportedException (version);
			}

			installed_modules = mods;
		}

		public static Registry empty ()
		{
			return new Registry (LATEST_REGISTRY_VERSION, new InstalledModule[] {});
		}

		public Registry append (InstalledModule mod)
		{
			/* UGH! I wish we could easily use 4.5's immutable collections */
			InstalledModule[] new_modules = new InstalledModule[installed_modules.Length + 1];
			installed_modules.CopyTo (new_modules, 0);
			new_modules.SetValue (mod, installed_modules.Length);
			return new Registry (registry_version, new_modules);
		}
	}
}