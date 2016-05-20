using System;
using System.IO;
using CKAN;
using NUnit.Framework;

namespace Tests.Core.Net
{
    [TestFixture]
    public class Net
    {
        // TODO: Test certificate errors. How?
        // URL we expect to always be up.
        const string KnownURL = "http://example.com/";
        private static void BadDownload()
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
            string downloaded = CKAN.Net.Download(KnownURL, savefile);
            Assert.AreEqual(downloaded, savefile);
            Assert.That(File.Exists(savefile));
            File.Delete(savefile);
        }

        [Test]
        [Category("Online")]
        public void SingleArgumentDownloadSavesToTemporaryFile()
        {
            string downloaded = CKAN.Net.Download(KnownURL);
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
            
        [Test]
        [Category("Security")]
        public void SSLenforced()
        {
            // We don't use curl on Windows, and so we just skip
            // this test on that platform.
            if (Platform.IsWindows) return;

            var curl = Curl.CreateEasy("https://example.com", (FileStream) null);
            Assert.IsTrue(curl.SslVerifyPeer, "We should enforce SSL");
        }

    }
}
