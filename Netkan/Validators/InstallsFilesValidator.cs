using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;

namespace CKAN.NetKAN.Validators
{
    internal sealed class InstallsFilesValidator : IValidator
    {
        private readonly IHttpService _http;
        private readonly IModuleService _moduleService;

        public InstallsFilesValidator(IHttpService http, IModuleService moduleService)
        {
            _http = http;
            _moduleService = moduleService;
        }

        public void Validate(Metadata metadata)
        {
            var mod = CkanModule.FromJson(metadata.Json().ToString());
            var file = _http.DownloadPackage(metadata.Download, metadata.Identifier, metadata.RemoteTimestamp);

            // Make sure this would actually generate an install.

            if (!_moduleService.HasInstallableFiles(mod, file))
            {
                throw new Kraken(string.Format(
                    "Module contains no files matching: {0}",
                    mod.DescribeInstallStanzas()
                ));
            }
        }
    }
}
