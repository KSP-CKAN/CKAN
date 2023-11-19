using CKAN.Versioning;
using NUnit.Framework;

namespace Tests.Core.Types
{
    [TestFixture]
    public class RelationshipDescriptor
    {
        private readonly ModuleVersion autodetected = new UnmanagedModuleVersion(null);

        [Test]
        [TestCase("0.23","0.23", true)]
        [TestCase("wibble","wibble",true)]
        [TestCase("0.23", "0.23.1", false)]
        [TestCase("wibble","wobble", false)]
        public void VersionWithinBounds_ExactFalse(string version, string other_version, bool expected)
        {
            var rd = new CKAN.ModuleRelationshipDescriptor { version = new ModuleVersion(version) };
            Assert.AreEqual(expected, rd.WithinBounds(new ModuleVersion(other_version)));
        }

        [Test]
        [TestCase("0.20","0.23","0.21", true)]
        [TestCase("0.20","0.23","0.20", true)]
        [TestCase("0.20","0.23","0.23", true)]
        public void VersionWithinBounds_MinMax(string min, string max, string compare_to, bool expected)
        {
            var rd = new CKAN.ModuleRelationshipDescriptor
            {
                min_version = new ModuleVersion(min),
                max_version = new ModuleVersion(max)
            };

            Assert.AreEqual(expected, rd.WithinBounds(new ModuleVersion(compare_to)));
        }

        [Test]
        [TestCase("0.23")]
        public void VersionWithinBounds_vs_AutoDetectedMod(string version)
        {
            var rd = new CKAN.ModuleRelationshipDescriptor { version = new ModuleVersion(version) };

            Assert.True(rd.WithinBounds(autodetected));
        }

        [Test]
        [TestCase("0.20","0.23")]
        public void VersionWithinBounds_MinMax_vs_AutoDetectedMod(string min, string max)
        {
            var rd = new CKAN.ModuleRelationshipDescriptor
            {
                min_version = new ModuleVersion(min),
                max_version = new ModuleVersion(max)
            };

            Assert.True(rd.WithinBounds(autodetected));
        }

        [Test]
        [TestCase("wibble")]
        public void VersionWithinBounds_Null(string version)
        {
            var rd = new CKAN.ModuleRelationshipDescriptor();

            Assert.True(rd.WithinBounds(new ModuleVersion(version)));
        }

        [Test]
        public void VersionWithinBounds_AllNull()
        {
            var rd = new CKAN.ModuleRelationshipDescriptor();

            Assert.True(rd.WithinBounds(null));        }
    }
}
