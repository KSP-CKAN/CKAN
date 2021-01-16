using System;
using System.Transactions;
using System.Collections.Generic;
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
        /// <param name="cp">Plan of mods to install or remove</param>
        /// <param name="dbg">True if debug options should be available, false otherwise</param>
        public InstallScreen(GameInstanceManager mgr, ChangePlan cp, bool dbg)
            : base(
                "Installing, Updating, and Removing Mods",
                "Calculating..."
            )
        {
            debug   = dbg;
            manager = mgr;
            plan    = cp;
        }

        /// <summary>
        /// Run the screen
        /// </summary>
        /// <param name="process">Framework parameter not used by this object</param>
        public override void Run(Action process = null)
        {
            HashSet<string> rejected = new HashSet<string>();
            DrawBackground();
            using (TransactionScope trans = CkanTransaction.CreateTransactionScope()) {
                bool retry = false;
                do {
                    Draw();
                    try {
                        // Reset this so we stop unless an exception sets it to true
                        retry = false;

                        // GUI prompts user to choose recs/sugs,
                        // CmdLine assumes recs and ignores sugs
                        if (plan.Install.Count > 0) {
                            // Track previously rejected optional dependencies and don't prompt for them again.
                            DependencyScreen ds = new DependencyScreen(manager, plan, rejected, debug);
                            if (ds.HaveOptions()) {
                                LaunchSubScreen(ds);
                            }
                        }

                        // FUTURE: BackgroundWorker

                        HashSet<string> possibleConfigOnlyDirs = null;

                        RegistryManager regMgr = RegistryManager.Instance(manager.CurrentInstance);
                        ModuleInstaller inst = ModuleInstaller.GetInstance(manager.CurrentInstance, manager.Cache, this);
                        inst.onReportModInstalled = OnModInstalled;
                        if (plan.Remove.Count > 0) {
                            inst.UninstallList(plan.Remove, ref possibleConfigOnlyDirs, regMgr, true, plan.Install);
                            plan.Remove.Clear();
                        }
                        NetAsyncModulesDownloader dl = new NetAsyncModulesDownloader(this, manager.Cache);
                        if (plan.Upgrade.Count > 0) {
                            inst.Upgrade(plan.Upgrade, dl, ref possibleConfigOnlyDirs, regMgr);
                            plan.Upgrade.Clear();
                        }
                        if (plan.Install.Count > 0) {
                            List<CkanModule> iList = new List<CkanModule>(plan.Install);
                            inst.InstallList(iList, resolvOpts, regMgr, ref possibleConfigOnlyDirs, dl);
                            plan.Install.Clear();
                        }
                        if (plan.Replace.Count > 0) {
                            inst.Replace(AllReplacements(plan.Replace), resolvOpts, dl, ref possibleConfigOnlyDirs, regMgr, true);
                        }

                        trans.Complete();
                        // Don't let the installer re-use old screen references
                        inst.User = null;
                        inst.onReportModInstalled = null;

                    } catch (CancelledActionKraken) {
                        // Don't need to tell the user they just cancelled out.
                    } catch (FileNotFoundKraken ex) {
                        // Possible file corruption
                        RaiseError(ex.Message);
                    } catch (DirectoryNotFoundKraken ex) {
                        RaiseError(ex.Message);
                    } catch (FileExistsKraken ex) {
                        if (ex.owningModule != null) {
                            RaiseMessage($"{ex.installingModule} tried to install {ex.filename}, but {ex.owningModule} has already installed it.");
                            RaiseMessage($"Please report this problem at https://github.com/KSP-CKAN/NetKAN/issues/new/choose");
                        } else {
                            RaiseMessage($"{ex.installingModule} tried to install {ex.filename}, but it is already installed.");
                            RaiseMessage($"Please manually uninstall the mod that owns this file to install {ex.installingModule}.");
                        }
                        RaiseError("Game files reverted.");
                    } catch (DownloadErrorsKraken ex) {
                        RaiseError(ex.ToString());
                    } catch (ModuleDownloadErrorsKraken ex) {
                        RaiseError(ex.ToString());
                    } catch (DownloadThrottledKraken ex) {
                        if (RaiseYesNoDialog($"{ex.ToString()}\n\nEdit authentication tokens now?")) {
                            if (ex.infoUrl != null) {
                                ModInfoScreen.LaunchURL(ex.infoUrl);
                            }
                            LaunchSubScreen(new AuthTokenScreen());
                        }
                    } catch (MissingCertificateKraken ex) {
                        RaiseError(ex.ToString());
                    } catch (InconsistentKraken ex) {
                        RaiseError(ex.InconsistenciesPretty);
                    } catch (TooManyModsProvideKraken ex) {

                        ConsoleChoiceDialog<CkanModule> ch = new ConsoleChoiceDialog<CkanModule>(
                            $"Module {ex.requested} is provided by multiple modules. Which would you like to install?",
                            "Name",
                            ex.modules,
                            (CkanModule mod) => mod.ToString()
                        );
                        CkanModule chosen = ch.Run();
                        DrawBackground();
                        if (chosen != null) {
                            // Use chosen to continue installing
                            plan.Install.Add(chosen);
                            retry = true;
                        }

                    } catch (BadMetadataKraken ex) {
                        RaiseError($"Bad metadata detected for {ex.module}: {ex.Message}");
                    } catch (DependencyNotSatisfiedKraken ex) {
                        RaiseError($"{ex.parent} requires {ex.module}, but it is not listed in the index, or not available for your version of the game.\r\n{ex.Message}");
                    } catch (ModuleNotFoundKraken ex) {
                        RaiseError($"Module {ex.module} required but it is not listed in the index, or not available for your version of the game.\r\n{ex.Message}");
                    } catch (ModNotInstalledKraken ex) {
                        RaiseError($"{ex.mod} is not installed, can't remove");
                    } catch (DllLocationMismatchKraken ex) {
                        RaiseError(ex.Message);
                    }
                } while (retry);
            }
        }

        private void OnModInstalled(CkanModule mod)
        {
            RaiseMessage($"{Symbols.checkmark} Successfully installed {mod.name} {ModuleInstaller.StripEpoch(mod.version)}");
        }

        private IEnumerable<ModuleReplacement> AllReplacements(IEnumerable<string> identifiers)
        {
            IRegistryQuerier registry = RegistryManager.Instance(manager.CurrentInstance).registry;

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

        private GameInstanceManager manager;
        private ChangePlan plan;
        private bool       debug;
    }

}
