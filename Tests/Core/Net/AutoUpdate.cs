using System;
using CKAN;
using NUnit.Framework;
using System.Net;

namespace Tests.Core.AutoUpdate
{
    [TestFixture]
    public class AutoUpdate
    {
        // pjf's repo has no releases, so tests on this URL should fail
        private readonly Uri test_ckan_release = new Uri("https://api.github.com/repos/pjf/CKAN/releases/latest");

        [Test]
        [Category("Online")]
        [Category("FlakyNetwork")]
        // We expect a kraken when looking at a URL with no releases.
        public void FetchCkanUrl()
        {
            Assert.Throws<CKAN.Kraken>(delegate
            {
                Fetch(test_ckan_release, 0);
            });
        }

        [Test]
        [Category("Online")]
        // This could fail if run during a release, so it's marked as Flaky.
        [Category("FlakyNetwork")]
        public void FetchLatestReleaseInfo()
        {
            // Force-allow TLS 1.2 for HTTPS URLs, because GitHub requires it.
            // This is on by default in .NET 4.6, but not in 4.5.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            var updater = CKAN.AutoUpdate.Instance;

            // Is is a *really* basic test to just make sure we get release info
            // if we ask for it.
            updater.FetchLatestReleaseInfo();
            Assert.IsNotNull(updater.ReleaseNotes);
            Assert.IsNotNull(updater.LatestVersion);
            Assert.IsTrue(updater.IsFetched());
        }

        [Test]
        [TestCase("aaa\r\n---\r\nbbb", "bbb", "Release note marker included")]
        [TestCase("aaa\r\nbbb", "aaa\r\nbbb", "No release note marker")]
        [TestCase("aaa\r\n---\r\nbbb\r\n---\r\nccc", "bbb\r\n---\r\nccc", "Multi release notes markers")]
        public void ExtractReleaseNotes(string body, string expected, string comment)
        {
            Assert.AreEqual(
                expected,
                CKAN.AutoUpdate.ExtractReleaseNotes(body),
                comment
            );
        }

        private void Fetch(Uri url, int whichOne)
        {
            // Force-allow TLS 1.2 for HTTPS URLs, because GitHub requires it.
            // This is on by default in .NET 4.6, but not in 4.5.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            CKAN.AutoUpdate.Instance.RetrieveUrl(CKAN.AutoUpdate.Instance.MakeRequest(url), whichOne);
        }
    }
}
