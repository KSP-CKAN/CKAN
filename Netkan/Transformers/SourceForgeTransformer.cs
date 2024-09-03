using System;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;
using log4net;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Sources.SourceForge;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that looks up data from GitLab.
    /// </summary>
    internal sealed class SourceForgeTransformer : ITransformer
    {
        /// <summary>
        /// Initialize the transformer
        /// </summary>
        /// <param name="api">Object to use for accessing the SourceForge API</param>
        public SourceForgeTransformer(ISourceForgeApi api)
        {
            this.api = api;
        }

        /// <summary>
        /// Defines the name of this transformer
        /// </summary>
        public string Name => "sourceforge";

        /// <summary>
        /// If input metadata has a GitLab kref, inflate it with whatever info we can get,
        /// otherwise return it unchanged
        /// </summary>
        /// <param name="metadata">Input netkan</param>
        /// <param name="opts">Inflation options from command line</param>
        /// <returns></returns>
        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions? opts)
        {
            if (metadata.Kref?.Source == Name)
            {
                log.InfoFormat("Executing SourceForge transformation with {0}", metadata.Kref);
                var reference = new SourceForgeRef(metadata.Kref);
                var mod       = api.GetMod(reference);
                var releases  = mod.Versions
                                   .Skip(opts?.SkipReleases ?? 0)
                                   .Take(opts?.Releases     ?? 1)
                                   .ToArray();
                if (releases.Length < 1)
                {
                    log.WarnFormat("No releases found for {0}", reference);
                    return Enumerable.Repeat(metadata, 1);
                }
                return releases.Select(ver => TransformOne(metadata.Json(), mod, ver));
            }
            else
            {
                // Passthrough for non-GitLab mods
                return Enumerable.Repeat(metadata, 1);
            }
        }

        private static Metadata TransformOne(JObject            json,
                                             SourceForgeMod     mod,
                                             SourceForgeVersion version)
        {
            json.SafeAdd("name",     mod.Title);
            json.SafeMerge("resources", JObject.FromObject(new Dictionary<string, string?>()
            {
                { "homepage",   mod.HomepageLink   },
                { "repository", mod.RepositoryLink },
                { "bugtracker", mod.BugTrackerLink },
            }));
            // SourceForge doesn't send redirects to user agents it considers browser-like
            json.SafeAdd("download", Net.ResolveRedirect(version.Link, "Wget")
                                        ?.OriginalString);
            json.SafeAdd(Metadata.UpdatedPropertyName, version.Timestamp);

            json.Remove("$kref");

            log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);
            return new Metadata(json);
        }

        private        readonly ISourceForgeApi api;
        private static readonly ILog            log = LogManager.GetLogger(typeof(GitlabTransformer));
    }
}
