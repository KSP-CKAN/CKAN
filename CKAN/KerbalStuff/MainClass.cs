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
        private static readonly ILog log = LogManager.GetLogger(typeof(MainClass));
       
        public static void Main (string[] args) {

            BasicConfigurator.Configure ();
            LogManager.GetRepository ().Threshold = Level.Debug;
            log.Debug ("KerbalStuff2CKAN started");

            string identifier = args[0];
            int ksid = Convert.ToInt32(args[1]);

            log.InfoFormat ("Processing {0}", identifier);

            KSMod ks = KSAPI.Mod(ksid);
            CKAN.Version version = ks.versions[0].friendly_version;

            log.DebugFormat ("Mod: {0} {1}", ks.name, version);

            string filename = ks.versions[0].Download(identifier);

            ZipFile zipfile = new ZipFile (File.OpenRead (filename));

            foreach (ZipEntry entry in zipfile) {

                // Skip everything but embedded .ckan files.
                if (! Regex.IsMatch (entry.Name, ".CKAN$", RegexOptions.IgnoreCase)) {
                    continue;
                }

                log.DebugFormat ("Reading {0}", entry.Name);

                Stream zipStream = zipfile.GetInputStream (entry);

                JObject meta_ckan = DeserializeFromStream (zipStream);

                log.DebugFormat ("We're working with {0} {1}", meta_ckan.GetValue("identifier"), meta_ckan.GetValue("$ref"));

                var propertyInfo = meta_ckan.GetType ();
                // var value = propertyInfo.GetValue(meta_ckan, null);

                log.DebugFormat ("Reference is {0}", propertyInfo);
            }
        }

        // Courtesy https://stackoverflow.com/questions/8157636/can-json-net-serialize-deserialize-to-from-a-stream/17788118#17788118 
        private static JObject DeserializeFromStream(Stream stream) {
            // var serializer = new JsonSerializer();

            using (var sr = new StreamReader(stream)) {
                using (var jsonTextReader = new JsonTextReader(sr)) {
                    return (JObject) JObject.ReadFrom(jsonTextReader);
                    // return serializer.Deserialize (jsonTextReader);
                }
            }
        }
    }

}
