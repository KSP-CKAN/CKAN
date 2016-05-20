using System;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using log4net;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that adds the size of the download.
    /// </summary>
    internal sealed class DownloadSizeTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DownloadSizeTransformer));

        private readonly IHttpService _http;
        private readonly IFileService _fileService;

        public DownloadSizeTransformer(IHttpService http, IFileService fileService)
        {
            _http = http;
            _fileService = fileService;
        }

        public Metadata Transform(Metadata metadata)
        {
            if (metadata.Download != null)
            {
                var json = metadata.Json();

                Log.InfoFormat("Executing Download Size transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

                var file = _http.DownloadPackage(metadata.Download, metadata.Identifier);

                if (file != null)
                {
                    json["download_size"] = _fileService.GetSizeBytes(file);
                }

                Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

                return new Metadata(json);
            }

            return metadata;
        }
    }
}
