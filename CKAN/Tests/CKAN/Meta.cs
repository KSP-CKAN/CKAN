using NUnit.Framework;
using System.Text.RegularExpressions;

namespace CKANTests
{
    [TestFixture]
    public class Meta
    {
        [Test]
        public void Version()
        {
            string version = CKAN.Meta.Version();

            if (version == CKAN.Meta.Development)
            {
                Assert.Inconclusive("No version detected - development build");
            }

            // Let's check there's a commits-since-last-tag and git ID.

            Assert.IsTrue(Regex.IsMatch(version, @"\d+-g[0-9a-f]{7}$"), version);
        }

        [Test]
        public void ReleaseNumber()
        {
            CKAN.Version version = CKAN.Meta.ReleaseNumber();

            if (version == null)
            {
                Assert.Inconclusive("No version detected - developmet build");
            }

            // We should always be in the form v0.xx (pre-release series),
            // or vx.x.x (released)

            Assert.IsTrue(
                Regex.IsMatch(version.ToString(), @"^v(?:0.\d+|\d+\.\d+\.\d+)$"),
                version.ToString());
        }
    }
}

