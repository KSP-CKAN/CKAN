using System.Collections.Generic;

using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Sources.Jenkins
{
    internal interface IJenkinsApi
    {
        JenkinsBuild? GetLatestBuild(JenkinsRef reference, JenkinsOptions options);
        IEnumerable<JenkinsBuild> GetAllBuilds(JenkinsRef reference, JenkinsOptions options);
    }
}
