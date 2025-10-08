using System;
using System.IO;
using System.Net;

using NUnit.Framework;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace Tests.Core.Net
{
    using Net = CKAN.Net;

    [TestFixture]
    public class NetTests
    {
        // TODO: Test certificate errors. How?

        [TestCase("example.txt")]
        public void Download_WithFilename_ReturnsSavefileNameAndSavefileExists(string savefile)
        {
            // Arrange
            using (var server = MakeMockServer())
            {
                var uri = new Uri(server.Url!);

                // Act
                // Three-argument test, should save to the file we supply
                string downloaded = Net.Download(uri, out string? etag, null, savefile);

                // Assert
                Assert.AreEqual(savefile, downloaded);
                Assert.That(File.Exists(savefile));
                Assert.AreEqual("deadbeef", etag);

                // Teardown
                File.Delete(savefile);
            }
        }

        [Test]
        public void Download_NoFilename_SavesToTemporaryFile()
        {
            // Arrange
            using (var server = MakeMockServer())
            {
                var uri = new Uri(server.Url!);

                // Act
                string downloaded = Net.Download(uri, out string? etag);

                // Assert
                Assert.That(File.Exists(downloaded));
                Assert.AreEqual("deadbeef", etag);

                // Teardown
                File.Delete(downloaded);
            }
        }

        [TestCase("https://spacedock.info/mod/132/Contract%20Reward%20Modifier/download/2.1")]
        [Category("FlakyNetwork")]
        [Category("Online")]
        public void Download_SpaceDock_Works(string url)
        {
            // Arrange
            var uri = new Uri(url);

            // Act
            string downloaded = Net.Download(uri, out _);

            // Assert
            Assert.That(File.Exists(downloaded), "File not downloaded");

            // Teardown
            File.Delete(downloaded);
        }

        [Test]
        public void Download_Redirect_Works()
        {
            // Arrange
            using (var server = MakeMockServer())
            {
                var uri = new Uri($"{server.Url}/redirect-me");

                // Act
                var downloaded = Net.Download(uri, out string? etag);

                // Assert
                Assert.That(File.Exists(downloaded));
                Assert.AreEqual("deadbeef", etag);

                // Teardown
                File.Delete(downloaded);
            }
        }

        [Test]
        public void CurrentETag_ValidHost_NonEmpty()
        {
            // Arrange
            using (var server = MakeMockServer())
            {
                var uri = new Uri(server.Url!);

                // Act
                var etag = Net.CurrentETag(uri);

                // Assert
                Assert.AreEqual("deadbeef", etag);
            }
        }

        [TestCase("https://github.com/",
                  ExpectedResult = "https://github.com/")]
        [TestCase("www.google.com",
                  ExpectedResult = "http://www.google.com")]
        [TestCase("https://spacedock.info/1234/A mod name with spaces",
                  ExpectedResult = "https://spacedock.info/1234/A+mod+name+with+spaces")]
        [TestCase("https://spacedock.info/1234/A\"Mod\"NameWith\"Quotes\"",
                  ExpectedResult = "https://spacedock.info/1234/A%22Mod%22NameWith%22Quotes%22")]
        [TestCase("gopher://gopher-is-dead\test",
                  ExpectedResult = null)]
        public string? NormalizeUri(string url)
            => Net.NormalizeUri(url);

        [TestCase("https://notgithub.com/",
                  ExpectedResult = "https://notgithub.com/")]
        [TestCase("https://github.com/KSP-CKAN/CKAN/raw/whatever",
                  ExpectedResult = "https://github.com/KSP-CKAN/CKAN/raw/whatever")]
        [TestCase("https://github.com/KSP-CKAN/CKAN/blob/branchname/whatever",
                  ExpectedResult = "https://raw.githubusercontent.com:443/KSP-CKAN/CKAN/branchname/whatever")]
        [TestCase("https://github.com/KSP-CKAN/CKAN/tree/branchname/whatever",
                  ExpectedResult = "https://raw.githubusercontent.com:443/KSP-CKAN/CKAN/branchname/whatever")]
        [TestCase("https://github.com/KSP-CKAN/CKAN/releases/lastest/download/whatever",
                  ExpectedResult = "https://github.com/KSP-CKAN/CKAN/releases/lastest/download/whatever")]
        public string GetRawUri(string url)
            => Net.GetRawUri(new Uri(url)).OriginalString;

        private static WireMockServer MakeMockServer()
        {
            var server = WireMockServer.Start();
            server.Given(Request.Create()
                                .WithPath("/")
                                .UsingGet())
                  .RespondWith(Response.Create()
                                       .WithStatusCode(HttpStatusCode.OK)
                                       .WithHeader("ETag", "\"deadbeef\"")
                                       .WithBody("<html></html>"));
            server.Given(Request.Create()
                                .WithPath("/")
                                .UsingHead())
                  .RespondWith(Response.Create()
                                       .WithStatusCode(HttpStatusCode.OK)
                                       .WithHeader("ETag", "\"deadbeef\""));
            server.Given(Request.Create()
                                .WithPath("/redirect-me")
                                .UsingGet())
                  .RespondWith(Response.Create()
                                       .WithStatusCode(HttpStatusCode.TemporaryRedirect)
                                       .WithHeader("Location", server.Url!));
            return server;
        }

    }
}
