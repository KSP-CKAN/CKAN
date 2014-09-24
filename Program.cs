using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;


// Reference CKAN client
// Paul '@pjf' Fenwick
//
// License: CC-BY 4.0, LGPL, or MIT (your choice)

namespace CKAN {
	class MainClass {
		public static void Main (string[] args) {

			if (args.Length == 0) {

				new ModuleDict();

				Console.WriteLine ("Usage: ckan [filenames]");
				return;
			}

			// TODO: Replace this with Mono.Options or NDesk.Options,
			// TODO: Less awful magic indexes!
			// if I can ever figure out how to install them!

			if (args [0] == "-f" && args.Length == 3) {
				string zipFilename  = args [1];
				string ckanFilename = args [2];

				Console.WriteLine ("Installing " + ckanFilename + " from " + zipFilename);
				// Aha! We've been called as ckan -f somefile.zip somefile.ckan
				Module module = Module.from_file (ckanFilename);

				Console.WriteLine ("Processing " + module._identifier);

				module.install (zipFilename);
				return;
			}

			// Regular invocation, walk through all CKAN files on the cmdline.

			string[] filenames = args;

			// Walk through all our files. :)
			foreach (string filename in filenames) {
				Module module = Module.from_file (filename);

				Console.WriteLine ("Processing " + 	module._identifier);

				module.install ();
			}
		}
	}
}
