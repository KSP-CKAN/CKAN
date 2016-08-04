using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

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
        [JsonConverter(typeof(JsonSingleOrArrayConverter<string>))]
        public List<string> filter;

        [JsonProperty("filter_regexp")]
        [JsonConverter(typeof(JsonSingleOrArrayConverter<string>))]
        public List<string> filter_regexp;

        [OnDeserialized]
        internal void DeSerialisationFixes(StreamingContext like_i_could_care)
        {
            // Make sure our install_to fields exists. We may be able to remove
            // this check now that we're doing better json-fu above.
            if (install_to == null)
            {
                throw new BadMetadataKraken(null, "Install stanzas must have a file an install_to");
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

        #endregion Properties

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

        #endregion Constructors and clones

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
            if (!Regex.IsMatch(normalised_path, wanted_filter))
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
        public ModuleInstallDescriptor ConvertFindToFile(ZipFile zipfile)
        {
            // If we're already a file type stanza, then we have nothing to do.
            if (this.file != null)
            {
                return this;
            }

            var stanza = (ModuleInstallDescriptor)this.Clone();

            // Candidate top-level directories.
            var candidate_set = new HashSet<string>();

            // Match *only* things with our find string as a directory.
            // We can't just look for directories, because some zipfiles
            // don't include entries for directories, but still include entries
            // for the files they contain.
            string filter;
            if (this.find != null)
            {
                filter = @"(?:^|/)" + Regex.Escape(this.find) + @"$";
            }
            else
            {
                filter = this.find_regexp;
            }

            // Let's find that directory

            // Normalise our path.
            var normalised = zipfile
                .Cast<ZipEntry>()
                .Select(entry => find_matches_files ? entry.Name : Path.GetDirectoryName(entry.Name))
                .Select(entry =>
                {
                    var dir = entry.Replace('\\', '/');
                    return Regex.Replace(dir, "/$", "");
                });

            // If this looks like what we're after, remember it.
            var directories = normalised.Where(entry => Regex.IsMatch(entry, filter, RegexOptions.IgnoreCase));
            candidate_set.UnionWith(directories);

            // Sort to have shortest first. It's not *quite* top-level directory order,
            // but it's good enough for now.
            var candidates = new List<string>(candidate_set);
            candidates.Sort((a, b) => a.Length.CompareTo(b.Length));

            if (candidates.Count == 0)
            {
                throw new FileNotFoundKraken(
                    this.find ?? this.find_regexp,
                    String.Format("Could not find {0} entry in zipfile to install", this.find ?? this.find_regexp)
                );
            }

            // Fill in our stanza, and remove our old `find` and `find_regexp` info.
            stanza.file = candidates[0];
            stanza.find = null;
            stanza.find_regexp = null;
            return stanza;
        }
    }
}