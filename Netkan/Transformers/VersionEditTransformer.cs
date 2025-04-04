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
            if (metadata.VersionEdit?.Find != null)
            {
                Log.InfoFormat("Executing version edit transformation");
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, metadata.AllJson);

                var findRegex = new Regex(metadata.VersionEdit.Find);
                if (findRegex.IsMatch(metadata.Version?.ToString() ?? ""))
                {
                    var json = metadata.Json();
                    string version = new Regex(metadata.VersionEdit.Find)
                        .Replace(metadata.Version?.ToString() ?? "",
                                 metadata.VersionEdit.Replace);

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
                else if (metadata.VersionEdit.Strict)
                {
                    throw new Kraken(string.Format(
                        "Could not match version {0} with find pattern {1}",
                        metadata.Version,
                        metadata.VersionEdit.Find));
                }
            }
            yield return metadata;
        }
    }
}
