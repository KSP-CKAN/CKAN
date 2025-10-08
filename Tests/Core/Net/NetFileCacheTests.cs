using System;
using System.IO;
using System.Linq;
using System.Threading;

using NUnit.Framework;

using CKAN;
using Tests.Data;

namespace Tests.Core
{
    [TestFixture]
    [Category("Cache")]
    public class NetFileCacheTests
    {
        private string?       cache_dir;
        private NetFileCache? cache;

        [SetUp]
        public void MakeCache()
        {
            cache_dir = TestData.NewTempDir();
            Directory.CreateDirectory(cache_dir);
            cache = new NetFileCache(cache_dir);
        }

        [TearDown]
        public void RemoveCache()
        {
            cache?.Dispose();
            cache = null;
            if (cache_dir != null)
            {
                Directory.Delete(cache_dir, true);
            }
        }

        [Test]
        public void Sanity()
        {
            Assert.IsInstanceOf<NetFileCache>(cache);
        }

        [Test]
        public void Store_AFile_Works()
        {
            Uri url = new Uri("http://example.com/");
            string file = TestData.DogeCoinFlagZip();

            // Our URL shouldn't be cached to begin with.
            Assert.IsFalse(cache?.IsCached(url));

            // Store our file.
            cache?.Store(url, file);

            // Now it should be cached.
            Assert.IsTrue(cache?.IsCached(url));
            string? filename = null;
            Assert.IsTrue(cache?.IsCached(url, out filename));
            Assert.AreEqual(Path.Combine(cache_dir!, "9C17E047-DogeCoinFlag-1.01.zip"), filename);
            CollectionAssert.AreEquivalent(new [] { ("9C17E047", 53647) },
                                           cache?.CachedHashesAndSizes());

            // Check contents match.
            var cached_file = cache?.GetCachedFilename(url);
            FileAssert.AreEqual(file, cached_file);
        }

        [Test, TestCase("cheesy.zip",        "cheesy.zip"),
               TestCase("Foo-1-2.3",         "Foo-1-2.3"),
               TestCase("Foo-1-2-3",         "Foo-1-2-3"),
               TestCase("Foo-..-etc-passwd", "Foo-..-etc-passwd")]
        public void Store_WithNameHint_Obeyed(string hint, string appendage)
        {
            Uri url = new Uri("http://example.com/");
            string file = TestData.DogeCoinFlagZip();

            Assert.IsFalse(cache?.IsCached(url));
            cache?.Store(url, file, hint);

            StringAssert.EndsWith(appendage, cache?.GetCachedFilename(url));
        }

        [Test]
        public void Remove_StoredFile_Works()
        {
            Uri url = new Uri("http://example.com/");
            string file = TestData.DogeCoinFlagZip();

            Assert.IsFalse(cache?.IsCached(url));
            cache?.Store(url, file);
            Assert.IsTrue(cache?.IsCached(url));

            cache?.Remove(url);

            Assert.IsFalse(cache?.IsCached(url));
        }

        [Test]
        public void Constructor_InvalidPath_Throws()
        {
            string dir = "/this/path/better/not/exist";

            var exc = Assert.Throws<DirectoryNotFoundKraken>(() =>
            {
                using (var cache = new NetFileCache(dir))
                {
                }
            })!;
            Assert.AreEqual(dir, exc.directory);
        }

        [Test]
        public void Store_SameURLMultipleFiles_Overrites()
        {
            // Store and flip files in our cache. We should always get
            // the most recent file we store for any given URL.

            Uri url = new Uri("http://Double.Rainbow.What.Does.It.Mean/");
            Assert.IsFalse(cache?.IsCached(url));

            string file1 = TestData.DogeCoinFlagZip();
            string file2 = TestData.ModuleManagerZip();

            cache?.Store(url, file1);
            FileAssert.AreEqual(file1, cache?.GetCachedFilename(url));

            cache?.Store(url, file2);
            FileAssert.AreEqual(file2, cache?.GetCachedFilename(url));

            cache?.Store(url, file1);
            FileAssert.AreEqual(file1, cache?.GetCachedFilename(url));
        }

