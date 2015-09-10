using System;
using CKAN;
using NUnit.Framework;
using System.Net;

namespace Tests.Core.AutoUpdate
{
    [TestFixture]
    public class AutoUpdate
    {
        private readonly Uri test_ckan_release = new Uri("https://api.github.com/repos/pjf/CKAN/releases/latest");

        [Test]
        [Category("Online")]
        // We should get a kraken if something exists but has no release yet
        public void FetchCkanUrl()
        {
            Assert.Throws<CKAN.Kraken>(delegate
                {
                    Fetch(test_ckan_release);
                }
            );
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