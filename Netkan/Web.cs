using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CurlSharp;
using log4net;

namespace CKAN.NetKAN
{
    public class Web :IDisposable
    {
        private static bool init_complete = false;
        private readonly CurlEasy easy;
        private static readonly ILog log = LogManager.GetLogger(typeof (Web));

        /// <summary>
        /// Initializes a new instance of the <see cref="CKAN.NetKAN.Web"/> class.
        /// </summary>
        public Web()
        {
            if (!init_complete)
            {
                CurlSharp.Curl.GlobalInit(CurlInitFlag.All);
                init_complete = true;
            }

            easy = new CurlEasy
            {
                UserAgent = Net.UserAgentString,
                CaInfo = ResolveCurlCaBundle()
            };
        }

        /// <summary>
        /// Takes a URL, and returns the content found there as a string.
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="url">URL.</param>
        public string DownloadString(string url)
        {
            log.DebugFormat ("About to download {0}", url);

            var content = string.Empty;

            easy.Url = url;
            easy.WriteData = null;
            easy.WriteFunction = delegate(byte[] buf, int size, int nmemb, object extraData) {
                content += Encoding.UTF8.GetString(buf);
                return size*nmemb;
            };

            CurlCode result = easy.Perform();

            if (result != CurlCode.Ok)
            {
                throw new Exception("Curl download failed with error " + result);
            }

            log.DebugFormat ("Download from {0}:\n\n{1}", url, content);

            return content;
        }

        /// <summary>
        /// Releases all resource used by the <see cref="CKAN.NetKAN.Web"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="CKAN.NetKAN.Web"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="CKAN.NetKAN.Web"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the <see cref="CKAN.NetKAN.Web"/> so the garbage
        /// collector can reclaim the memory that the <see cref="CKAN.NetKAN.Web"/> was occupying.</remarks>
        public void Dispose()
        {
            CurlSharp.Curl.GlobalCleanup();
            easy.Dispose();
        }

        /// <summary>
        /// Resolves the location of the cURL CA bundle file to use.
        /// </summary>
        /// <returns>The absolute file path to the bundle file or null if none is found.</returns>
        private static string ResolveCurlCaBundle()
        {
            const string caBundleFileName = "curl-ca-bundle.crt";
            const string ckanSubDirectoryName = "CKAN";

            string bundle = new[]
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

            log.InfoFormat("Using curl-ca bundle: {0}",bundle ?? "(none)");

            return bundle;
        }
    }
}

