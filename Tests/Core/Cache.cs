using System;
using System.IO;
using System.Threading;
using System.Globalization;

using NUnit.Framework;
using Tests.Data;

using CKAN;

namespace Tests.Core
{
    [TestFixture]
    public class Cache
    {
        private string cache_dir;

        private NetFileCache   cache;
        private NetModuleCache module_cache;

        [SetUp]
        public void MakeCache()
        {
            cache_dir = TestData.NewTempDir();
            Directory.CreateDirectory(cache_dir);
            cache        = new NetFileCache(cache_dir);
            module_cache = new NetModuleCache(cache_dir);
        }

        [TearDown]
        public void RemoveCache()
        {
            cache.Dispose();
            cache = null;
            module_cache.Dispose();
            module_cache = null;
            Directory.Delete(cache_dir, true);
        }

        [Test]
        public void Sanity()
        {
            Assert.IsInstanceOf<NetFileCache>(cache);
            Assert.IsInstanceOf<NetModuleCache>(module_cache);
        }

        [Test]
        public void StoreRetrieve()
        {
            Uri url = new Uri("http://example.com/");
            string file = TestData.DogeCoinFlagZip();

            // Our URL shouldn't be cached to begin with.
            Assert.IsFalse(cache.IsCached(url));

            // Store our file.
            cache.Store(url, file);

            // Now it should be cached.
            Assert.IsTrue(cache.IsCached(url));

            // Check contents match.
            string cached_file = cache.GetCachedFilename(url);
            FileAssert.AreEqual(file, cached_file);
        }

        [Test, TestCase("cheesy.zip","cheesy.zip"), TestCase("Foo-1-2.3","Foo-1-2.3"),
            TestCase("Foo-1-2-3","Foo-1-2-3"), TestCase("Foo-..-etc-passwd","Foo-..-etc-passwd")]
        public void NamingHints(string hint, string appendage)
        {
            Uri url = new Uri("http://example.com/");
            string file = TestData.DogeCoinFlagZip();

            Assert.IsFalse(cache.IsCached(url));
            cache.Store(url, file, hint);

            StringAssert.EndsWith(appendage, cache.GetCachedFilename(url));
        }

        [Test]
        public void StoreRemove()
        {
            Uri url = new Uri("http://example.com/");
            string file = TestData.DogeCoinFlagZip();

            Assert.IsFalse(cache.IsCached(url));
            cache.Store(url, file);
            Assert.IsTrue(cache.IsCached(url));

            cache.Remove(url);

            Assert.IsFalse(cache.IsCached(url));
        }

        [Test]
        public void CacheKraken()
        {
            string dir = "/this/path/better/not/exist";

            try
            {
                new NetFileCache(dir);
            }
            catch (DirectoryNotFoundKraken kraken)
            {
                Assert.AreSame(dir, kraken.directory);
            }
        }

        [Test]
        public void StoreInvalid()
        {
            // Try to store a nonexistent zip into a NetModuleCache
            // and expect an FileNotFoundKraken
            Assert.Throws<FileNotFoundKraken>(() =>
                module_cache.Store(
                    TestData.DogeCoinFlag_101_LZMA_module,
                    "/DoesNotExist.zip", new Progress<int>(percent => {})));

            // Try to store the LZMA-format DogeCoin zip into a NetModuleCache
            // and expect an InvalidModuleFileKraken
            Assert.Throws<InvalidModuleFileKraken>(() =>
                module_cache.Store(
                    TestData.DogeCoinFlag_101_LZMA_module,
                    TestData.DogeCoinFlagZipLZMA, new Progress<int>(percent => {})));

            // Try to store the normal DogeCoin zip into a NetModuleCache
            // using the WRONG metadata (file size and hashes)
            // and expect an InvalidModuleFileKraken
            Assert.Throws<InvalidModuleFileKraken>(() =>
                module_cache.Store(
                    TestData.DogeCoinFlag_101_LZMA_module,
                    TestData.DogeCoinFlagZip(), new Progress<int>(percent => {})));
        }

        [Test]
        public void DoubleCache()
        {
            // Store and flip files in our cache. We should always get
            // the most recent file we store for any given URL.

            Uri url = new Uri("http://Double.Rainbow.What.Does.It.Mean/");
            Assert.IsFalse(cache.IsCached(url));

            string file1 = TestData.DogeCoinFlagZip();
            string file2 = TestData.ModuleManagerZip();

            cache.Store(url, file1);
            FileAssert.AreEqual(file1, cache.GetCachedFilename(url));

            cache.Store(url, file2);
            FileAssert.AreEqual(file2, cache.GetCachedFilename(url));

            cache.Store(url, file1);
            FileAssert.AreEqual(file1, cache.GetCachedFilename(url));
        }

        [Test]
        public void ZipValidation()
        {
            // We could use any URL, but this one is awesome. <3
            Uri url = new Uri("http://kitte.nz/");

            Assert.IsFalse(cache.IsCached(url));

            // Store a bad zip.
            cache.Store(url, TestData.DogeCoinFlagZipCorrupt());

            // Make sure it's stored
            Assert.IsTrue(cache.IsCached(url));
            // Make sure it's not valid as a zip
            Assert.IsFalse(NetModuleCache.ZipValid(cache.GetCachedFilename(url), out _, null));

            // Store a good zip.
            cache.Store(url, TestData.DogeCoinFlagZip());

            // Make sure it's stored, and valid.
            Assert.IsTrue(cache.IsCached(url));
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
            string reason = null;

            Assert.DoesNotThrow(() =>
                valid = NetModuleCache.ZipValid(TestData.ZipWithUnicodeChars, out reason, null));
            Assert.IsTrue(valid, reason);
        }

        [Test]
        public void EnforceSizeLimit_UnderLimit_FileRetained()
        {
            // Arrange
            CKAN.Registry registry = CKAN.Registry.Empty();
            long fileSize = new FileInfo(TestData.DogeCoinFlagZip()).Length;

            // Act
            Uri url = new Uri("http://kitte.nz/");
            cache.Store(url, TestData.DogeCoinFlagZip());
            cache.EnforceSizeLimit(fileSize + 100, registry);

            // Assert
            Assert.IsTrue(cache.IsCached(url));
        }

        [Test]
        public void EnforceSizeLimit_OverLimit_FileRemoved()
        {
            // Arrange
            CKAN.Registry registry = CKAN.Registry.Empty();
            long fileSize = new FileInfo(TestData.DogeCoinFlagZip()).Length;

            // Act
            Uri url = new Uri("http://kitte.nz/");
            cache.Store(url, TestData.DogeCoinFlagZip());
            cache.EnforceSizeLimit(fileSize - 100, registry);

            // Assert
            Assert.IsFalse(cache.IsCached(url));
        }

    }
}
