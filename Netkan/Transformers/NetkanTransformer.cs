using System.Collections.Generic;
using System.Linq;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Validators;
using CKAN.NetKAN.Sources.Curse;
using CKAN.NetKAN.Sources.Github;
using CKAN.NetKAN.Sources.Gitlab;
using CKAN.NetKAN.Sources.Jenkins;
using CKAN.NetKAN.Sources.Spacedock;
using CKAN.Games;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that can perform a complete transform from a NetKAN to CKAN metadata.
    /// </summary>
    internal sealed class NetkanTransformer : ITransformer
    {
        private readonly List<ITransformer> _transformers;
        private readonly IValidator _validator;

        public string Name => "netkan";

        public NetkanTransformer(
            IHttpService http,
            IFileService fileService,
            IModuleService moduleService,
            string githubToken,
            string gitlabToken,
            bool prerelease,
            IGame game,
            IValidator validator
        )
        {
            _validator = validator;
            var ghApi = new GithubApi(http, githubToken);
            var glApi = new GitlabApi(http, gitlabToken);
            _transformers = InjectVersionedOverrideTransformers(new List<ITransformer>
            {
                new StagingTransformer(game),
                new MetaNetkanTransformer(http, ghApi),
                new SpacedockTransformer(new SpacedockApi(http), ghApi),
                new CurseTransformer(new CurseApi(http)),
                new GithubTransformer(ghApi, prerelease),
                new GitlabTransformer(glApi),
                new HttpTransformer(),
                new JenkinsTransformer(new JenkinsApi(http)),
                new AvcKrefTransformer(http, ghApi),
                new InternalCkanTransformer(http, moduleService, game),
                new SpaceWarpInfoTransformer(http, ghApi, moduleService, game),
                new AvcTransformer(http, moduleService, ghApi, game),
                new LocalizationsTransformer(http, moduleService, game),
                new VersionEditTransformer(),
                new ForcedVTransformer(),
                new EpochTransformer(),
                // This is the "default" VersionedOverrideTransformer for compatibility with overrides that don't
                // specify a before or after property.
                new VersionedOverrideTransformer(before: new string[] { null }, after: new string[] { null }),
                new DownloadAttributeTransformer(http, fileService),
                new InstallSizeTransformer(http, moduleService, game),
                new StagingLinksTransformer(),
                new GeneratedByTransformer(),
                new OptimusPrimeTransformer(),
                new StripNetkanMetadataTransformer(),
                new PropertySortTransformer()
            });
        }

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            Metadata[] modules = new Metadata[] { metadata };
            foreach (ITransformer tr in _transformers)
            {
                modules = modules.SelectMany(meta => tr.Transform(meta, opts))
                                 .ToArray();
                // The metadata should be valid after each step
                foreach (Metadata meta in modules)
                {
                    _validator.Validate(meta);
                }
            }
            return modules;
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
                {
                    after.Add(transformers[i - 1].Name);
                }

                result.Add(new VersionedOverrideTransformer(before, after));
                result.Add(transformers[i]);
            }

            if (result.Any())
            {
                if (result.First() is VersionedOverrideTransformer firstVersionedOverride)
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
