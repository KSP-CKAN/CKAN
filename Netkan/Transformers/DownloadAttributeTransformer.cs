using System;
using System.Collections.Generic;

using log4net;
using Newtonsoft.Json.Linq;

using CKAN.Versioning;
using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;

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

        public string Name => "download_attributes";

        public DownloadAttributeTransformer(IHttpService http, IFileService fileService)
        {
            _http = http;
            _fileService = fileService;
        }

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            if (metadata.Download != null)
            {

                Log.Debug("Executing Download attribute transformation");
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, metadata.AllJson);

                var file = _http.DownloadModule(metadata);

                if (file != null)
                {
                    var json = metadata.Json();
                    Log.Debug("Calculating download size...");
                    json["download_size"] = _fileService.GetSizeBytes(file);

                    var download_hashJson = new JObject();

                    // Older clients will complain if download_hash is set without sha1
                    var specVersion = SpecVersionAnalyzer.MinimumSpecVersion(json);
                    if (specVersion <= v1p34)
                    {
                        Log.InfoFormat("Spec version is {0}, calculating download SHA1...",
                                       specVersion);
                        download_hashJson.SafeAdd("sha1", _fileService.GetFileHashSha1(file));
                    }
                    Log.Debug("Calculating download SHA256...");
                    download_hashJson.SafeAdd("sha256", _fileService.GetFileHashSha256(file));
                    json["download_hash"] = download_hashJson;

                    Log.Debug("Calculating download MIME type...");
                    json.SafeAdd("download_content_type", _fileService.GetMimetype(file));
                    Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);
                    yield return new Metadata(json);
                    yield break;
                }
            }
            yield return metadata;
        }
        private static readonly ModuleVersion v1p34 = new ModuleVersion("v1.34");
    }
}
