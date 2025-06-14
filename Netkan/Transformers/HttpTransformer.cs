using System;
using System.Collections.Generic;

using log4net;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that looks up data from an arbitrary HTTP endpoint.
    /// </summary>
    internal sealed class HttpTransformer : ITransformer
    {
        public HttpTransformer(IHttpService httpSvc, string? userAgent = null)
        {
            this.userAgent = userAgent;
            http           = httpSvc;
        }

        public string Name => "http";

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            if (metadata.Kref?.Source == "http")
            {
                Log.InfoFormat("Executing HTTP transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, metadata.AllJson);

                if (Uri.IsWellFormedUriString(metadata.Kref.Id, UriKind.Absolute))
                {
                    var resolvedUri = http.ResolveRedirect(new Uri(metadata.Kref.Id), userAgent);

                    Log.InfoFormat("URL {0} resolved to {1}", metadata.Kref.Id, resolvedUri);

                    if (resolvedUri != null)
                    {
                        var json = metadata.Json();
                        json.Remove("$kref");
                        json["download"] = resolvedUri.ToString();

                        Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

                        yield return new Metadata(json);
                        yield break;
                    }
                    else
                    {
                        throw new Kraken("Could not resolve HTTP $kref URL, exceeded number of redirects.");
                    }
                }
                else
                {
                    throw new Kraken("Invalid URL in HTTP $kref: " + metadata.Kref.Id);
                }
            }
            yield return metadata;
        }

        private readonly string?      userAgent;
        private readonly IHttpService http;

        private static readonly ILog Log = LogManager.GetLogger(typeof(HttpTransformer));
    }
}
