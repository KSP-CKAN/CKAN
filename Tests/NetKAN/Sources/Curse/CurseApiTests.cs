using System;
using System.IO;
using CKAN;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Sources.Curse;
using NUnit.Framework;

namespace Tests.NetKAN.Sources.Curse
{
    [TestFixture]
    [Category("Online")]
    [Category("FlakyNetwork")]
    public sealed class CurseApiTests
    {
        private string       _cachePath;
        private NetFileCache _cache;

        [OneTimeSetUp]
        public void TestFixtureSetup()
        {
            _cachePath = Path.Combine(Path.GetTempPath(), "CKAN", Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(_cachePath);

            _cache = new NetFileCache(_cachePath);
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            _cache.Dispose();
            _cache = null;
            Directory.Delete(_cachePath, recursive: true);
        }

        [Test]
        [Category("Online")]
        [Category("FlakyNetwork")]
        public void GetsOldModCorrectly()
        {
            // Arrange
            var sut = new CurseApi(new CachingHttpService(_cache));

            // Act
            var result = sut.GetMod("220221"); // MechJeb

            // Assert
            var latestVersion = result.Latest();

            Assert.That(result.id, Is.EqualTo(220221));
            Assert.That(result.members, Is.Not.Null);
            Assert.That(result.thumbnail, Is.Not.Null);
            Assert.That(result.license, Is.Not.Null);
            Assert.That(result.title, Is.Not.Null);
            Assert.That(result.description, Is.Not.Null);
            Assert.That(result.files.Count, Is.GreaterThan(0));
            Assert.That(latestVersion.GetDownloadUrl(), Is.Not.Null);
            Assert.That(latestVersion.GetFileVersion(), Is.Not.Null);
            Assert.That(latestVersion.version, Is.Not.Null); // KSP Version
        }

        [Test]
        [Category("Online")]
        [Category("FlakyNetwork")]
        public void GetsModCorrectly()
        {
            // Arrange
            var sut = new CurseApi(new CachingHttpService(_cache));

            // Act
            var result = sut.GetMod("photonsail");

            // Assert
            var latestVersion = result.Latest();

            Assert.That(result.id, Is.EqualTo(296653));
            Assert.That(result.members, Is.Not.Null);
            Assert.That(result.thumbnail, Is.Not.Null);
            Assert.That(result.license, Is.Not.Null);
            Assert.That(result.title, Is.Not.Null);
            Assert.That(result.description, Is.Not.Null);
            Assert.That(result.files.Count, Is.GreaterThan(0));
            Assert.That(latestVersion.GetDownloadUrl(), Is.Not.Null);
            Assert.That(latestVersion.GetFileVersion(), Is.Not.Null);
            Assert.That(latestVersion.version, Is.Not.Null); // KSP Version
        }

        [Test]
        [Category("Online")]
        [Category("FlakyNetwork")]
        public void ThrowsWhenModMissing()
        {
            // Arrange
            var sut = new CurseApi(new CachingHttpService(_cache));

            // Act
            TestDelegate act = () => sut.GetMod("-1");

            // Assert
            Assert.That(act, Throws.Exception.InstanceOf<Kraken>());
        }
    }
}
