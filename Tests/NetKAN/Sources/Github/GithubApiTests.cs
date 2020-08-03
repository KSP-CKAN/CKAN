using System;
using System.IO;
using NUnit.Framework;
using CKAN;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Sources.Github;

namespace Tests.NetKAN.Sources.Github
{
    [TestFixture]
    public sealed class GithubApiTests
    {
        // Ironically, despite the fact that these run on travis-ci, which is strongly integrated
        // to github, these sometimes cause test failures because github will throw random
        // 403s. (Hence we disable them in travis with --exclude=FlakyNetwork)

        private string       _cachePath;
        private NetFileCache _cache;

        [OneTimeSetUp]
        public void TestFixtureSetup()
        {
            _cachePath = Path.Combine(Path.GetTempPath(), "CKAN");
            var path = Path.Combine(_cachePath, Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(path);
            _cache = new NetFileCache(path);
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            _cache.Dispose();
            _cache = null;
            Directory.Delete(_cachePath, recursive: true);
        }

        [Test]
        [Category("FlakyNetwork")]
        [Category("Online")]
        public void GetsLatestReleaseCorrectly()
        {
            // Arrange
            var sut = new GithubApi(new CachingHttpService(_cache));

            // Act
            var githubRelease = sut.GetLatestRelease(new GithubRef("#/ckan/github/KSP-CKAN/Test", false, false));

            // Assert
            Assert.IsNotNull(githubRelease.Author);
            Assert.IsNotNull(githubRelease.Download);
            Assert.IsNotNull(githubRelease.Version);
        }
    }
}
