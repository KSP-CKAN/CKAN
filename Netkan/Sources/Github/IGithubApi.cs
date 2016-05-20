namespace CKAN.NetKAN.Sources.Github
{
    internal interface IGithubApi
    {
        GithubRelease GetLatestRelease(GithubRef reference);
    }
}