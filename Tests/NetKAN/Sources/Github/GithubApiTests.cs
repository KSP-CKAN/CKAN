using System;
using System.Linq;

using NUnit.Framework;

using CKAN;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Sources.Github;

using Tests.Data;

namespace Tests.NetKAN.Sources.Github
{
    [TestFixture]
    [Category("Online")]
    public sealed class GithubApiTests
    {
        [Test]
        public void GetRepoLatestReleaseAndOrgMembers_CKANTest_Works()
        {
            // Arrange
            using (var dir   = new TemporaryDirectory())
            using (var cache = new NetFileCache(dir.Directory.FullName))
            {
                var sut       = new GithubApi(new CachingHttpService(cache),
                                              Environment.GetEnvironmentVariable("GITHUB_TOKEN"));
                var githubRef = new GithubRef("KSP-CKAN", "Test");

                // Act
                var repo    = sut.GetRepo(githubRef)!;
                var release = sut.GetLatestRelease(githubRef, false)!;
                var members = sut.getOrgMembers(repo.Owner!);

                // Assert
                Assert.IsNotNull(repo.Name);
                Assert.IsNotNull(repo.FullName);
                Assert.IsNotNull(repo.Description);
                Assert.IsNotNull(repo.Owner);

                Assert.IsNotNull(release.Author);
                Assert.IsNotNull(release.Tag);
                Assert.IsNotNull(release.Assets?.FirstOrDefault());
                Assert.IsNotNull(release.Assets?.FirstOrDefault()?.Download);
                Assert.IsNotNull(release.SourceArchiveAsset);

                Assert.That(members.Count, Is.GreaterThanOrEqualTo(3));
                Assert.Contains("HebaruSan", members.Select(u => u.Login).ToArray());
            }
        }
    }
}
