using System;
using System.IO;
using System.Net;
using System.Text;

using log4net;
using Newtonsoft.Json;

using CKAN.NetKAN.Services;

namespace CKAN.NetKAN.Sources.Curse
{
    internal sealed class CurseApi : ICurseApi
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CurseApi));

        private const string CurseApiBaseOld = "https://api.cfwidget.com/project/";
        private const string CurseApiBase    = "https://api.cfwidget.com/kerbal/ksp-mods/";

        private readonly IHttpService _http;

        public CurseApi(IHttpService http)
        {
            _http = http;
        }

        public CurseMod GetMod(string nameOrId)
        {
            string json;
            try
            {
                json = Call(nameOrId);
            }
            catch (WebException e)
            {
                // CurseForge returns a valid json with an error message in some cases.
                json = new StreamReader(e.Response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
            }
            // Check if the mod has been removed from Curse and if it corresponds to a KSP mod.
            var error = JsonConvert.DeserializeObject<CurseError>(json);
            if (!string.IsNullOrWhiteSpace(error.error))
            {
                throw new Kraken($"Could not get the mod from Curse, reason: {error.message}.");
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

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse) request.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    response = e.Response as HttpWebResponse;
                    if (response?.StatusCode == HttpStatusCode.Forbidden)
                    {
                        throw new Kraken("CKAN blocked by CurseForge");
                    }
                }
                throw;
            }
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

        private string Call(string nameOrId)
        {
            // If it's numeric, use the old URL format,
            // otherwise use the new.
            var url = Int32.TryParse(nameOrId, out int id)
                ? CurseApiBaseOld + id
                : CurseApiBase + nameOrId;
            Log.InfoFormat("Calling {0}", url);
            return _http.DownloadText(new Uri(url));
        }
    }
}
