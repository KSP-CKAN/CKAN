using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using log4net;
using Newtonsoft.Json.Linq;

using CKAN.NetKAN.Model;
using CKAN.Versioning;
using CKAN.Extensions;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that overrides properties based on the version.
    /// </summary>
    internal sealed class VersionedOverrideTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(VersionedOverrideTransformer));

        private readonly List<string?> _before;
        private readonly List<string?> _after;

        public string Name => "versioned_override";

        public VersionedOverrideTransformer(IEnumerable<string?> before,
                                            IEnumerable<string?> after)
        {
            _before = new List<string?>(before);
            _after  = new List<string?>(after);
        }

        public void AddBefore(string before)
        {
            _before.Add(before);
        }

        public void AddAfter(string after)
        {
            _after.Add(after);
        }

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            if (metadata.Overrides != null)
            {
                Log.InfoFormat("Executing override transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, metadata.AllJson);

                // There's an override section, process them

                var json = metadata.Json();
                foreach (var overrideStanza in metadata.Overrides)
                {
                    ProcessOverrideStanza(overrideStanza, metadata, json);
                }

                Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

                yield return new Metadata(json);
                yield break;
            }
            yield return metadata;
        }

        /// <summary>
        /// Processes an individual override stanza, altering the object's
        /// metadata in the process if applicable.
        /// </summary>
        private void ProcessOverrideStanza(OverrideOptions overrideStanza,
                                           Metadata        metadata,
                                           JObject         json)
        {
            if (_before.Contains(overrideStanza.BeforeStep)
                || _after.Contains(overrideStanza.AfterStep))
            {
                Log.InfoFormat("Processing override: {0}", overrideStanza);

                if (overrideStanza.VersionConstraints == null)
                {
                    throw new Kraken(
                        string.Format(
                            "Can't find version in override stanza {0}",
                            overrideStanza));
                }

                // If the constraints don't apply, then do nothing.
                if (metadata.Version == null
                    || !ConstraintsApply(overrideStanza.VersionConstraints, metadata.Version))
                {
                    return;
                }

                // All the constraints pass; let's replace the metadata we have with what's
                // in the override.
                if (overrideStanza.Override != null)
                {
                    if (gameVersionProperties.Any(overrideStanza.Override.ContainsKey))
                    {
                        GameVersion.SetJsonCompatibility(
                            json,
                            VersionProperty(overrideStanza.Override, "ksp_version"),
                            VersionProperty(overrideStanza.Override, "ksp_version_min"),
                            VersionProperty(overrideStanza.Override, "ksp_version_max"));
                        foreach (var p in gameVersionProperties)
                        {
                            overrideStanza.Override.Remove(p);
                        }
                    }
                    foreach ((string propName, JToken propVal) in overrideStanza.Override)
                    {
                        json[propName] = propVal;
                    }
                }

                // Delete anything that needs deleting
                if (overrideStanza.Delete != null)
                {
                    foreach (var key in overrideStanza.Delete)
                    {
                        json.Remove(key);
                    }
                }
            }
        }

        private static GameVersion? VersionProperty(Dictionary<string, JToken> dict,
                                                    string                     name)
            => dict.TryGetValue(name, out JToken? tok) && (string?)tok is string v
                   ? GameVersion.Parse(v)
                   : null;

        private readonly string[] gameVersionProperties = new string[]
        {
            "ksp_version", "ksp_version_min", "ksp_version_max"
        };

        /// <summary>
        /// Walks through a list of constraints, and returns true if they're all satisifed
        /// for the mod version we're examining.
        /// </summary>
        private static bool ConstraintsApply(IEnumerable<string> constraints,
                                             ModuleVersion       version)
            => constraints.All(c => ConstraintPattern.TryMatch(c, out Match? match)
                                    && ConstraintPasses(match.Groups["op"].Value,
                                                        version,
                                                        new ModuleVersion(match.Groups["version"].Value)));

        private static readonly Regex ConstraintPattern = new Regex(
            @"^(?<op> [<>=]*) \s* (?<version> .*)$",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// Returns whether the given constraint matches the desired version
        /// for the mod we're processing.
        /// </summary>
        private static bool ConstraintPasses(string        op,
                                             ModuleVersion version,
                                             ModuleVersion desiredVersion)
            => op switch
               {
                   "" or "=" => version.IsEqualTo(desiredVersion),
                   "<"       => version.IsLessThan(desiredVersion),
                   ">"       => version.IsGreaterThan(desiredVersion),
                   "<="      => version.CompareTo(desiredVersion) <= 0,
                   ">="      => version.CompareTo(desiredVersion) >= 0,
                   _         => throw new Kraken(
                                    string.Format("Unknown x_netkan_override comparator: {0}",
                                                  op)),
               };
    }
}
