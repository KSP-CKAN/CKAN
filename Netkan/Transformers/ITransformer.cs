using System.Collections.Generic;
using CKAN.NetKAN.Model;
using CKAN.Versioning;

namespace CKAN.NetKAN.Transformers
{
    internal class TransformOptions
    {
        public TransformOptions(int? releases, int? skipReleases, ModuleVersion highVer, bool staged, string stagingReason)
        {
            Releases       = releases;
            SkipReleases   = skipReleases;
            HighestVersion = highVer;
            Staged         = staged;
            StagingReasons = new List<string>();
            if (!string.IsNullOrEmpty(stagingReason))
            {
                StagingReasons.Add(stagingReason);
            }
        }

        public readonly int?          Releases;
        public readonly int?          SkipReleases;
        public readonly ModuleVersion HighestVersion;
        public          bool          Staged;
        public readonly List<string>  StagingReasons;
        public          bool          FlakyAPI = false;
    }

    /// <summary>
    /// Represents an object that can perform transformations on NetKAN metadata.
    /// </summary>
    internal interface ITransformer
    {
        /// <summary>
        /// A unique name which identifies the transformer.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Transform the given metadata.
        /// </summary>
        /// <param name="metadata">The metadata to transform.</param>
        /// <returns>The transformed metadata.</returns>
        IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts);
    }
}
