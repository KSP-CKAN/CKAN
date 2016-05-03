using System;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using log4net;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that adds a hash of the downloaded file.
    /// </summary>
    internal sealed class DownloadHashTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DownloadHashTransformer));

        private readonly IHttpService _http;
        private readonly IFileHash _fileHash;

        public string Name { get { return "download_hash"; } }

        public DownloadHashTransformer(IHttpService http, IFileHash fileHash)
        {
            _http = http;
            _fileHash = fileHash;
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
                    json["download_hash"] = _fileHash.GetFileHash(file);
                }

                Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

                return new Metadata(json);
            }

            return metadata;
        }
    }
}
