using NUnit.Framework;
using System;
using CKAN.KerbalStuff;

namespace GitHubTests
{
    [TestFixture()]
    public class GithubAPITests
    {
        [Test()]
        public void Release ()
        {
            GithubRelease ckan = CKAN.KerbalStuff.GithubAPI.GetLatestRelease("KSP-CKAN/CKAN");
            Assert.IsNotNull (ckan.author);
            Assert.IsNotNull (ckan.download);
            Assert.IsNotNull (ckan.size);
            Assert.IsNotNull (ckan.version);
        }

        [Test()]
        public void TestGithubRelease()
        {
            Assert.AreEqual(GithubRelease.GithubPage("KSP-CKAN/CKAN"), "https://github.com/KSP-CKAN/CKAN");
        }
    }
}

