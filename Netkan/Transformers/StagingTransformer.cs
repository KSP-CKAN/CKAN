using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;
using log4net;

using CKAN.Versioning;
using CKAN.Games;
using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Transformers
{
    internal sealed class StagingTransformer : ITransformer
    {
        public StagingTransformer(IGame game)
        {
            this.game      = game;
            currentRelease = game.KnownVersions.Max().ToVersionRange();
        }

        public string Name => "staging";

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            if (VersionsNeedManualReview(metadata, out string reason))
            {
                Log.DebugFormat("Enabling staging, reason: {0}", reason);
                opts.StagingReasons.Add(reason);
                opts.Staged = true;
            }
            // This transformer never changes the metadata
            yield return metadata;
        }

        private bool VersionsNeedManualReview(Metadata metadata, out string reason)
        {
            JObject json = metadata.Json();
            var minStr = json["ksp_version_min"] ?? json["ksp_version"];
            var maxStr = json["ksp_version_max"] ?? json["ksp_version"];
            var minVer = minStr == null ? GameVersion.Any : GameVersion.Parse((string)minStr);
            var maxVer = maxStr == null ? GameVersion.Any : GameVersion.Parse((string)maxStr);
            if (currentRelease.IntersectWith(new GameVersionRange(minVer, maxVer)) == null)
            {
                reason = $"Hard-coded game versions not compatible with current release: {GameVersionRange.VersionSpan(game, minVer, maxVer)}\r\nPlease check that they match the forum thread.";
                return true;
            }
            else
            {
                // Compatible with latest release, no manual review needed
                reason = "";
                return false;
            }
        }

        private readonly GameVersionRange currentRelease;
        private readonly IGame            game;

        private static readonly ILog Log = LogManager.GetLogger(typeof(StagingTransformer));
    }
}
