﻿using System.Collections.Generic;
using System.Linq;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Sources.Github;
using CKAN.NetKAN.Sources.Kerbalstuff;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that can perform a complete transform from a NetKAN to CKAN metadata.
    /// </summary>
    internal sealed class NetkanTransformer : ITransformer
    {
        private readonly List<ITransformer> _transformers;

        public NetkanTransformer(
            IHttpService http,
            IFileService fileService,
            IModuleService moduleService,
            string githubToken,
            bool prerelease
        )
        {
            _transformers = new List<ITransformer>
            {
                new MetaNetkanTransformer(http),
                new KerbalstuffTransformer(new KerbalstuffApi(http)),
                new GithubTransformer(new GithubApi(githubToken), prerelease),
                new HttpTransformer(),
                new JenkinsTransformer(),
                new InternalCkanTransformer(http, moduleService),
                new AvcTransformer(http, moduleService),
                new ForcedVTransformer(),
                new EpochTransformer(),
                new VersionedOverrideTransformer(),
                new DownloadSizeTransformer(http, fileService),
                new GeneratedByTransformer(),
                new OptimusPrimeTransformer(),
                new StripNetkanMetadataTransformer()
            };
        }

        public Metadata Transform(Metadata metadata)
        {
            return _transformers
                .Aggregate(
                    metadata,
                    (transformedMetadata, transformer) => transformer.Transform(transformedMetadata)
                );
        }
    }
}
