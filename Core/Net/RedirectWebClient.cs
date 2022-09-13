using System;
using System.Net;

namespace CKAN
{
    // HACK: The ancient WebClient doesn't support setting the request type to HEAD and WebRequest doesn't support
    // setting the User-Agent header.
    // Maybe one day we'll be able to use HttpClient (https://msdn.microsoft.com/en-us/library/system.net.http.httpclient%28v=vs.118%29.aspx)
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
