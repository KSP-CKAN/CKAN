using System;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Extensions;
using log4net;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that adds a hash of the downloaded file.
    /// </summary>
    internal sealed class DownloadHashTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DownloadHashTransformer));

        private readonly IHttpService _http;
        private readonly IFileService _fileService;

        public string Name { get { return "download_hash"; } }

        public DownloadHashTransformer(IHttpService http, IFileService fileService)
        {
            _http = http;
            _fileService = fileService;
        }

        public Metadata Transform(Metadata metadata)
        {
            if (metadata.Download != null)
            {
                var json = metadata.Json();

                Log.InfoFormat("Executing Download Hash transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

                var file = _http.DownloadPackage(metadata.Download, metadata.Identifier);

                if (file != null)
                {
                    json["download_hash"] = new JObject();

                    var download_hashJson = (JObject)json["download_hash"];
                    download_hashJson.SafeAdd("sha1", _fileService.GetFileHashSha1(file));
                    download_hashJson.SafeAdd("sha256", _fileService.GetFileHashSha256(file));
                }

                Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

                return new Metadata(json);
            }

            return metadata;
        }
    }
}
