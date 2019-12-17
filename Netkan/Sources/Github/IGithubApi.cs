using System;
using System.Collections.Generic;

namespace CKAN.NetKAN.Sources.Github
{
    internal interface IGithubApi
    {
        GithubRepo GetRepo(GithubRef reference);
        GithubRelease GetLatestRelease(GithubRef reference);
        IEnumerable<GithubRelease> GetAllReleases(GithubRef reference);
        List<GithubUser> getOrgMembers(GithubUser organization);
        string DownloadText(Uri url);
    }
}
