// KerbalStuff to CKAN generator.

namespace CKAN.KerbalStuff {
    using System;
    using System.IO;
    using CKAN;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Text.RegularExpressions;
    using ICSharpCode.SharpZipLib.Zip;
    using log4net;
    using log4net.Config;
    using log4net.Core;
    using System.Net;

    class MainClass {
        private static readonly int EXIT_OK = 0;
        private static readonly int EXIT_BADOPT = 1;
        private static readonly ILog log = LogManager.GetLogger(typeof(MainClass));
        private static readonly string expand_token = "$kref"; // See #70 for naming reasons
        private static readonly string ks_expand_path = "#/ckan/kerbalstuff";
       
        public static int Main (string[] args) {

            BasicConfigurator.Configure ();
            LogManager.GetRepository ().Threshold = Level.Debug;
            log.Debug ("KerbalStuff2CKAN started");

            if (args.Length < 2)
            {
                User.WriteLine("I need an identifier and a KSID.");
                return EXIT_BADOPT;
            }

            string identifier = args[0];
            int ksid = Convert.ToInt32(args[1]);

            log.InfoFormat ("Processing {0}", identifier);

            KSMod ks = KSAPI.Mod(ksid);
            CKAN.Version version = ks.versions[0].friendly_version;

            log.DebugFormat ("Mod: {0} {1}", ks.name, version);

            KSVersion latest = ks.versions[0];
            string filename = latest.Download(identifier);

            JObject metadata = ExtractCkanInfo(filename);

            // Check if we should auto-inflate!
            if ((string) metadata[expand_token] == ks_expand_path)
            {
                log.InfoFormat("Inflating...");
                ks.InflateMetadata(metadata, latest);
                metadata.Remove(expand_token);
            }

            Console.WriteLine(metadata.ToString());

            return EXIT_OK;
        }

        // Courtesy https://stackoverflow.com/questions/8157636/can-json-net-serialize-deserialize-to-from-a-stream/17788118#17788118 
        private static JObject DeserializeFromStream(Stream stream) {
            // var serializer = new JsonSerializer();

            using (var sr = new StreamReader(stream)) {
                using (var jsonTextReader = new JsonTextReader(sr)) {
                    return (JObject) JObject.ReadFrom(jsonTextReader);
                }
            }
        }

        /// <summary>
        /// Given a zip filename, finds and returns the first embedded .ckan file it finds.
        /// </summary>
        private static JObject ExtractCkanInfo(string filename)
        {
            ZipFile zipfile = new ZipFile (File.OpenRead (filename));

            foreach (ZipEntry entry in zipfile) {

                // Skip everything but embedded .ckan files.
                if (! Regex.IsMatch (entry.Name, ".CKAN$", RegexOptions.IgnoreCase))
                {
                    continue;
                }

                log.DebugFormat ("Reading {0}", entry.Name);

                Stream zipStream = zipfile.GetInputStream (entry);

                JObject meta_ckan = DeserializeFromStream (zipStream);

                return meta_ckan;
            }

            throw new MetadataNotFoundKraken(filename);
        }
    }

    class MetadataNotFoundKraken : Exception
    {
        public string filename = null;

        public MetadataNotFoundKraken(string filename)
        {
            this.filename = filename;   // Is there a better way?
        }
    }
}
