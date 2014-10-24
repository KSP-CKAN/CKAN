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

namespace CKAN
{
    internal class MainClass
    {
        public const int EXIT_OK = 0;
        public const int EXIT_ERROR = 1;
        public const int EXIT_BADOPT = 2;

        private static readonly ILog log = LogManager.GetLogger(typeof (MainClass));

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
                return EXIT_BADOPT;
            }

            // Process commandline options.

            var options = (CommonOptions) cmdline.options;

            if (options.Debug)
            {
                LogManager.GetRepository().Threshold = Level.Debug;
            }
            else if (options.Verbose)
            {
                LogManager.GetRepository().Threshold = Level.Info;
            }

            // User provided KSP instance
            if (options.KSP != null)
            {

                try
                {
                    KSPManager.SetCurrentInstance(options.KSP);
                }
                catch (InvalidKSPInstanceKraken)
                {
                    User.WriteLine("Invalid KSP installation specified \"{0}\", use 'list-installs' to see known KSP installations", options.KSP);
                    return EXIT_BADOPT;
                }
            }
            else
            {
                // auto-start instance
                KSPManager.GetPreferredInstance();
            }

            User.WriteLine("Using KSP installation at \"{0}\"", KSPManager.CurrentInstance.GameDir());

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

                // TODO: Combine all of these into `ckan kspdir ...` or something similar
                // Eg: `ckan kspdir add` `ckan kspdir list` etc

                case "list-installs":
                    return ListInstalls();

                case "add-install":
                    return AddInstall((AddInstallOptions) cmdline.options);

                case "rename-install":
                    return RenameInstall((RenameInstallOptions)cmdline.options);

                case "remove-install":
                    return RemoveInstall((RemoveInstallOptions)cmdline.options);

                case "set-default-install":
                    return SetDefaultInstall((SetDefaultInstallOptions)cmdline.options);

                case "clear-cache":
                    return ClearCache((ClearCacheOptions)cmdline.options);

