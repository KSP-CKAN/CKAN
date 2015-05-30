using NUnit.Framework;

namespace CKANTests
{
    [TestFixture]
    public class ReleaseStatus
    {

        // These get used by our tests, but we have to disable 'used only once' (0414)
        // to stop the compiler from giving us warnings.
        #pragma warning disable 0414

        private static string[] GoodStatuses = {
            "stable", "testing", "development"
        };

        private static string[] BadStatuses = {
            "cheese", "some thing I wrote last night" , "",
            "yo dawg I heard you like tests",
            "42"
        };

        #pragma warning restore 0414

        [Test][TestCaseSource("GoodStatuses")]
        public void ReleaseGood(string status)
        {
            var release = new CKAN.ReleaseStatus(status);
            Assert.IsInstanceOf<CKAN.ReleaseStatus>(release);
            Assert.AreEqual(status, release.ToString());
        }

        [Test][TestCaseSource("BadStatuses")]
        public void ReleaseBad(string status)
        {
            Assert.Throws<CKAN.BadMetadataKraken>(delegate
            {
                new CKAN.ReleaseStatus(status);
            });
        }

        [Test]
        public void Null()
        {
            // According to the spec, no release status means "stable".
            var release = new CKAN.ReleaseStatus(null);
            Assert.AreEqual("stable", release.ToString());
        }
    }
}

