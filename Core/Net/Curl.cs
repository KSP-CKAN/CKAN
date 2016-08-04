using CurlSharp;
using log4net;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CKAN
{
    /// <summary>
    /// Utility layer on top of curlsharp for common operations.
    /// </summary>
    public static class Curl
    {
        private static bool _initComplete;
        private static readonly ILog Log = LogManager.GetLogger(typeof(Curl));

        /// <summary>
        /// Has libcurl do all the work it needs to work correctly.
        /// NOT THREADSAFE AT ALL. Do this before forking any threads!
        /// </summary>
        public static void Init()
        {
            if (_initComplete)
            {
                Log.Info("Curl init already performed, not running twice");
                return;
            }
            CurlSharp.Curl.GlobalInit(CurlInitFlag.All);
            _initComplete = true;
        }

        /// <summary>
        /// Release any resources used by libcurl. NOT THREADSAFE AT ALL.
        /// Do this after all other threads are done.
        /// </summary>
        public static void CleanUp()
        {
            CurlSharp.Curl.GlobalCleanup();
            _initComplete = false;
        }

        /// <summary>
        /// Creates a CurlEasy object that calls the given writeback function
        /// when data is received.
        /// </summary>
        /// <returns>The CurlEasy obect</returns>
        ///
        /// Adapted from MultiDemo.cs in the curlsharp repo
        public static CurlEasy CreateEasy(string url, CurlWriteCallback wf)
        {
            if (!_initComplete)
            {
                Log.Warn("Curl environment not pre-initialised, performing non-threadsafe init.");
                Init();
            }

            var easy = new CurlEasy();
            easy.Url = url;
            easy.WriteData = null;
            easy.WriteFunction = wf;
            easy.Encoding = "deflate, gzip";
            easy.FollowLocation = true; // Follow redirects
            easy.UserAgent = Net.UserAgentString;
            easy.SslVerifyPeer = true;

            // ksp.sarbian.com uses a SSL cert that libcurl can't
            // verify, so we skip verification. Yeah, that sucks, I know,
            // but this sucks less than our previous solution that disabled
            // SSL checking entirely.

            if (url.StartsWith("https://ksp.sarbian.com/"))
            {
                easy.SslVerifyPeer = false;
            }

            var caBundle = ResolveCurlCaBundle();
            if (caBundle != null)
            {
                easy.CaInfo = caBundle;
            }

            return easy;
        }

        /// <summary>
        /// Creates a CurlEasy object that writes to the given stream.
        /// </summary>
        public static CurlEasy CreateEasy(string url, FileStream stream)
        {
            // Let's make a happy closure around this stream!
            return CreateEasy(url, delegate (byte[] buf, int size, int nmemb, object extraData)
            {
                stream.Write(buf, 0, size * nmemb);
                return size * nmemb;
            });
        }

        public static CurlEasy CreateEasy(Uri url, FileStream stream)
        {
            // Curl interacts poorly with KS for some (but not all) modules unless
            // the original string is used, hence .OriginalString rather than .ToString
            // here.
            return CreateEasy(url.OriginalString, stream);
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

            Log.InfoFormat("Using curl-ca bundle: {0}", bundle ?? "(none)");

            return bundle;
        }
    }
}