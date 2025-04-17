using System;
using System.IO;

using NUnit.Framework;

using CKAN;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Sources.Jenkins;

using Tests.Data;

namespace Tests.NetKAN.Sources.Jenkins
{
    [TestFixture]
    public sealed class JenkinsApiTests
    {
        [OneTimeSetUp]
        public void TestFixtureSetup()
        {
            _cachePath = TestData.NewTempDir();
            var path = Path.Combine(_cachePath, Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(path);
            _cache = new NetFileCache(path);
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            _cache?.Dispose();
            _cache = null;
            if (_cachePath != null)
            {
                Directory.Delete(_cachePath, recursive: true);
            }
        }


        [Test]
        [Category("FlakyNetwork")]
        [Category("Online")]
        public void GetLatestBuild_ModuleManager_Works()
        {
            // Arrange
            IJenkinsApi sut = new JenkinsApi(new CachingHttpService(_cache!));

            // Act
            var build = sut.GetLatestBuild(
                new JenkinsRef("#/ckan/jenkins/https://ksp.sarbian.com/jenkins/job/ModuleManager/"),
                new JenkinsOptions());

            // Assert
            Assert.IsNotNull(build?.Url);
            Assert.IsNotNull(build?.Artifacts);
            Assert.IsNotNull(build?.Timestamp);
        }

        private string?       _cachePath;
        private NetFileCache? _cache;
    }
}
