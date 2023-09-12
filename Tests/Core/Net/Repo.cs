using System.Collections.Generic;

using NUnit.Framework;

using Tests.Data;

using CKAN;
using CKAN.Versioning;

namespace Tests.Core.Net
{
    [TestFixture]
    public class Repo
    {
        private CKAN.RegistryManager manager;
        private CKAN.Registry registry;
        private DisposableKSP ksp;
        private NetAsyncDownloader downloader;

        [SetUp]
        public void Setup()
        {
            ksp = new DisposableKSP();
            manager = CKAN.RegistryManager.Instance(ksp.KSP);
            registry = manager.registry;
            registry.Installed().Clear();
            downloader = new NetAsyncDownloader(new NullUser());
        }

        [TearDown]
        public void TearDown()
        {
            ksp.Dispose();
        }

        [Test]
        public void UpdateRegistryTarGz()
        {
            manager.registry.RepositoriesSet(new SortedDictionary<string, Repository>()
            {
                {
                    "testRepo",
                    new Repository("testRepo", TestData.TestKANTarGz())
                }
            });
            CKAN.Repo.UpdateAllRepositories(manager, ksp.KSP, downloader, null, new NullUser());
            // Test we've got an expected module.
            CkanModule far = registry.LatestAvailable("FerramAerospaceResearch", new GameVersionCriteria(GameVersion.Parse("0.25.0")));

            Assert.AreEqual("v0.14.3.2", far.version.ToString());
        }

        [Test]
        public void UpdateRegistryZip()
        {
            manager.registry.RepositoriesSet(new SortedDictionary<string, Repository>()
            {
                {
                    "testRepo",
                    new Repository("testRepo", TestData.TestKANZip())
                }
            });
            CKAN.Repo.UpdateAllRepositories(manager, ksp.KSP, downloader, null, new NullUser());

            // Test we've got an expected module.
            CkanModule far = registry.LatestAvailable("FerramAerospaceResearch", new GameVersionCriteria(GameVersion.Parse("0.25.0")));

            Assert.AreEqual("v0.14.3.2", far.version.ToString());
        }

        [Test]
        public void BadKanTarGz()
        {
            Assert.DoesNotThrow(delegate
            {
                manager.registry.RepositoriesSet(new SortedDictionary<string, Repository>()
                {
                    {
                        "testRepo",
                        new Repository("testRepo", TestData.BadKANTarGz())
                    }
                });
                CKAN.Repo.UpdateAllRepositories(manager, ksp.KSP, downloader, null, new NullUser());
            });
        }

        [Test]
        public void BadKanZip()
        {
            Assert.DoesNotThrow(delegate
            {
                manager.registry.RepositoriesSet(new SortedDictionary<string, Repository>()
                {
                    {
                        "testRepo",
                        new Repository("testRepo", TestData.BadKANZip())
                    }
                });
                CKAN.Repo.UpdateAllRepositories(manager, ksp.KSP, downloader, null, new NullUser());
            });
        }
    }
}
