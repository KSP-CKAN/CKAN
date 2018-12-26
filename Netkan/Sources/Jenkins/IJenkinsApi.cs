namespace CKAN.NetKAN.Sources.Jenkins
{
    internal interface IJenkinsApi
    {
        JenkinsBuild GetLatestBuild(JenkinsRef reference, JenkinsOptions options);
    }
}
