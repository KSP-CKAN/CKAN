using System;
using System.Linq;

using NUnit.Framework;
using Moq;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Sources.Gitlab;

namespace Tests.NetKAN.Sources.Gitlab
{
    [TestFixture]
    public sealed class GitlabApiTests
    {
        [Test]
        public void GetAllReleases_Starilex_Works()
        {
            // Arrange
            var http = new Mock<IHttpService>();
            http.Setup(h => h.DownloadText(It.IsAny<Uri>(),
                                           It.IsAny<string?>(),
                                           It.IsAny<string?>()))
                .Returns(
                    @"[
                        {
                            ""tag_name"": ""v1.0"",
                            ""released_at"": ""2022-08-22T13:53:56.162Z"",
                            ""author"": {
                                ""name"": ""Alex P""
                            },
                            ""assets"": {
                                ""sources"": [
                                    {
                                        ""format"": ""zip"",
                                        ""url"": ""https://gitlab.com/Ailex-/starilex-mk1-iva/-/archive/v1.0/starilex-mk1-iva-v1.0.zip""
                                    },
                                    {
                                        ""format"": ""tar.gz"",
                                        ""url"": ""https://gitlab.com/Ailex-/starilex-mk1-iva/-/archive/v1.0/starilex-mk1-iva-v1.0.tar.gz""
                                    },
                                    {
                                        ""format"": ""tar.bz2"",
                                        ""url"": ""https://gitlab.com/Ailex-/starilex-mk1-iva/-/archive/v1.0/starilex-mk1-iva-v1.0.tar.bz2""
                                    },
                                    {
                                        ""format"": ""tar"",
                                        ""url"": ""https://gitlab.com/Ailex-/starilex-mk1-iva/-/archive/v1.0/starilex-mk1-iva-v1.0.tar""
                                    }
                                ]
                            }
                        }
                    ]");
            var sut       = new GitlabApi(http.Object,
                                          Environment.GetEnvironmentVariable("GITLAB_TOKEN"));
            var gitlabRef = new GitlabRef(new RemoteRef("#/ckan/gitlab/Ailex-/starilex-mk1-iva"));

            // Act
            var releases = sut.GetAllReleases(gitlabRef).ToArray();

            // Assert
            Assert.That(releases.Length, Is.GreaterThanOrEqualTo(1));
            var first = releases.First();
            Assert.IsNotNull(first.Author);
            Assert.IsNotNull(first.TagName);
            Assert.IsNotNull(first.ReleasedAt);
        }
    }
}
