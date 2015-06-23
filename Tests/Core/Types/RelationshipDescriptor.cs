using CKAN;
using NUnit.Framework;
using Tests.Data;

namespace Tests.Core.Types
{
    [TestFixture]
    public class RelationshipDescriptor
    {
        [Test]
        public void VersionWithinBounds_Exact()
        {
            var rd = new CKAN.RelationshipDescriptor { version = new CKAN.Version("0.23") };

            Assert.That(rd.version_within_bounds(new CKAN.Version("0.23")));
        }
    }
}

