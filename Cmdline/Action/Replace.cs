using System.Collections.Generic;
using System.Linq;
using CKAN.Versioning;
using CommandLine;
using log4net;

namespace CKAN.CmdLine.Action
{
    /// <summary>
    /// Class for managing the replacing of mods.
    /// </summary>
    public class Replace : ICommand
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Replace));

        private readonly GameInstanceManager _manager;
        private readonly IUser _user;

        /// <summary>
        /// Initializes a new instance of the <see cref="CKAN.CmdLine.Action.Replace"/> class.
        /// </summary>
        /// <param name="manager">The manager to provide game instances.</param>
        /// <param name="user">The current <see cref="CKAN.IUser"/> to raise messages to the user.</param>
        public Replace(GameInstanceManager manager, IUser user)
        {
            _manager = manager;
            _user = user;
        }

        /// <summary>
        /// Run the 'replace' command.
        /// </summary>
        /// <inheritdoc cref="ICommand.RunCommand"/>
        public int RunCommand(CKAN.GameInstance inst, object args)
        {
            var opts = (ReplaceOptions)args;
            if (!opts.Mods.Any() && !opts.All)
            {
                _user.RaiseMessage("replace <mod> [<mod2> ...] - argument(s) missing, perhaps you forgot it?");
                _user.RaiseMessage("If you want to replace all mods, use:   ckan replace --all");
                return Exit.BadOpt;
            }

            if (opts.CkanFile != null)
            {
                opts.Mods.ToList().Add(MainClass.LoadCkanFromFile(inst, opts.CkanFile).identifier);
            }

            var regMgr = RegistryManager.Instance(inst);
            var registry = regMgr.registry;
            var toReplace = new List<ModuleReplacement>();

            if (opts.All)
            {
                Log.Debug("Running replace all...");
                var installed = new Dictionary<string, ModuleVersion>(registry.Installed());

                foreach (var mod in installed)
                {
                    var currentVersion = mod.Value;
                    if (currentVersion is ProvidesModuleVersion || currentVersion is UnmanagedModuleVersion)
                    {
                        continue;
                    }

                    try
                    {
                        Log.DebugFormat("Testing \"{0}\" {1} for possible replacement...", mod.Key, mod.Value);

                        // Check if replacement is available
                        var replacement = registry.GetReplacement(mod.Key, inst.VersionCriteria());
                        if (replacement != null)
                        {
                            // Replaceable
                            Log.InfoFormat("Replacement \"{0}\" {1} found for \"{2}\" {3}.",
                                replacement.ReplaceWith.identifier, replacement.ReplaceWith.version,
                                replacement.ToReplace.identifier, replacement.ToReplace.version);
                            toReplace.Add(replacement);
                        }
                    }
                    catch (ModuleNotFoundKraken)
                    {
                        Log.InfoFormat("\"{0}\" is installed, but it, or its replacement, is not in the registry.", mod.Key);
                    }
                }
            }
            else
            {
                foreach (var mod in opts.Mods)
                {
                    try
                    {
                        Log.DebugFormat("Checking that \"{0}\" is installed...", mod);
                        var modToReplace = registry.GetInstalledVersion(mod);
                        if (modToReplace != null)
                        {
                            Log.DebugFormat("Testing \"{0}\" {1} for possible replacement...", modToReplace.identifier, modToReplace.version);
                            try
                            {
                                // Check if replacement is available
                                var replacement = registry.GetReplacement(modToReplace.identifier, inst.VersionCriteria());
                                if (replacement != null)
                                {
                                    // Replaceable
                                    Log.InfoFormat("Replacement \"{0}\" {1} found for \"{2}\" {3}.",
                                        replacement.ReplaceWith.identifier, replacement.ReplaceWith.version,
                                        replacement.ToReplace.identifier, replacement.ToReplace.version);
                                    toReplace.Add(replacement);
                                }

                                if (modToReplace.replaced_by != null)
                                {
                                    Log.InfoFormat("The attempt to replace \"{0}\" failed, the replacement \"{1}\" is not compatible with your version of {2}.", mod, modToReplace.replaced_by.name, inst.game.ShortName);
                                }
                                else
                                {
                                    Log.InfoFormat("The mod \"{0}\" has no replacement defined for the current version {1}.", modToReplace.identifier, modToReplace.version);
                                }
                            }
                            catch (ModuleNotFoundKraken)
                            {
                                Log.InfoFormat("\"{0}\" is installed, but its replacement \"{1}\" is not in the registry.", mod, modToReplace.replaced_by.name);
                            }
                        }
                    }
                    catch (ModuleNotFoundKraken kraken)
                    {
                        _user.RaiseMessage("The mod \"{0}\" could not found.", kraken.module);
                    }
                }
            }

            if (toReplace.Count != 0)
            {
                _user.RaiseMessage("\r\nReplacing modules...\r\n");
                foreach (var r in toReplace)
                {
                    _user.RaiseMessage("Replacement \"{0}\" {1} found for \"{2}\" {3}.",
                        r.ReplaceWith.identifier, r.ReplaceWith.version,
                        r.ToReplace.identifier, r.ToReplace.version);
                }

                var ok = _user.RaiseYesNoDialog("Continue?");

                if (!ok)
                {
                    _user.RaiseMessage("Replacements canceled at user request.");
                    return Exit.Error;
                }

                var replaceOpts = new RelationshipResolverOptions
                {
                    with_all_suggests = opts.WithAllSuggests,
                    with_suggests = opts.WithSuggests,
                    with_recommends = !opts.NoRecommends,
                    allow_incompatible = opts.AllowIncompatible
                };

                // TODO: These instances all need to go
                try
                {
                    HashSet<string> possibleConfigOnlyDirs = null;
                    new ModuleInstaller(inst, _manager.Cache, _user).Replace(toReplace, replaceOpts, new NetAsyncModulesDownloader(_user, _manager.Cache), ref possibleConfigOnlyDirs, regMgr);
                    _user.RaiseMessage("");
                }
                catch (DependencyNotSatisfiedKraken kraken)
                {
                    _user.RaiseMessage("Dependencies not satisfied for replacement, \"{0}\" requires \"{1}\" {2} but it is not listed in the index, or not available for your version of {3}.", kraken.parent, kraken.module, kraken.version, inst.game.ShortName);
                }
            }
            else
            {
                _user.RaiseMessage("No replacements found.");
                return Exit.Ok;
            }

            return Exit.Ok;
        }
    }

    [Verb("replace", HelpText = "Replace list of replaceable mods")]
    internal class ReplaceOptions : InstanceSpecificOptions
    {
        [Option('c', "ckanfile", HelpText = "Local CKAN file to process")]
        public string CkanFile { get; set; }

        [Option("no-recommends", HelpText = "Do not install recommended mods")]
        public bool NoRecommends { get; set; }

        [Option("with-suggests", HelpText = "Install suggested mods")]
        public bool WithSuggests { get; set; }

        [Option("with-all-suggests", HelpText = "Install suggested mods all the way down")]
        public bool WithAllSuggests { get; set; }

        [Option("allow-incompatible", HelpText = "Install mods that are not compatible with the current game version")]
        public bool AllowIncompatible { get; set; }

        [Option("all", HelpText = "Replace all available replaced mods")]
        public bool All { get; set; }

        [Value(0, MetaName = "Mod name(s)", HelpText = "The mod name(s) to replace")]
        public IEnumerable<string> Mods { get; set; }
    }
}
