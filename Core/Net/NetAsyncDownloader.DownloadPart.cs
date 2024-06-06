using System;
using System.ComponentModel;

using Autofac;

using CKAN.Configuration;

namespace CKAN
{
    public partial class NetAsyncDownloader
    {
        // Private utility class for tracking downloads
        private class DownloadPart
        {
            public readonly DownloadTarget target;

            public DateTime  lastProgressUpdateTime;
            public long      lastProgressUpdateSize;
            public long      bytesLeft;
            public long      size;
            public long      bytesPerSecond;
            public Exception error;

            // Number of target URLs already tried and failed
            private int triedDownloads;

            /// <summary>
            /// Percentage, bytes received, total bytes to receive
            /// </summary>
            public event Action<int, long, long>                         Progress;
            public event Action<object, AsyncCompletedEventArgs, string> Done;

            private string mimeType => target.mimeType;
            private ResumingWebClient agent;

            public DownloadPart(DownloadTarget target)
            {
                this.target = target;
                size = bytesLeft = target.size;
                lastProgressUpdateTime = DateTime.Now;
                triedDownloads = 0;
            }

            public void Download()
            {
                var url = CurrentUri;
                ResetAgent();
                // Check whether to use an auth token for this host
                if (url.IsAbsoluteUri
                    && ServiceLocator.Container.Resolve<IConfiguration>().TryGetAuthToken(url.Host, out string token)
                        && !string.IsNullOrEmpty(token))
                {
                    log.InfoFormat("Using auth token for {0}", url.Host);
                    // Send our auth token to the GitHub API (or whoever else needs one)
                    agent.Headers.Add("Authorization", $"token {token}");
                }
                target.DownloadWith(agent, url);
            }

            public Uri CurrentUri => target.urls[triedDownloads];

            public bool HaveMoreUris => triedDownloads + 1 < target.urls.Count;

            public void NextUri()
            {
                if (HaveMoreUris)
                {
                    ++triedDownloads;
                }
            }

            public void Abort()
            {
                agent?.CancelAsyncOverridden();
            }

            private void ResetAgent()
            {
                // This WebClient child class does some complicated stuff, let's keep using it for now
                #pragma warning disable SYSLIB0014
                agent = new ResumingWebClient();
                #pragma warning restore SYSLIB0014

                agent.Headers.Add("User-Agent", Net.UserAgentString);

                // Tell the server what kind of files we want
                if (!string.IsNullOrEmpty(mimeType))
                {
                    log.InfoFormat("Setting MIME type {0}", mimeType);
                    agent.Headers.Add("Accept", mimeType);
                }

                // Forward progress and completion events to our listeners
                agent.DownloadProgressChanged += (sender, args) =>
                {
                    Progress?.Invoke(args.ProgressPercentage, args.BytesReceived, args.TotalBytesToReceive);
                };
                agent.DownloadProgress += (percent, bytesReceived, totalBytesToReceive) =>
                {
                    Progress?.Invoke(percent, bytesReceived, totalBytesToReceive);
                };
                agent.DownloadFileCompleted += (sender, args) =>
                {
                    Done?.Invoke(sender, args,
                                 args.Cancelled || args.Error != null
                                     ? null
                                     : agent.ResponseHeaders?.Get("ETag")?.Replace("\"", ""));
                };
            }
        }
    }
}
