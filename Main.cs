
// Reference CKAN client
// Paul '@pjf' Fenwick
//
// License: CC-BY 4.0, LGPL, or MIT (your choice)

namespace CKAN {

    using System;
    using System.IO;
    using System.Text;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Reflection;
    using CommandLine;
    using log4net;
    using log4net.Config;
    using log4net.Core;
    using CKAN;

    class MainClass {

        public const int EXIT_OK     = 0;
        public const int EXIT_ERROR  = 1;
        public const int EXIT_BADOPT = 2;

        private static readonly ILog log = LogManager.GetLogger(typeof(MainClass));

        public static int Main (string[] args) {

            BasicConfigurator.Configure ();
            LogManager.GetRepository ().Threshold = Level.Warn;
            log.Debug ("CKAN started");

            // If we're starting with no options, invoke the GUI instead.

            if (args.Length == 0) {
                return Gui ();
            }

            Options cmdline;

            try {
                cmdline = new Options (args);
            }
            catch (NullReferenceException) {
                // Oops, something went wrong. Generate the help screen instead!

                string[] help = { "--help" }; // Is there a nicer way than a temp var?
                new Options ( help );
                return EXIT_BADOPT;
            }

            // Process commandline options.

            CommonOptions options = (CommonOptions) cmdline.options;

            if (options.Debug) {
                LogManager.GetRepository ().Threshold = Level.Debug;
            } else if (options.Verbose) {
                LogManager.GetRepository ().Threshold = Level.Info;
            }

            // User provided KSP directory
            if (options.KSP != null) {
                try {
                    log.DebugFormat("Setting KSP directory to {0}", options.KSP);
                    KSP.SetGameDir (options.KSP);
                }
                catch (DirectoryNotFoundException) {
                    log.FatalFormat ("KSP not found in {0}", options.KSP);
                    User.WriteLine ("Error: {0} does not appear to be a KSP directory.", options.KSP);
                    return EXIT_BADOPT;
                }
            }

            // Find KSP, create CKAN dir, perform housekeeping.
            KSP.Init ();

            switch (cmdline.action) {

                case "gui":
                    return Gui();
                
                case "version":
                    return Version ();

                case "update":
                    return Update ((UpdateOptions) options);

                case "available":
                    return Available ();

                case "install":
                    return Install ((InstallOptions) cmdline.options);
                
                case "scan":
                    return Scan ();

                case "list":
                    return List ();

                case "show":
                    return Show ((ShowOptions) cmdline.options);

                case "remove":
                    return Remove ((RemoveOptions)cmdline.options);

                case "clean":
                    return Clean ();

                case "config":
                    return Config ((ConfigOptions) cmdline.options);

                default :
                    User.WriteLine ("Unknown command, try --help");
                    return EXIT_BADOPT;

            }
        }

        static int Gui() {

            // TODO: Sometimes when the GUI exits, we get a System.ArgumentException,
            // but trying to catch it here doesn't seem to help. Dunno why.

            GUI.Main ();

            return EXIT_OK;
        }

        static int Version() {

            User.WriteLine (Meta.Version ());

            return EXIT_OK;
        }

        static int Update(UpdateOptions options) {

            User.WriteLine ("Downloading updates...");

            int updated = Repo.Update (options.repo);

            User.WriteLine ("Updated information on {0} available modules", updated);

            return EXIT_OK;
        }

        static int Available() {
            List<CkanModule> available = RegistryManager.Instance().registry.Available();

            User.WriteLine ("Mods available for KSP {0}", KSP.Version());
            User.WriteLine ("");

            foreach (CkanModule module in available) {
                User.WriteLine("* {0}", module);
            }

            return EXIT_OK;
        }

        static int Scan() {
            KSP.ScanGameData();

            return EXIT_OK;
        }

        static int List() {

            string ksp_path = KSP.GameDir ();

            User.WriteLine ("\nKSP found at {0}\n", ksp_path);
            User.WriteLine ("KSP Version: {0}\n", KSP.Version ());

            RegistryManager registry_manager = RegistryManager.Instance();
            Registry registry = registry_manager.registry;

            User.WriteLine ("Installed Modules:\n");

            foreach (InstalledModule mod in registry.installed_modules.Values) {
                User.WriteLine ("* {0} {1}", mod.source_module.identifier, mod.source_module.version);
            }

            User.WriteLine ("\nDetected DLLs (`ckan scan` to rebuild):\n");

            // Walk our dlls, but *don't* show anything we've already displayed as
            // a module.
            foreach (string dll in registry.installed_dlls.Keys) {
                if (! registry.installed_modules.ContainsKey(dll)) {
                    User.WriteLine ("* {0}", dll);
                }
            }

            // Blank line at the end makes for nicer looking output.
            User.WriteLine ("");

            return EXIT_OK;

        }

        // Uninstalls a module, if it exists.
        static int Remove(RemoveOptions options) {

            ModuleInstaller installer = new ModuleInstaller ();
            installer.Uninstall (options.Modname, true);

            return EXIT_OK;
        }

        static int Clean() {
            KSP.CleanCache ();
            return EXIT_OK;
        }

