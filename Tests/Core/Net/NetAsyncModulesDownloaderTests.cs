using System;
using System.Collections.Generic;
using System.Linq;

using log4net;
using NUnit.Framework;

using Tests.Data;
using CKAN;
using CKAN.Extensions;

namespace Tests.Core.Net
{
    /// <summary>
    /// Test the async modules downloader.
    /// </summary>

    [TestFixture]
    public class NetAsyncModulesDownloaderTests
    {
        private CKAN.GameInstanceManager      manager;
        private CKAN.RegistryManager registry_manager;
        private CKAN.Registry        registry;
        private DisposableKSP        ksp;
        private CKAN.IDownloader     async;
        private NetModuleCache       cache;
        private NetAsyncDownloader   downloader;

        private static readonly ILog log = LogManager.GetLogger(typeof(NetAsyncModulesDownloaderTests));

        [SetUp]
        public void Setup()
        {
            manager = new GameInstanceManager(new NullUser());
            // Give us a registry to play with.
            ksp = new DisposableKSP();
            registry_manager = CKAN.RegistryManager.Instance(ksp.KSP);
            registry = registry_manager.registry;
            registry.Installed().Clear();
            // Make sure we have a registry we can use.

            var repos = new SortedDictionary<string, Repository>()
            {
                {
                    "testRepo",
                    new Repository("testRepo", TestData.TestKANZip())
                }
            };

            downloader = new NetAsyncDownloader(new NullUser());
            registry.RepositoriesSet(repos);

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

        [Test,
            // No modules, not valid
            TestCase(new string[] { },
                     new string[] { },
                     null),
            // One module, no settings, preserve order
            TestCase(new string[]
                     {
                         @"{
                            ""identifier"": ""ModA"",
                            ""version"": ""1.0"",
                            ""download"": [ ""https://spacedock.info/"", ""https://github.com/"" ]
                         }",
                     },
                     new string[] { },
                     new string[] { "https://spacedock.info/", "https://github.com/" }),
            // Multiple mods, redistributable license w/ hash, sort by priority w/ implicit archive.org fallback
            TestCase(new string[]
                     {
                         @"{
                            ""identifier"": ""ModA"",
                            ""version"": ""1.0"",
                            ""license"": ""GPL-3.0"",
                            ""download"": [ ""https://spacedock.info/"", ""https://github.com/"" ],
                            ""download_hash"": { ""sha1"": ""DEADBEEFDEADBEEF""}
                         }",
                         @"{
                            ""identifier"": ""ModB"",
                            ""version"": ""1.0"",
                            ""license"": ""GPL-3.0"",
                            ""download"": [ ""https://spacedock.info/"", ""https://github.com/"" ],
                            ""download_hash"": { ""sha1"": ""DEADBEEFDEADBEEF""}
                         }",
                         @"{
                            ""identifier"": ""ModC"",
                            ""version"": ""1.0"",
                            ""license"": ""GPL-3.0"",
                            ""download"": [ ""https://spacedock.info/"", ""https://github.com/"" ],
                            ""download_hash"": { ""sha1"": ""DEADBEEFDEADBEEF""}
                         }",
                     },
                     new string[] { "github.com", null },
                     new string[]
                     {
                         "https://github.com/",
                         "https://spacedock.info/",
                         "https://archive.org/download/ModA-1.0/DEADBEEF-ModA-1.0.zip",
                         "https://archive.org/download/ModB-1.0/DEADBEEF-ModB-1.0.zip",
                         "https://archive.org/download/ModC-1.0/DEADBEEF-ModC-1.0.zip"
                     }),
        ]
        public void TargetFromModuleGroup_WithModules_ExpectedTarget(string[] moduleJsons, string[] preferredHosts, string[] correctURLs)
        {
            // Arrange
            var group = moduleJsons.Select(j => CkanModule.FromJson(j))
                                   .ToHashSet();
            var downloader = new NetAsyncModulesDownloader(new NullUser(), cache);

            if (correctURLs == null)
            {
                // Act / Assert
                Assert.Throws<InvalidOperationException>(() =>
                    downloader.TargetFromModuleGroup(group, preferredHosts));
            }
            else
            {
                // Act
                var result = downloader.TargetFromModuleGroup(group, preferredHosts);
                var urls   = result.urls.Select(u => u.ToString()).ToArray();

                // Assert
                Assert.AreEqual(correctURLs, urls);
            }
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
