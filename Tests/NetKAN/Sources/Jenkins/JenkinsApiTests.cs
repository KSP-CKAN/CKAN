using System;
using System.IO;
using NUnit.Framework;
using CKAN;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Sources.Jenkins;

namespace Tests.NetKAN.Sources.Jenkins
{
    [TestFixture]
    public sealed class JenkinsApiTests
    {
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
        public void GetLatestBuild_ModuleManager_Works()
        {
            // Arrange
            IJenkinsApi sut = new JenkinsApi(new CachingHttpService(_cache));

            // Act
            JenkinsBuild build = sut.GetLatestBuild(
                new JenkinsRef("#/ckan/jenkins/https://ksp.sarbian.com/jenkins/job/ModuleManager/"),
                new JenkinsOptions()
            );

            // Assert
            Assert.IsNotNull(build.Url);
            Assert.IsNotNull(build.Artifacts);
        }

        private string       _cachePath;
        private NetFileCache _cache;
    }
}
