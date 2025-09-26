using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using CKAN;
using CKAN.IO;
using Tests.Data;
using Tests.Core.Configuration;

namespace Tests.Core.Net
{
    /// <summary>
    /// Test the async modules downloader.
    /// </summary>

    [TestFixture]
    public class NetAsyncModulesDownloaderTests
    {
        private GameInstanceManager?     manager;
        private RegistryManager?         registry_manager;
        private CKAN.Registry?           registry;
        private DisposableKSP?           ksp;
        private NetModuleCache?          cache;
        private TemporaryRepositoryData? repoData;
        private FakeConfiguration?       config;

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

            // Give us a registry to play with.
            ksp = new DisposableKSP();
            config = new FakeConfiguration(ksp.KSP, ksp.KSP.Name);
            manager = new GameInstanceManager(user, config);
            registry_manager = RegistryManager.Instance(ksp.KSP, repoData.Manager,
                                                        repos.Values);
            registry = registry_manager.registry;
            registry.Installed().Clear();

            // General shortcuts
            cache = manager.Cache;
        }

        [TearDown]
        public void TearDown()
        {
            registry_manager?.Dispose();
            config?.Dispose();
            manager?.Dispose();
            ksp?.Dispose();
            repoData?.Dispose();
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
                            ""spec_version"": 1,
                            ""identifier"": ""ModA"",
                            ""author"": ""ModderA"",
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
                            ""spec_version"": 1,
                            ""identifier"": ""ModA"",
                            ""author"": ""Modder"",
                            ""version"": ""1.0"",
                            ""license"": ""GPL-3.0"",
                            ""download"": [ ""https://spacedock.info/"", ""https://github.com/"" ],
                            ""download_hash"": { ""sha1"": ""DEADBEEFDEADBEEF""}
                         }",
                         @"{
                            ""spec_version"": 1,
                            ""identifier"": ""ModB"",
                            ""author"": ""Modder"",
                            ""version"": ""1.0"",
                            ""license"": ""GPL-3.0"",
                            ""download"": [ ""https://spacedock.info/"", ""https://github.com/"" ],
                            ""download_hash"": { ""sha1"": ""DEADBEEFDEADBEEF""}
                         }",
                         @"{
                            ""spec_version"": 1,
                            ""identifier"": ""ModC"",
                            ""author"": ""Modder"",
                            ""version"": ""1.0"",
                            ""license"": ""GPL-3.0"",
                            ""download"": [ ""https://spacedock.info/"", ""https://github.com/"" ],
                            ""download_hash"": { ""sha1"": ""DEADBEEFDEADBEEF""}
                         }",
                     },
                     new string?[] { "github.com", null },
                     new string[]
                     {
                         "https://github.com/",
                         "https://spacedock.info/",
                         "https://archive.org/download/ModA-1.0/DEADBEEF-ModA-1.0.zip",
                         "https://archive.org/download/ModB-1.0/DEADBEEF-ModB-1.0.zip",
                         "https://archive.org/download/ModC-1.0/DEADBEEF-ModC-1.0.zip"
                     }),
        ]
        public void TargetFromModuleGroup_WithModules_ExpectedTarget(string[]  moduleJsons,
                                                                     string?[] preferredHosts,
                                                                     string[]  correctURLs)
        {
            // Arrange
            var group = moduleJsons.Select(CkanModule.FromJson)
                                   .ToHashSet();
            var downloader = new NetAsyncModulesDownloader(new NullUser(), cache!);

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

        [Test,
            // Only one bad URL
            TestCase(new string[] { "DoesNotExist.zip" },
                     new string[] { "DoesNotExist.zip" }),
            // First URL is bad, some fail to store
            TestCase(new string[]
                     {
                         "DoesNotExist.zip",
                         "gh221.zip",
                         "ModuleManager-2.5.1.zip",
                         "ZipWithUnicodeChars.zip",
                         "DogeCoinPlugin.zip",
                         "DogeCoinFlag-1.01-corrupt.zip",
                         "CKAN-meta-testkan.zip",
                         "DogeCoinFlag-1.01-no-dir-entries.zip",
                         "DogeTokenFlag-1.01.zip",
                         "DogeCoinFlag-1.01.zip",
                         "DogeCoinFlag-1.01-LZMA.zip",
                         "DogeCoinFlag-1.01-avc.zip",
                         "DogeCoinFlag-extra-files.zip",
                     },
                     new string[]
                     {
                         "DoesNotExist.zip",
                         "DogeCoinFlag-1.01-corrupt.zip",
                         "DogeCoinFlag-1.01-LZMA.zip",
                     }),
            // A URL in the middle is bad, some fail to store
            TestCase(new string[]
                     {
                         "gh221.zip",
                         "ModuleManager-2.5.1.zip",
                         "ZipWithUnicodeChars.zip",
                         "DogeCoinPlugin.zip",
                         "DogeCoinFlag-1.01-corrupt.zip",
                         "CKAN-meta-testkan.zip",
                         "DogeCoinFlag-1.01-no-dir-entries.zip",
                         "DoesNotExist.zip",
                         "DogeTokenFlag-1.01.zip",
                         "DogeCoinFlag-1.01.zip",
                         "DogeCoinFlag-1.01-LZMA.zip",
                         "DogeCoinFlag-1.01-avc.zip",
                         "DogeCoinFlag-extra-files.zip",
                     },
                     new string[]
                     {
                         "DoesNotExist.zip",
                         "DogeCoinFlag-1.01-corrupt.zip",
                         "DogeCoinFlag-1.01-LZMA.zip",
                     }),
            // Last URL is bad, some fail to store
            TestCase(new string[]
                     {
                         "gh221.zip",
                         "ModuleManager-2.5.1.zip",
                         "ZipWithUnicodeChars.zip",
                         "DogeCoinPlugin.zip",
                         "DogeCoinFlag-1.01-corrupt.zip",
                         "CKAN-meta-testkan.zip",
                         "DogeCoinFlag-1.01-no-dir-entries.zip",
                         "DogeTokenFlag-1.01.zip",
                         "DogeCoinFlag-1.01.zip",
                         "DogeCoinFlag-1.01-LZMA.zip",
                         "DogeCoinFlag-1.01-avc.zip",
                         "DogeCoinFlag-extra-files.zip",
                         "DoesNotExist.zip",
                     },
                     new string[]
                     {
                         "DoesNotExist.zip",
                         "DogeCoinFlag-1.01-corrupt.zip",
                         "DogeCoinFlag-1.01-LZMA.zip",
                     }),
            // Every other URL is bad, some fail to store
            TestCase(new string[]
                     {
                         "DoesNotExist1.zip",
                         "gh221.zip",
                         "DoesNotExist2.zip",
                         "ModuleManager-2.5.1.zip",
                         "DoesNotExist.zip",
                         "ZipWithUnicodeChars.zip",
                         "DoesNotExist3.zip",
                         "DogeCoinPlugin.zip",
                         "DoesNotExist4.zip",
                         "DogeCoinFlag-1.01-corrupt.zip",
                         "DoesNotExist5.zip",
                         "CKAN-meta-testkan.zip",
                         "DoesNotExist6.zip",
                         "DogeCoinFlag-1.01-no-dir-entries.zip",
                         "DoesNotExist7.zip",
                         "DogeTokenFlag-1.01.zip",
                         "DoesNotExist8.zip",
                         "DogeCoinFlag-1.01.zip",
                         "DoesNotExist9.zip",
                         "DogeCoinFlag-1.01-LZMA.zip",
                         "DoesNotExist10.zip",
                         "DogeCoinFlag-1.01-avc.zip",
                         "DoesNotExist11.zip",
                         "DogeCoinFlag-extra-files.zip",
                         "DoesNotExist12.zip",
                     },
                     new string[]
                     {
                         "DoesNotExist1.zip",
                         "DoesNotExist2.zip",
                         "DoesNotExist.zip",
                         "DoesNotExist3.zip",
                         "DoesNotExist4.zip",
                         "DogeCoinFlag-1.01-corrupt.zip",
                         "DoesNotExist5.zip",
                         "DoesNotExist6.zip",
                         "DoesNotExist7.zip",
                         "DoesNotExist8.zip",
                         "DoesNotExist9.zip",
                         "DoesNotExist10.zip",
                         "DogeCoinFlag-1.01-LZMA.zip",
                         "DoesNotExist11.zip",
                         "DoesNotExist12.zip",
                     }),
        ]
        public void ModulesAsTheyFinish_InvalidURLsAndFiles_ThrowsModuleDownloadErrorsKraken(
            string[] pathsWithinTestData, string[] failCases)
        {
            // Arrange
            using (var cacheDir = new TemporaryDirectory())
            using (var cache    = new NetModuleCache(cacheDir.Directory.FullName))
            {
                var downloader = new NetAsyncModulesDownloader(new NullUser(), cache);
                var modules    = pathsWithinTestData.Select(TestData.DataDir)
                                                    .Select(Path.GetFullPath)
                                                    .Select(CKANPathUtils.NormalizePath)
                                                    .Select((p, i) =>
                                                        $@"{{
                                                            ""identifier"": ""Mod{i}"",
                                                            ""version"":    ""1.0"",
                                                            ""download"":   ""{p}""
                                                        }}")
                                                    .Select(CkanModule.FromJson)
                                                    .ToArray();
                var badURLs    = failCases.Select(TestData.DataDir)
                                          .Select(Path.GetFullPath)
                                          .Select(CKANPathUtils.NormalizePath)
                                          .Select(p => new Uri(p))
                                          .ToArray();

                // Act / Assert
                var exc = Assert.Throws<ModuleDownloadErrorsKraken>(() =>
                {
                    var gotModules = downloader.ModulesAsTheyFinish(Array.Empty<CkanModule>(),
                                                                    modules)
                                               .ToArray();
                });
                CollectionAssert.AreEquivalent(badURLs,
                                               exc!.Exceptions.SelectMany(kvp => kvp.Key.download!));
            }
        }
    }
}
