using System;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
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
                    var sha1 = new JObject();
                    sha1.Add("type", "sha1");
                    sha1.Add("digest", _fileService.GetFileHashSha1(file));
                    
                    var sha256 = new JObject();
                    sha256.Add("type", "sha256");
                    sha256.Add("digest", _fileService.GetFileHashSha256(file));
                    
                    json["download_hash"] = new JArray();

                    var download_hashJson = (JArray)json["download_hash"];

                    download_hashJson.Add(sha1);
                    download_hashJson.Add(sha256);
                }

                Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

                return new Metadata(json);
            }

            return metadata;
        }
    }
}
