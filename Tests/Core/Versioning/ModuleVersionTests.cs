using NUnit.Framework;

using CKAN.Versioning;

namespace Tests.Core.Versioning
{
    [TestFixture]
    public class ModuleVersionTests
    {
        [Test]
        public void Alpha()
        {
            var v1 = new ModuleVersion("apple");
            var v2 = new ModuleVersion("banana");

            // alphabetical test
            Assert.That(v1.IsLessThan(v2));
        }

        [Test]
        public void Basic()
        {
            var v0 = new ModuleVersion("1.2.0");
            var v1 = new ModuleVersion("1.2.0");
            var v2 = new ModuleVersion("1.2.1");

            Assert.That(v1.IsLessThan(v2));
            Assert.That(v2.IsGreaterThan(v1));
            Assert.That(v1.Equals(v0));
        }

        [Test]
        public void Equals_ZeroPadded_EqualsUnpadded()
        {
            var v0 = new ModuleVersion("1.01");
            var v1 = new ModuleVersion("1.1");

            Assert.That(v1.Equals(v0));
        }

        [Test]
        public void SortAllNonNumbersBeforeDot()
        {
            var v0 = new ModuleVersion("1.0_beta");
            var v1 = new ModuleVersion("1.0.1_beta");

            Assert.That(v0.IsLessThan(v1));
            Assert.That(v1.IsGreaterThan(v0));
        }

        [Test]
        public void DotSeparatorForExtraData()
        {
            var v0 = new ModuleVersion("1.0");
            var v1 = new ModuleVersion("1.0.repackaged");
            var v2 = new ModuleVersion("1.0.1");

            Assert.That(v0.IsLessThan(v1));
            Assert.That(v1.IsLessThan(v2));
            Assert.That(v1.IsGreaterThan(v0));
            Assert.That(v2.IsGreaterThan(v1));
        }

        [Test]
        public void UnevenVersioning()
        {
            var v0 = new ModuleVersion("1.1.0.0");
            var v1 = new ModuleVersion("1.1.1");

            Assert.That(v0.IsLessThan(v1));
            Assert.That(v1.IsGreaterThan(v0));
        }

        [Test]
        public void Complex()
        {
            var v1 = new ModuleVersion("v6a12");
            var v2 = new ModuleVersion("v6a5");
            Assert.That(v2.IsLessThan(v1));
            Assert.That(v1.IsGreaterThan(v2));
            Assert.That(! v1.Equals(v2));
        }

        [Test]
        public void Epoch()
        {
            var v1 = new ModuleVersion("1.2.0");
            var v2 = new ModuleVersion("1:1.2.0");

            Assert.That(v1.IsLessThan(v2));
        }

        [Test]
        public void DllVersion()
        {
            var v1 = new UnmanagedModuleVersion("0");
            Assert.That(v1.ToString().Contains("unmanaged"));
        }

        [Test]
        public void ProvidesVersion()
        {
            var v1 = new ProvidesModuleVersion("SomeModule", "1.0");
            Assert.That(v1.ToString().Contains("provided"));
        }

        [Test]
        public void AgExt()
        {
            var v1 = new ModuleVersion("1.20");
            var v2 = new ModuleVersion("1.22a");

            Assert.That(v2.IsGreaterThan(v1));
        }

        [Test]
        public void DifferentEpochs()
        {
            var v1 = new ModuleVersion("1:1");
            var v2 = new ModuleVersion("2:1");

            Assert.That(!v1.Equals(v2));
        }

        [TestCase("1.0",    true,  true,  ExpectedResult = "1.0"),
         TestCase("1:1.0",  true,  false, ExpectedResult = "1.0"),
         TestCase("1:1.0",  false, true,  ExpectedResult = "1:1.0"),
         TestCase("v1.0",   false, true,  ExpectedResult = "1.0"),
         TestCase("1:v1.0", true,  false, ExpectedResult = "v1.0"),
         TestCase("1:v1.0", false, true,  ExpectedResult = "1:1.0"),
         TestCase("1:v1.0", true,  true,  ExpectedResult = "1.0"),
        ]
        public string ToString_WithVersions_StripsCorrectly(string ver,
                                                            bool   stripEpoch,
                                                            bool   stripV)
            => new ModuleVersion(ver).ToString(stripEpoch, stripV);

        [TestCase("1.0",    ExpectedResult = "1.0"),
         TestCase("1:1.0",  ExpectedResult = "1.0 (1:1.0)"),
         TestCase("1:v1.0", ExpectedResult = "v1.0 (1:v1.0)"),
        ]
        public string WithAndWithoutEpoch_WithVerions_WOrks(string ver)
            => new ModuleVersion(ver).WithAndWithoutEpoch();

    }
}