        [Test]
        public void Store_WithBadZip_Works()
        {
            // We could use any URL, but this one is awesome. <3
            Uri url = new Uri("http://kitte.nz/");

            Assert.IsFalse(cache?.IsCached(url));

            // Store a bad zip.
            cache?.Store(url, TestData.DogeCoinFlagZipCorrupt());

            // Make sure it's stored
            Assert.IsTrue(cache?.IsCached(url));

            // Make sure it's not valid as a zip
            Assert.IsFalse(NetModuleCache.ZipValid(cache?.GetCachedFilename(url) ?? "",
                                                   out _, null));

            // Store a good zip.
            cache?.Store(url, TestData.DogeCoinFlagZip());

            // Make sure it's stored, and valid.
            Assert.IsTrue(cache?.IsCached(url));
        }

        [Test]
        public void EnforceSizeLimit_UnderLimit_FileRetained()
        {
            var user = new NullUser();
            using (var repoData = new TemporaryRepositoryData(user))
            {
                // Arrange
                CKAN.Registry registry = CKAN.Registry.Empty(repoData.Manager);
                long fileSize = new FileInfo(TestData.DogeCoinFlagZip()).Length;

                // Act
                Uri url = new Uri("http://kitte.nz/");
                cache?.Store(url, TestData.DogeCoinFlagZip());
                cache?.EnforceSizeLimit(fileSize + 100, registry);

                // Assert
                Assert.IsTrue(cache?.IsCached(url));
            }
        }

        [Test]
        public void EnforceSizeLimit_OverLimit_FileRemoved()
        {
            var user = new NullUser();
            using (var repoData = new TemporaryRepositoryData(user))
            {
                // Arrange
                CKAN.Registry registry = CKAN.Registry.Empty(repoData.Manager);
                long fileSize = new FileInfo(TestData.DogeCoinFlagZip()).Length;
                Uri url1 = new Uri("http://kitte.nz/");
                Uri url2 = new Uri("http://puppi.ez/");

                // Act
                cache?.Store(url1, TestData.DogeCoinFlagZip());
                cache?.Store(url2, TestData.DogeCoinFlagZip());
                cache?.EnforceSizeLimit(fileSize - 100, registry);

                // Assert
                Assert.IsFalse(cache?.IsCached(url1));
            }
        }

        private static object[] HashCachingTestCases()
            => new[]
            {
                new object[]
                {
                    TestData.DogeCoinFlagZip(),
                    TestData.DogeCoinFlag_101_module(),
                }
            };

        [TestCaseSource(nameof(HashCachingTestCases))]
        public void Store_ExternalDeletion_HashesPurged(string     zipPath,
                                                        CkanModule module)
        {
            // Arrange
            if (cache == null)
            {
                // Assertions don't declare non-nullability
                throw new Kraken("Cache is null!");
            }
            var url = module.download?.First();
            if (url == null)
            {
                throw new Kraken("URL is null!");
            }
            var pathInCache = cache.Store(url, zipPath,
                                          CkanModule.StandardName($"{module.identifier}-ExternalDeletionTest",
                                                                  module.version));
            // Calculate the hashes to populate the hash caches
            Assert.AreEqual(module.download_hash?.sha1,
                            cache.GetFileHashSha1(pathInCache, null),
                            "SHA1 hash does not match!");
            Assert.AreEqual(module.download_hash?.sha256,
                            cache.GetFileHashSha256(pathInCache, null),
                            "SHA256 hash does not match!");
            var sha1File = $"{pathInCache}.sha1";
            FileAssert.Exists(sha1File, $"{sha1File} does not exist!");
            var sha256File = $"{pathInCache}.sha256";
            FileAssert.Exists(sha256File, $"{sha256File} does not exist!");

            // Act
            File.Delete(pathInCache);

            // Give the asynchronous event that reacts to deletion time to happen
            Thread.Sleep(100);

            // Assert
            Assert.IsFalse(File.Exists(sha1File),
                           $"{sha1File} not deleted!");
            Assert.IsFalse(File.Exists(sha256File),
                           $"{sha256File} not deleted!");
            Assert.Throws<FileNotFoundException>(() => cache.GetFileHashSha1(pathInCache, null),
                                                 "SHA1 hash is still cached!");
            Assert.Throws<FileNotFoundException>(() => cache.GetFileHashSha256(pathInCache, null),
                                                 "SHA256 hash is still cached!!");
        }


