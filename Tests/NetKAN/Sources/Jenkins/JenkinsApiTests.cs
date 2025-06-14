using NUnit.Framework;

using CKAN;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Sources.Jenkins;

using Tests.Data;

namespace Tests.NetKAN.Sources.Jenkins
{
    [TestFixture]
    [Category("Online")]
    public sealed class JenkinsApiTests
    {
        [Test]
        public void GetLatestBuild_ModuleManager_Works()
        {
            // Arrange
            using (var dir   = new TemporaryDirectory())
            using (var cache = new NetFileCache(dir.Path.FullName))
            {
                var sut        = new JenkinsApi(new CachingHttpService(cache));
                var jenkinsRef = new JenkinsRef("#/ckan/jenkins/https://ksp.sarbian.com/jenkins/job/ModuleManager/");

                // Act
                var build = sut.GetLatestBuild(jenkinsRef, new JenkinsOptions());

                // Assert
                Assert.IsNotNull(build?.Url);
                Assert.IsNotNull(build?.Artifacts);
                Assert.IsNotNull(build?.Timestamp);
            }
        }
    }
}
