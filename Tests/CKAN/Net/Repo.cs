using CKAN;
using NUnit.Framework;
using Tests;

namespace CKANTests
{
    [TestFixture]
    public class Repo
    {
        private CKAN.Registry registry;
        private DisposableKSP ksp;

        [SetUp]
        public void Setup()
        {
            ksp = new DisposableKSP();
            registry = ksp.KSP.Registry;

            registry.ClearAvailable();
            registry.ClearDlls();
            registry.Installed().Clear();
        }

        [Test]
        public void UpdateRegistry()
        {
            CKAN.Repo.UpdateRegistry(TestData.TestKAN(), registry, ksp.KSP, new NullUser());

            // Test we've got an expected module.
            CKAN.CkanModule far = registry.LatestAvailable("FerramAerospaceResearch", new CKAN.KSPVersion("0.25.0"));

            Assert.AreEqual("v0.14.3.2", far.version.ToString());
        }

		[Test]
		public void UpdateRegistryTarGz()
		{
            CKAN.Repo.UpdateRegistry(TestData.TestKANTarGz(), registry, ksp.KSP, new NullUser());

			// Test we've got an expected module.
			CKAN.CkanModule far = registry.LatestAvailable("FerramAerospaceResearch", new CKAN.KSPVersion("0.25.0"));

			Assert.AreEqual("v0.14.3.2", far.version.ToString());
		}

        [Test]
        public void BadKan()
        {
            Assert.DoesNotThrow(delegate
            {
                CKAN.Repo.UpdateRegistry(TestData.BadKAN(), registry, ksp.KSP, new NullUser());
            });
        }
    }
}
