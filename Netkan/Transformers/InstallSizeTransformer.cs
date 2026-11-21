using System.Linq;
using System.Collections.Generic;

using ICSharpCode.SharpZipLib.Zip;

using CKAN.IO;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Utils;

namespace CKAN.NetKAN.Transformers
{
    internal sealed class InstallSizeTransformer : ITransformer
    {
        public string Name => "install_size";

        public InstallSizeTransformer(IModuleService moduleService, ICacheService cacheService)
        {
            _moduleService = moduleService;
            _cacheService  = cacheService;
        }

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            if (metadata.Download == null)
            {
                return EnumerableHelpers.Once(metadata);
            }

            return _cacheService.ExecuteCached(metadata, typeof(InstallSizeTransformer), file =>
            {
                if (file == null)
                {
                    return metadata;
                }

                var json = metadata.Json();
                var mod = CkanModule.FromJson(json.ToString());
                using var zip = new ZipFile(file);

                json["install_size"] = _moduleService.FileSources(mod, zip)
                                                     .Where(ze => !ModuleInstaller.IsInternalCkan(ze))
                                                     .Sum(ze => ze.Size);
                return new Metadata(json);
            });
        }

        private readonly IModuleService _moduleService;
        private readonly ICacheService  _cacheService;
    }
}
