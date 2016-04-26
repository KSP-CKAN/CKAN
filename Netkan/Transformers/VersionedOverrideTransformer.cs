using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CKAN.NetKAN.Model;
using log4net;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that overrides properties based on the version.
    /// </summary>
    internal sealed class VersionedOverrideTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(VersionedOverrideTransformer));

        private readonly HashSet<string> _before;
        private readonly HashSet<string> _after;

        public string Name { get { return "versioned_override"; } }

        public VersionedOverrideTransformer(IEnumerable<string> before, IEnumerable<string> after)
        {
            _before = new HashSet<string>(before);
            _after = new HashSet<string>(after);
        }

        public void AddBefore(string before)
        {
            _before.Add(before);
        }

        public void AddAfter(string after)
        {
            _after.Add(after);
        }

        public Metadata Transform(Metadata metadata)
        {
            var json = metadata.Json();

            JToken overrideList;
            if (json.TryGetValue("x_netkan_override", out overrideList))
            {
                Log.InfoFormat("Executing Override transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

                // There's an override section, process them

                Log.DebugFormat("Override section:{0}{1}", Environment.NewLine, overrideList);

                if (overrideList.Type == JTokenType.Array)
                {
                    // Sweet! We have an override. Let's walk through and see if we can find
                    // an applicable section for us.
                    foreach (var overrideStanza in overrideList)
                    {
                        ProcessOverrideStanza((JObject)overrideStanza, json);
                    }

                    Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

                    return new Metadata(json);
                }
                else
                {
                    throw new Kraken(
                        string.Format(
                            "x_netkan_override expects a list of overrides, found: {0}",
                            overrideList));
                }
            }

            return metadata;
        }

        /// <summary>
        /// Processes an individual override stanza, altering the object's
        /// metadata in the process if applicable.
        /// </summary>
        private void ProcessOverrideStanza(JObject overrideStanza, JObject metadata)
        {
            JToken jBefore;
            JToken jAfter;
            string before = null;
            string after = null;

            if (overrideStanza.TryGetValue("before", out jBefore))
            {
                if (jBefore.Type == JTokenType.String)
                    before = (string)jBefore;
                else
                    throw new Kraken("override before property must be a string");
            }

            if (overrideStanza.TryGetValue("after", out jAfter))
            {
                if (jAfter.Type == JTokenType.String)
                    after = (string)jAfter;
                else
                    throw new Kraken("override after property must be a string");
            }

            if (_before.Contains(before) || _after.Contains(after))
            {
                Log.InfoFormat("Processing override: {0}", overrideStanza);

                JToken stanzaConstraints;

                if (!overrideStanza.TryGetValue("version", out stanzaConstraints))
                {
                    throw new Kraken(
                        string.Format(
                            "Can't find version in override stanza {0}",
                            overrideStanza));
                }

                IEnumerable<string> constraints;

                // First let's get our constraints into a list of strings.

                if (stanzaConstraints is JValue)
                {
                    constraints = new List<string> { stanzaConstraints.ToString() };
                }
                else if (stanzaConstraints is JArray)
                {
                    // Pop the constraints in 'constraints'
                    constraints = ((JArray)stanzaConstraints).Values().Select(x => x.ToString());
                }
                else
                {
                    throw new Kraken(
                        string.Format("Totally unexpected x_netkan_override - {0}", stanzaConstraints));
                }

                // If the constraints don't apply, then do nothing.
                if (!ConstraintsApply(constraints, new Version(metadata["version"].ToString())))
                    return;

                // All the constraints pass; let's replace the metadata we have with what's
                // in the override.

                JToken overrideBlock;

                if (overrideStanza.TryGetValue("override", out overrideBlock))
                {
                    foreach (var property in ((JObject)overrideBlock).Properties())
                    {
                        metadata[property.Name] = property.Value;
                    }
                }

                // And let's delete anything that needs deleting.

                JToken deleteList;
                if (overrideStanza.TryGetValue("delete", out deleteList))
                {
                    foreach (string key in (JArray)deleteList)
                    {
                        metadata.Remove(key);
                    }
                }
            }
        }

        /// <summary>
        /// Walks through a list of constraints, and returns true if they're all satisifed
        /// for the mod version we're examining.
        /// </summary>
        private static bool ConstraintsApply(IEnumerable<string> constraints, Version version)
        {
            foreach (var constraint in constraints)
            {
                var match = Regex.Match(
                    constraint,
                    @"^(?<op> [<>=]*) \s* (?<version> .*)$",
                    RegexOptions.IgnorePatternWhitespace);

                if (!match.Success)
                    throw new Kraken(
                        string.Format("Unable to parse x_netkan_override - {0}", constraint));

                var op = match.Groups["op"].Value;
                var desiredVersion = new Version(match.Groups["version"].Value);

                // This contstraint failed. This stanza is not for us.
                if (!ConstraintPasses(op, version, desiredVersion))
                    return false;
            }

            // All the constraints passed! We want to apply this stanza!
            return true;
        }

        /// <summary>
        /// Returns whether the given constraint matches the desired version
        /// for the mod we're processing.
        /// </summary>
        private static bool ConstraintPasses(string op, Version version, Version desiredVersion)
        {
            switch (op)
            {
                case "":
                case "=":
                    return version.IsEqualTo(desiredVersion);

                case "<":
                    return version.IsLessThan(desiredVersion);

                case ">":
                    return version.IsGreaterThan(desiredVersion);

                case "<=":
                    return version.CompareTo(desiredVersion) <= 0;

                case ">=":
                    return version.CompareTo(desiredVersion) >= 0;

                default:
                    throw new Kraken(
                        string.Format("Unknown x_netkan_override comparator: {0}", op));
            }
        }
    }
}
