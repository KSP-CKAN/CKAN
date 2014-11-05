using System;
using System.Collections.Generic;

namespace CKAN
{
    public class InstalledModuleFile
    {
        public string sha1_sum;
    }

    /// <summary>
    /// A simple clss that represents an installed module. Includes the time of installation,
    /// the module itself, and a list of files installed with it.
    /// 
    /// Primarily used by the Registry class.
    /// </summary>
    public class InstalledModule
    {
        public DateTime install_time;
        public Dictionary<string, InstalledModuleFile> installed_files; // file => metadata
        public Module source_module;

        public InstalledModule(Dictionary<string, InstalledModuleFile> installed_files, Module source_module,
            DateTime install_time)
        {
            this.installed_files = installed_files;
            this.source_module = source_module;
            this.install_time = install_time;
        }
    }
}