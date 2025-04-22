using System;
using System.IO;
using System.Transactions;
using System.Collections.Generic;
using System.Linq;

using CKAN.IO;
using CKAN.Configuration;
using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Screen for installing mods
    /// </summary>
    public class InstallScreen : ProgressScreen {

        /// <summary>
        /// Initialize the Screen
        /// </summary>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="mgr">Game instance manager containing instances</param>
        /// <param name="repoData">Repository data manager providing info from repos</param>
        /// <param name="userAgent">HTTP useragent string to use</param>
        /// <param name="cp">Plan of mods to install or remove</param>
        /// <param name="dbg">True if debug options should be available, false otherwise</param>
        public InstallScreen(ConsoleTheme          theme,
                             GameInstanceManager   mgr,
                             RepositoryDataManager repoData,
                             string?               userAgent,
                             ChangePlan            cp,
                             bool                  dbg)
            : base(
                theme,
                Properties.Resources.InstallTitle,
                Properties.Resources.InstallMessage)
        {
            debug   = dbg;
            manager = mgr;
            plan    = cp;
            this.repoData = repoData;
            this.userAgent = userAgent;
        }

        /// <summary>
        /// Run the screen
        /// </summary>
        /// <param name="process">Framework parameter not used by this object</param>
        public override void Run(Action? process = null)
        {
            HashSet<string> rejected = new HashSet<string>();
            DrawBackground();
            if (manager.CurrentInstance != null && manager.Cache != null)
            {
                using (TransactionScope trans = CkanTransaction.CreateTransactionScope()) {
                    bool retry = false;
                    do {
                        Draw();
                        try {
                            // Reset this so we stop unless an exception sets it to true
                            retry = false;

                            var regMgr   = RegistryManager.Instance(manager.CurrentInstance, repoData);
                            var registry = regMgr.registry;
                            var stabilityTolerance = manager.CurrentInstance.StabilityToleranceConfig;

                            // GUI prompts user to choose recs/sugs,
                            // CmdLine assumes recs and ignores sugs
                            if (plan.Install.Count > 0) {
                                // Track previously rejected optional dependencies and don't prompt for them again.
                                DependencyScreen ds = new DependencyScreen(theme, manager, manager.CurrentInstance, registry, userAgent, plan, rejected, debug);
                                if (ds.HaveOptions()) {
                                    LaunchSubScreen(ds);
                                }
                            }

                            // FUTURE: BackgroundWorker

                            HashSet<string>? possibleConfigOnlyDirs = null;

                            var deduper = new InstalledFilesDeduplicator(manager.CurrentInstance,
                                                                         manager.Instances.Values,
                                                                         repoData);

                            ModuleInstaller inst = new ModuleInstaller(manager.CurrentInstance, manager.Cache, this, userAgent);
                            inst.OneComplete += OnModInstalled;
                            if (plan.Remove.Count > 0) {
                                inst.UninstallList(plan.Remove, ref possibleConfigOnlyDirs, regMgr, true, new List<CkanModule>(plan.Install));
                                plan.Remove.Clear();
                            }
                            NetAsyncModulesDownloader dl = new NetAsyncModulesDownloader(this, manager.Cache, userAgent);
                            if (plan.Install.Count > 0) {
                                var iList = plan.Install
                                                .Select(m => Utilities.DefaultIfThrows(() =>
                                                                 registry.LatestAvailable(m.identifier, stabilityTolerance,
                                                                                          manager.CurrentInstance.VersionCriteria(),
                                                                                          null,
                                                                                          registry.InstalledModules
                                                                                                  .Select(im => im.Module)
                                                                                                  .ToArray(),
                                                                                          plan.Install))
                                                             ?? m)
                                                .ToArray();
                                inst.InstallList(iList, resolvOpts(stabilityTolerance), regMgr,
                                                 ref possibleConfigOnlyDirs, deduper, userAgent, dl);
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
                                inst.Upgrade(upgGroups[true], dl, ref possibleConfigOnlyDirs, regMgr, deduper);
                                plan.Upgrade.Clear();
                            }
                            if (plan.Replace.Count > 0) {
                                inst.Replace(AllReplacements(plan.Replace), resolvOpts(stabilityTolerance), dl,
                                             ref possibleConfigOnlyDirs, regMgr, deduper, true);
                            }

                            trans.Complete();
                            inst.OneComplete -= OnModInstalled;
                            // Don't let the installer re-use old screen references
                            inst.User = new NullUser();

                            HandlePossibleConfigOnlyDirs(theme, registry, possibleConfigOnlyDirs);

                        } catch (CancelledActionKraken) {
                            // Don't need to tell the user they just cancelled out.
                        } catch (FileNotFoundKraken ex) {
                            // Possible file corruption
                            RaiseError("{0}", ex.Message);
                        } catch (DirectoryNotFoundKraken ex) {
                            RaiseError("{0}", ex.Message);
                        } catch (FileExistsKraken ex) {
                            if (ex.owningModule != null) {
                                RaiseMessage(Properties.Resources.InstallOwnedFileConflict, ex.installingModule?.ToString() ?? "", ex.filename, ex.owningModule);
                            } else {
                                RaiseMessage(Properties.Resources.InstallUnownedFileConflict, ex.installingModule?.ToString() ?? "", ex.filename, ex.installingModule?.ToString() ?? "");
                            }
                            RaiseError(Properties.Resources.InstallFilesReverted);
                        } catch (DownloadErrorsKraken ex) {
                            RaiseError("{0}", ex.ToString());
                        } catch (ModuleDownloadErrorsKraken ex) {
                            RaiseError("{0}", ex.ToString());
                        } catch (RequestThrottledKraken ex) {
                            if (RaiseYesNoDialog(string.Format(Properties.Resources.InstallAuthTokenPrompt, ex.Message))) {
                                if (ex.infoUrl != null) {
                                    ModInfoScreen.LaunchURL(theme, ex.infoUrl);
                                }
                                LaunchSubScreen(new AuthTokenScreen(theme));
                            }
                        } catch (MissingCertificateKraken ex) {
                            RaiseError("{0}", ex.ToString());
                        } catch (InconsistentKraken ex) {
                            RaiseError("{0}", ex.Message);
                        } catch (TooManyModsProvideKraken ex) {

                            var ch = new ConsoleChoiceDialog<CkanModule>(
                                theme,
                                ex.Message,
                                Properties.Resources.InstallTooManyModsNameHeader,
                                ex.modules,
                                mod => mod.ToString()
                            );
                            var chosen = ch.Run();
                            DrawBackground();
                            if (chosen != null) {
                                // Use chosen to continue installing
                                plan.Install.Add(chosen);
                                retry = true;
                            }

                        } catch (BadMetadataKraken ex) {
                            RaiseError(Properties.Resources.InstallBadMetadata, ex.module?.ToString() ?? "", ex.Message);
                        } catch (DependenciesNotSatisfiedKraken ex) {
                            RaiseError("{0}", ex.Message);
                        } catch (ModuleNotFoundKraken ex) {
                            RaiseError(Properties.Resources.InstallModuleNotFound, ex.module, ex.Message);
                        } catch (ModNotInstalledKraken ex) {
                            RaiseError(Properties.Resources.InstallNotInstalled, ex.mod);
                        } catch (DllLocationMismatchKraken ex) {
                            RaiseError("{0}", ex.Message);
                        }
                    } while (retry);
                }
            }
        }

        private void HandlePossibleConfigOnlyDirs(ConsoleTheme     theme,
                                                  Registry         registry,
                                                  HashSet<string>? possibleConfigOnlyDirs)
        {
            if (possibleConfigOnlyDirs != null && manager.CurrentInstance != null)
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
                    LaunchSubScreen(new DeleteDirectoriesScreen(theme, manager.CurrentInstance, possibleConfigOnlyDirs));
                }
            }
        }

        private void OnModInstalled(CkanModule mod)
        {
            RaiseMessage(Properties.Resources.InstallModInstalled,
                         Symbols.checkmark, mod.name, mod.version.StripEpoch());
        }

        private IEnumerable<ModuleReplacement> AllReplacements(IEnumerable<string> identifiers)
        {
            if (manager.CurrentInstance != null)
            {
                IRegistryQuerier registry = RegistryManager.Instance(manager.CurrentInstance, repoData).registry;

                foreach (string id in identifiers) {
                    var repl = registry.GetReplacement(
                        id, manager.CurrentInstance.StabilityToleranceConfig,
                        manager.CurrentInstance.VersionCriteria());
                    if (repl != null) {
                        yield return repl;
                    }
                }
            }
        }

        private static RelationshipResolverOptions resolvOpts(StabilityToleranceConfig stabTolCfg)
            => new RelationshipResolverOptions(stabTolCfg) {
            with_all_suggests              = false,
            with_suggests                  = false,
            with_recommends                = false,
            without_toomanyprovides_kraken = false,
            without_enforce_consistency    = false
        };

        private readonly GameInstanceManager   manager;
        private readonly RepositoryDataManager repoData;
        private readonly string?               userAgent;
        private readonly ChangePlan            plan;
        private readonly bool                  debug;
    }

}
