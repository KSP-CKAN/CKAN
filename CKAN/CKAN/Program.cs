using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO.Compression; // For ZipArchive

// Reference CKAN client
// Paul '@pjf' Fenwick
//
// License: CC-BY 4.0, LGPL, or MIT (your choice)

namespace CKAN {
	class MainClass {
		public static void Main (string[] args) {

			if (args.Length == 0) {
				Console.WriteLine ("Usage: ckan [filenames]");
				return;
			}

			string[] filenames = args;

			// Walk through all our files. :)
			foreach (string filename in filenames) {
				Module module = Module.from_file (filename);
				Console.WriteLine (module.identifier);

				string file = module.fetch ();
				Console.WriteLine ("Saved to " + file);
			}
		}
	}
}
