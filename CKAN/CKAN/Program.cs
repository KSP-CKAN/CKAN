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
			string filename = args [0];

			// We should be able to pass a stream through to our JSON parsers,
			// but heaven help me if I can find a method that accepts a
			// StreamReader argument.

			string json = System.IO.File.ReadAllText (filename);

			dynamic metadata = JsonConvert.DeserializeObject(json);

			Console.WriteLine ( metadata.identifier );
		}
	}
}
