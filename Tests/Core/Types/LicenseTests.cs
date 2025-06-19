using NUnit.Framework;

using CKAN;

namespace Tests.Core.Types
{
    [TestFixture]
    public class LicenseTests
    {
        [Test]
        public void LicenseGood()
        {
            var license = new License("GPL-3.0");
            Assert.IsInstanceOf<License>(license);
            Assert.AreEqual("GPL-3.0", license.ToString());
        }

        [Test]
        public void LicenseBad()
        {
            Assert.Throws<BadMetadataKraken>(delegate
            {
                // Not a valid license string, contains spaces.
                new License("GPL 3.0");
            });
        }
    }
}
