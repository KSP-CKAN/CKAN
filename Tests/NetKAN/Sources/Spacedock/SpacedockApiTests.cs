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

namespace Tests.NetKAN.Sources.Spacedock
{
    [TestFixture]
    public sealed class SpacedockApiTests
    {
        [Test]
        public void GetMod_PlaneMode_Works()
        {
            // Arrange
            var http = new Mock<IHttpService>();
            http.Setup(h => h.DownloadText(It.IsAny<Uri>(),
                                           It.IsAny<string?>(),
                                           It.IsAny<string?>()))
                .Returns(
                    @"{
                        ""id"": 20,
                        ""author"": ""Y3mo"",
                        ""background"": ""https://spacedock.info/content/Y3mo_158/Unmanned_Before_Manned/Unmanned_Before_Manned-1456574347.1219552.png"",
                        ""license"": ""restricted"",
                        ""name"": ""Unmanned Before Manned"",
                        ""short_description"": ""Moves stock parts around for an unmanned start and a more challenging early game. Works very well with stock tech tree and Community Tech Tree."",
                        ""source_code"": """",
                        ""website"": ""https://forum.kerbalspaceprogram.com/index.php?/topic/95645-*"",
                        ""versions"": [
                            {
                                ""friendly_version"": ""1.3.0.2"",
                                ""game_version"": ""1.3.0"",
                                ""id"": 6392,
                                ""created"": ""2017-07-05T16:13:04.912024+00:00"",
                                ""download_path"": ""/mod/20/Unmanned%20Before%20Manned/download/1.3.0.2"",
                                ""changelog"": ""**TechTree changes**\r\n\r\n* HG-5 High Gain Antenna and stock Fuel Cell earlier @engineering101\r\n* Surface Sampler (SETIrebalance) from unmannedTech to advConstruction\r\n* Radial Chutes earlier @survivability, no reason to put them later with Mk1 Retro Cockpit\r\n\r\n**ModSupport**\r\n\r\n* HullCameraVDS"",
                                ""downloads"": 68723
                            }
                        ]
                    }");
            var sut = new SpacedockApi(http.Object);

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

        [Test]
        public void GetMod_ModMissing_Throws()
        {
            // Arrange
            var http = new Mock<IHttpService>();
            http.Setup(h => h.DownloadText(It.IsAny<Uri>(),
                                           It.IsAny<string?>(),
                                           It.IsAny<string?>()))
                .Returns(
                    @"{
                        ""error"":  true,
                        ""reason"": ""404 Not Found: Requested page not found. Looks like this was deleted, or maybe was never here."",
                        ""code"":   404
                    }");
            var sut = new SpacedockApi(http.Object);

            // Act
            TestDelegate act = () => sut.GetMod(-1);

            // Assert
            Assert.That(act, Throws.Exception.InstanceOf<Kraken>());
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
