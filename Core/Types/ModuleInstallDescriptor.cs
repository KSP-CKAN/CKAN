using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Reflection;

using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;

using CKAN.Games;

[assembly: InternalsVisibleTo("CKAN.Tests")]

namespace CKAN
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ModuleInstallDescriptor : ICloneable, IEquatable<ModuleInstallDescriptor>
    {

        #region Properties

        // Either file, find, or find_regexp is required, we check this manually at deserialise.
        [JsonProperty("file", NullValueHandling = NullValueHandling.Ignore)]
        public string file;

        [JsonProperty("find", NullValueHandling = NullValueHandling.Ignore)]
        public string find;

        [JsonProperty("find_regexp", NullValueHandling = NullValueHandling.Ignore)]
        public string find_regexp;

        [JsonProperty("find_matches_files", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        public bool find_matches_files;

        [JsonProperty("install_to", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue("GameData")]
        public string install_to;

        [JsonProperty("as", NullValueHandling = NullValueHandling.Ignore)]
        public string @as;

        [JsonProperty("filter", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonSingleOrArrayConverter<string>))]
        public List<string> filter;

        [JsonProperty("filter_regexp", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonSingleOrArrayConverter<string>))]
        public List<string> filter_regexp;

        [JsonProperty("include_only", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonSingleOrArrayConverter<string>))]
        public List<string> include_only;

        [JsonProperty("include_only_regexp", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonSingleOrArrayConverter<string>))]
        public List<string> include_only_regexp;

        [JsonIgnore]
        private Regex inst_pattern = null;

        private static readonly Regex trailingSlashPattern = new Regex("/$",
            RegexOptions.Compiled);

        [OnDeserialized]
        internal void DeSerialisationFixes(StreamingContext like_i_could_care)
        {
            // Make sure our install_to fields exists. We may be able to remove
            // this check now that we're doing better json-fu above.
            if (install_to == null)
            {
                throw new BadMetadataKraken(null, Properties.Resources.ModuleInstallDescriptorMustHaveInstallTo);
            }

            var setCount = new[] { file, find, find_regexp }.Count(i => i != null);

            // Make sure we have either a `file`, `find`, or `find_regexp` stanza.
            if (setCount == 0)
            {
                throw new BadMetadataKraken(null, Properties.Resources.ModuleInstallDescriptorRequireFileFind);
            }

            if (setCount > 1)
            {
                throw new BadMetadataKraken(null, Properties.Resources.ModuleInstallDescriptorTooManyFileFind);
            }

            // Make sure only filter or include_only fields exist but not both at the same time
            var filterCount = new[] { filter, filter_regexp }.Count(i => i != null);
            var includeOnlyCount = new[] { include_only, include_only_regexp }.Count(i => i != null);

            if (filterCount > 0 && includeOnlyCount > 0)
            {
                throw new BadMetadataKraken(null, Properties.Resources.ModuleInstallDescriptorTooManyFilterInclude);
            }

            // Normalize paths on load (note, doesn't cover assignment like in tests)
            install_to = CKANPathUtils.NormalizePath(install_to);
        }

        #endregion

        #region Constructors and clones

        [JsonConstructor]
        private ModuleInstallDescriptor()
        {
            install_to = typeof(ModuleInstallDescriptor).GetTypeInfo()
                                                        .GetDeclaredField("install_to")
                                                        .GetCustomAttribute<DefaultValueAttribute>()
                                                        .Value
                                                        .ToString();
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
        /// Compare two install stanzas
        /// </summary>
        /// <param name="other">The other stanza for comparison</param>
        /// <returns>
        /// True if they're equivalent, false if they're different.
        /// </returns>
        public override bool Equals(object other)
        {
            return Equals(other as ModuleInstallDescriptor);
        }

        /// <summary>
        /// Compare two install stanzas
        /// </summary>
        /// <param name="other">The other stanza for comparison</param>
        /// <returns>
        /// True if they're equivalent, false if they're different.
        /// IEquatable<> uses this for more efficient comparisons.
        /// </returns>
        public bool Equals(ModuleInstallDescriptor otherStanza)
        {
            if (otherStanza == null)
            {
                // Not even the right type!
                return false;
            }

            if (CKANPathUtils.NormalizePath(file) != CKANPathUtils.NormalizePath(otherStanza.file))
            {
                return false;
            }

            if (CKANPathUtils.NormalizePath(find) != CKANPathUtils.NormalizePath(otherStanza.find))
            {
                return false;
            }

            if (find_regexp != otherStanza.find_regexp)
            {
                return false;
            }

            if (CKANPathUtils.NormalizePath(install_to) != CKANPathUtils.NormalizePath(otherStanza.install_to))
            {
                return false;
            }

            if (@as != otherStanza.@as)
            {
                return false;
            }

            if ((filter == null) != (otherStanza.filter == null))
            {
                return false;
            }

            if (filter != null
                && !filter.SequenceEqual(otherStanza.filter))
            {
                return false;
            }

            if ((filter_regexp == null) != (otherStanza.filter_regexp == null))
            {
                return false;
            }

            if (filter_regexp != null
                && !filter_regexp.SequenceEqual(otherStanza.filter_regexp))
            {
                return false;
            }

            if (find_matches_files != otherStanza.find_matches_files)
            {
                return false;
            }

            if ((include_only == null) != (otherStanza.include_only == null))
            {
                return false;
            }

            if (include_only != null
                && !include_only.SequenceEqual(otherStanza.include_only))
            {
                return false;
            }

            if ((include_only_regexp == null) != (otherStanza.include_only_regexp == null))
            {
                return false;
            }

            if (include_only_regexp != null
                && !include_only_regexp.SequenceEqual(otherStanza.include_only_regexp))
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            // Tuple.Create only handles up to 8 params, we have 10+
            return Tuple.Create(
                Tuple.Create(
                    file,
                    find,
                    find_regexp,
                    find_matches_files,
                    install_to,
                    @as
                ),
                Tuple.Create(
                    filter,
                    filter_regexp,
                    include_only,
                    include_only_regexp
                )
            ).GetHashCode();
        }

        /// <summary>
        /// Returns a default install stanza for the identifier provided.
        /// </summary>
        /// <returns>
        /// { "find": "ident", "install_to": "GameData" }
        /// </returns>
        public static ModuleInstallDescriptor DefaultInstallStanza(IGame game, string ident)
        {
            return new ModuleInstallDescriptor()
            {
                find       = ident,
                install_to = game.PrimaryModDirectoryRelative,
            };
        }

        #endregion

        private void EnsurePattern()
        {
            if (inst_pattern == null)
            {
                if (file != null)
                {
                    file = CKANPathUtils.NormalizePath(file);
                    inst_pattern = new Regex(@"^" + Regex.Escape(file) + @"(/|$)",
                        RegexOptions.IgnoreCase | RegexOptions.Compiled);
                }
                else if (find != null)
                {
                    find = CKANPathUtils.NormalizePath(find);
                    inst_pattern = new Regex(@"(?:^|/)" + Regex.Escape(find) + @"(/|$)",
                        RegexOptions.IgnoreCase | RegexOptions.Compiled);
                }
                else if (find_regexp != null)
                {
                    inst_pattern = new Regex(find_regexp,
                        RegexOptions.IgnoreCase | RegexOptions.Compiled);
                }
                else
                {
                    throw new Kraken(Properties.Resources.ModuleInstallDescriptorRequireFileFind);
                }
            }
        }

        /// <summary>
        /// Returns true if the path provided should be installed by this stanza.
        /// Can *only* be used on `file` stanzas, throws an UnsupportedKraken if called
        /// on a `find` stanza.
        /// Use `ConvertFindToFile` to convert `find` to `file` stanzas.
        /// </summary>
        private bool IsWanted(string path, int? matchWhere)
        {
            EnsurePattern();

            // Make sure our path always uses slashes we expect.
            string normalised_path = path.Replace('\\', '/');

            var match = inst_pattern.Match(normalised_path);
            if (!match.Success)
            {
                // Doesn't match our install pattern, ignore it
                return false;
            }
            else if (matchWhere.HasValue && match.Index != matchWhere.Value)
            {
                // Matches too late in the string, not our folder
                return false;
            }

            // Get all our path segments. If our filter matches of any them, skip.
            // All these comparisons are case insensitive.
            var path_segments = new List<string>(normalised_path.ToLower().Split('/'));

            if (filter != null && filter.Any(filter_text => path_segments.Contains(filter_text.ToLower())))
            {
                return false;
            }

            if (filter_regexp != null && filter_regexp.Any(regexp => Regex.IsMatch(normalised_path, regexp)))
            {
                return false;
            }

            if (include_only != null && include_only.Any(text => path_segments.Contains(text.ToLower())))
            {
                return true;
            }

            if (include_only_regexp != null && include_only_regexp.Any(regexp => Regex.IsMatch(normalised_path, regexp)))
            {
                return true;
            }

            return include_only == null && include_only_regexp == null;
        }

        /// <summary>
        /// Given an open zipfile, returns all files that would be installed
        /// for this stanza.
        ///
        /// If a KSP instance is provided, it will be used to generate output paths, otherwise these will be null.
        ///
        /// Throws a BadInstallLocationKraken if the install stanza targets an
        /// unknown install location (eg: not GameData, Ships, etc)
        ///
        /// Throws a BadMetadataKraken if the stanza resulted in no files being returned.
        /// </summary>
        /// <exception cref="BadInstallLocationKraken">Thrown when the installation path is not valid according to the spec.</exception>
        public List<InstallableFile> FindInstallableFiles(ZipFile zipfile, GameInstance ksp)
        {
            string installDir;
            var files = new List<InstallableFile>();

            // Normalize the path before doing everything else
            string install_to = CKANPathUtils.NormalizePath(this.install_to);

            // The installation path cannot contain updirs
            if (install_to.Contains("/../") || install_to.EndsWith("/.."))
            {
                throw new BadInstallLocationKraken(string.Format(
                    Properties.Resources.ModuleInstallDescriptorInvalidInstallPath, install_to));
            }

            if (ksp == null)
            {
                installDir = null;
            }
            else if (install_to == ksp.game.PrimaryModDirectoryRelative
                || install_to.StartsWith($"{ksp.game.PrimaryModDirectoryRelative}/"))
            {
                // The installation path can be either "GameData" or a sub-directory of "GameData"
                string subDir = install_to.Substring(ksp.game.PrimaryModDirectoryRelative.Length);    // remove "GameData"
                subDir = subDir.StartsWith("/") ? subDir.Substring(1) : subDir;    // remove a "/" at the beginning, if present

                // Add the extracted subdirectory to the path of KSP's GameData
                installDir = CKANPathUtils.NormalizePath(ksp.game.PrimaryModDirectory(ksp) + "/" + subDir);
            }
            else
            {
                switch (install_to)
                {
                    case "GameRoot":
                        installDir = ksp.GameDir();
                        break;

                    default:
                        if (ksp.game.AllowInstallationIn(install_to, out string path))
                        {
                            installDir = ksp.ToAbsoluteGameDir(path);
                        }
                        else
                        {
                            throw new BadInstallLocationKraken(string.Format(
                                Properties.Resources.ModuleInstallDescriptorUnknownInstallPath, install_to));
                        }
                        break;
                }
            }

            EnsurePattern();

            // `find` is supposed to match the "topmost" folder. Find it.
            var shortestMatch = find == null ? null
                : zipfile.Cast<ZipEntry>()
                    .Select(entry => inst_pattern.Match(entry.Name.Replace('\\', '/')))
                    .Where(match => match.Success)
                    .DefaultIfEmpty()
                    .Min(match => match?.Index);

            // O(N^2) solution, as we're walking the zipfile for each stanza.
            // Surely there's a better way, although this is fast enough we may not care.
            foreach (ZipEntry entry in zipfile)
            {
                // Backslashes are not allowed in filenames according to the ZIP spec,
                // but there's a non-conformant PowerShell tool that uses them anyway.
                // Try to accommodate those mods.
                string entryName = entry.Name.Replace('\\', '/');

                // Skips dirs and things not prescribed by our install stanza.
                if (!IsWanted(entryName, shortestMatch))
                {
                    continue;
                }

                // Prepare our file info.
                InstallableFile file_info = new InstallableFile
                {
                    source = entry,
                    makedir = false,
                    destination = null
                };

                // If we have a place to install it, fill that in...
                if (installDir != null)
                {
                    // Get the full name of the file.
                    // Update our file info with the install location
                    file_info.destination = TransformOutputName(
                        ksp.game, entryName, installDir, @as);
                    file_info.makedir = AllowDirectoryCreation(
                        ksp.game,
                        ksp?.ToRelativeGameDir(file_info.destination)
                            ?? file_info.destination);
                }

                files.Add(file_info);
            }

            // If we have no files, then something is wrong! (KSP-CKAN/CKAN#93)
            if (files.Count == 0)
            {
                // We have null as the first argument here, because we don't know which module we're installing
                throw new BadMetadataKraken(null, string.Format(
                    Properties.Resources.ModuleInstallDescriptorNoFilesFound, DescribeMatch()));
            }

            return files;
        }

        private bool AllowDirectoryCreation(IGame game, string relativePath)
        {
            return game.CreateableDirs.Any(dir =>
                relativePath == dir || relativePath.StartsWith($"{dir}/"));
        }

        /// <summary>
        /// Transforms the name of the output. This will strip the leading directories from the stanza file from
        /// output name and then combine it with the installDir.
        /// EX: "kOS-1.1/GameData/kOS", "kOS-1.1/GameData/kOS/Plugins/kOS.dll", "GameData" will be transformed
        /// to "GameData/kOS/Plugins/kOS.dll"
        /// </summary>
        /// <param name="outputName">The name of the file to transform</param>
        /// <param name="installDir">The installation dir where the file should end up with</param>
        /// <returns>The output name</returns>
        internal string TransformOutputName(IGame game, string outputName, string installDir, string @as)
        {
            string leadingPathToRemove = Path
                .GetDirectoryName(ShortestMatchingPrefix(outputName))
                .Replace('\\', '/');

            if (!string.IsNullOrEmpty(leadingPathToRemove))
            {
                Regex leadingRE = new Regex(
                    "^" + Regex.Escape(leadingPathToRemove) + "/",
                    RegexOptions.Compiled);
                if (!leadingRE.IsMatch(outputName))
                {
                    throw new BadMetadataKraken(null, string.Format(
                        "Output file name ({0}) not matching leading path of stanza ({1})",
                        outputName, leadingPathToRemove));
                }
                // Strip off leading path name
                outputName = leadingRE.Replace(outputName, "");
            }

            // Now outputname looks like PATH/what/ever/file.ext, where
            // PATH is the part that matched `file` or `find` or `find_regexp`

            if (!string.IsNullOrWhiteSpace(@as))
            {
                if (@as.Contains("/") || @as.Contains("\\"))
                {
                    throw new BadMetadataKraken(null, Properties.Resources.ModuleInstallDescriptorAsNoPathSeparators);
                }
                // Replace first path component with @as
                outputName = ReplaceFirstPiece(outputName, "/", @as);
            }
            else
            {
                var reservedPrefix = game.ReservedPaths.FirstOrDefault(prefix =>
                    outputName.StartsWith(prefix + "/", StringComparison.InvariantCultureIgnoreCase));
                if (reservedPrefix != null)
                {
                    // If we try to install a folder with the same name as
                    // one of the reserved directories, strip it off.
                    // Delete reservedPrefix and one forward slash
                    outputName = outputName.Substring(reservedPrefix.Length + 1);
                }
            }

            // Return our snipped, normalised, and ready to go output filename!
            return CKANPathUtils.NormalizePath(
                Path.Combine(installDir, outputName)
            );
        }

        private string ShortestMatchingPrefix(string fullPath)
        {
            EnsurePattern();

            string shortest = fullPath;
            for (string path = trailingSlashPattern.Replace(fullPath.Replace('\\', '/'), "");
                    !string.IsNullOrEmpty(path);
                    path = Path.GetDirectoryName(path).Replace('\\', '/'))
            {
                if (inst_pattern.IsMatch(path))
                {
                    shortest = path;
                }
                else
                {
                    break;
                }
            }
            return shortest;
        }

        private static string ReplaceFirstPiece(string text, string delimiter, string replacement)
        {
            int pos = text.IndexOf(delimiter);
            if (pos < 0)
            {
                // No delimiter, replace whole string
                return replacement;
            }
            return replacement + text.Substring(pos);
        }

        public string DescribeMatch()
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(file))
            {
                sb.AppendFormat("file=\"{0}\"", file);
            }
            if (!string.IsNullOrEmpty(find))
            {
                sb.AppendFormat("find=\"{0}\"", find);
            }
            if (!string.IsNullOrEmpty(find_regexp))
            {
                sb.AppendFormat("find_regexp=\"{0}\"", find_regexp);
            }
            return sb.ToString();
        }
    }
}
