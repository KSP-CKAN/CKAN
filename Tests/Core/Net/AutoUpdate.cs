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
                    CKAN.AutoUpdate.FetchCkanUrl(test_ckan_release);
                }
            );
        }

        [Test]
        [Category("Online")]
        public void FetchUpdaterUrl()
        {
            Assert.Throws<CKAN.Kraken>(delegate
                {
                    CKAN.AutoUpdate.FetchUpdaterUrl(test_ckan_release);
                }
            );
        }
    }
}