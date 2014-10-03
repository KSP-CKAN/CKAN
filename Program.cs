using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using CommandLine;

// Reference CKAN client
// Paul '@pjf' Fenwick
//
// License: CC-BY 4.0, LGPL, or MIT (your choice)

namespace CKAN {
	class MainClass {

		public const int EXIT_OK     = 0;
		public const int EXIT_ERROR  = 1;
		public const int EXIT_BADOPT = 2;

		public static int Main (string[] args) {

			Options cmdline;

			// If called with no arguments, the parser throws an exception.
			// TODO: It would be nice if we just *displayed* the help here,
			//       rather than asking the user to try --help.

			try {
				cmdline = new Options (args);
			}
			catch (NullReferenceException) {
				Console.WriteLine ("Try --help");
				return EXIT_BADOPT;
			}

			// Make sure we have a functional CKAN set up.
			KSP.init ();

			switch (cmdline.action) {
				case "install":
					return install ((InstallOptions) cmdline.options);
				
				case "scan":
					return scan ();

				case "list":
					return list ();

				case "show":
					return show ((ShowOptions) cmdline.options);

				default :
					Console.WriteLine ("Unknown command, try --help");
					return EXIT_BADOPT;

			}
		}

		public static int scan() {
			KSP.scanGameData();

			return EXIT_OK;
		}

		public static int list() {

			RegistryManager registry_manager = RegistryManager.Instance();
			Registry registry = registry_manager.registry;

			foreach (InstalledModule mod in registry.installed_modules.Values) {
				Console.WriteLine ("{0} {1}", mod.source_module.identifier, mod.source_module.version);
			}

			// Walk our dlls, but *don't* show anything we've already displayed as
			// a module.
			foreach (string dll in registry.installed_dlls.Keys) {
				if (! registry.installed_modules.ContainsKey(dll)) {
					Console.WriteLine ("{0} (detected dll)", dll);
				}
			}

			return EXIT_OK;

		}

		public static int install(InstallOptions options) { 

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

				installer.install (module, zipFilename);
				return EXIT_OK;
			}

			// Regular invocation, walk through all CKAN files on the cmdline

			foreach (string filename in options.Files) {
				CkanModule module = CkanModule.from_file (filename);
				ModuleInstaller installer = new ModuleInstaller ();
				installer.install (module);
			}

			Console.WriteLine ("\nDone!\n");

			return EXIT_OK;
		}

		static int show(ShowOptions options) {
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

		[VerbOption("install", HelpText = "Install a KSP mod")]
		public InstallOptions Install { get; set; }

		[VerbOption("scan", HelpText = "Scan for manually installed KSP mods")]
		public ScanOptions Scan { get; set; }

		[VerbOption("list", HelpText = "List installed modules")]
		public ListOptions List { get; set; }

		[VerbOption("show", HelpText = "Show information about a mod")]
		public ShowOptions Show { get; set; }
	
	}

	// Options common to all classes.

	abstract class CommonOptions {

		[Option('v', "verbose", DefaultValue = false, HelpText = "Show more of what's going on when running.")]
		public bool Verbose { get; set; }
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

	class ScanOptions : CommonOptions { }
	class ListOptions : CommonOptions { }

	class ShowOptions : CommonOptions {
		[ValueOption(0)]
		public string Modname { get; set; } 
	}

	// Exception class, so we can signal errors in command options.
	
	class BadCommandException : Exception {
		public BadCommandException(string message) : base(message) {}
	}

}
