using System;
using System.Collections.Generic;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Transformers;

namespace CKAN.NetKAN.Services
{
    internal interface ICacheService
    {
        IEnumerable<Metadata> ExecuteCached(Metadata metadata,
                                            TransformOptions options,
                                            Type caller,
                                            Func<string?, Metadata> action);

        /// <summary>
        /// Executes <paramref name="action"/> and caches its results. If the
        /// inputs and the remote file to download are the same then this will
        /// directly return the cached result without running action or
        /// downloading the file.
        /// </summary>
        IEnumerable<Metadata> ExecuteCached(Metadata metadata,
                                            TransformOptions options,
                                            Type caller,
                                            Func<string?, IEnumerable<Metadata>> action);
    }
}