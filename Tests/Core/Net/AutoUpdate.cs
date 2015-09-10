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
        // We expect a kraken when looking at a URL with no releases.
        public void FetchCkanUrl()
        {
            Assert.Throws<CKAN.Kraken>(delegate
                {
                    Fetch(test_ckan_release);
                }
            );
        }

        [Test]
        [Category("Online")]
        // This could fail if run during a release, so it's marked as Flaky.
        [Category("FlakyNetwork")]
        public void FetchLatestReleaseInfo()
        {
            var updater = CKAN.AutoUpdate.Instance;

            // Is is a *really* basic test to just make sure we get release info
            // if we ask for it.
            updater.FetchLatestReleaseInfo();
            Assert.IsTrue(updater.IsFetched());
            Assert.IsNotNull(updater.ReleaseNotes);
            Assert.IsNotNull(updater.LatestVersion);
        }

        [Test]
        [TestCase("aaa\r\n---\r\nbbb", "bbb", "Release note marker included")]
        [TestCase("aaa\r\nbbb", "aaa\r\nbbb", "No release note marker")]
        [TestCase("aaa\r\n---\r\nbbb\r\n---\r\nccc", "bbb\r\n---\r\nccc", "Multi release notes markers")]
        public void ExtractReleaseNotes(string body, string expected, string comment)
        {
            Assert.AreEqual(
                expected,
                CKAN.AutoUpdate.Instance.ExtractReleaseNotes(body),
                comment
            );
        }

        private void Fetch(Uri url)
        {
            CKAN.AutoUpdate.Instance.RetrieveUrl(CKAN.AutoUpdate.Instance.MakeRequest(url));
        }
    }
}