using System;
using System.IO;
using System.Xml;
using System.ServiceModel.Syndication;

using CKAN.NetKAN.Services;

namespace CKAN.NetKAN.Sources.SourceForge
{
    internal sealed class SourceForgeApi : ISourceForgeApi
    {
        public SourceForgeApi(IHttpService httpSvc)
        {
            this.httpSvc = httpSvc;
        }

        public SourceForgeMod GetMod(SourceForgeRef sfRef)
            => new SourceForgeMod(sfRef,
                                  SyndicationFeed.Load(XmlReader.Create(new StringReader(
                                      httpSvc.DownloadText(new Uri(
                                          $"https://sourceforge.net/projects/{sfRef.Name}/rss"))
                                      ?? ""))));

        private readonly IHttpService httpSvc;
    }
}
