﻿using System.Collections.Generic;

using log4net;
using NUnit.Framework;

using Tests.Data;
using CKAN;

namespace Tests.Core.Net
{
    /// <summary>
    /// Test the async downloader.
    /// </summary>

    [TestFixture]
    public class NetAsyncModulesDownloader
    {
        private CKAN.GameInstanceManager manager;
        private CKAN.RegistryManager registry_manager;
        private CKAN.Registry registry;
        private DisposableKSP ksp;
        private CKAN.IDownloader async;
        private NetModuleCache cache;
        private NetAsyncDownloader downloader;

        private static readonly ILog log = LogManager.GetLogger(typeof(NetAsyncModulesDownloader));

        [SetUp]
        public void Setup()
        {
            manager = new GameInstanceManager(new NullUser());
            // Give us a registry to play with.
            ksp = new DisposableKSP();
            registry_manager = CKAN.RegistryManager.Instance(ksp.KSP);
            registry = registry_manager.registry;
            registry.ClearDlls();
            registry.Installed().Clear();
            // Make sure we have a registry we can use.

            registry.Repositories = new SortedDictionary<string, Repository>()
            {
                {
                    "testRepo",
                    new Repository("testRepo", TestData.TestKANZip())
                }
            };

            downloader = new NetAsyncDownloader(new NullUser());

            CKAN.Repo.UpdateAllRepositories(registry_manager, ksp.KSP, downloader, null, new NullUser());

            // Ready our downloader.
            async = new CKAN.NetAsyncModulesDownloader(new NullUser(), manager.Cache);

            // General shortcuts
            cache = manager.Cache;
        }

        [TearDown]
        public void TearDown()
        {
            manager.Dispose();
            ksp.Dispose();
        }

        [Test]
        [Category("Online")]
        [Category("NetAsyncModulesDownloader")]
        [Explicit]
        public void SingleDownload()
        {
            log.Info("Performing single download test.");

            // We know kOS is in the TestKAN data, and hosted in KS. Let's get it.

            var modules = new List<CkanModule>();

            CkanModule kOS = registry.LatestAvailable("kOS", null);
            Assert.IsNotNull(kOS);

            modules.Add(kOS);

            // Make sure we don't alread have kOS somehow.
            Assert.IsFalse(cache.IsCached(kOS));

            //
            log.InfoFormat("Downloading kOS from {0}", kOS.download);

            // Download our module.
            async.DownloadModules(modules);

            // Assert that we have it, and it passes zip validation.
            Assert.IsTrue(cache.IsCachedZip(kOS));
        }

        [Test]
        [Category("Online")]
        [Category("NetAsyncModulesDownloader")]
        [Explicit]
        public void MultiDownload()
        {
            var modules = new List<CkanModule>();

            CkanModule kOS = registry.LatestAvailable("kOS", null);
            CkanModule quick_revert = registry.LatestAvailable("QuickRevert", null);

            modules.Add(kOS);
            modules.Add(quick_revert);

            Assert.IsFalse(cache.IsCachedZip(kOS));
            Assert.IsFalse(cache.IsCachedZip(quick_revert));

            async.DownloadModules(modules);

            Assert.IsTrue(cache.IsCachedZip(kOS));
            Assert.IsTrue(cache.IsCachedZip(quick_revert));
        }

        [Test]
        [Category("Online")]
        [Category("NetAsyncModulesDownloader")]
        [Explicit]
        public void RandSdownload()
        {
            var modules = new List<CkanModule>();

            var rAndS = TestData.RandSCapsuleDyneModule();

            modules.Add(rAndS);

            Assert.IsFalse(cache.IsCachedZip(rAndS), "Module not yet downloaded");

            async.DownloadModules(modules);

            Assert.IsTrue(cache.IsCachedZip(rAndS), "Module download successful");
        }

    }
}
