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
		public static void Main (string[] args) {

			Options options = new Options ();

			if (! CommandLine.Parser.Default.ParseArgumentsStrict(args, options)) {
				Console.WriteLine("Usage: ckan [filenames]");
				return;
			}

			// TODO: Replace this with Mono.Options or NDesk.Options,
			// TODO: Less awful magic indexes!
			// if I can ever figure out how to install them!

			if (options.ZipFile != null) {
				string zipFilename  = options.ZipFile;
				string ckanFilename = options.Files[0];

				Console.WriteLine ("Installing " + ckanFilename + " from " + zipFilename);
				// Aha! We've been called as ckan -f somefile.zip somefile.ckan
				Module module = Module.from_file (ckanFilename);

				Console.WriteLine ("Processing " + module._identifier);

				module.install (zipFilename);
				return;
			}

			// Regular invocation, walk through all CKAN files on the cmdline

			// TODO: How on earth do we get *all* the filenames using the Cmdline
			// library? Where's my Getopt::Std?

			// string[] filenames = { options.File };

			// Walk through all our files. :)
			foreach (string filename in options.Files) {
				Module module = Module.from_file (filename);

				Console.WriteLine ("Processing " + 	module._identifier);

				module.install ();
			}
		}
	}

	class Options {
		[Option('v', "verbose", DefaultValue = false, HelpText = "Show more of what's going on when running.")]
		public bool Verbose { get; set; }

		[Option('f', "file", HelpText = "Zipfile to process")]
		public string ZipFile { get; set; }

		[ValueList(typeof(List<string>))]
		public List<string> Files { get; set; }
	}

}
