using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Sources.Github;
using CKAN.NetKAN.Sources.Spacedock;
using System.Collections.Generic;
using System.Linq;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that can perform a complete transform from a NetKAN to CKAN metadata.
    /// </summary>
    internal sealed class NetkanTransformer : ITransformer
    {
        private readonly List<ITransformer> _transformers;

        public string Name { get { return "netkan"; } }

        public NetkanTransformer(
            IHttpService http,
            IFileService fileService,
            IModuleService moduleService,
            string githubToken,
            bool prerelease
        )
        {
            _transformers = InjectVersionedOverrideTransformers(new List<ITransformer>
            {
                new MetaNetkanTransformer(http),
                new SpacedockTransformer(new SpacedockApi(http)),
                new GithubTransformer(new GithubApi(githubToken), prerelease),
                new HttpTransformer(),
                new JenkinsTransformer(http),
                new InternalCkanTransformer(http, moduleService),
                new AvcTransformer(http, moduleService),
                new VersionEditTransformer(),
                new ForcedVTransformer(),
                new EpochTransformer(),
                // This is the "default" VersionedOverrideTransformer for compatability with overrides that don't
                // specify a before or after property.
                new VersionedOverrideTransformer(before: new string[] { null }, after: new string[] { null }),
                new DownloadAttributeTransformer(http, fileService),
                new GeneratedByTransformer(),
                new OptimusPrimeTransformer(),
                new StripNetkanMetadataTransformer(),
                new PropertySortTransformer()
            });
        }

        public Metadata Transform(Metadata metadata)
        {
            return _transformers
                .Aggregate(
                    metadata,
                    (transformedMetadata, transformer) => transformer.Transform(transformedMetadata)
                );
        }

        private static List<ITransformer> InjectVersionedOverrideTransformers(List<ITransformer> transformers)
        {
            var result = new List<ITransformer>();

            for (var i = 0; i < transformers.Count; i++)
            {
                var before = new List<string>();
                var after = new List<string>();

                before.Add(transformers[i].Name);

                if (i - 1 >= 0)
                    after.Add(transformers[i - 1].Name);

                result.Add(new VersionedOverrideTransformer(before, after));
                result.Add(transformers[i]);
            }

            if (result.Any())
            {
                var firstVersionedOverride = result.First() as VersionedOverrideTransformer;

                if (firstVersionedOverride != null)
                {
                    firstVersionedOverride.AddBefore("$all");
                    firstVersionedOverride.AddAfter("$none");
                }

                result.Add(new VersionedOverrideTransformer(
                    new[] { "$none" },
                    new[] { result.Last().Name, "$all" }
                ));
            }

            return result;
        }
    }
}