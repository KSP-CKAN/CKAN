using System.IO;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class Net
    {
        private void BadDownload()
        {
            CKAN.Net.Download("cheese sandwich");
        }

        [Test]
        public void Download()
        {
            // URL we expect to always be up.
            string KNOWN_URL = "http://example.com/";

            // Download should throw an exception on an invalid URL.
            Assert.That(BadDownload, Throws.Exception);

            // TODO: Mark these as "online" tests. How?

            {
                // Two-argument test, should save to the file we supply
                string savefile = "example.txt";
                string downloaded = CKAN.Net.Download(KNOWN_URL, savefile);
                Assert.AreEqual(downloaded, savefile);
                File.Delete(savefile);
            }

            {
                // Single-argumeng test, should save to a temporary filename.
                string downloaded = CKAN.Net.Download(KNOWN_URL);
                Assert.IsNotNullOrEmpty(downloaded);
                File.Delete(downloaded);
            }

            // TODO: Test certificate errors. How?
        }
    }
}