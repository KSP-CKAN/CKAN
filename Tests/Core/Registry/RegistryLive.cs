using CKAN;
using NUnit.Framework;
using Tests.Data;

namespace Tests.Core.Registry
{
    /// <summary>
    /// These are tests on a live registry extracted from one of the developers'
    /// systems.
    /// </summary>

    [TestFixture]
    public class RegistryLive
    {
        private static string test_registry = TestData.TestRegistry();
        private DisposableKSP temp_ksp;
        private CKAN.IRegistryQuerier registry;

        [SetUp]
        public void Setup()
        {
            // Make a fake KSP install
            temp_ksp = new DisposableKSP(null, test_registry);

            // Easy short-cut
            registry = CKAN.RegistryManager.Instance(temp_ksp.KSP).registry;
        }

        [TearDown]
        public void TearDown()
        {
            temp_ksp.Dispose();
        }

        [Test]
        public void LatestAvailable()
        {
            CkanModule module =
                registry.LatestAvailable("AGExt", temp_ksp.KSP.Version());

            Assert.AreEqual("AGExt", module.identifier);
            Assert.AreEqual("1.24a", module.version.ToString());
        }
    }
}

