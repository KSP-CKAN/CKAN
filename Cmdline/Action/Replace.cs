using System.Collections.Generic;
using System.Linq;

using CommandLine;
using log4net;

using CKAN.Versioning;

namespace CKAN.CmdLine
{
    public class Replace : ICommand
    {
        public Replace(GameInstanceManager mgr, RepositoryDataManager repoData, IUser user)
        {
            manager       = mgr;
            this.repoData = repoData;
            this.user     = user;
        }

        public int RunCommand(CKAN.GameInstance instance, object raw_options)
        {
            ReplaceOptions options = (ReplaceOptions) raw_options;

            if (options.ckan_file != null)
            {
                options.modules.Add(MainClass.LoadCkanFromFile(options.ckan_file).identifier);
            }

            if (options.modules.Count == 0 && ! options.replace_all)
            {
                user.RaiseError(Properties.Resources.ArgumentMissing);
                foreach (var h in Actions.GetHelp("replace"))
                {
                    user.RaiseError(h);
                }
                return Exit.BADOPT;
            }

            // Prepare options. Can these all be done in the new() somehow?
            var replace_ops = new RelationshipResolverOptions
                {
                    with_all_suggests  = options.with_all_suggests,
                    with_suggests      = options.with_suggests,
                    with_recommends    = !options.no_recommends,
                    allow_incompatible = options.allow_incompatible
                };

            var regMgr = RegistryManager.Instance(instance, repoData);
            var registry = regMgr.registry;
            var to_replace = new List<ModuleReplacement>();

            if (options.replace_all)
            {
                log.Debug("Running Replace all");
                var installed = new Dictionary<string, ModuleVersion>(registry.Installed());

                foreach (KeyValuePair<string, ModuleVersion> mod in installed)
                {
                    ModuleVersion current_version = mod.Value;

                    if ((current_version is ProvidesModuleVersion) || (current_version is UnmanagedModuleVersion))
                    {
                        continue;
                    }
                    else
                    {
                        try
                        {
                            log.DebugFormat("Testing {0} {1} for possible replacement", mod.Key, mod.Value);
                            // Check if replacement is available

                            ModuleReplacement replacement = registry.GetReplacement(mod.Key, instance.VersionCriteria());
                            if (replacement != null)
                            {
                                // Replaceable
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
                        log.DebugFormat("Checking that {0} is installed", mod);
                        CkanModule modToReplace = registry.GetInstalledVersion(mod);
                        if (modToReplace != null)
                        {
                            log.DebugFormat("Testing {0} {1} for possible replacement", modToReplace.identifier, modToReplace.version);
                            try
                            {
                                // Check if replacement is available
                                ModuleReplacement replacement = registry.GetReplacement(modToReplace.identifier, instance.VersionCriteria());
                                if (replacement != null)
                                {
                                    // Replaceable
                                    log.InfoFormat("Replacement {0} {1} found for {2} {3}",
                                        replacement.ReplaceWith.identifier, replacement.ReplaceWith.version,
                                        replacement.ToReplace.identifier, replacement.ToReplace.version);
                                    to_replace.Add(replacement);
                                }
                                if (modToReplace.replaced_by != null)
                                {
                                    log.InfoFormat("Attempt to replace {0} failed, replacement {1} is not compatible",
                                        mod, modToReplace.replaced_by.name);
                                }
                                else
                                {
                                    log.InfoFormat("Mod {0} has no replacement defined for the current version {1}",
                                        modToReplace.identifier, modToReplace.version);
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
                        user.RaiseMessage(Properties.Resources.ReplaceModuleNotFound, kraken.module);
                    }
                }
            }
            if (to_replace.Count() != 0)
            {
                user.RaiseMessage("");
                user.RaiseMessage(Properties.Resources.Replacing);
                user.RaiseMessage("");
                foreach (ModuleReplacement r in to_replace)
                {
                    user.RaiseMessage(Properties.Resources.ReplaceFound,
                        r.ReplaceWith.identifier, r.ReplaceWith.version,
                        r.ToReplace.identifier, r.ToReplace.version);
                }

                bool ok = user.RaiseYesNoDialog(Properties.Resources.ReplaceContinuePrompt);

                if (!ok)
                {
                    user.RaiseMessage(Properties.Resources.ReplaceCancelled);
                    return Exit.ERROR;
                }

                // TODO: These instances all need to go.
                try
                {
                    HashSet<string> possibleConfigOnlyDirs = null;
                    new ModuleInstaller(instance, manager.Cache, user).Replace(to_replace, replace_ops, new NetAsyncModulesDownloader(user, manager.Cache), ref possibleConfigOnlyDirs, regMgr);
                    user.RaiseMessage("");
                }
                catch (DependencyNotSatisfiedKraken ex)
                {
                    user.RaiseMessage(Properties.Resources.ReplaceDependencyNotSatisfied,
                        ex.parent, ex.module, ex.version, instance.game.ShortName);
                }
            }
            else
            {
                user.RaiseMessage(Properties.Resources.ReplaceNotFound);
                return Exit.OK;
            }

            return Exit.OK;
        }

        private readonly GameInstanceManager   manager;
        private readonly RepositoryDataManager repoData;
        private readonly IUser                 user;

        private static readonly ILog log = LogManager.GetLogger(typeof(Replace));
    }

    internal class ReplaceOptions : InstanceSpecificOptions
    {
        [Option('c', "ckanfile", HelpText = "Local CKAN file to process")]
        public string ckan_file { get; set; }

        [Option("no-recommends", HelpText = "Do not install recommended modules")]
        public bool no_recommends { get; set; }

        [Option("with-suggests", HelpText = "Install suggested modules")]
        public bool with_suggests { get; set; }

        [Option("with-all-suggests", HelpText = "Install suggested modules all the way down")]
        public bool with_all_suggests { get; set; }

        [Option("allow-incompatible", DefaultValue = false, HelpText = "Install modules that are not compatible with the current game version")]
        public bool allow_incompatible { get; set; }

        [Option("all", HelpText = "Replace all available replaced modules")]
        public bool replace_all { get; set; }

        // TODO: How do we provide helptext on this?
        [ValueList(typeof (List<string>))]
        [InstalledIdentifiers]
        public List<string> modules { get; set; }
    }

}
