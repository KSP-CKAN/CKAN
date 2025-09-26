using System.Collections.Generic;
using System.Linq;

using CKAN.Games;
using CKAN.Extensions;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Validators;
using CKAN.NetKAN.Sources.Github;
using CKAN.NetKAN.Sources.Gitlab;
using CKAN.NetKAN.Sources.Jenkins;
using CKAN.NetKAN.Sources.Spacedock;
using CKAN.NetKAN.Sources.SourceForge;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that can perform a complete transform from a NetKAN to CKAN metadata.
    /// </summary>
    internal sealed class NetkanTransformer : ITransformer
    {
        public NetkanTransformer(IHttpService   http,
                                 IFileService   fileService,
                                 IModuleService moduleService,
                                 string?        githubToken,
                                 string?        gitlabToken,
                                 string?        userAgent,
                                 bool?          prerelease,
                                 IGame          game,
                                 IValidator     validator)
        {
            _validator = validator;
            var ghApi = new GithubApi(http, githubToken);
            var glApi = new GitlabApi(http, gitlabToken);
            var sfApi = new SourceForgeApi(http);
            _transformers = InjectVersionedOverrideTransformers(new ITransformer[]
            {
                new StagingTransformer(game),
                new MetaNetkanTransformer(http, ghApi),
                new SpacedockTransformer(new SpacedockApi(http), ghApi),
                new GithubTransformer(ghApi, prerelease),
                new GitlabTransformer(glApi),
                new SourceForgeTransformer(sfApi),
                new HttpTransformer(http, userAgent),
                new JenkinsTransformer(new JenkinsApi(http)),
                new AvcKrefTransformer(http, ghApi),
                new InternalCkanTransformer(http, moduleService),
                new SpaceWarpInfoTransformer(http, ghApi, moduleService),
                new AvcTransformer(http, moduleService, ghApi),
                new LocalizationsTransformer(http, moduleService),
                new VersionEditTransformer(),
                new ForcedVTransformer(),
                new EpochTransformer(),
                // This is the "default" VersionedOverrideTransformer for compatibility with overrides that don't
                // specify a before or after property.
                new VersionedOverrideTransformer(before: new string?[] { null },
                                                 after:  new string?[] { null }),
                new DownloadAttributeTransformer(http, fileService),
                new InstallSizeTransformer(http, moduleService),
                new StagingLinksTransformer(),
                new GeneratedByTransformer(),
                new OptimusPrimeTransformer(),
                new StripNetkanMetadataTransformer(),
                new PropertySortTransformer()
            }).ToArray();
        }

        public string Name => "netkan";

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            var modules = RunTransformers(metadata, opts);
            foreach (var meta in modules)
            {
                _validator.Validate(meta);
            }
            return modules;
        }

        private Metadata[] RunTransformers(Metadata         metadata,
                                           TransformOptions opts)
            => _transformers.Aggregate(new Metadata[] { metadata },
                                       (modules, tr) => modules.SelectMany(meta => tr.Transform(meta, opts))
                                                               .ToArray());

        private static IEnumerable<ITransformer> InjectVersionedOverrideTransformers(IEnumerable<ITransformer> transformers)
            => transformers.Inject((after, before) => (after, before) switch
                                   {
                                       (null, ITransformer b) =>
                                           new VersionedOverrideTransformer(new[] { b.Name, "$all" },
                                                                            Enumerable.Repeat("$none", 1)),
                                       (ITransformer a, ITransformer b) =>
                                           new VersionedOverrideTransformer(Enumerable.Repeat(b.Name, 1),
                                                                            Enumerable.Repeat(a.Name, 1)),
                                       (ITransformer a, null) =>
                                           new VersionedOverrideTransformer(Enumerable.Repeat("$none", 1),
                                                                            new[] { a.Name, "$all" }),
                                       _ => throw new Kraken(),
                                   });

        private readonly ITransformer[] _transformers;
        private readonly IValidator     _validator;
    }
}
