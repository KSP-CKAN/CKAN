using System;
using System.Net;
using CKAN.NetKAN.Services;
using log4net;
using Newtonsoft.Json;

namespace CKAN.NetKAN.Sources.Curse
{
    internal sealed class CurseApi : ICurseApi
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (CurseApi));

        public static string CurseApiBase = "https://widget.mcf.li/project/";
        public static string CurseApiEnd = ".json";

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

            if (error.code != 0)
            {
                var errorMessage = string.Format("Could not get the mod from Curse, reason: {0}.", error.message);
                throw new Kraken(errorMessage);
            }
            else if (!error.game.Equals("Kerbal Space Program"))
            {
                throw new Kraken("Could not get the mod from Curse, reason: Specified id is not a KSP mod");
            }

            return CurseMod.FromJson(json);
        }

        public static Uri ResolveRedirect(Uri url)
        {
            Uri redirUrl = url;
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(redirUrl);
            request.AllowAutoRedirect = false;
            request.UserAgent = Net.UserAgentString;
            HttpWebResponse response = (HttpWebResponse) request.GetResponse();
            response.Close();
            while (response.Headers["Location"] != null)
            {
                redirUrl = new Uri(redirUrl, response.Headers["Location"]);
                request = (HttpWebRequest) WebRequest.Create(redirUrl);
                request.AllowAutoRedirect = false;
                request.UserAgent = Net.UserAgentString;
                response = (HttpWebResponse) request.GetResponse();
                response.Close();
            }
            return redirUrl;
        }

        private string Call(int modid)
        {
            var url = CurseApiBase + modid + CurseApiEnd;

            Log.DebugFormat("Calling {0}", url);

            return _http.DownloadText(new Uri(url));
        }
    }
}