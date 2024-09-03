using System;
using System.Linq;
using System.ServiceModel.Syndication;

namespace CKAN.NetKAN.Sources.SourceForge
{
    internal class SourceForgeMod
    {
        public SourceForgeMod(SourceForgeRef  sfRef,
                              SyndicationFeed feed)
        {
            Title          = feed.Title.Text;
            Description    = feed.Description.Text;
            HomepageLink   = $"https://sourceforge.net/projects/{sfRef.Name}/";
            RepositoryLink = $"https://sourceforge.net/p/{sfRef.Name}/code/";
            BugTrackerLink = $"https://sourceforge.net/p/{sfRef.Name}/bugs/";
            Versions       = feed.Items.Where(item => item.Title.Text.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                                       .Select(item => new SourceForgeVersion(item))
                                       .ToArray();
        }

        public readonly string               Title;
        public readonly string               Description;
        public readonly string               HomepageLink;
        public readonly string               RepositoryLink;
        public readonly string               BugTrackerLink;
        public readonly SourceForgeVersion[] Versions;
    }
}
