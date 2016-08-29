using CKAN;
using CKAN.Versioning;
using NUnit.Framework;
using Tests.Data;

namespace Tests.Core.Net
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
            registry = CKAN.RegistryManager.Instance(ksp.KSP).registry;
            registry.ClearAvailable();
            registry.ClearDlls();
            registry.Installed().Clear();
        }

        [TearDown]
        public void TearDown()
        {
            ksp.Dispose();
        }

        [Test]
        public void UpdateRegistryTarGz()
        {
            CKAN.Repo.UpdateRegistry(TestData.TestKANTarGz(), registry, ksp.KSP, new NullUser());

            // Test we've got an expected module.
            CkanModule far = registry.LatestAvailable("FerramAerospaceResearch", KspVersion.Parse("0.25.0"));

            Assert.AreEqual("v0.14.3.2", far.version.ToString());
        }

        [Test]
        public void UpdateRegistryZip()
        {
            CKAN.Repo.UpdateRegistry(TestData.TestKANZip(), registry, ksp.KSP, new NullUser());

            // Test we've got an expected module.
            CkanModule far = registry.LatestAvailable("FerramAerospaceResearch", KspVersion.Parse("0.25.0"));

            Assert.AreEqual("v0.14.3.2", far.version.ToString());
        }

        [Test]
        public void BadKanTarGz()
        {
            Assert.DoesNotThrow(delegate
            {
                CKAN.Repo.UpdateRegistry(TestData.BadKANTarGz(), registry, ksp.KSP, new NullUser());
            });
        }

        [Test]
        public void BadKanZip()
        {
            Assert.DoesNotThrow(delegate
                {
                    CKAN.Repo.UpdateRegistry(TestData.BadKANZip(), registry, ksp.KSP, new NullUser());
                });
        }
    }
}
