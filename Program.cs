
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

    class MainClass {

        public const int EXIT_OK     = 0;
        public const int EXIT_ERROR  = 1;
        public const int EXIT_BADOPT = 2;

        private static readonly ILog log = LogManager.GetLogger(typeof(MainClass));

        public static int Main (string[] args) {

            BasicConfigurator.Configure ();
            LogManager.GetRepository ().Threshold = Level.Warn;
            log.Debug ("CKAN started");

            Options cmdline;

            // If called with no arguments, the parser throws an exception.
            // TODO: It would be nice if we just *displayed* the help here,
            //       rather than asking the user to try --help.

            try {
                cmdline = new Options (args);
            }
            catch (NullReferenceException) {
                Console.WriteLine ("Try ckan --help");
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
                    Console.WriteLine ("Error: {0} does not appear to be a KSP directory.", options.KSP);
                    return EXIT_BADOPT;
                }
            }

            // Find KSP, create CKAN dir, perform housekeeping.
            KSP.Init ();

            switch (cmdline.action) {

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
                    Console.WriteLine ("Unknown command, try --help");
                    return EXIT_BADOPT;

            }
        }

        static int Version() {

            // SeriouslyLongestClassNamesEverThanksMicrosoft
            AssemblyInformationalVersionAttribute[] assemblies = (AssemblyInformationalVersionAttribute[]) Assembly.GetAssembly (typeof(MainClass)).GetCustomAttributes (typeof(AssemblyInformationalVersionAttribute), false);

            if (assemblies.Length == 0 || assemblies[0].InformationalVersion == null) {
                // Dunno the version. Some dev probably built it. 
                Console.WriteLine ("development");
            } else {
                Console.WriteLine (assemblies[0].InformationalVersion);
            }

            return EXIT_OK;
        }

        static int Update(UpdateOptions options) {

            Console.WriteLine ("Downloading updates...");

            int updated = CKAN.Update(options.repo);

            Console.WriteLine ("Updated information on {0} available modules", updated);

            return EXIT_OK;
        }

        static int Available() {
            string[] available = RegistryManager.Instance().registry.Available();

            foreach (string module in available) {
                Console.WriteLine("* {0}", module);
            }

            return EXIT_OK;
        }

        static int Scan() {
            KSP.ScanGameData();

            return EXIT_OK;
        }

        static int List() {

            string ksp_path = KSP.GameDir ();

            Console.WriteLine ("\nKSP found at {0}\n", ksp_path);
            Console.WriteLine ("KSP Version: {0}\n", KSP.Version ());

            RegistryManager registry_manager = RegistryManager.Instance();
            Registry registry = registry_manager.registry;

            Console.WriteLine ("Installed Modules:\n");

            foreach (InstalledModule mod in registry.installed_modules.Values) {
                Console.WriteLine ("* {0} {1}", mod.source_module.identifier, mod.source_module.version);
            }

            Console.WriteLine ("\nDetected DLLs (`ckan scan` to rebuild):\n");

            // Walk our dlls, but *don't* show anything we've already displayed as
            // a module.
            foreach (string dll in registry.installed_dlls.Keys) {
                if (! registry.installed_modules.ContainsKey(dll)) {
                    Console.WriteLine ("* {0}", dll);
                }
            }

            // Blank line at the end makes for nicer looking output.
            Console.WriteLine ("");

            return EXIT_OK;

        }

        // Uninstalls a module, if it exists.
        static int Remove(RemoveOptions options) {

            ModuleInstaller installer = new ModuleInstaller ();
            installer.Uninstall (options.Modname);

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
                    Console.WriteLine ("Usage: ckan install [-z zipfile] [-c ckanfile] Mod [Mod2, ...]");
                    return EXIT_BADOPT;
                }

                Registry registry = RegistryManager.Instance ().registry;

                // Install everything requested. :)

                foreach (string module_name in options.modules) {
                    CkanModule module = registry.LatestAvailable (module_name);

                    // TODO: Do we *need* a new module installer each iteration?
                    ModuleInstaller installer = new ModuleInstaller ();
                    installer.Install (module);
                }

                Console.WriteLine ("\nDone!\n");

                return EXIT_OK;
            }

            Console.WriteLine("\nUnsupported option at this time.");

            return EXIT_BADOPT;
        }

        // TODO: We should have a command (probably this one) that shows
        // info about uninstalled modules.
        static int Show(ShowOptions options) {
            RegistryManager registry_manager = RegistryManager.Instance();
            InstalledModule module;

            try {
                module = registry_manager.registry.installed_modules [options.Modname];
            }
            catch (KeyNotFoundException) {
                Console.WriteLine ("{0} not installed.", options.Modname);
                Console.WriteLine ("Try `ckan list` to show installed modules");
                return EXIT_BADOPT;
            }

            // TODO: Print *lots* of information out; I should never have to dig through JSON

            Console.WriteLine ("{0} version {1}", module.source_module.name, module.source_module.version);

            Console.WriteLine ("\n== Files ==\n");

            Dictionary<string, InstalledModuleFile> files = module.installed_files;

            foreach (string file in files.Keys) {
                Console.WriteLine (file);
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
                        Console.WriteLine ("Sorry, {0} doesn't look like a KSP dir", options.value);
                        return EXIT_BADOPT;
                    }

                default: 
                    Console.WriteLine ("Unknown config option {0}", options.option);
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

        // TODO: How do we provide helptext on this?
        [ValueList(typeof(List<string>))]
        public List<string> modules { get; set; }
    }

    class ScanOptions      : CommonOptions { }
    class ListOptions      : CommonOptions { }
    class VersionOptions   : CommonOptions { }
    class CleanOptions     : CommonOptions { }
    class AvailableOptions : CommonOptions { }

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
