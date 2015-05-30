using System;
using System.Collections.Generic;
using NUnit.Framework;
using Tests;
using log4net;
using log4net.Core;
using log4net.Config;

namespace CKANTests
{
    /// <summary>
    /// Test the async downloader.
    /// </summary>
    
    [TestFixture]
    public class NetAsyncDownloader
    {

        private CKAN.Registry registry;
        private DisposableKSP ksp;
        private CKAN.NetAsyncDownloader async;
        private CKAN.NetFileCache cache;

        private static readonly ILog log = LogManager.GetLogger(typeof (NetAsyncDownloader));

        [SetUp]
        public void Setup()
        {
            // Make sure curl is all set up.
            CKAN.Curl.Init();

            // Give us a registry to play with.
            ksp = new DisposableKSP();
            registry = ksp.KSP.Registry;

            registry.ClearAvailable();
            registry.ClearDlls();
            registry.Installed().Clear();

            // Make sure we have a registry we can use.
            CKAN.Repo.UpdateRegistry(TestData.TestKAN(), registry, ksp.KSP, new CKAN.NullUser());

            // Ready our downloader.
            async = new CKAN.NetAsyncDownloader(new CKAN.NullUser());

            // General shortcuts
            cache = ksp.KSP.Cache;
        }

        [TearDown]
        public void TearDown()
        {
            CKAN.Curl.CleanUp();
            ksp.Dispose();
        }

        [Test]
        [Category("Online")]
        [Category("NetAsyncDownloader")]
        [Explicit]
        public void SingleDownload()
        {
            // Force log4net on.
            // BasicConfigurator.Configure();
            // LogManager.GetRepository().Threshold = Level.Debug;
            log.Info("Performing single download test.");

            // We know kOS is in the TestKAN data, and hosted in KS. Let's get it.

            var modules = new List<CKAN.CkanModule>();

            CKAN.CkanModule kOS = registry.LatestAvailable("kOS", null);
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
        [Category("NetAsyncDownloader")]
        [Explicit]
        public void MultiDownload()
        {
            var modules = new List<CKAN.CkanModule>();

            CKAN.CkanModule kOS = registry.LatestAvailable("kOS", null);
            CKAN.CkanModule QuickRevert = registry.LatestAvailable("QuickRevert", null);

            modules.Add(kOS);
            modules.Add(QuickRevert);

            Assert.IsFalse(cache.IsCachedZip(kOS.download));
            Assert.IsFalse(cache.IsCachedZip(QuickRevert.download));

            async.DownloadModules(cache, modules);

            Assert.IsTrue(cache.IsCachedZip(kOS.download));
            Assert.IsTrue(cache.IsCachedZip(QuickRevert.download));
        }

        [Test]
        [Category("Online")]
        [Category("NetAsyncDownloader")]
        [Explicit]
        public void RandSdownload()
        {
            var modules = new List<CKAN.CkanModule>();

            var rAndS = TestData.RandSCapsuleDyneModule();

            modules.Add(rAndS);

            Assert.IsFalse(cache.IsCachedZip(rAndS.download), "Module not yet downloaded");

            async.DownloadModules(cache, modules);

            Assert.IsTrue(cache.IsCachedZip(rAndS.download),"Module download successful");
        }

    }
}

