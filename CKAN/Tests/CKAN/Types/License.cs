using NUnit.Framework;

namespace CKANTests
{
    [TestFixture]
    public class License
    {
        [Test]
        public void LicenseGood()
        {
            var license = new CKAN.License("GPL-3.0");
            Assert.AreEqual("GPL-3.0", license.ToString());
        }

        [Test]
        public void LicenseBad()
        {
            Assert.Throws<CKAN.BadMetadataKraken>(delegate
            {
                // Not a valid license string.
                new CKAN.License("this is a really invalid license");
            });
        }

        [Test]
        public void SynonymCheck()
        {
            var license = new CKAN.License("GPLv3");
            Assert.AreEqual("GPL-3.0", license.ToString());
        }
    }
}

