using System;
using System.Security.Cryptography;

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

            public long       bytesLeft;
            public long       size;
            public Exception? error;

            // Number of target URLs already tried and failed
            private int triedDownloads;

            /// <summary>
            /// Percentage, bytes received, total bytes to receive
            /// </summary>
            public event Action<DownloadPart, long, long>?                        Progress;
            public event Action<DownloadPart, Exception?, bool, string?, string>? Done;

            private          string             mimeType => target.mimeType;
            private readonly string             userAgent;
            private readonly HashAlgorithm?     hasher;
            private          ResumingWebClient? agent;

            public DownloadPart(DownloadTarget target,
                                string         userAgent,
                                HashAlgorithm? hasher)
            {
                this.target    = target;
                this.userAgent = userAgent ?? "";
                this.hasher    = hasher;
                size = bytesLeft = target.size;
                triedDownloads = 0;
            }

            public void Download()
            {
                var url = CurrentUri;
                ResetAgent();
                // Check whether to use an auth token for this host
                if (agent != null)
                {
                    if (url.IsAbsoluteUri
                        && ServiceLocator.Container.Resolve<IConfiguration>().TryGetAuthToken(url.Host, out string? token)
                            && !string.IsNullOrEmpty(token))
                    {
                        log.InfoFormat("Using auth token for {0}", url.Host);
                        // Send our auth token to the GitHub API (or whoever else needs one)
                        agent.Headers.Add("Authorization", $"token {token}");
                    }
                    // Raise the initial progress report if we know the size
                    if (size > 0)
                    {
                        Progress?.Invoke(this, 0, size);
                    }
                    target.DownloadWith(agent, url, hasher);
                }
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

                agent.Headers.Add("User-Agent", userAgent);

                // Tell the server what kind of files we want
                if (!string.IsNullOrEmpty(mimeType))
                {
                    log.InfoFormat("Setting MIME type {0}", mimeType);
                    agent.Headers.Add("Accept", mimeType);
                }

                // Forward progress and completion events to our listeners
                agent.DownloadProgressChanged += (sender, args) =>
                {
                    Progress?.Invoke(this, args.BytesReceived, args.TotalBytesToReceive);
                };
                agent.DownloadProgress += (percent, bytesReceived, totalBytesToReceive) =>
                {
                    Progress?.Invoke(this, bytesReceived, totalBytesToReceive);
                };
                agent.DownloadFileCompleted += (sender, args) =>
                {
                    Done?.Invoke(this, args.Error, args.Cancelled,
                                 args.Cancelled || args.Error != null
                                     ? null
                                     : agent.ResponseHeaders?.Get("ETag")?.Replace("\"", ""),
                                 args.Cancelled || args.Error != null
                                     ? ""
                                     : BitConverter.ToString(hasher?.Hash ?? Array.Empty<byte>())
                                                   .Replace("-", ""));
                };
            }
        }
    }
}
