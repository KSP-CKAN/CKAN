using NUnit.Framework;
using Tests;

namespace CKANTests
{
    [TestFixture]
    public class Repo
    {
        private CKAN.Registry registry;

        [SetUp]
        public void Setup()
        {
            registry = CKAN.Registry.Empty();
        }

        [Test]
        public void UpdateRegistry()
        {
            CKAN.Repo.UpdateRegistry(TestData.TestKAN(), registry);

            // Test we've got an expected module.
            CKAN.CkanModule far = registry.LatestAvailable("FerramAerospaceResearch", new CKAN.KSPVersion("0.25.0"));

            Assert.AreEqual("v0.14.3.2", far.version.ToString());
        }

        [Test]
        public void BadKan()
        {
            Assert.DoesNotThrow(delegate
            {
                CKAN.Repo.UpdateRegistry(TestData.BadKAN(), registry);
            });
        }
    }
}

