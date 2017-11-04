using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;

namespace CKAN.CmdLine
{
    public class Replace : ICommand
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Upgrade));

        public IUser User { get; set; }

        public Replace(IUser user)
        {
            User = user;
        }

        public int RunCommand(CKAN.KSP ksp, object raw_options)
        {
            ReplaceOptions options = (ReplaceOptions) raw_options;

            if (options.ckan_file != null)
            {                
                options.modules.Add(LoadCkanFromFile(ksp, options.ckan_file).identifier);
            }

            if (options.modules.Count == 0 && ! options.replace_all)
            {
                // What? No mods specified?
                User.RaiseMessage("Usage: ckan replace Mod [Mod2, ...]");
                User.RaiseMessage("  or   ckan replace --all");
                return Exit.BADOPT;
            }

            User.RaiseMessage("\r\nReplacing modules...\r\n");
            var registry = RegistryManager.Instance(ksp).registry;

            try
            {
                if (options.replace_all)
                {
                    
                    var installed = new Dictionary<string, Version>(registry.Installed());
                    var to_replace = new List<ModuleReplacement>();

                    foreach (KeyValuePair<string, Version> mod in installed)
                    {
                        Version current_version = mod.Value;

                        if ((current_version is ProvidesVersion) || (current_version is DllVersion))
                        {
                            continue;
                        }
                        else
                        {
                            try
                            {
                                // Check if replacement is available
                                if (registry.HasReplacement(mod.Key, ksp.VersionCriteria()))
                                {
                                    // Replaceable
                                    CkanModule toReplace = registry.GetModuleByVersion(mod.Key, mod.Value);
                                    CkanModule replacement = registry.LatestAvailable(toReplace.replaced_by.name, ksp.VersionCriteria());
                                    log.InfoFormat("Replacement {0} {1} found for {2} {3}",
                                        replacement.identifier, replacement.version,
                                        toReplace.identifier, toReplace.version);
                                    to_replace.Add(toReplace);
                                }

                            }
                            catch (ModuleNotFoundKraken)
                            {
                                log.InfoFormat("{0} is installed, but it or its replacement is not in the registry",
                                    mod.Key);
                            }
                        }
                    }
                    ModuleInstaller.GetInstance(ksp, User).Replace(to_replace, new NetAsyncModulesDownloader(User));
                }
                else
                {
                    var to_replace = new List<ModuleReplacement>();
                    foreach (string mod in options.modules)
                    {
                        try
                        {
                            CkanModule toReplace = registry.GetInstalledVersion(mod);
                            // Check if replacement is available
                            if (registry.HasReplacement(mod, ksp.VersionCriteria()))
                            {
                                // Replaceable
                                CkanModule replacement = registry.LatestAvailable(toReplace.replaced_by.name, ksp.VersionCriteria());
                                log.InfoFormat("Replacement {0} {1} found for {2} {3}",
                                    replacement.identifier, replacement.version,
                                    toReplace.identifier, toReplace.version);
                                to_replace.Add(toReplace);
                            }

                        }
                        catch (ModuleNotFoundKraken)
                        {
                            log.InfoFormat("{0} is installed, but it or its replacement is not in the registry",
                                mod);
                        }
                    }
                    // TODO: These instances all need to go.
                    ModuleInstaller.GetInstance(ksp, User).Replace(to_replace, new NetAsyncModulesDownloader(User));
                }
            }
            catch (ModuleNotFoundKraken kraken)
            {
                User.RaiseMessage("Module {0} not found", kraken.module);
                return Exit.ERROR;
            }
            User.RaiseMessage("\r\nDone!\r\n");

            return Exit.OK;
        }

        internal static CkanModule LoadCkanFromFile(CKAN.KSP current_instance, string ckan_file)
        {
            CkanModule module = CkanModule.FromFile(ckan_file);

            // We'll need to make some registry changes to do this.
            RegistryManager registry_manager = RegistryManager.Instance(current_instance);

            // Remove this version of the module in the registry, if it exists.
            registry_manager.registry.RemoveAvailable(module);

            // Sneakily add our version in...
            registry_manager.registry.AddAvailable(module);

            return module;
        }
    }
}

