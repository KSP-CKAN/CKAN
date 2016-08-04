namespace CKAN.NetKAN.Sources.Github
{
    internal interface IGithubApi
    {
        GithubRepo GetRepo(GithubRef reference);

        GithubRelease GetLatestRelease(GithubRef reference);
    }
}