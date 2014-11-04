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

            // If we're starting with no options, invoke the GUI instead.

            if (args.Length == 0)
            {
                return Gui();
            }

            Options cmdline;

            try
            {
                cmdline = new Options(args);
            }
            catch (NullReferenceException)
            {
                // Oops, something went wrong. Generate the help screen instead!

                string[] help = {"--help"}; // Is there a nicer way than a temp var?
                new Options(help);
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

                case "remove":
                    return Remove((RemoveOptions) cmdline.options);

                case "upgrade":
                    return Upgrade((UpgradeOptions) cmdline.options);

                case "clean":
                    return Clean();

                case "ksp":
                    return KSP((KSPOptions) cmdline.options);

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

            int updated = Repo.Update(options.repo);

            User.WriteLine("Updated information on {0} available modules", updated);

            return Exit.OK;
        }

        private static int Available()
        {
            List<CkanModule> available = RegistryManager.Instance(KSPManager.CurrentInstance).registry.Available();

            User.WriteLine("Mods available for KSP {0}", KSPManager.CurrentInstance.Version());
            User.WriteLine("");

            foreach (CkanModule module in available)
            {
                User.WriteLine("* {0}", module);
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
            var installer = ModuleInstaller.Instance;
            installer.UninstallList(options.Modname);

            return Exit.OK;
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
                using (var transaction = new TransactionScope ())
                {
                    var installer = ModuleInstaller.Instance;

                    foreach (string module in options.modules)
                    {
                        installer.UninstallList(module);
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
                    User.WriteLine("* {0}", mod.identifier);
                }

                return Exit.ERROR;
            }

            User.WriteLine("\nDone!\n");

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
            InstalledModule module;

            try
            {
                module = registry_manager.registry.installed_modules[options.Modname];
            }
            catch (KeyNotFoundException)
            {
                User.WriteLine("{0} not installed.", options.Modname);
                User.WriteLine("Try `ckan list` to show installed modules");
                return Exit.BADOPT;
            }

            // TODO: Print *lots* of information out; I should never have to dig through JSON

            User.WriteLine("{0} version {1}", module.source_module.name, module.source_module.version);

            User.WriteLine("\n== Files ==\n");

            Dictionary<string, InstalledModuleFile> files = module.installed_files;

            foreach (string file in files.Keys)
            {
                User.WriteLine(file);
            }

            return Exit.OK;
        }

        internal static int KSP(KSPOptions options)
        {
            string[] options_array = options.options.ToArray();

            var subopt = new CmdLine.KSP(options_array);

            return subopt.RunSubCommand();
        }

    }

    // Exception class, so we can signal errors in command options.

    internal class BadCommandException : Exception
    {
        public BadCommandException(string message) : base(message)
        {
        }
    }
}