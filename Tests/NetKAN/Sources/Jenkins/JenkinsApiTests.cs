using System;

using NUnit.Framework;
using Moq;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Sources.Jenkins;

namespace Tests.NetKAN.Sources.Jenkins
{
    [TestFixture]
    public sealed class JenkinsApiTests
    {
        [Test]
        public void GetLatestBuild_ModuleManager_Works()
        {
            // Arrange
            var http = new Mock<IHttpService>();
            http.Setup(h => h.DownloadText(It.IsAny<Uri>(),
                                           It.IsAny<string?>(),
                                           It.IsAny<string?>()))
                .Returns(
                    @"{
                        ""artifacts"": [
                            {
                                ""displayPath"": ""ModuleManager-4.2.3.zip"",
                                ""fileName"": ""ModuleManager-4.2.3.zip"",
                                ""relativePath"": ""ModuleManager-4.2.3.zip""
                            },
                            {
                                ""displayPath"": ""ModuleManager.4.2.3.dll"",
                                ""fileName"": ""ModuleManager.4.2.3.dll"",
                                ""relativePath"": ""ModuleManager.4.2.3.dll""
                            }
                        ],
                        ""result"": ""SUCCESS"",
                        ""timestamp"": 1688407301446,
                        ""url"": ""https://ksp.sarbian.com/jenkins/job/ModuleManager/163/"",
                    }");
            var sut        = new JenkinsApi(http.Object);
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
