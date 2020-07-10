using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CurlSharp;
using log4net;


namespace CKAN
{
    /// <summary>
    /// Utility layer on top of curlsharp for common operations.
    /// </summary>
    public static class Curl
    {
        private static bool _initComplete;
        private static readonly ILog Log = LogManager.GetLogger(typeof (Curl));

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
            try
            {
                CurlSharp.Curl.GlobalInit(CurlInitFlag.All);
                _initComplete = true;
            }
            catch (DllNotFoundException exc)
            {
                Log.Info("Curl initialization failed. Maybe you should install the DLL?\r\n\r\nhttps://github.com/KSP-CKAN/CKAN/wiki/libcurl");
            }
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

        private static CurlSlist GetHeaders(string token)
        {
            var l = new CurlSlist();
            if (!string.IsNullOrEmpty(token))
            {
                l.Append($"Authorization: token {token}");
            }
            return l;
        }

        /// <summary>
        /// Creates a CurlEasy object that calls the given writeback function
        /// when data is received.
        /// Can also write back the header.
        /// </summary>
        /// <returns>The CurlEasy object</returns>
        ///
        /// Adapted from MultiDemo.cs in the curlsharp repo
        public static CurlEasy CreateEasy(string url, string authToken, CurlWriteCallback wf, CurlHeaderCallback hwf = null)
        {
            if (!_initComplete)
            {
                Log.Warn("Curl environment not pre-initialised, performing non-threadsafe init.");
                Init();
            }

            var easy = new CurlEasy()
            {
                Url = url,
                WriteData = null,
                WriteFunction = wf,
                Encoding = "deflate, gzip",
                // Follow redirects
                FollowLocation = true,
                UserAgent = Net.UserAgentString,
                SslVerifyPeer = true,
                HeaderData = null,
                HttpHeader = GetHeaders(authToken),
            };
            if (hwf != null)
            {
                easy.HeaderFunction = hwf;
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
        /// Can call a writeback function for the header.
        /// </summary>
        public static CurlEasy CreateEasy(string url, string authToken, FileStream stream, CurlHeaderCallback hwf = null)
        {
            // Let's make a happy closure around this stream!
            return CreateEasy(url, authToken, delegate(byte[] buf, int size, int nmemb, object extraData)
            {
                stream.Write(buf, 0, size * nmemb);
                return size * nmemb;
            }, hwf);
        }

        public static CurlEasy CreateEasy(Uri url, string authToken, FileStream stream)
        {
            // Curl interacts poorly with KS for some (but not all) modules unless
            // the original string is used, hence .OriginalString rather than .ToString
            // here.
            return CreateEasy(url.OriginalString, authToken, stream);
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
