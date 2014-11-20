// Reference CKAN client
// Paul '@pjf' Fenwick
//
// License: CC-BY 4.0, LGPL, or MIT (your choice)

using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using log4net;
using log4net.Config;
using log4net.Core;
using System.Transactions;

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

            // If we're starting with no options, and this isn't
            // a stable build, then invoke the GUI instead.

            if (args.Length == 0)
            {
                if (Meta.IsStable())
                {
                    args = new string[] { "--help" };
                }
                else
                {
                    return Gui();
                }
            }

            Options cmdline;

            try
            {
                cmdline = new Options(args);
            }
            catch (BadCommandKraken)
            {
                // Our help screen will already be shown. Let's add some extra data.
                User.WriteLine("You are using CKAN version {0}", Meta.Version());

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
                User.WriteLine("--ksp and --kspdir can't be specified at the same time");
                return Exit.BADOPT;
            }

            if (options.KSP != null)
            {
                // Set a KSP directory by its alias.

                try
                {
                    KSPManager.SetCurrentInstance(options.KSP);
                }
                catch (InvalidKSPInstanceKraken)
                {
                    User.WriteLine("Invalid KSP installation specified \"{0}\", use '--kspdir' to specify by path, or 'list-installs' to see known KSP installations", options.KSP);
                    return Exit.BADOPT;
                }
            }
            else if (options.KSPdir != null)
            {
                // Set a KSP directory by its path

                KSPManager.SetCurrentInstanceByPath(options.KSPdir);
            }
            else if (! (cmdline.action == "ksp" || cmdline.action == "version"))
            {
                // Find whatever our preferred instance is.
                // We don't do this on `ksp/version` commands, they don't need it.
                CKAN.KSP ksp = KSPManager.GetPreferredInstance();

                if (ksp == null)
                {
                    User.WriteLine("I don't know where KSP is installed.");
                    User.WriteLine("Use 'ckan ksp help' for assistance on setting this.");
                    return Exit.ERROR;
                }
                else
                {
                    log.InfoFormat("Using KSP install at {0}",ksp.GameDir());
                }
            }

            switch (cmdline.action)
            {
                case "gui":
                    return Gui();

                case "version":
                    return Version();

                case "update":
                    return Update((UpdateOptions) options);

                case "available":
                    return Available();

                case "install":
                    return Install((InstallOptions) cmdline.options);

                case "scan":
                    return Scan();

                case "list":
                    return List();

                case "show":
                    return Show((ShowOptions) cmdline.options);

                case "uninstall":
                case "remove":
                    return Remove((RemoveOptions) cmdline.options);

                case "upgrade":
                    return Upgrade((UpgradeOptions) cmdline.options);

                case "clean":
                    return Clean();

                case "repair":
                    return RunSubCommand<Repair>((SubCommandOptions) cmdline.options);

                case "ksp":
                    return RunSubCommand<KSP>((SubCommandOptions) cmdline.options);

                default:
                    User.WriteLine("Unknown command, try --help");
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

        private static int Version()
        {
            User.WriteLine(Meta.Version());

            return Exit.OK;
        }

        private static int Update(UpdateOptions options)
        {
            User.WriteLine("Downloading updates...");

            try
            {
                int updated = Repo.Update(options.repo);
                User.WriteLine("Updated information on {0} available modules", updated);
            }
            catch (MissingCertificateKraken kraken)
            {
                // Handling the kraken means we have prettier output.
                Console.WriteLine(kraken);
                return Exit.ERROR;
            }

            return Exit.OK;
        }

        private static int Available()
        {
            List<CkanModule> available = RegistryManager.Instance(KSPManager.CurrentInstance).registry.Available();

            User.WriteLine("Mods available for KSP {0}", KSPManager.CurrentInstance.Version());
            User.WriteLine("");

            var width = Console.WindowWidth;

            foreach (CkanModule module in available)
            {
                string entry = String.Format("* {0} ({1}) - {2}", module.identifier, module.version, module.name);
                if (width > 0) {
                    User.WriteLine(entry.PadRight(Console.WindowWidth).Substring(0,Console.WindowWidth));
                }
                else
                {
                    User.WriteLine(entry);
                }
            }

            return Exit.OK;
        }

        private static int Scan()
        {
            KSPManager.CurrentInstance.ScanGameData();

            return Exit.OK;
        }

        private static int List()
        {
            CKAN.KSP ksp = KSPManager.CurrentInstance;

            User.WriteLine("\nKSP found at {0}\n", ksp.GameDir());
            User.WriteLine("KSP Version: {0}\n",ksp.Version());

            Registry registry = RegistryManager.Instance(ksp).registry;

            User.WriteLine("Installed Modules:\n");

            var installed = new SortedDictionary<string, Version>(registry.Installed());

            foreach (var mod in installed)
            {
                User.WriteLine("* {0} {1}", mod.Key, mod.Value);
            }

            // Blank line at the end makes for nicer looking output.
            User.WriteLine("");

            return Exit.OK;
        }

        // Uninstalls a module, if it exists.
        private static int Remove(RemoveOptions options)
        {
            if (options.Modname != null && options.Modname.Length > 0)
            {
                try
                {
                    var installer = ModuleInstaller.Instance;
                    installer.UninstallList(options.Modname);
                    return Exit.OK;
                }
                catch (ModNotInstalledKraken kraken)
                {
                    User.WriteLine("I can't do that, {0} isn't installed.", kraken.mod);
                    User.WriteLine("Try `ckan list` for a list of installed mods.");
                    return Exit.BADOPT;
                }
            }
            else
            {
                User.WriteLine("No mod selected, nothing to do");
                return Exit.BADOPT;
            }
        }

        // TODO: This needs work! See GH #160.
        private static int Upgrade(UpgradeOptions options)
        {
            if (options.ckan_file == null)
            {
                // Typical case, install from cached CKAN info.

                if (options.modules.Count == 0)
                {
                    // What? No files specified?
                    User.WriteLine(
                        "Usage: ckan upgrade [--with-suggests] [--with-all-suggests] [--no-recommends] Mod [Mod2, ...]");
                    return Exit.BADOPT;
                }

                // Do our un-installs and re-installs in a transaction. If something goes wrong,
                // we put the user's data back the way it was. (Both Install and Uninstall support transactions.)
                using (var transaction = new TransactionScope ()) {
                    var installer = ModuleInstaller.Instance;

                    try
                    {
                        installer.UninstallList(options.modules);
                    }
                    catch (ModNotInstalledKraken kraken)
                    {
                        User.WriteLine("I can't do that, {0} is not installed.", kraken.mod);
                        return Exit.BADOPT;
                    }

                    // Prepare options. Can these all be done in the new() somehow?
                    var install_ops = new RelationshipResolverOptions();
                    install_ops.with_all_suggests = options.with_all_suggests;
                    install_ops.with_suggests = options.with_suggests;
                    install_ops.with_recommends = !options.no_recommends;

                    // Install everything requested. :)
                    try
                    {
                        installer.InstallList(options.modules, install_ops);
                    }
                    catch (ModuleNotFoundKraken ex)
                    {
                        User.WriteLine("Module {0} required, but not listed in index.", ex.module);
                        User.WriteLine("If you're lucky, you can do a `ckan update` and try again.");
                        return Exit.ERROR;
                    }

                    transaction.Complete();
                }

                User.WriteLine("\nDone!\n");

                return Exit.OK;
            }

            User.WriteLine("\nUnsupported option at this time.");

            return Exit.BADOPT;
        }

        private static int Clean()
        {
            KSPManager.CurrentInstance.CleanCache();
            return Exit.OK;
        }

        private static int Install(InstallOptions options)
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
                User.WriteLine(
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
                var installer = ModuleInstaller.Instance;

                installer.onReportProgress = ProgressReporter.FormattedDownloads;

                installer.InstallList(options.modules, install_ops);
            }
            catch (ModuleNotFoundKraken ex)
            {
                User.WriteLine("Module {0} required, but not listed in index, or not available for your version of KSP", ex.module);
                User.WriteLine("If you're lucky, you can do a `ckan update` and try again.");
                User.WriteLine("Try `ckan install --no-recommends` to skip installation of recommended modules");
                return Exit.ERROR;
            }
            catch (BadMetadataKraken ex)
            {
                User.WriteLine("Bad metadata detected for module {0}", ex.module);
                User.WriteLine(ex.Message);
                return Exit.ERROR;
            }
            catch (TooManyModsProvideKraken ex)
            {
                User.WriteLine("Too many mods provide {0}. Please pick from the following:\n", ex.requested);

                foreach (CkanModule mod in ex.modules)
                {
                    User.WriteLine("* {0} ({1})", mod.identifier, mod.name);
                }

                User.WriteLine(""); // Looks tidier.

                return Exit.ERROR;
            }
            catch (FileExistsKraken ex)
            {
                if (ex.owning_module != null)
                {
                    User.WriteLine(
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
                    User.WriteLine(
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

                User.WriteLine("Your GameData has been returned to its original state.\n");
                return Exit.ERROR;
            }
            catch (InconsistentKraken ex)
            {
                // The prettiest Kraken formats itself for us.
                User.WriteLine(ex.InconsistenciesPretty);
                return Exit.ERROR;
            }
            catch (CancelledActionKraken)
            {
                User.WriteLine("Installation cancelled at user request.");
                return Exit.ERROR;
            }
            catch (MissingCertificateKraken kraken)
            {
                // Another very pretty kraken.
                Console.WriteLine(kraken);
                return Exit.ERROR;
            }

            return Exit.OK;
        }

        // TODO: We should have a command (probably this one) that shows
        // info about uninstalled modules.
        private static int Show(ShowOptions options)
        {
            if (options.Modname == null)
            {
                // empty argument
                User.WriteLine("show <module> - module name argument missing, perhaps you forgot it?");
                return Exit.BADOPT;
            }

            RegistryManager registry_manager = RegistryManager.Instance(KSPManager.CurrentInstance);
            InstalledModule module = registry_manager.registry.InstalledModule(options.Modname);

            if (module == null)
            {
                User.WriteLine("{0} not installed.", options.Modname);
                User.WriteLine("Try `ckan list` to show installed modules");
                return Exit.BADOPT;
            }

            // TODO: Print *lots* of information out; I should never have to dig through JSON

            User.WriteLine("{0} version {1}", module.Module.name, module.Module.version);

            User.WriteLine("\n== Files ==\n");

            IEnumerable<string> files = module.Files;

            foreach (string file in files)
            {
                User.WriteLine(file);
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