                default:
                    User.WriteLine("Unknown command, try --help");
                    return EXIT_BADOPT;
            }
        }

        private static int Gui()
        {
            // TODO: Sometimes when the GUI exits, we get a System.ArgumentException,
            // but trying to catch it here doesn't seem to help. Dunno why.

            GUI.Main();

            return EXIT_OK;
        }

        private static int Version()
        {
            User.WriteLine(Meta.Version());

            return EXIT_OK;
        }

        private static int Update(UpdateOptions options)
        {
            User.WriteLine("Downloading updates...");

            int updated = Repo.Update(options.repo);

            User.WriteLine("Updated information on {0} available modules", updated);

            return EXIT_OK;
        }

        private static int Available()
        {
            List<CkanModule> available = RegistryManager.Instance().registry.Available();

            User.WriteLine("Mods available for KSP {0}", KSPManager.CurrentInstance.Version());
            User.WriteLine("");

            foreach (CkanModule module in available)
            {
                User.WriteLine("* {0}", module);
            }

            return EXIT_OK;
        }

        private static int Scan()
        {
            KSPManager.CurrentInstance.ScanGameData();

            return EXIT_OK;
        }

        private static int List()
        {
            string ksp_path = KSPManager.CurrentInstance.GameDir();

            User.WriteLine("\nKSP found at {0}\n", ksp_path);
            User.WriteLine("KSP Version: {0}\n", KSPManager.CurrentInstance.Version());

            RegistryManager registry_manager = RegistryManager.Instance();
            Registry registry = registry_manager.registry;

            User.WriteLine("Installed Modules:\n");

            foreach (InstalledModule mod in registry.installed_modules.Values)
            {
                User.WriteLine("* {0} {1}", mod.source_module.identifier, mod.source_module.version);
            }

            User.WriteLine("\nDetected DLLs (`ckan scan` to rebuild):\n");

            // Walk our dlls, but *don't* show anything we've already displayed as
            // a module.
            foreach (string dll in registry.installed_dlls.Keys)
            {
                if (! registry.installed_modules.ContainsKey(dll))
                {
                    User.WriteLine("* {0}", dll);
                }
            }

            // Blank line at the end makes for nicer looking output.
            User.WriteLine("");

            return EXIT_OK;
        }

        // Uninstalls a module, if it exists.
        private static int Remove(RemoveOptions options)
        {
            var installer = ModuleInstaller.Instance;
            installer.Uninstall(options.Modname, true);

            return EXIT_OK;
        }

        private static int Upgrade(UpgradeOptions options)
        {
            if (options.zip_file == null && options.ckan_file == null)
            {
                // Typical case, install from cached CKAN info.

                if (options.modules.Count == 0)
                {
                    // What? No files specified?
                    User.WriteLine(
                        "Usage: ckan upgrade [--with-suggests] [--with-all-suggests] [--no-recommends] Mod [Mod2, ...]");
                    return EXIT_BADOPT;
                }

                var installer = ModuleInstaller.Instance;

                foreach (string module in options.modules)
                {
                    installer.Uninstall(module, false);
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
                    return EXIT_ERROR;
                }

                User.WriteLine("\nDone!\n");

                return EXIT_OK;
            }

            User.WriteLine("\nUnsupported option at this time.");

            return EXIT_BADOPT;
        }

        private static int Clean()
        {
            KSPManager.CurrentInstance.CleanCache();
            return EXIT_OK;
        }

        private static int Install(InstallOptions options)
        {
            if (options.zip_file == null && options.ckan_file == null)
            {
                // Typical case, install from cached CKAN info.

                if (options.modules.Count == 0)
                {
                    // What? No files specified?
                    User.WriteLine(
                        "Usage: ckan install [--with-suggests] [--with-all-suggests] [--no-recommends] Mod [Mod2, ...]");
                    return EXIT_BADOPT;
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
                    User.WriteLine("Module {0} required, but not listed in index.", ex.module);
                    User.WriteLine("If you're lucky, you can do a `ckan update` and try again.");
                    return EXIT_ERROR;
                }

                User.WriteLine("\nDone!\n");

                return EXIT_OK;
            }

            User.WriteLine("\nUnsupported option at this time.");

            return EXIT_BADOPT;
        }

        // TODO: We should have a command (probably this one) that shows
        // info about uninstalled modules.
        private static int Show(ShowOptions options)
        {
            if (options.Modname == null)
            {
                // empty argument
                User.WriteLine("show <module> - module name argument missing, perhaps you forgot it?");
                return EXIT_BADOPT;
            }

            RegistryManager registry_manager = RegistryManager.Instance();
            InstalledModule module;

            try
            {
                module = registry_manager.registry.installed_modules[options.Modname];
            }
            catch (KeyNotFoundException)
            {
                User.WriteLine("{0} not installed.", options.Modname);
                User.WriteLine("Try `ckan list` to show installed modules");
                return EXIT_BADOPT;
            }

            // TODO: Print *lots* of information out; I should never have to dig through JSON

            User.WriteLine("{0} version {1}", module.source_module.name, module.source_module.version);

            User.WriteLine("\n== Files ==\n");

            Dictionary<string, InstalledModuleFile> files = module.installed_files;

            foreach (string file in files.Keys)
            {
                User.WriteLine(file);
            }

            return EXIT_OK;
        }

        private static int ListInstalls()
        {
            User.WriteLine("Listing all known KSP installations:");
            User.WriteLine("");

            int count = 1;
            foreach (var instance in KSPManager.Instances)
            {
                User.WriteLine("{0}) \"{1}\" - {2}", count, instance.Key, instance.Value.GameDir());
                count++;
            }

            return EXIT_OK;
        }

        private static int AddInstall(AddInstallOptions options)
        {
            if (options.name == null || options.path == null)
            {
                User.WriteLine("add-install <name> <path> - argument missing, perhaps you forgot it?");
                return EXIT_BADOPT;
            }

            if (KSPManager.Instances.ContainsKey(options.name))
            {
                User.WriteLine("Install with name \"{0}\" already exists, aborting..", options.name);
                return EXIT_BADOPT;
            }

            try
            {

                KSPManager.AddInstance(options.name, options.path);
                User.WriteLine("Added \"{0}\" with root \"{1}\" to known installs", options.name, options.path);
                return EXIT_OK;
            }
            catch (NotKSPDirKraken ex)
            {
                User.WriteLine("Sorry, {0} does not appear to be a KSP directory", ex.path);
                return EXIT_BADOPT;
            }
        }

        private static int RenameInstall(RenameInstallOptions options)
        {
            if (options.old_name == null || options.new_name == null)
            {
                User.WriteLine("rename-install <old_name> <new_name> - argument missing, perhaps you forgot it?");
                return EXIT_BADOPT;
            }

            if (!KSPManager.Instances.ContainsKey(options.old_name))
            {
                User.WriteLine("Couldn't find install with name \"{0}\", aborting..", options.old_name);
                return EXIT_BADOPT;
            }

            KSPManager.RenameInstance(options.old_name, options.new_name);

            User.WriteLine("Successfully renamed \"{0}\" to \"{1}\"", options.old_name, options.new_name);
            return EXIT_OK;
        }

        private static int RemoveInstall(RemoveInstallOptions options)
        {
            if (options.name == null)
            {
                User.WriteLine("remove-install <name> - argument missing, perhaps you forgot it?");
                return EXIT_BADOPT;
            }

            if (!KSPManager.Instances.ContainsKey(options.name))
            {
                User.WriteLine("Couldn't find install with name \"{0}\", aborting..", options.name);
                return EXIT_BADOPT;
            }

            KSPManager.RemoveInstance(options.name);

            User.WriteLine("Successfully removed \"{0}\"", options.name);
            return EXIT_OK;
        }

        private static int SetDefaultInstall(SetDefaultInstallOptions options)
        {
            if (options.name == null)
            {
                User.WriteLine("set-default-install <name> - argument missing, perhaps you forgot it?");
                return EXIT_BADOPT;
            }

            if (!KSPManager.Instances.ContainsKey(options.name))
            {
                User.WriteLine("Couldn't find install with name \"{0}\", aborting..", options.name);
                return EXIT_BADOPT;
            }

            KSPManager.SetAutoStart(options.name);

            User.WriteLine("Successfully set \"{0}\" as the default KSP installation", options.name);
            return EXIT_OK;
        }

        private static int ClearCache(ClearCacheOptions options)
        {
            User.WriteLine("Clearing download cache..");

            var cachePath = Path.Combine(KSPManager.CurrentInstance.CkanDir(), "downloads");
            foreach (var file in Directory.GetFiles(cachePath))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {
                }
            }

            return EXIT_OK;
        }

    }


    // Look, parsing options is so easy and beautiful I made
    // it into a special class for you to admire!

    internal class Options
    {
        public Options(string[] args)
        {
            if (! Parser.Default.ParseArgumentsStrict(
                args, new Actions(), (verb, suboptions) =>
                {
                    action = verb;
                    options = suboptions;
                }
                ))
            {
                throw (new BadCommandException("Try ckan --help"));
            }

            // If we're here, success!
        }

        public string action { get; set; }
        public object options { get; set; }
    }

    // Actions supported by our client go here.
    // TODO: Figure out how to do per action help screens.

    internal class Actions
    {
        [VerbOption("gui", HelpText = "Start the CKAN GUI")]
        public GuiOptions GuiOptions { get; set; }

        [VerbOption("update", HelpText = "Update list of available mods")]
        public UpdateOptions Update { get; set; }

        [VerbOption("available", HelpText = "List available mods")]
        public AvailableOptions Available { get; set; }

        [VerbOption("install", HelpText = "Install a KSP mod")]
        public InstallOptions Install { get; set; }

        [VerbOption("remove", HelpText = "Remove an installed mod")]
        public RemoveOptions Remove { get; set; }

        [VerbOption("upgrade", HelpText = "Upgrade an installed mod")]
        public UpgradeOptions Upgrade { get; set; }

        [VerbOption("scan", HelpText = "Scan for manually installed KSP mods")]
        public ScanOptions Scan { get; set; }

        [VerbOption("list", HelpText = "List installed modules")]
        public ListOptions List { get; set; }

        [VerbOption("show", HelpText = "Show information about a mod")]
        public ShowOptions Show { get; set; }

        [VerbOption("clean", HelpText = "Clean away downloaded files from the cache")]
        public CleanOptions Clean { get; set; }

        [VerbOption("list-installs", HelpText = "List all known KSP installations")]
        public ListInstallsOptions ListInstalls { get; set; }

        [VerbOption("add-install", HelpText = "Add a new KSP installation")]
        public AddInstallOptions AddInstall { get; set; }

        [VerbOption("rename-install", HelpText = "Rename a known KSP installation")]
        public RenameInstallOptions RenameInstall { get; set; }

        [VerbOption("remove-install", HelpText = "Remove a known KSP installation")]
        public RemoveInstallOptions RemoveInstall { get; set; }

        [VerbOption("set-default-install", HelpText = "Sets a known KSP installation as default")]
        public SetDefaultInstallOptions SetDefaultInstall { get; set; }

        [VerbOption("clear-cache", HelpText = "Clears the download cache")]
        public ClearCacheOptions ClearCache { get; set; }

        [VerbOption("version", HelpText = "Show the version of the CKAN client being used.")]
        public VersionOptions Version { get; set; }
    }

    // Options common to all classes.

    internal class CommonOptions
    {
        [Option('v', "verbose", DefaultValue = false, HelpText = "Show more of what's going on when running.")]
        public bool Verbose { get; set; }

        [Option('d', "debug", DefaultValue = false, HelpText = "Show debugging level messages. Implies verbose")]
        public bool Debug { get; set; }

        [Option('k', "ksp", DefaultValue = null, HelpText = "KSP directory to use")]
        public string KSP { get; set; }
    }

    // Each action defines its own options that it supports.
    // Don't forget to cast to this type when you're processing them later on.

    internal class InstallOptions : CommonOptions
    {
        [Option('z', "zipfile", HelpText = "Zipfile to process")]
        public string zip_file { get; set; }

        [Option('c', "ckanfile", HelpText = "Local CKAN file to process")]
        public string ckan_file { get; set; }

        [Option("no-recommends", HelpText = "Do not install recommended modules")]
        public bool no_recommends { get; set; }

        [Option("with-suggests", HelpText = "Install suggested modules")]
        public bool with_suggests { get; set; }

        [Option("with-all-suggests", HelpText = "Install suggested modules all the way down")]
        public bool with_all_suggests { get; set; }

        // TODO: How do we provide helptext on this?
        [ValueList(typeof (List<string>))]
        public List<string> modules { get; set; }
    }

    internal class UpgradeOptions : CommonOptions
    {
        [Option('z', "zipfile", HelpText = "Zipfile to process")]
        public string zip_file { get; set; }

        [Option('c', "ckanfile", HelpText = "Local CKAN file to process")]
        public string ckan_file { get; set; }

        [Option("no-recommends", HelpText = "Do not install recommended modules")]
        public bool no_recommends { get; set; }

        [Option("with-suggests", HelpText = "Install suggested modules")]
        public bool with_suggests { get; set; }

        [Option("with-all-suggests", HelpText = "Install suggested modules all the way down")]
        public bool with_all_suggests { get; set; }

        // TODO: How do we provide helptext on this?
        [ValueList(typeof (List<string>))]
        public List<string> modules { get; set; }
    }

    internal class ScanOptions : CommonOptions
    {
    }

    internal class ListOptions : CommonOptions
    {
    }

    internal class VersionOptions : CommonOptions
    {
    }

    internal class CleanOptions : CommonOptions
    {
    }

    internal class AvailableOptions : CommonOptions
    {
    }

    internal class GuiOptions : CommonOptions
    {
    }

    internal class UpdateOptions : CommonOptions
    {
        // This option is really meant for devs testing their CKAN-meta forks.
        [Option('r', "repo", HelpText = "CKAN repository to use (experimental!)")]
        public string repo { get; set; }
    }

    internal class RemoveOptions : CommonOptions
    {
        [ValueOption(0)]
        public string Modname { get; set; }
    }

    internal class ShowOptions : CommonOptions
    {
        [ValueOption(0)]
        public string Modname { get; set; }
    }

    internal class ListInstallsOptions : CommonOptions
    {
    }

    internal class AddInstallOptions : CommonOptions
    {
        [ValueOption(0)]
        public string name { get; set; }

        [ValueOption(1)]
        public string path { get; set; }
    }

    internal class RenameInstallOptions : CommonOptions
    {
        [ValueOption(0)]
        public string old_name { get; set; }

        [ValueOption(1)]
        public string new_name { get; set; }
    }

    internal class RemoveInstallOptions : CommonOptions
    {
        [ValueOption(0)]
        public string name { get; set; }
    }

    internal class SetDefaultInstallOptions : CommonOptions
    {
        [ValueOption(0)]
        public string name { get; set; }
    }

    internal class ClearCacheOptions : CommonOptions
    {
    }

    // Exception class, so we can signal errors in command options.

    internal class BadCommandException : Exception
    {
        public BadCommandException(string message) : base(message)
        {
        }
    }
}