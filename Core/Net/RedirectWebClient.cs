using System;
using System.Net;

#pragma warning disable SYSLIB0014

namespace CKAN
{
    // HttpClient doesn't handle redirects well on Mono, but net7.0 considers WebClient obsolete
    internal sealed class RedirectWebClient : WebClient
    {
        public RedirectWebClient()
        {
            Headers.Add("User-Agent", Net.UserAgentString);
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var webRequest = (HttpWebRequest)base.GetWebRequest(address);
            webRequest.AllowAutoRedirect = false;
            webRequest.Method = "HEAD";

            return webRequest;
        }
    }
}
