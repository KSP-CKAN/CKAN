using System.Collections.Generic;

namespace CKAN.NetKAN.Sources.Jenkins
{
    internal interface IJenkinsApi
    {
        JenkinsBuild GetLatestBuild(JenkinsRef reference, JenkinsOptions options);
        IEnumerable<JenkinsBuild> GetAllBuilds(JenkinsRef reference, JenkinsOptions options);
    }
}
