using System;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using log4net;
using MonoTorrent.Common;
using System.IO;
using System.Linq;
using MonoTorrent.BEncoding;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that generates a .torrent file and sets the btih field.
    /// </summary>
    internal sealed class TorrentTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(JenkinsTransformer));

        private IHttpService _http;
        private String _torrentPath;
        public TorrentTransformer(IHttpService http, string torrentPath)
        {
            _http = http;
            _torrentPath = torrentPath;
        }

        public Metadata Transform(Metadata metadata){
            var json = metadata.Json();

            if (!CKAN.TorrentDownloader.IsTorrentable(new string[]{ json["license"].ToString() }))
            {
                Log.InfoFormat("Torrent transformation of {0} skipped due to licensing concerns.", metadata.Kref);
                return metadata;
            }
            Log.InfoFormat("Executing Torrent transformation with {0}", metadata.Kref);
            Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

            string filesource = _http.DownloadPackage(metadata.Download, metadata.Identifier);

            TorrentCreator tc = new TorrentCreator();
            var dic = tc.Create(new TorrentFileSource(filesource));

            string filename = String.Format(
                "{0}-{1}-{2}{3}",
                NetFileCache.CreateURLHash(metadata.Download),
                metadata.Identifier,
                metadata.Version,
                Path.GetExtension(filesource));

            BEncodedDictionary info = (BEncodedDictionary)dic["info"];
            info["name"] = new BEncodedString(filename);
            dic["info"] = info;
            dic["created by"] = new BEncodedString("NetKAN");

            Torrent t = Torrent.Load(dic);

            json["btih"] = t.InfoHash.ToHex();
            Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

            // save .torrent file
            string savepath = Path.Combine(_torrentPath, filename + ".torrent");

            return new Metadata(json);
        }
    }
}

