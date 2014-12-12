// Reference CKAN client
// Paul '@pjf' Fenwick
//
// License: CC-BY 4.0, LGPL, or MIT (your choice)

using System;
using System.Collections.Generic;
using log4net;
using log4net.Config;
using log4net.Core;

namespace CKAN.CmdLine
{
    internal class MainClass
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (MainClass));        

        /*
         * When the STAThread is applied, it changes the apartment state of the current thread to be single threaded. 
         * Without getting into a huge discussion about COM and threading,
         * this attribute ensures the communication mechanism between the current thread an
         * other threads that may want to talk to it via COM.  When you're using Windows Forms,
         * depending on the feature you're using, it may be using COM interop in order to communicate with
         * operating system components.  Good examples of this are the Clipboard and the File Dialogs. 
         */
        [STAThread]
        public static int Main(string[] args)
        {

            BasicConfigurator.Configure();
            LogManager.GetRepository().Threshold = Level.Warn;
            log.Debug("CKAN started");

            // If we're starting with no options then invoke the GUI instead.

            if (args.Length == 0)
            {
                return Gui();
            }

            var user = new ConsoleUser();
            Options cmdline;

            try
            {
                cmdline = new Options(args);
            }
            catch (BadCommandKraken)
            {
                // Our help screen will already be shown. Let's add some extra data.
                user.DisplayMessage("You are using CKAN version {0}", Meta.Version());

                return Exit.BADOPT;
            }

            // Process commandline options.

            var options = (CommonOptions) cmdline.options;

            if (options.Debug)
            {
                LogManager.GetRepository().Threshold = Level.Debug;
                log.Info("Debug logging enabled");
            }
            else if (options.Verbose)
            {
                LogManager.GetRepository().Threshold = Level.Info;
                log.Info("Verbose logging enabled");
            }

            // TODO: Allow the user to specify just a directory.
            // User provided KSP instance

            if (options.KSPdir != null && options.KSP != null)
            {
                user.DisplayMessage("--ksp and --kspdir can't be specified at the same time");
                return Exit.BADOPT;
            }

            if (options.KSP != null)
            {
                // Set a KSP directory by its alias.

                try
                {
                    KSPManager.SetCurrentInstance(options.KSP, user);
                }
                catch (InvalidKSPInstanceKraken)
                {
                    user.DisplayMessage("Invalid KSP installation specified \"{0}\", use '--kspdir' to specify by path, or 'list-installs' to see known KSP installations", options.KSP);
                    return Exit.BADOPT;
                }
            }
            else if (options.KSPdir != null)
            {
                // Set a KSP directory by its path

                KSPManager.SetCurrentInstanceByPath(options.KSPdir, user);
            }
            else if (! (cmdline.action == "ksp" || cmdline.action == "version"))
            {
                // Find whatever our preferred instance is.
                // We don't do this on `ksp/version` commands, they don't need it.
                CKAN.KSP ksp = KSPManager.GetPreferredInstance(user);

                if (ksp == null)
                {
                    user.DisplayMessage("I don't know where KSP is installed.");
                    user.DisplayMessage("Use 'ckan ksp help' for assistance on setting this.");
                    return Exit.ERROR;
                }
                else
                {
                    log.InfoFormat("Using KSP install at {0}",ksp.GameDir());
                }
            }

            CKAN.KSP current_ksp = KSPManager.CurrentInstance;

            switch (cmdline.action)
            {
                case "gui":
                    return Gui();

                case "version":
                    return Version(user);

                case "update":
                    return Update((UpdateOptions) options,user);

                case "available":
                    return Available(user);

                case "install":
                    return Install((InstallOptions) cmdline.options,user);

                case "scan":
                    return Scan();

                case "list":
                    return List(user);

                case "show":
                    return Show((ShowOptions)cmdline.options, user);

                case "remove":
                    return Remove((RemoveOptions)cmdline.options, user);

                case "upgrade":
                    var upgrade = new Upgrade();
                    return upgrade.RunCommand(current_ksp, cmdline.options);

                case "clean":
                    return Clean();

                case "repair":
                    return RunSubCommand<Repair>((SubCommandOptions) cmdline.options);

                case "ksp":
                    return RunSubCommand<KSP>((SubCommandOptions) cmdline.options);

                default:
                    user.DisplayMessage("Unknown command, try --help");
                    return Exit.BADOPT;
            }
        }

        private static int Gui()
        {
            // TODO: Sometimes when the GUI exits, we get a System.ArgumentException,
            // but trying to catch it here doesn't seem to help. Dunno why.

            GUI.Main();

            return Exit.OK;
        }

        private static int Version(IUser user)
        {
            user.DisplayMessage(Meta.Version());

            return Exit.OK;
        }

        private static int Update(UpdateOptions options, IUser user)
        {
            user.DisplayMessage("Downloading updates...");

            try
            {
                int updated = Repo.Update(options.repo);
                user.DisplayMessage("Updated information on {0} available modules", updated);
            }
            catch (MissingCertificateKraken kraken)
            {
                // Handling the kraken means we have prettier output.
                user.DisplayError(kraken.ToString());
                return Exit.ERROR;
            }

            return Exit.OK;
        }

        private static int Available(IUser user)
        {
            List<CkanModule> available = RegistryManager.Instance(KSPManager.CurrentInstance).registry.Available();

            user.DisplayMessage("Mods available for KSP {0}", KSPManager.CurrentInstance.Version());
            user.DisplayMessage("");

            var width = user.WindowWidth;

            foreach (CkanModule module in available)
            {
                string entry = String.Format("* {0} ({1}) - {2}", module.identifier, module.version, module.name);
                if (width > 0) {
                    user.DisplayMessage(entry.PadRight(width).Substring(0, width - 1));
                }
                else
                {
                    user.DisplayMessage(entry);
                }
            }

            return Exit.OK;
        }

        private static int Scan()
        {
            KSPManager.CurrentInstance.ScanGameData();

            return Exit.OK;
        }

        private static int List(IUser user)
        {
            CKAN.KSP ksp = KSPManager.CurrentInstance;

            user.DisplayMessage("\nKSP found at {0}\n", ksp.GameDir());
            user.DisplayMessage("KSP Version: {0}\n", ksp.Version());

            Registry registry = RegistryManager.Instance(ksp).registry;

            user.DisplayMessage("Installed Modules:\n");

            var installed = new SortedDictionary<string, Version>(registry.Installed());

            foreach (KeyValuePair<string, Version> mod in installed)
            {
                string identifier = mod.Key;
                Version current_version = mod.Value;

                string bullet = "*";

                if (current_version is ProvidesVersion)
                {
                    // Skip virtuals for now.
                    continue;
                }
                else if (current_version is DllVersion)
                {
                    // Autodetected dll
                    bullet = "-";
                }
                else
                {
                    try
                    {
                        // Check if upgrades are available, and show appropriately.
                        CkanModule latest = registry.LatestAvailable(mod.Key, ksp.Version());

                        log.InfoFormat("Latest {0} is {1}", mod.Key, latest);

                        if (latest == null)
                        {
                            // Not compatible!
                            bullet = "✗";
                        }
                        else if (latest.version.IsEqualTo(current_version))
                        {
                            // Up to date
                            bullet = "✓";
                        }
                        else if (latest.version.IsGreaterThan(mod.Value))
                        {
                            // Upgradable
                            bullet = "↑";
                        }

                    }
                    catch (ModuleNotFoundKraken) {
                        log.InfoFormat("{0} is installed, but no longer in the registry",
                            mod.Key);

                        bullet = "?";
                    }
                }

                user.DisplayMessage("{0} {1} {2}", bullet, mod.Key, mod.Value);
            }

            user.DisplayMessage("\nLegend: ✓ - Up to date. ✗ - Incompatible. ↑ - Upgradable. ? - Unknown ");

            return Exit.OK;
        }

        // Uninstalls a module, if it exists.
        private static int Remove(RemoveOptions options, IUser user)
        {
            if (options.modules != null && options.modules.Count > 0)
            {
                try
                {
                    var installer = ModuleInstaller.GetInstance(user);
                    installer.UninstallList(options.modules);
                    return Exit.OK;
                }
                catch (ModNotInstalledKraken kraken)
                {
                    user.DisplayMessage("I can't do that, {0} isn't installed.", kraken.mod);
                    user.DisplayMessage("Try `ckan list` for a list of installed mods.");
                    return Exit.BADOPT;
                }
            }
            else
            {
                user.DisplayMessage("No mod selected, nothing to do");
                return Exit.BADOPT;
            }
        }

        private static int Clean()
        {
            KSPManager.CurrentInstance.CleanCache();
            return Exit.OK;
        }

        private static int Install(InstallOptions options, IUser user)
        {
            if (options.ckan_file != null)
            {
                // Oooh! We're installing from a CKAN file.
                log.InfoFormat("Installing from CKAN file {0}", options.ckan_file);

                CkanModule module = CkanModule.FromFile(options.ckan_file);

                // We'll need to make some registry changes to do this.
                RegistryManager registry_manager = RegistryManager.Instance(KSPManager.CurrentInstance);

                // Remove this version of the module in the registry, if it exists.
                registry_manager.registry.RemoveAvailable(module);

                // Sneakily add our version in...
                registry_manager.registry.AddAvailable(module);

                // Add our module to the things we should install...
                options.modules.Add(module.identifier);

                // And continue with our install as per normal.
            }
 
            if (options.modules.Count == 0)
            {
                // What? No files specified?
                user.DisplayMessage(
                    "Usage: ckan install [--with-suggests] [--with-all-suggests] [--no-recommends] Mod [Mod2, ...]");
                return Exit.BADOPT;
            }

            // Prepare options. Can these all be done in the new() somehow?
            var install_ops = new RelationshipResolverOptions();
            install_ops.with_all_suggests = options.with_all_suggests;
            install_ops.with_suggests = options.with_suggests;
            install_ops.with_recommends = ! options.no_recommends;

            // Install everything requested. :)
            try
            {
                var installer = ModuleInstaller.GetInstance(user);

                installer.onReportProgress = ProgressReporter.FormattedDownloads;

                installer.InstallList(options.modules, install_ops);
            }
            catch (ModuleNotFoundKraken ex)
            {
                user.DisplayMessage("Module {0} required, but not listed in index, or not available for your version of KSP", ex.module);
                user.DisplayMessage("If you're lucky, you can do a `ckan update` and try again.");
                user.DisplayMessage("Try `ckan install --no-recommends` to skip installation of recommended modules");
                return Exit.ERROR;
            }
            catch (BadMetadataKraken ex)
            {
                user.DisplayMessage("Bad metadata detected for module {0}", ex.module);
                user.DisplayMessage(ex.Message);
                return Exit.ERROR;
            }
            catch (TooManyModsProvideKraken ex)
            {
                user.DisplayMessage("Too many mods provide {0}. Please pick from the following:\n", ex.requested);

                foreach (CkanModule mod in ex.modules)
                {
                    user.DisplayMessage("* {0} ({1})", mod.identifier, mod.name);
                }

                user.DisplayMessage(String.Empty); // Looks tidier.

                return Exit.ERROR;
            }
            catch (FileExistsKraken ex)
            {
                if (ex.owning_module != null)
                {
                    user.DisplayMessage(
                        "\nOh no! We tried to overwrite a file owned by another mod!\n"+
                        "Please try a `ckan update` and try again.\n\n"+
                        "If this problem re-occurs, then it maybe a packaging bug.\n"+
                        "Please report it at:\n\n" +
                        "https://github.com/KSP-CKAN/CKAN-meta/issues/new\n\n"+
                        "Please including the following information in your report:\n\n" +
                        "File           : {0}\n" +
                        "Installing Mod : {1}\n" +
                        "Owning Mod     : {2}\n" +
                        "CKAN Version   : {3}\n",
                        ex.filename, ex.installing_module, ex.owning_module,
                        Meta.Version()
                    );
                }
                else
                {
                    user.DisplayMessage(
                        "\n\nOh no!\n\n"+
                        "It looks like you're trying to install a mod which is already installed,\n"+
                        "or which conflicts with another mod which is already installed.\n\n"+
                        "As a safety feature, the CKAN will *never* overwrite or alter a file\n"+
                        "that it did not install itself.\n\n"+
                        "If you wish to install {0} via the CKAN,\n"+
                        "then please manually uninstall the mod which owns:\n\n"+
                        "{1}\n\n"+"and try again.\n",
                        ex.installing_module, ex.filename
                    );
                }

                user.DisplayMessage("Your GameData has been returned to its original state.\n");
                return Exit.ERROR;
            }
            catch (InconsistentKraken ex)
            {
                // The prettiest Kraken formats itself for us.
                user.DisplayMessage(ex.InconsistenciesPretty);
                return Exit.ERROR;
            }
            catch (CancelledActionKraken)
            {
                user.DisplayMessage("Installation cancelled at user request.");
                return Exit.ERROR;
            }
            catch (MissingCertificateKraken kraken)
            {
                // Another very pretty kraken.
                user.DisplayMessage(kraken.ToString());
                return Exit.ERROR;
            }
            catch (DownloadErrorsKraken)
            {
                user.DisplayMessage("One or more files failed to download, stopped.");
                return Exit.ERROR;
            }

            return Exit.OK;
        }

        // TODO: We should have a command (probably this one) that shows
        // info about uninstalled modules.
        private static int Show(ShowOptions options,IUser user)
        {
            if (options.Modname == null)
            {
                // empty argument
                user.DisplayMessage("show <module> - module name argument missing, perhaps you forgot it?");
                return Exit.BADOPT;
            }

            RegistryManager registry_manager = RegistryManager.Instance(KSPManager.CurrentInstance);
            InstalledModule module = registry_manager.registry.InstalledModule(options.Modname);

            if (module == null)
            {
                user.DisplayMessage("{0} not installed.", options.Modname);
                user.DisplayMessage("Try `ckan list` to show installed modules");
                return Exit.BADOPT;
            }

            // TODO: Print *lots* of information out; I should never have to dig through JSON

            user.DisplayMessage("{0} version {1}", module.Module.name, module.Module.version);

            user.DisplayMessage("\n== Files ==\n");

            IEnumerable<string> files = module.Files;

            foreach (string file in files)
            {
                user.DisplayMessage(file);
            }

            return Exit.OK;
        }

        private static int RunSubCommand<T>(SubCommandOptions options)
            where T : ISubCommand, new()
        {
            ISubCommand subopt = new T ();

            return subopt.RunSubCommand(options);
        }
    }
}
