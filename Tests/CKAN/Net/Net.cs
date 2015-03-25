using System;
using System.IO;
using NUnit.Framework;

namespace CKANTests
{
    [TestFixture]
    public class Net
    {
        // TODO: Test certificate errors. How?
        // URL we expect to always be up.
        const string KNOWN_URL = "http://example.com/";
        private void BadDownload()
        {
            CKAN.Net.Download("cheese sandwich");
        }

        [Test]
        [Category("Online")]
        public void DownloadThrowsOnInvaildURL()
        {
            // Download should throw an exception on an invalid URL.
            Assert.That(BadDownload, Throws.Exception);
        }

        [Test]
        [Category("Online")]
        public void DownloadReturnsSavefileNameAndSavefileExists()
        {
            // Two-argument test, should save to the file we supply
            string savefile = "example.txt";
            string downloaded = CKAN.Net.Download(KNOWN_URL, savefile);
            Assert.AreEqual(downloaded, savefile);
            Assert.That(File.Exists(savefile));
            File.Delete(savefile);
        }

        [Test]
        [Category("Online")]
        public void SingleArgumentDownloadSavesToTemporaryFile()
        {
            string downloaded = CKAN.Net.Download(KNOWN_URL);
            Assert.That(File.Exists(downloaded));
            File.Delete(downloaded);
        }

        [Test]
        [Category("Online")]
        public void KerbalStuffSSL()
        {
            Assert.DoesNotThrow(delegate
            {
                string file = CKAN.Net.Download("https://kerbalstuff.com/mod/646/Contract%20Reward%20Modifier/download/1.2");
                if (!File.Exists(file))
                {
                    throw new Exception("File not downloaded");
                }
            });
        }
    }
}