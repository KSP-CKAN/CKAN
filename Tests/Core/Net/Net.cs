using System;
using System.IO;

using NUnit.Framework;

namespace Tests.Core.Net
{
    using Net = CKAN.Net;

    [TestFixture]
    public class NetTests
    {
        // TODO: Test certificate errors. How?
        // URL we expect to always be up.
        private static readonly Uri KnownURL = new Uri("http://example.com/");

        [TestCase("cheese sandwich")]
        [Category("Online")]
        public void Download_InvalidURL_Throws(string url)
        {
            // Download should throw an exception on an invalid URL.
            Assert.Throws<UriFormatException>(() =>
            {
                Net.Download(new Uri(url), out _);
            });
        }

        [TestCase("example.txt")]
        [Category("Online")]
        public void Download_WithFilename_ReturnsSavefileNameAndSavefileExists(string savefile)
        {
            // Three-argument test, should save to the file we supply
            string downloaded = Net.Download(KnownURL, out _, null, savefile);
            Assert.AreEqual(savefile, downloaded);
            Assert.That(File.Exists(savefile));
            File.Delete(savefile);
        }

        [Test]
        [Category("Online")]
        public void Download_NoFilename_SavesToTemporaryFile()
        {
            string downloaded = Net.Download(KnownURL, out _);
            Assert.That(File.Exists(downloaded));
            File.Delete(downloaded);
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

        [TestCase("https://www.kerbaltek.com/_IamCKAN_Gimme_hyperedit_")]
        [Category("Online")]
        public void Download_Redirect_Works(string url)
        {
            // Arrange
            var uri = new Uri(url);

            // Act
            var downloaded = Net.Download(uri, out _);

            // Assert
            Assert.That(File.Exists(downloaded));

            // Teardown
            File.Delete(downloaded);
        }

        [TestCase("https://github.com/")]
        [Category("Online")]
        public void CurrentETag_ValidHost_NonEmpty(string url)
        {
            // Arrange
            var uri = new Uri(url);

            // Act
            var etag = Net.CurrentETag(uri);

            // Assert
            Assert.IsNotEmpty(etag);
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
    }
}
