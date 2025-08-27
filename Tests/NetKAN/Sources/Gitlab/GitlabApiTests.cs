using System;
using System.Linq;

using NUnit.Framework;

using CKAN;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Sources.Gitlab;
using CKAN.NetKAN.Services;
using Tests.Data;

namespace Tests.NetKAN.Sources.Gitlab
{
    [TestFixture]
    [Category("Online")]
    public sealed class GitlabApiTests
    {
        [Test]
        public void GetAllReleases_Starilex_Works()
        {
            // Arrange
            using (var dir   = new TemporaryDirectory())
            using (var cache = new NetFileCache(dir.Directory.FullName))
            {
                var sut       = new GitlabApi(new CachingHttpService(cache),
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
}
