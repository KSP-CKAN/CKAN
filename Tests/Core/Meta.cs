using CKAN;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace Tests.Core
{
    [TestFixture]
    public class Meta
    {
        [Test]
        public void Version()
        {
            string version = CKAN.Meta.BuildVersion();

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
            Version version = CKAN.Meta.ReleaseNumber();

            if (version == null)
            {
                Assert.Inconclusive("No version detected - development build");
            }

            // We should always be in the form v0.xx (pre-release series),
            // or vx.x.x (released). We also permit a (-RC\d+) extension for
            // release candidates, and -PRE\d for pre-releases.

            Assert.IsTrue(
                Regex.IsMatch(version.ToString(), @"^v(?:0.\d+|\d+\.\d+\.\d+(?:-(?:RC|PRE)\d+)?)$"),
                version.ToString());
        }
    }
}