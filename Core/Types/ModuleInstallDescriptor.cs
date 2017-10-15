using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System.IO;

namespace CKAN
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ModuleInstallDescriptor : ICloneable
    {

        #region Properties

        // Either file, find, or find_regexp is required, we check this manually at deserialise.
        [JsonProperty("file")]
        public string file;

        [JsonProperty("find")]
        public string find;

        [JsonProperty("find_regexp")]
        public string find_regexp;

        [JsonProperty("find_matches_files")]
        public bool find_matches_files;

        [JsonProperty("install_to", Required = Required.Always)]
        public string install_to;

        [JsonProperty("as")]
        public string @as;

        [JsonProperty("filter")]
        [JsonConverter(typeof (JsonSingleOrArrayConverter<string>))]
        public List<string> filter;

        [JsonProperty("filter_regexp")]
        [JsonConverter(typeof (JsonSingleOrArrayConverter<string>))]
        public List<string> filter_regexp;

        [OnDeserialized]
        internal void DeSerialisationFixes(StreamingContext like_i_could_care)
        {
            // Make sure our install_to fields exists. We may be able to remove
            // this check now that we're doing better json-fu above.
            if (install_to == null)
            {
                throw new BadMetadataKraken(null, "Install stanzas must have an install_to");
            }

            var setCount = new[] { file, find, find_regexp }.Count(i => i != null);

            // Make sure we have either a `file`, `find`, or `find_regexp` stanza.
            if (setCount == 0)
            {
                throw new BadMetadataKraken(null, "Install stanzas require either a file, find, or find_regexp directive");
            }

            if (setCount > 1)
            {
                throw new BadMetadataKraken(null, "Install stanzas must only include one of file, find, or find_regexp directives");
            }
        }

        #endregion

        #region Constructors and clones

        [JsonConstructor]
        private ModuleInstallDescriptor()
        {
        }

        /// <summary>
        /// Returns a deep clone of our object. Implements ICloneable.
        /// </summary>
        public object Clone()
        {
            // Deep clone our object by running it through a serialisation cycle.
            string json = JsonConvert.SerializeObject(this, Formatting.None);
            return JsonConvert.DeserializeObject<ModuleInstallDescriptor>(json);
        }

        /// <summary>
        /// Returns a default install stanza for the identifer provided.
        /// </summary>
        public static ModuleInstallDescriptor DefaultInstallStanza(string ident, ZipFile zipfile)
        {
            // Really this is just making a dummy `find` stanza and returning the processed
            // result.
            var stanza = new ModuleInstallDescriptor();
            stanza.install_to = "GameData";
            stanza.find = ident;
            return stanza.ConvertFindToFile(zipfile);
        }

        #endregion

        /// <summary>
        /// Returns true if the path provided should be installed by this stanza.
        /// Can *only* be used on `file` stanzas, throws an UnsupportedKraken if called
        /// on a `find` stanza.
        /// Use `ConvertFindToFile` to convert `find` to `file` stanzas.
        /// </summary>
        public bool IsWanted(string path)
        {
            if (file == null)
            {
                throw new UnsupportedKraken(".IsWanted only works with `file` stanzas.");
            }

            // Make sure our path always uses slashes we expect.
            string normalised_path = path.Replace('\\', '/');

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

            if (filter != null && filter.Any(filter_text => path_segments.Contains(filter_text.ToLower())))
            {
                return false;
            }

            if (filter_regexp == null)
            {
                return true;
            }

            // Finally, check our filter regexpes.
            return filter_regexp.All(regexp => !Regex.IsMatch(normalised_path, regexp));
        }

        /// <summary>
        /// Given a zipfile, returns a `file` ModuleInstallDescriptor that can be used for
        /// installation.
        /// Returns `this` if already of a `file` type.
        /// </summary>
        /// <param name="zipfile">Downloaded ZIP file containing the mod</param>
        public ModuleInstallDescriptor ConvertFindToFile(ZipFile zipfile)
        {
            // If we're already a file type stanza, then we have nothing to do.
            if (this.file != null)
                return this;

            // Match *only* things with our find string as a directory.
            // We can't just look for directories, because some zipfiles
            // don't include entries for directories, but still include entries
            // for the files they contain.
            Regex inst_filt = this.find != null
                ? new Regex(@"(?:^|/)" + Regex.Escape(this.find) + @"$", RegexOptions.IgnoreCase)
                : new Regex(this.find_regexp, RegexOptions.IgnoreCase);

            // Find the shortest directory path that matches our filter,
            // including all parent directories of all entries.
            string shortest = null;
            foreach (ZipEntry entry in zipfile)
            {
                bool is_file = !entry.IsDirectory;
                // Normalize path before searching (path separator as '/', no trailing separator)
                for (string path = Regex.Replace(entry.Name.Replace('\\', '/'), "/$", "");
                        !string.IsNullOrEmpty(path);
                        path = Path.GetDirectoryName(path).Replace('\\', '/'), is_file = false)
                {

                    // Skip file paths if not allowed
                    if (!find_matches_files && is_file)
                        continue;

                    // Is this a shorter matching path?
                    if ((string.IsNullOrEmpty(shortest) || path.Length < shortest.Length)
                            && inst_filt.IsMatch(path))
                        shortest = path;
                }
            }
            if (string.IsNullOrEmpty(shortest))
            {
                throw new FileNotFoundKraken(
                    this.find ?? this.find_regexp,
                    String.Format("Could not find {0} entry in zipfile to install", this.find ?? this.find_regexp)
                );
            }

            // Fill in our stanza, and remove our old `find` and `find_regexp` info.
            ModuleInstallDescriptor stanza = (ModuleInstallDescriptor) this.Clone();
            stanza.file        = shortest;
            stanza.find        = null;
            stanza.find_regexp = null;
            return stanza;
        }

        public string DescribeMatch()
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(file)) {
                sb.AppendFormat("file=\"{0}\"", file);
            }
            if (!string.IsNullOrEmpty(find)) {
                sb.AppendFormat("find=\"{0}\"", find);
            }
            if (!string.IsNullOrEmpty(find_regexp)) {
                sb.AppendFormat("find_regexp=\"{0}\"", find_regexp);
            }
            return sb.ToString();
        }
    }
}
