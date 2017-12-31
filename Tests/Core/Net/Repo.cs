using CKAN;
using CKAN.Versioning;
using NUnit.Framework;
using Tests.Data;

namespace Tests.Core.Net
{
    [TestFixture]
    public class Repo
    {
        private CKAN.RegistryManager manager;
        private CKAN.Registry registry;
        private DisposableKSP ksp;

        [SetUp]
        public void Setup()
        {
            ksp = new DisposableKSP();
            manager = CKAN.RegistryManager.Instance(ksp.KSP);
            registry = manager.registry;
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
            CKAN.Repo.Update(manager, ksp.KSP, new NullUser(), TestData.TestKANTarGz());

            // Test we've got an expected module.
            CkanModule far = registry.LatestAvailable("FerramAerospaceResearch", new KspVersionCriteria(KspVersion.Parse("0.25.0")));

            Assert.AreEqual("v0.14.3.2", far.version.ToString());
        }

        [Test]
        public void UpdateRegistryZip()
        {
            CKAN.Repo.Update(manager, ksp.KSP, new NullUser(), TestData.TestKANZip());

            // Test we've got an expected module.
            CkanModule far = registry.LatestAvailable("FerramAerospaceResearch", new KspVersionCriteria(KspVersion.Parse("0.25.0")));

            Assert.AreEqual("v0.14.3.2", far.version.ToString());
        }

        [Test]
        public void BadKanTarGz()
        {
            Assert.DoesNotThrow(delegate
            {
                CKAN.Repo.Update(manager, ksp.KSP, new NullUser(), TestData.BadKANTarGz());
            });
        }

        [Test]
        public void BadKanZip()
        {
            Assert.DoesNotThrow(delegate
            {
                CKAN.Repo.Update(manager, ksp.KSP, new NullUser(), TestData.BadKANZip());
            });
        }
    }
}
