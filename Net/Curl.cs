using System;
using System.IO;
using CurlSharp;
using log4net;


namespace CKAN
{
    /// <summary>
    /// Utility layer on top of curlsharp for common operations.
    /// </summary>
    public static class Curl
    {
        private static bool init_complete = false;
        private static readonly ILog log = LogManager.GetLogger(typeof (Curl));

        /// <summary>
        /// Has libcurl do all the work it needs to work correctly.
        /// NOT THREADSAFE AT ALL. Do this before forking any threads!
        /// </summary>
        public static void Init()
        {
            if (init_complete)
            {
                log.Info("Curl init already performed, not running twice");
                return;
            }
            CurlSharp.Curl.GlobalInit(CurlInitFlag.All);
            init_complete = true;
        }

        /// <summary>
        /// Release any resources used by libcurl. NOT THREADSAFE AT ALL.
        /// Do this after all other threads are done. 
        /// </summary>
        public static void CleanUp()
        {
            CurlSharp.Curl.GlobalCleanup();
            init_complete = false;
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
            if (!init_complete)
            {
                log.Warn("Curl environment not pre-initialised, performing non-threadsafe init.");
                Init();
            }

            var easy = new CurlEasy();
            easy.Url = url;
            easy.WriteData = null;
            easy.WriteFunction = wf;
            easy.Encoding = "deflate, gzip";
            return easy;
        }

        /// <summary>
        /// Creates a CurlEasy object that writes to the given stream.
        /// </summary>
        public static CurlEasy CreateEasy(string url, FileStream stream)
        {
            // Let's make a happy closure around this stream!
            return CreateEasy(url, delegate(byte[] buf, int size, int nmemb, object extraData)
            {
                stream.Write(buf, 0, size * nmemb);
                return size * nmemb;
            });
        }

        public static CurlEasy CreateEasy(Uri url, FileStream stream)
        {
            return CreateEasy(url.ToString(), stream);
        }
    }
}

