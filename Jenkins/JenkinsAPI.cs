using System;
using System.Net;
using System.Text.RegularExpressions;
using log4net;
using Newtonsoft.Json;

namespace CKAN.NetKAN
{
    // Jenkins API
    public class JenkinsAPI
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (JenkinsAPI));
        private static readonly WebClient web = new WebClient();

        public JenkinsAPI()
        {
            web.Headers.Add("user-agent", Net.UserAgentString);
        }

        public static string Call(string path)
        {
            string result = "";
            return result;
        }
    }
}