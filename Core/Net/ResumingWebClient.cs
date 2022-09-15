using System;
using System.IO;
using System.Net;
using System.ComponentModel;
using System.Threading;
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
            contentLength = 0;
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

        /// <summary>
        /// CancelAsync isn't virtual, so we make another function
        /// </summary>
        public void CancelAsyncOverridden()
        {
            if (cancelTokenSrc != null)
            {
                log.Debug("Cancellation requested, going through token");
                cancelTokenSrc?.Cancel();
            }
            else
            {
                log.Debug("Cancellation requested, using non-token means");
                CancelAsync();
            }
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request is HttpWebRequest webRequest && bytesToSkip > 0)
            {
                log.DebugFormat("Skipping {0} bytes of {1}", bytesToSkip, address);
                webRequest.AddRange(bytesToSkip);
                webRequest.ReadWriteTimeout = timeoutMs;
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
                log.DebugFormat("GetWebResponse failed with range error, closing stream for {0}", request.RequestUri);
                // Don't save the error page into a file
                response.Close();
                return response;
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
                    if (!netStream.CanRead || contentLength == 0)
                    {
                        log.DebugFormat("OnOpenReadCompleted got closed stream or zero contentLength, skipping download to {0}", destination);
                        // Synthesize a progress update for 100% completion
                        var fi = new FileInfo(destination);
                        DownloadProgress?.Invoke(100, fi.Length, fi.Length);
                    }
                    else
                    {
                        try
                        {
                            log.DebugFormat("OnOpenReadCompleted got open stream, appending to {0}", destination);
                            using (var fileStream = new FileStream(destination, FileMode.Append, FileAccess.Write))
                            {
                                try
                                {
                                    log.DebugFormat("Default stream read timeout is {0}", netStream.ReadTimeout);
                                    netStream.ReadTimeout = timeoutMs;
                                }
                                catch
                                {
                                    // file:// URLs don't support timeouts
                                }
                                cancelTokenSrc = new CancellationTokenSource();
                                netStream.CopyTo(fileStream, new Progress<long>(bytesDownloaded =>
                                    {
                                        DownloadProgress?.Invoke((int)(100 * bytesDownloaded / contentLength),
                                                                 bytesDownloaded, contentLength);
                                    }),
                                    cancelTokenSrc.Token);
                                DownloadProgress?.Invoke(100, contentLength, contentLength);
                                cancelTokenSrc = null;
                            }
                        }
                        catch (OperationCanceledException exc)
                        {
                            log.Debug("Cancellation token threw, sending cancel completion");
                            OnDownloadFileCompleted(new AsyncCompletedEventArgs(exc, true, e.UserState));
                            return;
                        }
                        catch (Exception exc)
                        {
                            OnDownloadFileCompleted(new AsyncCompletedEventArgs(exc, false, e.UserState));
                            return;
                        }
                    }
                }
            }
            OnDownloadFileCompleted(new AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
        }

        private long bytesToSkip   = 0;
        private long contentLength = 0;
        private CancellationTokenSource cancelTokenSrc;

        private const int timeoutMs = 30 * 1000;
        private static readonly ILog log = LogManager.GetLogger(typeof(ResumingWebClient));
    }
}
