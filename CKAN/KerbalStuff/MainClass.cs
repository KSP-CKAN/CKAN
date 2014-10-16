// KerbalStuff to CKAN generator.

namespace CKAN.KerbalStuff {
    using System;
    using System.IO;
    using CKAN;
    using Newtonsoft.Json;
    using System.Text.RegularExpressions;
    using ICSharpCode.SharpZipLib.Zip;
    using log4net;
    using log4net.Config;
    using log4net.Core;
    using System.Net;

    class MainClass {
        private static readonly string kerbalstuff_url     = "https://kerbalstuff.com";
        private static readonly string kerbalstuff_mod_api = kerbalstuff_url + "/api/mod/";
        private static readonly ILog log = LogManager.GetLogger(typeof(MainClass));

        public static void Main (string[] args) {

            BasicConfigurator.Configure ();
            LogManager.GetRepository ().Threshold = Level.Debug;
            log.Debug ("KerbalStuff2CKAN started");

            foreach (string mod in args) {

                log.InfoFormat ("Processing {0}", mod);

                WebClient web = new WebClient ();

                string api_call = kerbalstuff_mod_api + mod;
                log.DebugFormat ("Calling {0}", api_call);

                string kerbalstuff_json = web.DownloadString (api_call);

                KSMod ks = KSMod.FromString (kerbalstuff_json);

                log.DebugFormat ("Mod name: {0}", ks.name);

                string download_url = kerbalstuff_url + ks.versions[0].download_path;

                log.DebugFormat ("Downloading {0}", download_url);

                string filename = Net.Download (download_url);

                log.Debug ("Downloaded.");

                ZipFile zipfile = new ZipFile (File.OpenRead (filename));

                foreach (ZipEntry entry in zipfile) {

                    // Skip everything but embedded .ckan files.
                    if (! Regex.IsMatch (entry.Name, ".CKAN$", RegexOptions.IgnoreCase)) {
                        continue;
                    }

                    log.DebugFormat ("Reading {0}", entry.Name);

                    Stream zipStream = zipfile.GetInputStream (entry);

                    dynamic meta_ckan = DeserializeFromStream (zipStream);

                    log.DebugFormat ("We're working with {0}", meta_ckan.identifier);
                }
            }
        }


        // Courtesy https://stackoverflow.com/questions/8157636/can-json-net-serialize-deserialize-to-from-a-stream/17788118#17788118 
        public static object DeserializeFromStream(Stream stream) {
            var serializer = new JsonSerializer();

            using (var sr = new StreamReader(stream)) {
                using (var jsonTextReader = new JsonTextReader(sr)) {
                    return serializer.Deserialize (jsonTextReader);
                }
            }
        }
    }

}
