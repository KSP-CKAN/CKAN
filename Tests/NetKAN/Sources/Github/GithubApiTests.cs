using CKAN.NetKAN.Sources.Github;
using NUnit.Framework;

namespace Tests.NetKAN.Sources.Github
{
    [TestFixture]
    public sealed class GithubApiTests
    {
        // Ironically, despite the fact that these run on travis-ci, which is strongly integrated
        // to github, these sometimes cause test failures because github will throw random
        // 403s. (Hence we disable them in travis with --exclude=FlakyNetwork)
        
        [Test]
        [Category("FlakyNetwork")]
        [Category("Online")]
        public void GetsLatestReleaseCorrectly()
        {
            // Arrange
            var sut = new GithubApi();

            // Act
            var githubRelease = sut.GetLatestRelease(new GithubRef("#/ckan/github/KSP-CKAN/Test"));

            // Assert
            Assert.IsNotNull(githubRelease.Author);
            Assert.IsNotNull(githubRelease.Download);
            Assert.IsNotNull(githubRelease.Size);
            Assert.IsNotNull(githubRelease.Version);
        }
    }
}
