using NUnit.Framework;

namespace CKANTests
{
    [TestFixture]
    public class Version
    {
        [Test]
        public void Alpha()
        {
            var v1 = new CKAN.Version("apple");
            var v2 = new CKAN.Version("banana");

            // alphabetical test
            Assert.That(v1.IsLessThan(v2));
        }

        [Test]
        public void Basic()
        {
            var v0 = new CKAN.Version("1.2.0");
            var v1 = new CKAN.Version("1.2.0");
            var v2 = new CKAN.Version("1.2.1");

            Assert.That(v1.IsLessThan(v2));
            Assert.That(v2.IsGreaterThan(v1));
            Assert.That(v1.IsEqualTo(v0));
        }

        [Test]
        public void Complex()
        {
            var v1 = new CKAN.Version("v6a12");
            var v2 = new CKAN.Version("v6a5");
            Assert.That(v2.IsLessThan(v1));
            Assert.That(v1.IsGreaterThan(v2));
            Assert.That(! v1.IsEqualTo(v2));
        }

        [Test]
        public void Epoch()
        {
            var v1 = new CKAN.Version("1.2.0");
            var v2 = new CKAN.Version("1:1.2.0");

            Assert.That(v1.IsLessThan(v2));
        }

        [Test]
        public void DllVersion()
        {
            var v1 = new CKAN.DllVersion();
            Assert.AreEqual("autodetected dll", v1.ToString());
        }

        [Test]
        public void ProvidesVersion()
        {
            var v1 = new CKAN.ProvidesVersion("SomeModule");
            Assert.AreEqual("provided by SomeModule", v1.ToString());
        }
    }
}