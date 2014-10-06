
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

            // Find KSP, create CKAN dir, perform housekeeping.
            KSP.Init ();

            switch (cmdline.action) {

                case "version":
                    return Version ();

                case "update":
                    return Update ();

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

        static int Update() {

            Console.WriteLine ("Downloading updates...");

            int updated = CKAN.Update();

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

            string KspPath = KSP.GameDir ();

            Console.WriteLine ("\nKSP found at {0}\n", KspPath);

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

            // If we have a zipfile, use it.

            if (options.ZipFile != null) {
                // Aha! We've been called as ckan -f somefile.zip somefile.ckan

                if (options.Files.Count > 1) {
                    Console.WriteLine ("Only a single CKAN file can be provided when installing from zip");
                    return EXIT_BADOPT;
                }

                // TODO: Support installing from CKAN file embedded in zip.

                string zipFilename  = options.ZipFile;
                string ckanFilename = options.Files[0];

                Console.WriteLine ("Installing " + ckanFilename + " from " + zipFilename);

                CkanModule module = CkanModule.from_file (ckanFilename);
                ModuleInstaller installer = new ModuleInstaller ();

                installer.Install (module, zipFilename);
                return EXIT_OK;
            }

            // Regular invocation, walk through all CKAN files on the cmdline

            foreach (string filename in options.Files) {
                CkanModule module = CkanModule.from_file (filename);
                ModuleInstaller installer = new ModuleInstaller ();
                installer.Install (module);
            }

            Console.WriteLine ("\nDone!\n");

            return EXIT_OK;
        }

        static int Show(ShowOptions options) {
            RegistryManager registry_manager = RegistryManager.Instance();
            InstalledModule module = registry_manager.registry.installed_modules [options.Modname];

            if (module != null) {
                // TODO: Print *lots* of information out; I should never have to dig through JSON

                Console.WriteLine ("{0} version {1}", module.source_module.name, module.source_module.version);

                Console.WriteLine ("\n== Files ==\n");

                Dictionary<string, InstalledModuleFile> files = module.installed_files;

                foreach (string file in files.Keys) {
                    Console.WriteLine (file);
                }
            }
            return EXIT_OK;
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

        [VerbOption("version", HelpText = "Show the version of the CKAN client being used.")]
        public VersionOptions Version { get; set; }
    
    }

    // Options common to all classes.

    class CommonOptions {

        [Option('v', "verbose", DefaultValue = false, HelpText = "Show more of what's going on when running.")]
        public bool Verbose { get; set; }

        [Option('d', "debug", DefaultValue = false, HelpText = "Show debugging level messages. Implies verbose")]
        public bool Debug { get; set; }
    }

    // Each action defines its own options that it supports.
    // Don't forget to cast to this type when you're processing them later on.

    class InstallOptions : CommonOptions {
        [Option('f', "file", HelpText = "Zipfile to process")]
        public string ZipFile { get; set; }

        // TODO: How do we provide helptext on this?
        [ValueList(typeof(List<string>))]
        public List<string> Files { get; set; }
    }

    class ScanOptions      : CommonOptions { }
    class ListOptions      : CommonOptions { }
    class VersionOptions   : CommonOptions { }
    class CleanOptions     : CommonOptions { }
    class UpdateOptions    : CommonOptions { }
    class AvailableOptions : CommonOptions { }

    class RemoveOptions : CommonOptions {
        [ValueOption(0)]
        public string Modname { get; set; }
    }

    class ShowOptions : CommonOptions {
        [ValueOption(0)]
        public string Modname { get; set; } 
    }

    // Exception class, so we can signal errors in command options.
    
    class BadCommandException : Exception {
        public BadCommandException(string message) : base(message) {}
    }

}
