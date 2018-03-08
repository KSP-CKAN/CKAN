using System;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Extensions;
using log4net;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that adds the size of the download.
    /// </summary>
    internal sealed class DownloadAttributeTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DownloadAttributeTransformer));

        private readonly IHttpService _http;
        private readonly IFileService _fileService;

        public string Name { get { return "download_attributes"; } }

        public DownloadAttributeTransformer(IHttpService http, IFileService fileService)
        {
            _http = http;
            _fileService = fileService;
        }

        public Metadata Transform(Metadata metadata)
        {
            if (metadata.Download != null)
            {
                var json = metadata.Json();

                Log.InfoFormat("Executing Download attribute transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

                string file = _http.DownloadPackage(metadata.Download, metadata.Identifier, metadata.RemoteTimestamp);

                if (file != null)
                {
                    json["download_size"] = _fileService.GetSizeBytes(file);

                    json["download_hash"] = new JObject();

                    var download_hashJson = (JObject)json["download_hash"];
                    download_hashJson.SafeAdd("sha1", _fileService.GetFileHashSha1(file));
                    download_hashJson.SafeAdd("sha256", _fileService.GetFileHashSha256(file));

                    json["download_content_type"] = _fileService.GetMimetype(file);
                }

                Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

                return new Metadata(json);
            }

            return metadata;
        }
    }
}
