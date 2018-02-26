using CKAN.Versioning;
using NUnit.Framework;

namespace Tests.Core.Versioning
{
    [TestFixture]
    public class Version
    {
        [Test]
        public void Alpha()
        {
            var v1 = new CKAN.ModuleVersion("apple");
            var v2 = new CKAN.ModuleVersion("banana");

            // alphabetical test
            Assert.That(v1.IsLessThan(v2));
        }

        [Test]
        public void Basic()
        {
            var v0 = new CKAN.ModuleVersion("1.2.0");
            var v1 = new CKAN.ModuleVersion("1.2.0");
            var v2 = new CKAN.ModuleVersion("1.2.1");

            Assert.That(v1.IsLessThan(v2));
            Assert.That(v2.IsGreaterThan(v1));
            Assert.That(v1.IsEqualTo(v0));
        }

        [Test]
        public void Issue1076()
        {
            var v0 = new CKAN.ModuleVersion("1.01");
            var v1 = new CKAN.ModuleVersion("1.1");

            Assert.That(v1.IsEqualTo(v0));
        }

        [Test]
        public void SortAllNonNumbersBeforeDot()
        {
            var v0 = new CKAN.ModuleVersion("1.0_beta");
            var v1 = new CKAN.ModuleVersion("1.0.1_beta");

            Assert.That(v0.IsLessThan(v1));
            Assert.That(v1.IsGreaterThan(v0));
        }

        [Test]
        public void DotSeparatorForExtraData()
        {
            var v0 = new CKAN.ModuleVersion("1.0");
            var v1 = new CKAN.ModuleVersion("1.0.repackaged");
            var v2 = new CKAN.ModuleVersion("1.0.1");

            Assert.That(v0.IsLessThan(v1));
            Assert.That(v1.IsLessThan(v2));
            Assert.That(v1.IsGreaterThan(v0));
            Assert.That(v2.IsGreaterThan(v1));
        }

        [Test]
        public void UnevenVersioning()
        {
            var v0 = new CKAN.ModuleVersion("1.1.0.0");
            var v1 = new CKAN.ModuleVersion("1.1.1");

            Assert.That(v0.IsLessThan(v1));
            Assert.That(v1.IsGreaterThan(v0));
        }

        [Test]
        public void Complex()
        {
            var v1 = new CKAN.ModuleVersion("v6a12");
            var v2 = new CKAN.ModuleVersion("v6a5");
            Assert.That(v2.IsLessThan(v1));
            Assert.That(v1.IsGreaterThan(v2));
            Assert.That(! v1.IsEqualTo(v2));
        }

        [Test]
        public void Epoch()
        {
            var v1 = new CKAN.ModuleVersion("1.2.0");
            var v2 = new CKAN.ModuleVersion("1:1.2.0");

            Assert.That(v1.IsLessThan(v2));
        }

        [Test]
        public void DllVersion()
        {
            var v1 = new DllModuleVersion();
            Assert.AreEqual("autodetected dll", v1.ToString());
        }

        [Test]
        public void ProvidesVersion()
        {
            var v1 = new ProvidesModuleVersion("SomeModule");
            Assert.AreEqual("provided by SomeModule", v1.ToString());
        }

        [Test]
        public void AgExt()
        {
            var v1 = new CKAN.ModuleVersion("1.20");
            var v2 = new CKAN.ModuleVersion("1.22a");

            Assert.That(v2.IsGreaterThan(v1));
        }

        [Test]
        public void DifferentEpochs()
        {
            var v1 = new CKAN.ModuleVersion("1:1");
            var v2 = new CKAN.ModuleVersion("2:1");

            Assert.That(!v1.IsEqualTo(v2));
        }
    }
}
