using System;
using System.IO;
using System.Linq;
using log4net;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN
{
    public class KSMod : CkanInflator
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (KSMod));
        public int id; // KSID

        // These get filled in from JSON deserialisation.
        public string license;
        public string name;
        public string short_description;
        public string author;
        public KSVersion[] versions;
        public Uri website;
        public int default_version_id;

        public override string ToString()
        {
            return string.Format("{0}", name);
        }

        /// <summary>
        ///     Takes a JObject and inflates it with KS metadata.
        ///     This will not overwrite fields that already exist.
        /// </summary>
        override public void InflateMetadata(JObject metadata, string filename, object context)
        {
            var version = (KSVersion)context;

            log.DebugFormat("Inflating {0}", metadata["identifier"]);

            // Check how big our file is
            long download_size = (new FileInfo (filename)).Length;

            // Make sure resources exist.
            if (metadata["resources"] == null)
            {
                metadata["resources"] = new JObject();
            }

            // Only pre-fill version info if there's none already. GH #199
            if ((string) metadata["ksp_version_min"] == null && (string) metadata["ksp_version_max"] == null)
            {
                // Inflate won't overwrite an existing key, so we don't need to check
                // for ksp_version itself. :)
                log.Debug("Pre-filling KSP version field");
                Inflate(metadata, "ksp_version", version.KSP_version.ToString());
            }

            Inflate(metadata, "name", name);
            Inflate(metadata, "license", license);
            Inflate(metadata, "abstract", short_description);
            Inflate(metadata, "author", author);
            Inflate(metadata, "version", version.friendly_version.ToString());
            Inflate(metadata, "download", Escape(version.download_path));
            Inflate(metadata, "x_generated_by", "netkan");
            Inflate(metadata, "download_size", download_size);

            if (website != null)
            {
                Inflate((JObject)metadata["resources"], "homepage", Escape(website));
            }

            Inflate((JObject) metadata["resources"], "kerbalstuff", Escape(KSHome()));
        }

        internal KSVersion Latest()
        {
            // The version we want is specified by `default_version_id`, it's not just
            // the latest. See GH #214. Thanks to @Starstrider42 for spotting this.

            var latest =
                from release in versions
                where release.id == default_version_id
                select release
            ;

            // There should only ever be one.
            return latest.First();
        }

        /// <summary>
        /// Provide an escaped version of the given URL, including converting
        /// square brackets to their escaped forms.
        /// </summary>
        private static string Escape(Uri url)
        {
            if (url == null)
            {
                return null;
            }

            string escaped = Uri.EscapeUriString(url.ToString());

            // Square brackets are "reserved characters" that should not appear
            // in strings to begin with, so C# doesn't try to escape them in case
            // they're being used in a special way. They're not; some mod authors
            // just have crazy ideas as to what should be in a URL, and KS doesn't
            // escape them in its API. There's probably more in RFC 3986.

            escaped = escaped.Replace("[",Uri.HexEscape('['));
            escaped = escaped.Replace("]",Uri.HexEscape(']'));

            return escaped;
        }
        
        /// <summary>
        /// Returns the path to the mod's home on KerbalStuff
        /// </summary>
        /// <returns>The home.</returns>
        internal Uri KSHome()
        {
            return KSAPI.ExpandPath(String.Format("/mod/{0}/{1}", id, name));
        }

    }
}
