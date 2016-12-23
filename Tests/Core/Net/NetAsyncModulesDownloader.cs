using System.Collections.Generic;
using CKAN;
using log4net;
using NUnit.Framework;
using Tests.Data;

namespace Tests.Core.Net
{
    /// <summary>
    /// Test the async downloader.
    /// </summary>

    [TestFixture]
    public class NetAsyncModulesDownloader
    {

        private CKAN.Registry registry;
        private DisposableKSP ksp;
        private CKAN.IDownloader async;
        private NetFileCache cache;

        private static readonly ILog log = LogManager.GetLogger(typeof (NetAsyncModulesDownloader));

        [SetUp]
        public void Setup()
        {
            // Make sure curl is all set up.
            Curl.Init();

            // Give us a registry to play with.
            ksp = new DisposableKSP();
            registry = CKAN.RegistryManager.Instance(ksp.KSP).registry;
            registry.ClearAvailable();
            registry.ClearDlls();
            registry.Installed().Clear();

            // Make sure we have a registry we can use.
            CKAN.Repo.UpdateRegistry(TestData.TestKANZip(), registry, ksp.KSP, new NullUser());

            // Ready our downloader.
            async = new CKAN.NetAsyncModulesDownloader(new NullUser());

            // General shortcuts
            cache = ksp.KSP.Cache;
        }

        [TearDown]
        public void TearDown()
        {
            Curl.CleanUp();
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
            Assert.IsFalse(cache.IsCached(kOS.download));

            //
            log.InfoFormat("Downloading kOS from {0}",kOS.download);

            // Download our module.
            async.DownloadModules(
                ksp.KSP.Cache,
                modules
            );

            // Assert that we have it, and it passes zip validation.
            Assert.IsTrue(cache.IsCachedZip(kOS.download));
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

            Assert.IsFalse(cache.IsCachedZip(kOS.download));
            Assert.IsFalse(cache.IsCachedZip(quick_revert.download));

            async.DownloadModules(cache, modules);

            Assert.IsTrue(cache.IsCachedZip(kOS.download));
            Assert.IsTrue(cache.IsCachedZip(quick_revert.download));
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

            Assert.IsFalse(cache.IsCachedZip(rAndS.download), "Module not yet downloaded");

            async.DownloadModules(cache, modules);

            Assert.IsTrue(cache.IsCachedZip(rAndS.download),"Module download successful");
        }

    }
}

