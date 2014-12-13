using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace CKAN
{
    public class ModuleInstallDescriptor
    {
        public /* required */ string file;
        public /* required */ string install_to;

        [JsonConverter(typeof (JsonSingleOrArrayConverter<string>))]
        public List<string> filter;

        [JsonConverter(typeof (JsonSingleOrArrayConverter<string>))]
        public List<string> filter_regexp;

        [OnDeserialized]
        internal void DeSerialisationFixes(StreamingContext like_i_could_care)
        {
            // Make sure our required fields exist.
            if (file == null || install_to == null)
            {
                throw new BadMetadataKraken(null, "Install stanzas must have a file and install_to");
            }
        }

        /// <summary>
        /// Returns true if the path provided should be installed by this stanza.
        /// </summary>
        public bool IsWanted(string path)
        {
            // Make sure our path always uses slashes we expect.
            string normalised_path = path.Replace('\\', '/');

            // Make sure our internal state is consistent. Is there a better way of doing this?
            filter = filter ?? new List<string> ();
            filter_regexp = filter_regexp ?? new List<string> ();

            // We want everthing that matches our 'file', either as an exact match,
            // or as a path leading up to it.
            string wanted_filter = "^" + Regex.Escape(file) + "(/|$)";

            // If it doesn't match our install path, ignore it.
            if (! Regex.IsMatch(normalised_path, wanted_filter))
            {
                return false;
            }

            // Skip the file if it's a ckan file, these should never be copied to GameData.
            if (Regex.IsMatch(normalised_path, ".ckan$", RegexOptions.IgnoreCase))
            {
                return false;
            }

            // Get all our path segments. If our filter matches of any them, skip.
            // All these comparisons are case insensitive.
            var path_segments = new List<string>(normalised_path.ToLower().Split('/'));

            foreach (string filter_text in filter)
            {
                if (path_segments.Contains(filter_text.ToLower()))
                {
                    return false;
                }
            }

            // Finally, check our filter regexpes.
            foreach (string regexp in filter_regexp)
            {
                if (Regex.IsMatch(normalised_path, regexp))
                {
                    return false;
                }
            }

            // I guess we want this file after all. ;)
            return true;
        }
    }
}