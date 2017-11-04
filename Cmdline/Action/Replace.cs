using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;

namespace CKAN.CmdLine
{
    public class Replace : ICommand
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Replace));

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
            var to_replace = new List<ModuleReplacement>();

            if (options.replace_all)
            {
                var installed = new Dictionary<string, Version>(registry.Installed());

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
                                ModuleReplacement replacement = new ModuleReplacement();
                                replacement.ToReplace = registry.GetModuleByVersion(mod.Key, mod.Value);
                                replacement.ReplaceWith = registry.LatestAvailable(replacement.ToReplace.replaced_by.name, ksp.VersionCriteria());
                                log.InfoFormat("Replacement {0} {1} found for {2} {3}",
                                    replacement.ReplaceWith.identifier, replacement.ReplaceWith.version,
                                    replacement.ToReplace.identifier, replacement.ToReplace.version);
                                to_replace.Add(replacement);
                            }

                        }
                        catch (ModuleNotFoundKraken)
                        {
                            log.InfoFormat("{0} is installed, but it or its replacement is not in the registry",
                                mod.Key);
                        }
                    }
                }
            }
            else
            {
                foreach (string mod in options.modules)
                {
                    try
                    {
                        CkanModule modToReplace = registry.GetInstalledVersion(mod);
                        if ( modToReplace != null) 
                        {
                            try
                            {
                                // Check if replacement is available
                                if (registry.HasReplacement(mod, ksp.VersionCriteria()))
                                {
                                    // Replaceable
                                    CkanModule ReplaceWith = registry.LatestAvailable(modToReplace.replaced_by.name, ksp.VersionCriteria());
                                    ModuleReplacement replacement = new ModuleReplacement();
                                    replacement.ToReplace = modToReplace;
                                    replacement.ReplaceWith = ReplaceWith;
                                    log.InfoFormat("Replacement {0} {1} found for {2} {3}",
                                        replacement.ReplaceWith.identifier, replacement.ReplaceWith.version,
                                        replacement.ToReplace.identifier, replacement.ToReplace.version);
                                    to_replace.Add(replacement);
                                }
                            }
                            catch (ModuleNotFoundKraken)
                            {
                                log.InfoFormat("{0} is installed, but its replacement {1} is not in the registry",
                                    mod, modToReplace.replaced_by.name);
                            }
                        }
                    }
                    catch (ModuleNotFoundKraken kraken)
                    {
                        User.RaiseMessage("Module {0} not found", kraken.module);
                    }                                           
                }
            }
            // TODO: These instances all need to go.
            ModuleInstaller.GetInstance(ksp, User).Replace(to_replace, new NetAsyncModulesDownloader(User));
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

