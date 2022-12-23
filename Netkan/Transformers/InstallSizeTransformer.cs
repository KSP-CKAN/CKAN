using System.Linq;
using System.Collections.Generic;

using ICSharpCode.SharpZipLib.Zip;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.Games;

namespace CKAN.NetKAN.Transformers
{
    internal sealed class InstallSizeTransformer : ITransformer
    {
        public string Name { get { return "install_size"; } }

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
                ZipFile zip = new ZipFile(_http.DownloadModule(metadata));
                GameInstance inst = new GameInstance(new KerbalSpaceProgram(), "/", "dummy", new NullUser());
                json["install_size"] = _moduleService.FileSources(mod, zip, inst)
                                                     .Select(ze => ze.Size)
                                                     .Sum();
                yield return new Metadata(json);
            }
            else
            {
                yield return metadata;
            }
        }

        private readonly IHttpService _http;
        private readonly IModuleService _moduleService;
    }
}
