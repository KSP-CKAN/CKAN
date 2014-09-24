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

		const int EXIT_OK     = 0;
		const int EXIT_ERROR  = 1;
		const int EXIT_BADOPT = 2;

		public static int Main (string[] args) {

			Options options = new Options ();

			if (! CommandLine.Parser.Default.ParseArgumentsStrict(args, options)) {
				Console.WriteLine("Usage: ckan [filenames]");
				return EXIT_BADOPT;
			}

			// If we have a zipfile, use it.

			if (options.ZipFile != null) {

				if (options.Files.Count > 1) {
					Console.WriteLine ("Only a single CKAN file can be provided when installing from zip");
					return EXIT_BADOPT;
				}

				// TODO: Support installing from CKAN file embedded in zip.

				string zipFilename  = options.ZipFile;
				string ckanFilename = options.Files[0];

				Console.WriteLine ("Installing " + ckanFilename + " from " + zipFilename);
				// Aha! We've been called as ckan -f somefile.zip somefile.ckan
				Module module = Module.from_file (ckanFilename);

				module.install (zipFilename);
				return EXIT_OK;
			}

			// Regular invocation, walk through all CKAN files on the cmdline

			foreach (string filename in options.Files) {
				Module module = Module.from_file (filename);

				module.install ();
			}

			Console.WriteLine ("\nDone!\n");

			return EXIT_OK;
		}
	}

	/// <summary>
	/// The Options helper class defines what commandline options we can take.
	/// See https://github.com/gsscoder/commandline/issues/50 for a discussion on how
	/// these can be set up.
	/// </summary>

	class Options {
		[Option('v', "verbose", DefaultValue = false, HelpText = "Show more of what's going on when running.")]
		public bool Verbose { get; set; }

		[Option('f', "file", HelpText = "Zipfile to process")]
		public string ZipFile { get; set; }

		// TODO: How do we provide helptext on this?
		[ValueList(typeof(List<string>))]
		public List<string> Files { get; set; }
	}

}
