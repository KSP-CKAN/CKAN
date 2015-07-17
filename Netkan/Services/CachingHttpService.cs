using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CurlSharp;
using log4net;

namespace CKAN.NetKAN.Services
{
    internal sealed partial class CachingHttpService : IHttpService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CachingHttpService));

        private readonly NetFileCache _cache;
        private CurlEasy _curl;

        public CachingHttpService(NetFileCache cache)
        {
            _cache = cache;

            CurlSharp.Curl.GlobalInit(CurlInitFlag.All);

            _curl = new CurlEasy { UserAgent = CKAN.Net.UserAgentString };

            var caBundle = ResolveCurlCaBundle();
            if (caBundle != null)
            {
                _curl.CaInfo = caBundle;
            }
        }

        public string DownloadPackage(Uri url, string identifier)
        {
            EnsureNotDisposed();

            return _cache.GetCachedFilename(url) ?? 
                _cache.Store(url, CKAN.Net.Download(url), string.Format("netkan-{0}.zip", identifier), move: true);
        }

        public string DownloadText(Uri url)
        {
            EnsureNotDisposed();

            Log.DebugFormat("About to download {0}", url);

            var content = string.Empty;

            _curl.Url = url.ToString();
            _curl.WriteData = null;
            _curl.WriteFunction = delegate(byte[] buf, int size, int nmemb, object extraData)
            {
                content += Encoding.UTF8.GetString(buf);
                return size * nmemb;
            };

            var result = _curl.Perform();

            if (result != CurlCode.Ok)
            {
                throw new Exception("Curl download failed with error " + result);
            }

            Log.DebugFormat("Download from {0}:\n\n{1}", url, content);

            return content;
        }

        /// <summary>
        /// Resolves the location of the cURL CA bundle file to use.
        /// </summary>
        /// <returns>The absolute file path to the bundle file or null if none is found.</returns>
        private static string ResolveCurlCaBundle()
        {
            const string caBundleFileName = "curl-ca-bundle.crt";
            const string ckanSubDirectoryName = "CKAN";

            var bundle = new[]
            {
                // Working Directory
                Environment.CurrentDirectory,

                // Executable Directory
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),

                // %LOCALAPPDATA%/CKAN
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ckanSubDirectoryName),

                // %APPDATA%/CKAN
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ckanSubDirectoryName),

                // %PROGRAMDATA%/CKAN
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), ckanSubDirectoryName),
            }
            .Select(i => Path.Combine(i, caBundleFileName))
            .FirstOrDefault(File.Exists);

            Log.InfoFormat("Using curl-ca bundle: {0}",bundle ?? "(none)");

            return bundle;
        }
    }

    internal sealed partial class CachingHttpService
    {
        private bool _isDisposed;

        ~CachingHttpService()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_curl != null)
                {
                    CurlSharp.Curl.GlobalCleanup();
                    _curl.Dispose();
                    _curl = null;
                }
            }

            _isDisposed = true;
        }

        private void EnsureNotDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(typeof(CachingHttpService).FullName);
            }
        }
    }
}
