using System;
using System.Net;
using CKAN.NetKAN.Services;
using log4net;
using Newtonsoft.Json;

namespace CKAN.NetKAN.Sources.Curse
{
    internal sealed class CurseApi : ICurseApi
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CurseApi));

        private static string CurseApiBase = "https://api.cfwidget.com/project/";

        private readonly IHttpService _http;

        public CurseApi(IHttpService http)
        {
            _http = http;
        }

        public CurseMod GetMod(int modId)
        {
            var json = Call(modId);

            // Check if the mod has been removed from Curse and if it corresponds to a KSP mod.
            var error = JsonConvert.DeserializeObject<CurseError>(json);
            if (!string.IsNullOrWhiteSpace(error.error))
            {
                throw new Kraken(string.Format(
                    "Could not get the mod from Curse, reason: {0}.",
                    error.message
                ));
            }

            return CurseMod.FromJson(json);
        }

        public static Uri ResolveRedirect(Uri url)
        {
            Uri redirUrl = url;
            int redirects = 0;
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(redirUrl);
            request.AllowAutoRedirect = false;
            request.UserAgent = Net.UserAgentString;
            HttpWebResponse response = (HttpWebResponse) request.GetResponse();
            response.Close();
            while (response.Headers["Location"] != null)
            {
                redirects++;
                if (redirects > 6)
                    throw new Kraken("More than 6 redirects when resolving the following url: " + url);
                redirUrl = new Uri(redirUrl, response.Headers["Location"]);
                request = (HttpWebRequest) WebRequest.Create(redirUrl);
                request.AllowAutoRedirect = false;
                request.UserAgent = Net.UserAgentString;
                response = (HttpWebResponse) request.GetResponse();
                response.Close();
            }
            return redirUrl;
        }

        private string Call(int modId)
        {
            var url = CurseApiBase + modId;

            Log.DebugFormat("Calling {0}", url);

            return _http.DownloadText(new Uri(url));
        }
    }
}
