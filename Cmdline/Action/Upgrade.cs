using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using CKAN.Versioning;
using CommandLine;
using log4net;

namespace CKAN.CmdLine.Action
{
    /// <summary>
    /// Class for managing the upgrading of mods.
    /// </summary>
    public class Upgrade : ICommand
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Upgrade));

        private readonly GameInstanceManager _manager;
        private readonly IUser _user;

        /// <summary>
        /// Initializes a new instance of the <see cref="CKAN.CmdLine.Action.Upgrade"/> class.
        /// </summary>
        /// <param name="manager">The manager to provide game instances.</param>
        /// <param name="user">The current <see cref="CKAN.IUser"/> to raise messages to the user.</param>
        public Upgrade(GameInstanceManager manager, IUser user)
        {
            _manager = manager;
            _user = user;
        }

        /// <summary>
        /// Run the 'upgrade' command.
        /// </summary>
        /// <inheritdoc cref="ICommand.RunCommand"/>
        public int RunCommand(CKAN.GameInstance inst, object args)
        {
            var opts = (UpgradeOptions)args;
            if (!opts.Mods.Any() && !opts.All)
            {
                _user.RaiseMessage("upgrade <mod> [<mod2> ...] - argument(s) missing, perhaps you forgot it?");
                _user.RaiseMessage("If you want to upgrade all mods, use:   ckan upgrade --all");
                if (AutoUpdate.CanUpdate)
                {
                    _user.RaiseMessage("To update CKAN itself, use:   ckan upgrade ckan");
                }

                return Exit.BadOpt;
            }

            if (opts.CkanFile != null)
            {
                opts.Mods.ToList().Add(MainClass.LoadCkanFromFile(inst, opts.CkanFile).identifier);
            }

            if (!opts.All && opts.Mods.ToList()[0] == "ckan" && AutoUpdate.CanUpdate)
            {
                _user.RaiseMessage("Getting the latest CKAN version...");
                AutoUpdate.Instance.FetchLatestReleaseInfo();

                var latestVersion = AutoUpdate.Instance.latestUpdate.Version;
                var currentVersion = new ModuleVersion(Meta.GetVersion(VersionFormat.Short));

                if (latestVersion.IsGreaterThan(currentVersion))
                {
                    var releaseNotes = AutoUpdate.Instance.latestUpdate.ReleaseNotes;

                    _user.RaiseMessage("There is a new CKAN version available: {0} ", latestVersion);
                    _user.RaiseMessage("{0}\r\n", releaseNotes);

                    if (_user.RaiseYesNoDialog("Proceed with install?"))
                    {
                        _user.RaiseMessage("Upgrading CKAN, please wait...");
                        AutoUpdate.Instance.StartUpdateProcess(false);
                    }
                }
                else
                {
                    _user.RaiseMessage("You already have the latest version of CKAN.");
                }

                return Exit.Ok;
            }

            try
            {
                var regMgr = RegistryManager.Instance(inst);
                var registry = regMgr.registry;
                if (opts.All)
                {
                    var toUpgrade = new List<CkanModule>();

                    foreach (var mod in registry.Installed(false))
                    {
                        try
                        {
                            // Check if upgrades are available
                            var latest = registry.LatestAvailable(mod.Key, inst.VersionCriteria());

                            // This may be an un-indexed mod. If so,
                            // skip rather than crash. See KSP-CKAN/CKAN#841
                            if (latest == null || latest.IsDLC)
                            {
                                continue;
                            }

                            if (latest.version.IsGreaterThan(mod.Value))
                            {
                                // Upgradable
                                Log.InfoFormat("Found a new version for \"{0}\".", latest.identifier);
                                toUpgrade.Add(latest);
                            }
                        }
                        catch (ModuleNotFoundKraken)
                        {
                            Log.InfoFormat("\"{0}\" is installed, but is no longer in the registry.", mod.Key);
                        }
                    }

                    UpgradeModules(_manager, _user, inst, true, toUpgrade);
                }
                else
                {
                    Search.AdjustModulesCase(inst, opts.Mods.ToList());
                    UpgradeModules(_manager, _user, inst, opts.Mods.ToList());
                }

                _user.RaiseMessage("");
            }
            catch (CancelledActionKraken kraken)
            {
                _user.RaiseMessage("Upgrade aborted: {0}.", kraken.Message);
                return Exit.Error;
            }
            catch (ModuleNotFoundKraken kraken)
            {
                _user.RaiseMessage("Could not find \"{0}\".", kraken.module);
                return Exit.Error;
            }
            catch (InconsistentKraken kraken)
            {
                _user.RaiseMessage(kraken.ToString());
                return Exit.Error;
            }
            catch (ModuleIsDLCKraken kraken)
            {
                _user.RaiseMessage("Can't upgrade the expansion \"{0}\".", kraken.module.name);
                var res = kraken?.module?.resources;
                var storePagesMsg = new[] { res?.store, res?.steamstore }
                    .Where(u => u != null)
                    .Aggregate("", (a, b) => $"{a}\r\n- {b}");

                if (!string.IsNullOrEmpty(storePagesMsg))
                {
                    _user.RaiseMessage("To upgrade this expansion, download any updates from the store page from which you purchased it:\r\n   {0}", storePagesMsg);
                }

                return Exit.Error;
            }

            _user.RaiseMessage("Successfully upgraded requested mods.");
            return Exit.Ok;
        }

        /// <summary>
        /// Upgrade some modules by their <see cref="CKAN.CkanModule"/>s.
        /// </summary>
        /// <param name="manager">The manager to provide game instances.</param>
        /// <param name="user">The current <see cref="CKAN.IUser"/> to raise messages to the user.</param>
        /// <param name="ksp">The game instance which to handle with mods.</param>
        /// <param name="confirmPrompt">Whether to confirm the prompt.</param>
        /// <param name="modules">List of modules to upgrade.</param>
        public static void UpgradeModules(GameInstanceManager manager, IUser user, CKAN.GameInstance ksp, bool confirmPrompt, List<CkanModule> modules)
        {
            UpgradeModules(manager, user, ksp,
                (ModuleInstaller installer, NetAsyncModulesDownloader downloader, RegistryManager regMgr, ref HashSet<string> possibleConfigOnlyDirs) =>
                    installer.Upgrade(modules, downloader,
                        ref possibleConfigOnlyDirs, regMgr, true, true, confirmPrompt),
                m => modules.Add(m)
            );
        }

        /// <summary>
        /// Upgrade some modules by their identifier and (optional) version.
        /// </summary>
        /// <param name="manager">The manager to provide game instances.</param>
        /// <param name="user">The current <see cref="CKAN.IUser"/> to raise messages to the user.</param>
        /// <param name="ksp">The game instance which to handle with mods.</param>
        /// <param name="identsAndVersions">List of identifier[=version] to upgrade.</param>
        public static void UpgradeModules(GameInstanceManager manager, IUser user, CKAN.GameInstance ksp, List<string> identsAndVersions)
        {
            UpgradeModules(manager, user, ksp,
                (ModuleInstaller installer, NetAsyncModulesDownloader downloader, RegistryManager regMgr, ref HashSet<string> possibleConfigOnlyDirs) =>
                    installer.Upgrade(identsAndVersions, downloader,
                        ref possibleConfigOnlyDirs, regMgr, true),
                m => identsAndVersions.Add(m.identifier)
            );
        }

        // System.Action<ref T> isn't allowed
        private delegate void AttemptUpgradeAction(ModuleInstaller installer, NetAsyncModulesDownloader downloader, RegistryManager regMgr, ref HashSet<string> possibleConfigOnlyDirs);

        /// <summary>
        /// The core of the module upgrading logic, with callbacks to support different input formats managed by the calling code.
        /// Handles transactions, creating commonly required objects, looping logic, prompting for <see cref="CKAN.TooManyModsProvideKraken"/> resolution.
        /// </summary>
        /// <param name="manager">The manager to provide game instances.</param>
        /// <param name="user">The current <see cref="CKAN.IUser"/> to raise messages to the user.</param>
        /// <param name="ksp">The game instance which to handle with mods.</param>
        /// <param name="attemptUpgradeCallback">Function to call to try to perform the actual upgrade, may throw <see cref="CKAN.TooManyModsProvideKraken"/>.</param>
        /// <param name="addUserChoiceCallback">Function to call when the user has requested a new module added to the change set in response to <see cref="CKAN.TooManyModsProvideKraken"/>.</param>
        private static void UpgradeModules(GameInstanceManager manager, IUser user, CKAN.GameInstance ksp, AttemptUpgradeAction attemptUpgradeCallback, System.Action<CkanModule> addUserChoiceCallback)
        {
            using (TransactionScope transact = CkanTransaction.CreateTransactionScope()) {
                var installer = new ModuleInstaller(ksp, manager.Cache, user);
                var downloader = new NetAsyncModulesDownloader(user, manager.Cache);
                var regMgr = RegistryManager.Instance(ksp);
                HashSet<string> possibleConfigOnlyDirs = null;
                bool done = false;

                while (!done)
                {
                    try
                    {
                        attemptUpgradeCallback?.Invoke(installer, downloader, regMgr, ref possibleConfigOnlyDirs);
                        transact.Complete();
                        done = true;
                    }
                    catch (TooManyModsProvideKraken k)
                    {
                        int choice = user.RaiseSelectionDialog(
                            $"Choose a module to provide {k.requested}:",
                            k.modules.Select(m => $"{m.identifier} ({m.name})").ToArray());

                        if (choice < 0)
                        {
                            return;
                        }
                        else
                        {
                            addUserChoiceCallback?.Invoke(k.modules[choice]);
                        }
                    }
                }
            }
        }

    }

    [Verb("upgrade", HelpText = "Upgrade an installed mod")]
    internal class UpgradeOptions : InstanceSpecificOptions
    {
        [Option('c', "ckanfile", HelpText = "Local CKAN file to process")]
        public string CkanFile { get; set; }

        [Option("no-recommends", HelpText = "Do not install recommended mods")]
        public bool NoRecommends { get; set; }

        [Option("with-suggests", HelpText = "Install suggested mods")]
        public bool WithSuggests { get; set; }

        [Option("with-all-suggests", HelpText = "Install suggested mods all the way down")]
        public bool WithAllSuggests { get; set; }

        [Option("all", HelpText = "Upgrade all available updated mods")]
        public bool All { get; set; }

        [Value(0, MetaName = "Mod name(s)", HelpText = "The mod name(s) to upgrade")]
        public IEnumerable<string> Mods { get; set; }
    }
}
