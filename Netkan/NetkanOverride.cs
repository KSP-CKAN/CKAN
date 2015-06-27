using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using CKAN;
using log4net;

namespace CKAN.NetKAN
{
    /// <summary>
    /// Allow per-version overrides for netkan processing. GH #1160.
    /// </summary>
    public class NetkanOverride
    {
        private JObject metadata;
        private CKAN.Version version;
        private static readonly ILog log = LogManager.GetLogger(typeof (NetkanOverride));

        public NetkanOverride(JObject metadata)
        {
            this.metadata = (JObject) metadata.DeepClone();
            version = new Version(metadata["version"].ToString());
        }

        /// <summary>
        /// Processes any x_netkan_override sections in the metadata
        /// </summary>
        /// <returns>The metadata after overrides have been processed,
        /// even if no work was performed.</returns>
        public JObject ProcessOverrides()
        {
            JToken override_list;

            // If there's no override section, then just return our existing metadata.
            if (! metadata.TryGetValue("x_netkan_override", out override_list))
                return metadata;

            log.InfoFormat("Found override section: {0}", override_list);

            if (!(override_list is JArray))
                throw new Kraken(
                    string.Format(
                        "x_netkan_override expects a list of overrides, found: {0}",
                        override_list));
            
            // Sweet! We have an override. Let's walk through and see if we can find
            // an applicable section for us.
            foreach (JToken override_stanza in override_list)
            {
                log.InfoFormat("Processing override: {0}", override_stanza);
                ProcessOverrideStanza((JObject) override_stanza);
            }

            return metadata;
        }

        /// <summary>
        /// Processes an individual override stanza, altering the object's
        /// metadata in the process if applicable.
        /// </summary>
        private void ProcessOverrideStanza(JObject override_stanza)
        {
            JToken stanza_constraints;

            if (!override_stanza.TryGetValue("version", out stanza_constraints))
            {
                throw new Kraken(
                    string.Format(
                        "Can't find version in override stanza {0}",
                        override_stanza));
            }

            IEnumerable<string> constraints;

            // First let's get our constraints into a list of strings.

            if (stanza_constraints is JValue)
            {
                constraints = new List<string> { stanza_constraints.ToString() };
            }
            else if (stanza_constraints is JArray)
            {
                // Pop the constraints in 'constraints'
                constraints = ((JArray)stanza_constraints).Values().Select(x => x.ToString());
            }
            else
            {
                throw new Kraken(
                    string.Format("Totally unexpected x_netkan_override - {0}", stanza_constraints));
            }

            // If the constraints don't apply, then do nothing.
            if (!ConstraintsApply(constraints))
                return;

            // All the constraints pass; let's replace the metadata we have with what's
            // in the override.

            JToken override_block;

            if (override_stanza.TryGetValue("override", out override_block))
            {
                foreach (JProperty property in ((JObject) override_block).Properties())
                {
                    metadata[property.Name] = property.Value;
                }
            }

            // And let's delete anything that needs deleting.

            JToken delete_list;
            if (override_stanza.TryGetValue("delete", out delete_list))
            {
                foreach (string key in (JArray) delete_list)
                {
                    metadata.Remove(key);
                }
            }
        }

        /// <summary>
        /// Walks through a list of constraints, and returns true if they're all satisifed
        /// for the mod version we're examining.
        /// </summary>
        private bool ConstraintsApply(IEnumerable<string> constraints)
        {
            foreach (string constraint in constraints)
            {
                Match match = Regex.Match(
                    constraint,
                    @"^(?<op> [<>=]*) \s* (?<version> .*)$",
                    RegexOptions.IgnorePatternWhitespace);

                if (!match.Success)
                    throw new Kraken(
                        string.Format("Unable to parse x_netkan_override - {0}", constraint));

                string op = match.Groups["op"].Value;
                CKAN.Version desired_version = new Version(match.Groups["version"].Value);

                // This contstraint failed. This stanza is not for us.
                if (!ConstraintPasses(op, desired_version))
                    return false;
            }

            // All the constraints passed! We want to apply this stanza!
            return true;
        }

        /// <summary>
        /// Returns whether the given constraint matches the desired version
        /// for the mod we're processing.
        /// </summary>
        private bool ConstraintPasses(string op, CKAN.Version desired_version)
        {
            switch (op)
            {
                case "":
                case "=":
                    return version.IsEqualTo(desired_version);

                case "<":
                    return version.IsLessThan(desired_version);

                case ">":
                    return version.IsGreaterThan(desired_version);

                case "<=":
                    return version.CompareTo(desired_version) <= 0;

                case ">=":
                    return version.CompareTo(desired_version) >= 0;

                default:
                    throw new Kraken(
                        string.Format("Unknown x_netkan_override comparator: {0}", op));
            }
        }
    }
}

