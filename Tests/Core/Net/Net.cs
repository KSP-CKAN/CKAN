using System.IO;
using System.Net;

using NUnit.Framework;

namespace Tests.Core.Net
{
    [TestFixture]
    [Category("Online")]
    public class NetTests
    {
        // TODO: Test certificate errors. How?
        // URL we expect to always be up.
        private const string KnownURL = "http://example.com/";

        [Test]
        public void DownloadThrowsOnInvalidURL()
        {
            // Download should throw an exception on an invalid URL.
            Assert.Throws<WebException>(
                delegate
                {
                    CKAN.Net.Download("cheese sandwich");
                });
        }

        [Test]
        public void DownloadReturnsSavefileNameAndSavefileExists()
        {
            // Three-argument test, should save to the file we supply
            string savefile = "example.txt";
            string downloaded = CKAN.Net.Download(KnownURL, null, savefile);
            Assert.AreEqual(savefile, downloaded);
            Assert.That(File.Exists(savefile));
            File.Delete(savefile);
        }

        [Test]
        public void SingleArgumentDownloadSavesToTemporaryFile()
        {
            string downloaded = CKAN.Net.Download(KnownURL);
            Assert.That(File.Exists(downloaded));
            File.Delete(downloaded);
        }

        [Test]
        [Category("FlakyNetwork")]
        public void SpaceDockSSL()
        {
            string downloaded = CKAN.Net.Download("https://spacedock.info/mod/132/Contract%20Reward%20Modifier/download/2.1");
            Assert.That(File.Exists(downloaded), "File not downloaded");
            File.Delete(downloaded);
        }
    }
}
