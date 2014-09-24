using System;
using System.Collections.Generic;

namespace CKAN
{
	public class InstalledModuleFile
	{
		public string sha1_sum;
	}

	public class InstalledModule
	{
		public Dictionary<string, InstalledModuleFile> installed_files;
		public Module source_module;
		public DateTime install_time;

		public InstalledModule (Dictionary <string, InstalledModuleFile> installed_files, Module source_module, DateTime install_time)
		{
			this.installed_files = installed_files;
			this.source_module = source_module;
			this.install_time = install_time;
		}
	}
}