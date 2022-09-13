using System;
using System.IO;
using System.Net;
using System.ComponentModel;
using System.Threading.Tasks;

using log4net;

using CKAN.Extensions;

namespace CKAN
{
    internal class ResumingWebClient : WebClient
    {
        /// <summary>
        /// A version of DownloadFileAsync that appends to its destination
        /// file if it already exists and skips downloading the bytes
        /// we already have.
        /// </summary>
        /// <param name="url">What to download</param>
        /// <param name="path">Where to save it</param>
        public void DownloadFileAsyncWithResume(Uri url, string path)
        {
            Task.Factory.StartNew(() =>
            {
                var fi = new FileInfo(path);
                if (fi.Exists)
                {
                    log.DebugFormat("File exists at {0}, {1} bytes", path, fi.Length);
                    bytesToSkip = fi.Length;
                }
                else
                {
                    // Reset in case we try multiple with same webclient
                    bytesToSkip = 0;
                }
                // Ideally the bytes to skip would be passed in the userToken param,
                // but GetWebRequest can't access it!!
                OpenReadAsync(url, path);
            });
        }

        /// <summary>
        /// Same as DownloadProgressChanged, but usable by us.
        /// Called with percent, bytes received, total bytes to receive.
        ///
        /// DownloadProgressChangedEventArg has an internal constructor
        /// and readonly properties, and everyplace that does make one
        /// is private instead of protected, so we have to reinvent this wheel.
        /// (Meanwhile AsyncCompletedEventArgs has none of these problems.)
        /// </summary>
        public event Action<int, long, long> DownloadProgress;

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request is HttpWebRequest webRequest && bytesToSkip > 0)
            {
                log.DebugFormat("Skipping {0} bytes of {1}", bytesToSkip, address);
                webRequest.AddRange(bytesToSkip);
            }
            return request;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            try
            {
                var response = base.GetWebResponse(request, result);
                contentLength = response.ContentLength;
                return response;
            }
            catch (WebException wexc)
            when (wexc.Status == WebExceptionStatus.ProtocolError
                  && wexc.Response is HttpWebResponse response
                  && response.StatusCode == HttpStatusCode.RequestedRangeNotSatisfiable)
            {
                log.Debug("GetWebResponse failed with range error, closing stream and suppressing exception");
                // Don't save the error page into a file
                response.Close();
                return response;
            }
            catch (Exception exc)
            {
                log.Debug("Failed to get web response", exc);
                OnDownloadFileCompleted(new AsyncCompletedEventArgs(exc, false, null));
                throw;
            }
        }

        protected override void OnOpenReadCompleted(OpenReadCompletedEventArgs e)
        {
            base.OnOpenReadCompleted(e);
            if (!e.Cancelled && e.Error == null)
            {
                var destination = e.UserState as string;
                using (var netStream = e.Result)
                {
                    if (!netStream.CanRead)
                    {
                        log.Debug("OnOpenReadCompleted got closed stream, skipping download");
                    }
                    else
                    {
                        log.DebugFormat("OnOpenReadCompleted got open stream, appending to {0}", destination);
                        using (var fileStream = new FileStream(destination, FileMode.Append, FileAccess.Write))
                        {
                            netStream.CopyTo(fileStream, new Progress<long>(bytesDownloaded =>
                            {
                                DownloadProgress?.Invoke(100 * (int)(bytesDownloaded / contentLength),
                                                         bytesDownloaded, contentLength);
                            }));
                        }
                    }
                }
            }
            OnDownloadFileCompleted(new AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
        }

        private long bytesToSkip   = 0;
        private long contentLength = 0;
        private static readonly ILog log = LogManager.GetLogger(typeof(ResumingWebClient));
    }
}
