using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Tests.Data;
using CKAN;

namespace Tests.Core.Net
{
    /// <summary>
    /// Test the async modules downloader.
    /// </summary>

    [TestFixture]
    public class NetAsyncModulesDownloaderTests
    {
        private GameInstanceManager     manager;
        private RegistryManager         registry_manager;
        private CKAN.Registry           registry;
        private DisposableKSP           ksp;
        private NetModuleCache          cache;
        private TemporaryRepositoryData repoData;

        [SetUp]
        public void Setup()
        {
            var user = new NullUser();

            var repos = new SortedDictionary<string, Repository>()
            {
                {
                    "testRepo",
                    new Repository("testRepo", TestData.TestKANZip())
                }
            };

            repoData = new TemporaryRepositoryData(user, repos.Values);

            manager = new GameInstanceManager(user);
            // Give us a registry to play with.
            ksp = new DisposableKSP();
            registry_manager = RegistryManager.Instance(ksp.KSP, repoData.Manager);
            registry = registry_manager.registry;
            registry.Installed().Clear();
            // Make sure we have a registry we can use.
            registry.RepositoriesSet(repos);

            // General shortcuts
            cache = manager.Cache;
        }

        [TearDown]
        public void TearDown()
        {
            manager.Dispose();
            ksp.Dispose();
            repoData.Dispose();
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
    }
}
