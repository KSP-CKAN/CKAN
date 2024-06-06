using System;
using System.IO;
using System.Transactions;
using System.Collections.Generic;
using System.Linq;

using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Screen for installing mods
    /// </summary>
    public class InstallScreen : ProgressScreen {

        /// <summary>
        /// Initialize the Screen
        /// </summary>
        /// <param name="mgr">Game instance manager containing instances</param>
        /// <param name="repoData">Repository data manager providing info from repos</param>
        /// <param name="cp">Plan of mods to install or remove</param>
        /// <param name="dbg">True if debug options should be available, false otherwise</param>
        public InstallScreen(GameInstanceManager mgr, RepositoryDataManager repoData, ChangePlan cp, bool dbg)
            : base(
                Properties.Resources.InstallTitle,
                Properties.Resources.InstallMessage
            )
        {
            debug   = dbg;
            manager = mgr;
            plan    = cp;
            this.repoData = repoData;
        }

        /// <summary>
        /// Run the screen
        /// </summary>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="process">Framework parameter not used by this object</param>
        public override void Run(ConsoleTheme theme, Action<ConsoleTheme> process = null)
        {
            HashSet<string> rejected = new HashSet<string>();
            DrawBackground(theme);
            using (TransactionScope trans = CkanTransaction.CreateTransactionScope()) {
                bool retry = false;
                do {
                    Draw(theme);
                    try {
                        // Reset this so we stop unless an exception sets it to true
                        retry = false;

                        var regMgr   = RegistryManager.Instance(manager.CurrentInstance, repoData);
                        var registry = regMgr.registry;

                        // GUI prompts user to choose recs/sugs,
                        // CmdLine assumes recs and ignores sugs
                        if (plan.Install.Count > 0) {
                            // Track previously rejected optional dependencies and don't prompt for them again.
                            DependencyScreen ds = new DependencyScreen(manager, registry, plan, rejected, debug);
                            if (ds.HaveOptions()) {
                                LaunchSubScreen(theme, ds);
                            }
                        }

                        // FUTURE: BackgroundWorker

                        HashSet<string> possibleConfigOnlyDirs = null;

                        ModuleInstaller inst = new ModuleInstaller(manager.CurrentInstance, manager.Cache, this);
                        inst.onReportModInstalled += OnModInstalled;
                        if (plan.Remove.Count > 0) {
                            inst.UninstallList(plan.Remove, ref possibleConfigOnlyDirs, regMgr, true, new List<CkanModule>(plan.Install));
                            plan.Remove.Clear();
                        }
                        NetAsyncModulesDownloader dl = new NetAsyncModulesDownloader(this, manager.Cache);
                        if (plan.Install.Count > 0) {
                            var iList = plan.Install
                                            .Select(m => Utilities.DefaultIfThrows(() =>
                                                             registry.LatestAvailable(m.identifier,
                                                                                      manager.CurrentInstance.VersionCriteria(),
                                                                                      null,
                                                                                      registry.InstalledModules
                                                                                              .Select(im => im.Module)
                                                                                              .ToArray(),
                                                                                      plan.Install))
                                                         ?? m)
                                            .ToArray();
                            inst.InstallList(iList, resolvOpts, regMgr, ref possibleConfigOnlyDirs, dl);
                            plan.Install.Clear();
                        }
                        if (plan.Upgrade.Count > 0) {
                            var upgGroups = registry
                                            .CheckUpgradeable(manager.CurrentInstance,
                                                              // Hold identifiers not chosen for upgrading
                                                              registry.Installed(false)
                                                                      .Keys
                                                                      .Except(plan.Upgrade)
                                                                      .ToHashSet());
                            inst.Upgrade(upgGroups[true], dl, ref possibleConfigOnlyDirs, regMgr);
                            plan.Upgrade.Clear();
                        }
                        if (plan.Replace.Count > 0) {
                            inst.Replace(AllReplacements(plan.Replace), resolvOpts, dl, ref possibleConfigOnlyDirs, regMgr, true);
                        }

                        trans.Complete();
                        inst.onReportModInstalled -= OnModInstalled;
                        // Don't let the installer re-use old screen references
                        inst.User = null;

                        HandlePossibleConfigOnlyDirs(theme, registry, possibleConfigOnlyDirs);

                    } catch (CancelledActionKraken) {
                        // Don't need to tell the user they just cancelled out.
                    } catch (FileNotFoundKraken ex) {
                        // Possible file corruption
                        RaiseError(ex.Message);
                    } catch (DirectoryNotFoundKraken ex) {
                        RaiseError(ex.Message);
                    } catch (FileExistsKraken ex) {
                        if (ex.owningModule != null) {
                            RaiseMessage(Properties.Resources.InstallOwnedFileConflict, ex.installingModule, ex.filename, ex.owningModule);
                        } else {
                            RaiseMessage(Properties.Resources.InstallUnownedFileConflict, ex.installingModule, ex.filename, ex.installingModule);
                        }
                        RaiseError(Properties.Resources.InstallFilesReverted);
                    } catch (DownloadErrorsKraken ex) {
                        RaiseError(ex.ToString());
                    } catch (ModuleDownloadErrorsKraken ex) {
                        RaiseError(ex.ToString());
                    } catch (DownloadThrottledKraken ex) {
                        if (RaiseYesNoDialog(string.Format(Properties.Resources.InstallAuthTokenPrompt, ex.ToString()))) {
                            if (ex.infoUrl != null) {
                                ModInfoScreen.LaunchURL(theme, ex.infoUrl);
                            }
                            LaunchSubScreen(theme, new AuthTokenScreen());
                        }
                    } catch (MissingCertificateKraken ex) {
                        RaiseError(ex.ToString());
                    } catch (InconsistentKraken ex) {
                        RaiseError(ex.Message);
                    } catch (TooManyModsProvideKraken ex) {

                        ConsoleChoiceDialog<CkanModule> ch = new ConsoleChoiceDialog<CkanModule>(
                            ex.Message,
                            Properties.Resources.InstallTooManyModsNameHeader,
                            ex.modules,
                            (CkanModule mod) => mod.ToString()
                        );
                        CkanModule chosen = ch.Run(theme);
                        DrawBackground(theme);
                        if (chosen != null) {
                            // Use chosen to continue installing
                            plan.Install.Add(chosen);
                            retry = true;
                        }

                    } catch (BadMetadataKraken ex) {
                        RaiseError(Properties.Resources.InstallBadMetadata, ex.module, ex.Message);
                    } catch (DependencyNotSatisfiedKraken ex) {
                        RaiseError(Properties.Resources.InstallUnsatisfiedDependency, ex.parent, ex.module, ex.Message);
                    } catch (ModuleNotFoundKraken ex) {
                        RaiseError(Properties.Resources.InstallModuleNotFound, ex.module, ex.Message);
                    } catch (ModNotInstalledKraken ex) {
                        RaiseError(Properties.Resources.InstallNotInstalled, ex.mod);
                    } catch (DllLocationMismatchKraken ex) {
                        RaiseError(ex.Message);
                    }
                } while (retry);
            }
        }

        private void HandlePossibleConfigOnlyDirs(ConsoleTheme    theme,
                                                  Registry        registry,
                                                  HashSet<string> possibleConfigOnlyDirs)
        {
            if (possibleConfigOnlyDirs != null)
            {
                // Check again for registered files, since we may
                // just have installed or upgraded some
                possibleConfigOnlyDirs.RemoveWhere(
                    d => !Directory.Exists(d)
                         || Directory.EnumerateFileSystemEntries(d, "*", SearchOption.AllDirectories)
                                     .Select(absF => manager.CurrentInstance.ToRelativeGameDir(absF))
                                     .Any(relF => registry.FileOwner(relF) != null));
                if (possibleConfigOnlyDirs.Count > 0)
                {
                    LaunchSubScreen(theme, new DeleteDirectoriesScreen(manager.CurrentInstance, possibleConfigOnlyDirs));
                }
            }
        }

        private void OnModInstalled(CkanModule mod)
        {
            RaiseMessage(Properties.Resources.InstallModInstalled, Symbols.checkmark, mod.name, ModuleInstaller.StripEpoch(mod.version));
        }

        private IEnumerable<ModuleReplacement> AllReplacements(IEnumerable<string> identifiers)
        {
            IRegistryQuerier registry = RegistryManager.Instance(manager.CurrentInstance, repoData).registry;

            foreach (string id in identifiers) {
                ModuleReplacement repl = registry.GetReplacement(
                    id, manager.CurrentInstance.VersionCriteria()
                );
                if (repl != null) {
                    yield return repl;
                }
            }
        }

        private static readonly RelationshipResolverOptions resolvOpts = new RelationshipResolverOptions() {
            with_all_suggests              = false,
            with_suggests                  = false,
            with_recommends                = false,
            without_toomanyprovides_kraken = false,
            without_enforce_consistency    = false
        };

        private readonly GameInstanceManager   manager;
        private readonly RepositoryDataManager repoData;
        private readonly ChangePlan            plan;
        private readonly bool                  debug;
    }

}
