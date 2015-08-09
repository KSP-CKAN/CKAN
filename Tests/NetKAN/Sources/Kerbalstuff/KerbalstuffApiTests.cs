using System;
using System.IO;
using CKAN;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Sources.Kerbalstuff;
using NUnit.Framework;

namespace Tests.NetKAN.Sources.Kerbalstuff
{
    [TestFixture]
    [Category("FlakyNetwork")]
    [Category("Online")]
    public sealed class KerbalstuffApiTests
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
            var sut = new KerbalstuffApi(new CachingHttpService(_cache));

            // Act
            var result = sut.GetMod(493); // PlaneMode

            // Assert
            var latestVersion = result.Latest();

            Assert.That(result.id, Is.EqualTo(493));
            Assert.That(result.author, Is.Not.Null);
            Assert.That(result.background, Is.Not.Null);
            Assert.That(result.license, Is.Not.Null);
            Assert.That(result.name, Is.Not.Null);
            Assert.That(result.short_description, Is.Not.Null);
            Assert.That(result.source_code, Is.Not.Null);
            Assert.That(result.website, Is.Not.Null);
            Assert.That(result.versions.Length, Is.GreaterThan(0));
            Assert.That(latestVersion.changelog, Is.Not.Null);
            Assert.That(latestVersion.download_path, Is.Not.Null);
            Assert.That(latestVersion.friendly_version, Is.Not.Null);
            Assert.That(latestVersion.KSP_version, Is.Not.Null);
        }

        [Test]
        [Category("FlakyNetwork"), Category("Online")]
        public void ThrowsWhenModMissing()
        {
            // Arrange
            var sut = new KerbalstuffApi(new CachingHttpService(_cache));

            // Act
            TestDelegate act = () => sut.GetMod(-1);

            // Assert
            Assert.That(act, Throws.Exception.InstanceOf<Kraken>());
        }
    }
}
