using System;
using System.IO;
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
        private string?         cache_dir;
        private NetModuleCache? module_cache;

        [SetUp]
        public void MakeCache()
        {
            cache_dir = TestData.NewTempDir();
            Directory.CreateDirectory(cache_dir);
            module_cache = new NetModuleCache(cache_dir);
        }

        [TearDown]
        public void RemoveCache()
        {
            module_cache?.Dispose();
            module_cache = null;
            if (cache_dir != null)
            {
                Directory.Delete(cache_dir, true);
            }
        }

        [Test]
        public void StoreInvalid()
        {
            // Try to store a nonexistent zip into a NetModuleCache
            // and expect an FileNotFoundKraken
            Assert.Throws<FileNotFoundKraken>(() =>
                module_cache?.Store(
                    TestData.DogeCoinFlag_101_LZMA_module,
                    "/DoesNotExist.zip", new Progress<long>(bytes => {})));

            // Try to store the LZMA-format DogeCoin zip into a NetModuleCache
            // and expect an InvalidModuleFileKraken
            Assert.Throws<InvalidModuleFileKraken>(() =>
                module_cache?.Store(
                    TestData.DogeCoinFlag_101_LZMA_module,
                    TestData.DogeCoinFlagZipLZMA, new Progress<long>(bytes => {})));

            // Try to store the normal DogeCoin zip into a NetModuleCache
            // using the WRONG metadata (file size and hashes)
            // and expect an InvalidModuleFileKraken
            Assert.Throws<InvalidModuleFileKraken>(() =>
                module_cache?.Store(
                    TestData.DogeCoinFlag_101_LZMA_module,
                    TestData.DogeCoinFlagZip(), new Progress<long>(bytes => {})));
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

    }
}
