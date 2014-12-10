using System;
using NUnit.Framework;
using System.IO;

namespace CKANTests
{
    /// <summary>
    /// These are tests on a live registry extracted from one of the developers'
    /// systems.
    /// </summary>

    [TestFixture]
    public class RegistryLive
    {
        private static string test_registry = Tests.TestData.TestRegistry();
        private Tests.DisposableKSP temp_ksp;
        private CKAN.Registry registry;

        [SetUp]
        public void Setup()
        {
            // Make a fake KSP install
            temp_ksp = new Tests.DisposableKSP(null, test_registry);

            // Easy short-cut
            registry = temp_ksp.KSP.Registry;
        }

        [TearDown]
        public void TearDown()
        {
            temp_ksp.Dispose();
        }

        [Test]
        public void LatestAvailable()
        {
            CKAN.CkanModule module = 
                registry.LatestAvailable("AGExt", temp_ksp.KSP.Version());

            Assert.AreEqual("AGExt", module.identifier);
            Assert.AreEqual("1.24a", module.version.ToString());
        }
    }
}

