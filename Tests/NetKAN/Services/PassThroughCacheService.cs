using System;
using System.Collections.Generic;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Transformers;

namespace Tests.NetKAN.Services
{
    /// <summary>
    /// An ICacheService that never caches; it downloads with the plain
    /// DownloadModule overload and always runs the action, so tests can keep
    /// mocking that overload.
    /// </summary>
    internal sealed class PassThroughCacheService : ICacheService
    {
        public PassThroughCacheService(IHttpService http)
        {
            this.http = http;
        }

        public IEnumerable<Metadata> ExecuteCached(Metadata                metadata,
                                                   TransformOptions        options,
                                                   Type                    caller,
                                                   Func<string?, Metadata> action)
            => new Metadata[] { action(http.DownloadModule(metadata)) };

        public IEnumerable<Metadata> ExecuteCached(Metadata                             metadata,
                                                   TransformOptions                     options,
                                                   Type                                 caller,
                                                   Func<string?, IEnumerable<Metadata>> action)
            => action(http.DownloadModule(metadata));

        private readonly IHttpService http;
    }
}
