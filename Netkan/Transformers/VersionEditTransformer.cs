using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using log4net;
using Newtonsoft.Json.Linq;

using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that allows simple editing of the version number using regular expressions.
    /// </summary>
    internal sealed class VersionEditTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(VersionEditTransformer));

        public string Name => "version_edit";

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            var versionEditInfo = GetVersionEditInfo(metadata.AllJson);
            if (versionEditInfo != null)
            {
                Log.InfoFormat("Executing version edit transformation");
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, metadata.AllJson);

                var findRegex = new Regex(versionEditInfo.Find);
                if (findRegex.IsMatch(versionEditInfo.Version))
                {
                    var json = metadata.Json();
                    string version = new Regex(versionEditInfo.Find)
                        .Replace(versionEditInfo.Version, versionEditInfo.Replace);

                    var versionPieces = json.Value<JObject>("x_netkan_version_pieces")
                                            ?.ToObject<Dictionary<string, string>>();

                    if (versionPieces != null)
                    {
                        version = versionPieces.Aggregate(
                            version,
                            (v, kvp) =>
                            {
                                Log.Debug($"Replacing ${{{kvp.Key}}} with {kvp.Value} in {v}");
                                return v.Replace($"${{{kvp.Key}}}", kvp.Value);
                            });
                    }

                    json["version"] = version;
                    Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);
                    yield return new Metadata(json);
                    yield break;
                }
                else if (versionEditInfo.Strict)
                {
                    throw new Kraken(string.Format(
                        "Could not match version {0} with find pattern {1}",
                        versionEditInfo.Version,
                        versionEditInfo.Find));
                }
            }
            yield return metadata;
        }

        private static VersionEditInfo? GetVersionEditInfo(JObject json)
        {
            if (json.TryGetValue("x_netkan_version_edit", out JToken? editProp)
                && editProp != null)
            {
                if (json.TryGetValue("version", out JToken? versionProp)
                    && (string?)versionProp is string ver)
                {
                    string? find;
                    var replace = "${version}";
                    var strict  = true;

                    switch (editProp)
                    {
                        case JValue val:
                            find = val.ToString();
                            break;

                        case JObject editObj:
                            if (editObj.TryGetValue("find", out JToken? findProp)
                                && findProp.Type == JTokenType.String
                                && (string?)findProp is string fp)
                            {
                                find = fp;
                            }
                            else
                            {
                                throw new Kraken("`x_netkan_version_edit` must contain `find` string property");
                            }
                            if (editObj.TryGetValue("replace", out JToken? replaceProp))
                            {
                                if (replaceProp.Type != JTokenType.String)
                                {
                                    throw new Kraken(
                                        "`x_netkan_version_edit` `replace` property must be a string");
                                }
                                replace = (string?)replaceProp ?? replace;
                            }
                            if (editObj.TryGetValue("strict", out JToken? strictProp))
                            {
                                if (strictProp.Type != JTokenType.Boolean)
                                {
                                    throw new Kraken(
                                        "`x_netkan_version_edit` `strict` property must be a bool");
                                }
                                strict = (bool)strictProp;
                            }
                            break;

                        default:
                            throw new Kraken(
                                string.Format("Unrecognized `x_netkan_version_edit` value: {0}", editProp));
                    }

                    return new VersionEditInfo(ver, find, replace, strict);
                }
                else
                {
                    throw new Kraken("`version` property must be a string property");
                }
            }
            return null;
        }

        private sealed class VersionEditInfo
        {
            public string Version { get; private set; }
            public string Find    { get; private set; }
            public string Replace { get; private set; }
            public bool   Strict  { get; private set; }

            public VersionEditInfo(string version, string find, string replace, bool strict)
            {
                Version = version;
                Find    = find;
                Replace = replace;
                Strict  = strict;
            }
        }
    }
}
