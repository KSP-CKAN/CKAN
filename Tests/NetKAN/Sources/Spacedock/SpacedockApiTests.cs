using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Net;

using NUnit.Framework;
using Moq;

using CKAN;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Sources.Spacedock;

using Tests.Data;

namespace Tests.NetKAN.Sources.Spacedock
{
    [TestFixture]
    public sealed class SpacedockApiTests
    {
        [Test]
        [Category("FlakyNetwork"),
         Category("Online")]
        public void GetMod_PlaneMode_Works()
        {
            using (var dir   = new TemporaryDirectory())
            using (var cache = new NetFileCache(dir.Directory.FullName))
            {
                // Arrange
                var sut = new SpacedockApi(new CachingHttpService(cache));

                // Act
                var result = sut.GetMod(20);

                // Assert
                var latestVersion = result?.All().FirstOrDefault();

                Assert.That(result?.id, Is.EqualTo(20));
                Assert.That(result?.author, Is.Not.Null);
                Assert.That(result?.background, Is.Not.Null);
                Assert.That(result?.license, Is.Not.Null);
                Assert.That(result?.name, Is.Not.Null);
                Assert.That(result?.short_description, Is.Not.Null);
                Assert.That(result?.source_code, Is.Not.Null);
                Assert.That(result?.website, Is.Not.Null);
                Assert.That(result?.versions?.Length, Is.GreaterThan(0));
                Assert.That(latestVersion?.changelog, Is.Not.Null);
                Assert.That(latestVersion?.download_path, Is.Not.Null);
                Assert.That(latestVersion?.friendly_version, Is.Not.Null);
                Assert.That(latestVersion?.KSP_version, Is.Not.Null);
            }
        }

        [Test]
        [Category("FlakyNetwork"),
         Category("Online")]
        public void GetMod_ModMissing_Throws()
        {
            // Arrange
            using (var dir   = new TemporaryDirectory())
            using (var cache = new NetFileCache(dir.Directory.FullName))
            {
                var sut = new SpacedockApi(new CachingHttpService(cache));

                // Act
                TestDelegate act = () => sut.GetMod(-1);

                // Assert
                Assert.That(act, Throws.Exception.InstanceOf<Kraken>());
            }
        }

        [Test]
        public void GetMod_WebExceptionWithJsonResponse_ThrowsKraken()
        {
            #pragma warning disable SYSLIB0014

            // Arrange
            var response = new Mock<WebResponse>();
            response.Setup(r => r.GetResponseStream())
                    .Returns(new MemoryStream(Encoding.ASCII.GetBytes(
                        @"{
                            ""error"":  true,
                            ""reason"": ""Fake failure reason for testing""
                        }")));
            var http = new Mock<IHttpService>();
            http.Setup(h => h.DownloadText(It.IsAny<Uri>(),
                                           It.IsAny<string?>(),
                                           It.IsAny<string?>()))
                .Throws(new WebException("Unused Message", null,
                                         WebExceptionStatus.ProtocolError,
                                         response.Object));
            var sut = new SpacedockApi(http.Object);

            // Act / Assert
            var exc = Assert.Throws<Kraken>(() => sut.GetMod(0))!;

            // Assert
            Assert.AreEqual("Could not get the mod from SpaceDock, reason: Fake failure reason for testing",
                            exc.Message);
        }
    }
}
