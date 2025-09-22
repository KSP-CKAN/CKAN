using System.Linq;
using System.Collections.Generic;

using ICSharpCode.SharpZipLib.Zip;

using CKAN.IO;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;

namespace CKAN.NetKAN.Transformers
{
    internal sealed class InstallSizeTransformer : ITransformer
    {
        public string Name => "install_size";

        public InstallSizeTransformer(IHttpService http, IModuleService moduleService)
        {
            _http = http;
            _moduleService = moduleService;
        }

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            if (metadata.Download != null)
            {
                var json = metadata.Json();
                CkanModule mod = CkanModule.FromJson(json.ToString());
                ZipFile    zip = new ZipFile(_http.DownloadModule(metadata));
                json["install_size"] = _moduleService.FileSources(mod, zip)
                                                     .Where(ze => !ModuleInstaller.IsInternalCkan(ze))
                                                     .Sum(ze => ze.Size);
                yield return new Metadata(json);
            }
            else
            {
                yield return metadata;
            }
        }

        private readonly IHttpService   _http;
        private readonly IModuleService _moduleService;
    }
}
