using System;
using System.IO;

using NUnit.Framework;

namespace Tests.Core.Net
{
    using Net = CKAN.Net;

    [TestFixture]
    [Category("Online")]
    public class NetTests
    {
        // TODO: Test certificate errors. How?
        // URL we expect to always be up.
        private static readonly Uri KnownURL = new Uri("http://example.com/");

        [TestCase("cheese sandwich")]
        public void Download_InvalidURL_Throws(string url)
        {
            // Download should throw an exception on an invalid URL.
            Assert.Throws<UriFormatException>(() =>
            {
                Net.Download(new Uri(url), out _);
            });
        }

        [TestCase("example.txt")]
        public void Download_WithFilename_ReturnsSavefileNameAndSavefileExists(string savefile)
        {
            // Three-argument test, should save to the file we supply
            string downloaded = Net.Download(KnownURL, out _, null, savefile);
            Assert.AreEqual(savefile, downloaded);
            Assert.That(File.Exists(savefile));
            File.Delete(savefile);
        }

        [Test]
        public void Download_NoFilename_SavesToTemporaryFile()
        {
            string downloaded = Net.Download(KnownURL, out _);
            Assert.That(File.Exists(downloaded));
            File.Delete(downloaded);
        }

        [TestCase("https://spacedock.info/mod/132/Contract%20Reward%20Modifier/download/2.1")]
        [Category("FlakyNetwork")]
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

    }
}
