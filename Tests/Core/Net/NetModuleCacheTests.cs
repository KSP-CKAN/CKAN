using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Globalization;

using NUnit.Framework;

using CKAN;
using Tests.Data;

namespace Tests.Core
{
    [TestFixture]
    [Category("Cache")]
    public class NetModuleCacheTests
    {
        [Test]
        public void StorePurgeIsCached_WithModule_Works()
        {
            // Arrange
            var module = TestData.DogeCoinFlag_101_module();
            using (var dir   = new TemporaryDirectory())
            using (var cache = new NetModuleCache(dir))
            {
                int storeCount = 0;
                int purgeCount = 0;
                cache.ModStored += m => ++storeCount;
                cache.ModPurged += m => ++purgeCount;

                // Act / Assert
                Assert.IsFalse(cache.IsCached(module));

                cache.Store(module, TestData.DogeCoinFlagZip(), null);
                Assert.AreEqual(1, storeCount);
                Assert.IsTrue(cache.IsCached(module));
                CollectionAssert.AreEquivalent(
                    new Dictionary<string, long>()
                    {
                        { "kerbalstuff.com", 53647 },
                    },
                    cache.CachedFileSizeByHost(
                        module.download!.ToDictionary(NetFileCache.CreateURLHash,
                                                      url => url)));

                cache.Purge(module);
                Assert.AreEqual(1, purgeCount);
                Assert.IsFalse(cache.IsCached(module));

                cache.Store(module, TestData.DogeCoinFlagZip(), null);
                Assert.AreEqual(2, storeCount);
                Assert.IsTrue(cache.IsCached(module));

                cache.Purge(new CkanModule[] { module });
                Assert.AreEqual(2, purgeCount);
                Assert.IsFalse(cache.IsCached(module));
            }
        }

        [Test]
        public void Store_Invalid_Throws()
        {
            using (var dir   = new TemporaryDirectory())
            using (var cache = new NetModuleCache(dir))
            {
                // Try to store a nonexistent zip into a NetModuleCache
                // and expect an FileNotFoundKraken
                Assert.Throws<FileNotFoundKraken>(() =>
                    cache?.Store(
                        TestData.DogeCoinFlag_101_LZMA_module,
                        "/DoesNotExist.zip", new Progress<long>(bytes => {})));

                // Try to store the LZMA-format DogeCoin zip into a NetModuleCache
                // and expect an InvalidModuleFileKraken
                Assert.Throws<InvalidModuleFileKraken>(() =>
                    cache?.Store(
                        TestData.DogeCoinFlag_101_LZMA_module,
                        TestData.DogeCoinFlagZipLZMA, new Progress<long>(bytes => {})));

                // Try to store the normal DogeCoin zip into a NetModuleCache
                // using the WRONG metadata (file size and hashes)
                // and expect an InvalidModuleFileKraken
                Assert.Throws<InvalidModuleFileKraken>(() =>
                    cache?.Store(
                        TestData.DogeCoinFlag_101_LZMA_module,
                        TestData.DogeCoinFlagZip(), new Progress<long>(bytes => {})));
            }
        }

        [Test]
        public void ZipValid_ContainsFilenameWithBadChars_NoException()
        {
            // We want to inspect a localized error message below.
            // Switch to English to ensure it's what we expect.
            CultureInfo origUICulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            bool valid = false;
            string reason = "";
            Assert.DoesNotThrow(() =>
                valid = NetModuleCache.ZipValid(TestData.ZipWithBadChars, out reason, null));

            // The file is considered valid on Linux;
            // only check the reason if found invalid
            if (!valid)
            {
                Assert.AreEqual(
                    @"Error in step EntryHeader for GameData/FlagPack/Flags/Weyland-Yutani from ""Alien"".png: Exception during test - 'Name is invalid'",
                    reason
                );
            }

            // Switch back to the original locale
            Thread.CurrentThread.CurrentUICulture = origUICulture;
        }

        [Test]
        public void ZipValid_ContainsFilenameWithUnicodeChars_Valid()
        {
            bool valid = false;
            string? reason = null;

            Assert.DoesNotThrow(() =>
                valid = NetModuleCache.ZipValid(TestData.ZipWithUnicodeChars, out reason, null));
            Assert.IsTrue(valid, reason);
        }

        [Test]
        public void EnforceSizeLimit_HighAndLowLimits_DeletesWithSmallLimit()
        {
            // Arrange
            using (var cacheDir = new TemporaryDirectory())
            using (var sut      = new NetModuleCache(cacheDir))
            {
                var registry = CKAN.Registry.Empty(new RepositoryDataManager());

                // Act
                var stored   = sut.Store(TestData.DogeCoinFlag_101_module(),
                                         TestData.DogeCoinFlagZip(), null);

                // Assert
                FileAssert.Exists(stored);

                // Act
                sut.EnforceSizeLimit(100000, registry);

                // Assert
                FileAssert.Exists(stored);

                // Act
                sut.EnforceSizeLimit(40000, registry);

                // Assert
                FileAssert.DoesNotExist(stored);
            }
        }

        [Test]
        public void CheckFreeSpace_WithSmallAndLargeSizes_ThrowsIfLarge()
        {
            // Arrange
            using (var cacheDir = new TemporaryDirectory())
            using (var sut      = new NetModuleCache(cacheDir))
            {

                // Act / Assert
                Assert.DoesNotThrow(() => sut.CheckFreeSpace(1024));
                Assert.Throws<NotEnoughSpaceKraken>(() => sut.CheckFreeSpace(long.MaxValue));
            }
        }
    }
}
