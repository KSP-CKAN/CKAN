using CKAN;
using NUnit.Framework;
using Tests.Data;

namespace Tests.Core.Types
{
    [TestFixture]
    public class RelationshipDescriptor
    {

        CKAN.Version autodetected = new CKAN.DllVersion();

        [Test]
        [TestCase("0.23","0.23", true)]
        [TestCase("wibble","wibble",true)]
        [TestCase("0.23", "0.23.1", false)]
        [TestCase("wibble","wobble", false)]
        public void VersionWithinBounds_ExactFalse(string version, string other_version, bool expected)
        {
            var rd = new CKAN.RelationshipDescriptor { version = new CKAN.Version(version) };
            Assert.AreEqual(expected, rd.version_within_bounds(new CKAN.Version(other_version)));
        }

        [Test]
        [TestCase("0.20","0.23","0.21", true)]
        [TestCase("0.20","0.23","0.20", true)]
        [TestCase("0.20","0.23","0.23", true)]
        public void VersionWithinBounds_MinMax(string min, string max, string compare_to, bool expected)
        {
            var rd = new CKAN.RelationshipDescriptor
            {
                min_version = new CKAN.Version(min),
                max_version = new CKAN.Version(max)
            };

            Assert.AreEqual(expected, rd.version_within_bounds(new CKAN.Version(compare_to)));
        }

        [Test]
        [TestCase("0.23")]
        public void VersionWithinBounds_vs_AutoDetectedMod(string version)
        {
            var rd = new CKAN.RelationshipDescriptor { version = new CKAN.Version(version) };

            Assert.True(rd.version_within_bounds(autodetected));
        }

        [Test]
        [TestCase("0.20","0.23")]
        public void VersionWithinBounds_MinMax_vs_AutoDetectedMod(string min, string max)
        {
            var rd = new CKAN.RelationshipDescriptor
            {
                min_version = new CKAN.Version(min),
                max_version = new CKAN.Version(max)
            };
            
            Assert.True(rd.version_within_bounds(autodetected));

        }
    }
}

