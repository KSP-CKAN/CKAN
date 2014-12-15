using CKAN.NetKAN;
using NUnit.Framework;

namespace NetKAN.GitHubTests
{
    [TestFixture]
    public class GithubAPITests
    {

        // Ironically, despite the fact that these run on travis-ci, which is strongly integrated
        // to github, these sometimes cause test failures because github will throw random
        // 403s. (Hence we disable them in travis with --exclude=FlakyNetwork)

        [Test]
        [Category("FlakyNetwork")]
        [Category("Online")]
        public void Release()
        {
            GithubRelease ckan = GithubAPI.GetLatestRelease("KSP-CKAN/Test");
            Assert.IsNotNull(ckan.author);
            Assert.IsNotNull(ckan.download);
            Assert.IsNotNull(ckan.size);
            Assert.IsNotNull(ckan.version);
        }

        [Test]
        [Category("FlakyNetwork")]
        [Category("Online")]
        public void TestGithubRelease()
        {
            Assert.AreEqual(GithubRelease.GithubPage("KSP-CKAN/CKAN"), "https://github.com/KSP-CKAN/CKAN");
        }
    }
}

