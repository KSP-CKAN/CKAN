using System;
using System.IO;
using CKAN;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Sources.Curse;
using NUnit.Framework;

namespace Tests.NetKAN.Sources.Curse
{
    [TestFixture]
    [Category("FlakyNetwork")]
    [Category("Online")]
    public sealed class CurseApiTests
    {
        private NetFileCache _cache;

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), "CKAN", Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(tempDirectory);

            _cache = new NetFileCache(tempDirectory);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            Directory.Delete(_cache.GetCachePath(), recursive: true);
        }

        [Test]
        [Category("FlakyNetwork"), Category("Online")]
        public void GetsModCorrectly()
        {
            // Arrange
            var sut = new CurseApi(new CachingHttpService(_cache));

            // Act
            var result = sut.GetMod(220221); // MechJeb

            // Assert
            var latestVersion = result.Latest();

            Assert.That(result.ModId, Is.EqualTo(220221));
            Assert.That(result.authors, Is.Not.Null);
            Assert.That(result.thumbnail, Is.Not.Null);
            Assert.That(result.license, Is.Not.Null);
            Assert.That(result.title, Is.Not.Null);
            //Assert.That(result.short_description, Is.Not.Null);
            //Assert.That(result.source_code, Is.Not.Null);
            //Assert.That(result.website, Is.Not.Null);
            Assert.That(result.files.Count, Is.GreaterThan(0));
            //Assert.That(latestVersion.changelog, Is.Not.Null);
            Assert.That(latestVersion.GetDownloadUrl(), Is.Not.Null);
            Assert.That(latestVersion.GetFileVersion(), Is.Not.Null);
            Assert.That(latestVersion.version, Is.Not.Null); // KSP Version
        }

        [Test]
        [Category("FlakyNetwork"), Category("Online")]
        public void ThrowsWhenModMissing()
        {
            // Arrange
            var sut = new CurseApi(new CachingHttpService(_cache));

            // Act
            TestDelegate act = () => sut.GetMod(-1);

            // Assert
            Assert.That(act, Throws.Exception.InstanceOf<Kraken>());
        }
    }
}