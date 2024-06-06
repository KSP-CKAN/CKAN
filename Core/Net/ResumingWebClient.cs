using System;
using System.IO;
using System.Net;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

using log4net;

using CKAN.Extensions;

// This WebClient child class does some complicated stuff, let's keep using it for now
#pragma warning disable SYSLIB0014

namespace CKAN
{
    public class ResumingWebClient : WebClient
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
                OpenReadAsync(url, path);
            });
        }

        public void DownloadFileAsyncWithResume(Uri url, Stream stream)
        {
            contentLength = 0;
            Task.Factory.StartNew(() =>
            {
                bytesToSkip = 0;
                OpenReadAsync(url, stream);
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
                        log.DebugFormat("OnOpenReadCompleted got closed stream or zero contentLength, skipping download to {0}",
                                        destination ?? "stream");
                        // Synthesize a progress update for 100% completion
                        if (!string.IsNullOrEmpty(destination)
                            && File.Exists(destination))
                        {
                            var fi = new FileInfo(destination);
                            DownloadProgress?.Invoke(100, fi.Length, fi.Length);
                        }
                    }
                    else
                    {
                        try
                        {
                            log.DebugFormat("OnOpenReadCompleted got open stream, appending to {0}",
                                            destination ?? "stream");
                            // file:// URLs don't support timeouts
                            if (netStream.CanTimeout)
                            {
                                log.DebugFormat("Default stream read timeout is {0}", netStream.ReadTimeout);
                                netStream.ReadTimeout = timeoutMs;
                            }
                            cancelTokenSrc = new CancellationTokenSource();
                            switch (e.UserState)
                            {
                                case string path:
                                    ToFile(netStream, path);
                                    break;
                                case Stream stream:
                                    ToStream(netStream, stream);
                                    break;
                            }
                            // Make sure caller knows we've finished
                            DownloadProgress?.Invoke(100, contentLength, contentLength);
                            cancelTokenSrc = null;
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

        private void ToFile(Stream netStream, string path)
        {
            using (var outStream = new FileStream(path, FileMode.Append, FileAccess.Write))
            {
                ToStream(netStream, outStream);
            }
        }

        private void ToStream(Stream netStream, Stream outStream)
        {
            netStream.CopyTo(outStream, new Progress<long>(bytesDownloaded =>
                {
                    DownloadProgress?.Invoke((int)(100 * bytesDownloaded / contentLength),
                                             bytesDownloaded, contentLength);
                }),
                TimeSpan.FromSeconds(5),
                cancelTokenSrc.Token);
        }

        /// <summary>
        /// Ideally the bytes to skip would be passed in the userToken param of OpenReadAsync,
        /// but GetWebRequest can't access it, so we store it here.
        /// </summary>
        private long bytesToSkip   = 0;
        private long contentLength = 0;
        private CancellationTokenSource cancelTokenSrc;

        private const int timeoutMs = 30 * 1000;
        private static readonly ILog log = LogManager.GetLogger(typeof(ResumingWebClient));
    }
}