        [TestCaseSource(nameof(HashCachingTestCases))]
        public void GetCachedFilename_FutureTimestamp_Deleted(string     zipPath,
                                                              CkanModule module)
        {
            // Arrange
            if (cache == null)
            {
                // Assertions don't declare non-nullability
                throw new Kraken("Cache is null!");
            }
            var url = module.download?.First();
            if (url == null)
            {
                throw new Kraken("URL is null!");
            }
            var pathInCache = cache.Store(url, zipPath,
                                          CkanModule.StandardName($"{module.identifier}-FutureTimestampTest",
                                                                  module.version));
            // Calculate the hashes to populate the hash caches
            Assert.AreEqual(module.download_hash?.sha1,
                            cache.GetFileHashSha1(pathInCache, null),
                            "SHA1 hash does not match!");
            Assert.AreEqual(module.download_hash?.sha256,
                            cache.GetFileHashSha256(pathInCache, null),
                            "SHA256 hash does not match!");
            var sha1File = $"{pathInCache}.sha1";
            FileAssert.Exists(sha1File, $"{sha1File} does not exist!");
            var sha256File = $"{pathInCache}.sha256";
            FileAssert.Exists(sha256File, $"{sha256File} does not exist!");

            // Act
            var fileTimestamp  = File.GetLastWriteTimeUtc(pathInCache);
            var timelessResult = cache.GetCachedFilename(url);
            var pastResult     = cache.GetCachedFilename(url, fileTimestamp - TimeSpan.FromMinutes(30));
            var futureResult   = cache.GetCachedFilename(url, fileTimestamp + TimeSpan.FromMinutes(30));

            // Assert
            Assert.AreEqual(pathInCache, timelessResult,
                            $"{pathInCache} missing with null timestamp!");
            Assert.AreEqual(pathInCache, pastResult,
                            $"{pathInCache} missing with past timestamp!");
            Assert.AreEqual(null, futureResult,
                            $"{pathInCache} not purged with future timestamp!");
            Assert.IsFalse(File.Exists(sha1File),
                           $"{sha1File} not deleted!");
            Assert.IsFalse(File.Exists(sha256File),
                           $"{sha256File} not deleted!");
            Assert.Throws<FileNotFoundException>(() => cache.GetFileHashSha1(pathInCache, null),
                                                 "SHA1 hash is still cached!");
            Assert.Throws<FileNotFoundException>(() => cache.GetFileHashSha256(pathInCache, null),
                                                 "SHA256 hash is still cached!!");
        }

        private static object[] HashReplacementTestCases()
            => new[]
            {
                new object[]
                {
                    new string[]
                    {
                        TestData.DogeCoinFlagZipLZMA,
                        TestData.DogeCoinFlagZip(),
                        TestData.ModuleManagerZip(),
                    },
                    new CkanModule[]
                    {
                        TestData.DogeCoinFlag_101_LZMA_module,
                        TestData.DogeCoinFlag_101_module(),
                        TestData.ModuleManagerModule()
                    },
                },
            };

        [TestCaseSource(nameof(HashReplacementTestCases))]
        public void GetCachedFilename_ReplaceZIP_HashesUpdated(string[]     zipPaths,
                                                               CkanModule[] modules)
        {
            // Arrange
            if (cache == null)
            {
                // Assertions don't declare non-nullability
                throw new Kraken("Cache is null!");
            }
            var first = modules.First();
            var url = first.download?.First();
            if (url == null)
            {
                throw new Kraken("URL is null!");
            }
            var nameInCache = CkanModule.StandardName($"{first.identifier}-FutureTimestampTest",
                                                      first.version);
            foreach ((string zipPath, CkanModule module) in zipPaths.Zip(modules))
            {
                var pathInCache = cache.Store(url, zipPath, nameInCache);
                Assert.AreEqual(module.download_hash?.sha1,
                                cache.GetFileHashSha1(pathInCache, null),
                                $"SHA1 hash does not match for {zipPath}!");
                Assert.AreEqual(module.download_hash?.sha256,
                                cache.GetFileHashSha256(pathInCache, null),
                                $"SHA256 hash does not match for {zipPath}!");
            }
        }


    }
}
