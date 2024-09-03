using System;
using System.Linq;
using System.ServiceModel.Syndication;

namespace CKAN.NetKAN.Sources.SourceForge
{
    internal class SourceForgeVersion
    {
        public SourceForgeVersion(SyndicationItem item)
        {
            Title     = item.Title.Text.TrimStart('/');
            // Throw an exception on missing or multiple <link/>s
            Link      = item.Links.Single().Uri;
            Timestamp = item.PublishDate;
        }

        public readonly string         Title;
        public readonly Uri            Link;
        public readonly DateTimeOffset Timestamp;
    }
}