        static int Install(InstallOptions options) { 

            if (options.zip_file == null && options.ckan_file == null) {
                // Typical case, install from cached CKAN info.

                if (options.modules.Count == 0) {
                    // What? No files specified?
                    User.WriteLine ("Usage: ckan install [--with-suggests] [--with-all-suggests] [--no-recommends] Mod [Mod2, ...]");
                    return EXIT_BADOPT;
                }

                // Prepare options. Can these all be done in the new() somehow?
                var install_ops = new RelationshipResolverOptions ( );
                install_ops.with_all_suggests =   options.with_all_suggests;
                install_ops.with_suggests     =   options.with_suggests;
                install_ops.with_recommends   = ! options.no_recommends;

                // Install everything requested. :)
                try {
                    ModuleInstaller installer = new ModuleInstaller ();
                    installer.InstallList (options.modules, install_ops);
                }
                catch (ModuleNotFoundException ex) {
                    User.WriteLine ("Module {0} required, but not listed in index.", ex.module);
                    User.WriteLine ("If you're lucky, you can do a `ckan update` and try again.");
                    return EXIT_ERROR;
                }

                User.WriteLine ("\nDone!\n");

                return EXIT_OK;
            }

            User.WriteLine("\nUnsupported option at this time.");

            return EXIT_BADOPT;
        }

        // TODO: We should have a command (probably this one) that shows
        // info about uninstalled modules.
        static int Show(ShowOptions options) {
            if (options.Modname == null)
            {
                // empty argument
                User.WriteLine("show <module> - module name argument missing, perhaps you forgot it?");
                return EXIT_BADOPT;
            }

            RegistryManager registry_manager = RegistryManager.Instance();
            InstalledModule module;

            try {
                module = registry_manager.registry.installed_modules [options.Modname];
            }
            catch (KeyNotFoundException) {
                User.WriteLine ("{0} not installed.", options.Modname);
                User.WriteLine ("Try `ckan list` to show installed modules");
                return EXIT_BADOPT;
            }

            // TODO: Print *lots* of information out; I should never have to dig through JSON

            User.WriteLine ("{0} version {1}", module.source_module.name, module.source_module.version);

            User.WriteLine ("\n== Files ==\n");

            Dictionary<string, InstalledModuleFile> files = module.installed_files;

            foreach (string file in files.Keys) {
                User.WriteLine (file);
            }

            return EXIT_OK;
        }

        static int Config(ConfigOptions options) {
            switch (options.option) {
                case "gamedir":
                    try {
                        KSP.PopulateGamedirRegistry (options.value);
                        return EXIT_OK;
                    }
                    catch (DirectoryNotFoundException) {
                        User.WriteLine ("Sorry, {0} doesn't look like a KSP dir", options.value);
                        return EXIT_BADOPT;
                    }

                default: 
                    User.WriteLine ("Unknown config option {0}", options.option);
                    return EXIT_BADOPT;
            }
        }
    }


    // Look, parsing options is so easy and beautiful I made
    // it into a special class for you to admire!

    class Options {
        public string action  { get; set; }
        public object options { get; set; }

        public Options( string[] args) {
            if (! CommandLine.Parser.Default.ParseArgumentsStrict (
                args, new Actions (), (verb, suboptions) => {
                    action = verb;
                    options = suboptions;
                }
            )) {
                throw(new BadCommandException("Try ckan --help"));
            }

            // If we're here, success!
        }
    }

    // Actions supported by our client go here.
    // TODO: Figure out how to do per action help screens.

    class Actions {
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

        [VerbOption("scan", HelpText = "Scan for manually installed KSP mods")]
        public ScanOptions Scan { get; set; }

        [VerbOption("list", HelpText = "List installed modules")]
        public ListOptions List { get; set; }

        [VerbOption("show", HelpText = "Show information about a mod")]
        public ShowOptions Show { get; set; }

        [VerbOption("clean", HelpText = "Clean away downloaded files from the cache")]
        public CleanOptions Clean { get; set; }

        [VerbOption("config", HelpText = "Configure CKAN")]
        public ConfigOptions Config { get; set; }

        [VerbOption("version", HelpText = "Show the version of the CKAN client being used.")]
        public VersionOptions Version { get; set; }
    
    }

    // Options common to all classes.

    class CommonOptions {

        [Option('v', "verbose", DefaultValue = false, HelpText = "Show more of what's going on when running.")]
        public bool Verbose { get; set; }

        [Option('d', "debug", DefaultValue = false, HelpText = "Show debugging level messages. Implies verbose")]
        public bool Debug { get; set; }

        [Option('k', "ksp", DefaultValue = null, HelpText = "KSP directory to use")]
        public string KSP { get; set; }
    }

    // Each action defines its own options that it supports.
    // Don't forget to cast to this type when you're processing them later on.

    class InstallOptions : CommonOptions {
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
        [ValueList(typeof(List<string>))]
        public List<string> modules { get; set; }
    }

    class ScanOptions      : CommonOptions { }
    class ListOptions      : CommonOptions { }
    class VersionOptions   : CommonOptions { }
    class CleanOptions     : CommonOptions { }
    class AvailableOptions : CommonOptions { }
    class GuiOptions       : CommonOptions { }

    class UpdateOptions    : CommonOptions {

        // This option is really meant for devs testing their CKAN-meta forks.
        [Option('r', "repo", HelpText = "CKAN repository to use (experimental!)")]
        public string repo { get; set; }
    }

    class RemoveOptions : CommonOptions {
        [ValueOption(0)]
        public string Modname { get; set; }
    }

    class ShowOptions : CommonOptions {
        [ValueOption(0)]
        public string Modname { get; set; } 
    }

    class ConfigOptions : CommonOptions {
        [ValueOption(0)]
        public string option { get; set; }

        [ValueOption(1)]
        public string value { get; set; }
    }

    // Exception class, so we can signal errors in command options.
    
    class BadCommandException : Exception {
        public BadCommandException(string message) : base(message) {}
    }

}
