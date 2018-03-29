using System;
using System.IO;
using System.Runtime.InteropServices;
using CKAN;
using NUnit.Framework;
using Tests.Data;

namespace Tests.Core
{
    [TestFixture]
    public class Cache
    {
        private string cache_dir;
        private string cache_dir_moved;

        private NetFileCache cache;
        private NetModuleCache module_cache;

        [SetUp]
        public void MakeCache()
        {
            cache_dir = TestData.NewTempDir();
            Directory.CreateDirectory(cache_dir);

            cache_dir_moved = TestData.NewTempDir();
            Directory.CreateDirectory(cache_dir_moved);

            cache = new NetFileCache(cache_dir);
            module_cache = new NetModuleCache(cache_dir);
        }

        [TearDown]
        public void RemoveCache()
        {
            Directory.Delete(cache_dir, true);
            Directory.Delete(cache_dir_moved, true);
        }

        [Test]
        public void Sanity()
        {
            Assert.IsInstanceOf<NetFileCache>(cache);
            Assert.IsTrue(Directory.Exists(cache.GetCachePath()));
        }

        [Test]
        public void StoreRetrieve()
        {
            Uri url = new Uri("http://example.com/");
            string file = TestData.DogeCoinFlagZip();

            // Sanity check, our cache dir is there, right?
            Assert.IsTrue(Directory.Exists(cache.GetCachePath()));

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
        public void MoveCacheFailsForNull()
        {
            Uri url = new Uri("http://example.com/");
            string file = TestData.DogeCoinFlagZip();

            Assert.IsFalse(cache.IsCached(url));
            cache.Store(url, file);

            Assert.IsFalse(cache.MoveDefaultCache(null));
        }

        [Test]
        public void MoveCacheFailsForEmptyOrWhitespace()
        {
            Uri url = new Uri("http://example.com/");
            string file = TestData.DogeCoinFlagZip();

            Assert.IsFalse(cache.IsCached(url));
            cache.Store(url, file);

            Assert.IsFalse(cache.MoveDefaultCache(string.Empty));
            Assert.IsFalse(cache.MoveDefaultCache(""));
            Assert.IsFalse(cache.MoveDefaultCache(" "));
        }

        [DllImport("libc", EntryPoint = "chmod", SetLastError = true)]
        private static extern int Sys_chmod(string path, uint mode);

        private const uint S_IRUSR = 0000400;
        private const uint S_IWUSR = 0000200;
        private const uint S_IXUSR = 0000100;

        [Test]
        public void MoveCacheFailsForNoAccess()
        {
            // The following will only work on POSIX systems
            if (Platform.IsWindows)
                return;

            Uri url = new Uri("http://example.com/");
            string file = TestData.DogeCoinFlagZip();

            Assert.IsFalse(cache.IsCached(url), "The example file shouldn't be stored.");
            cache.Store(url, file);

            // Change the permissions (Disable read/write)
            Assert.AreEqual(Sys_chmod(cache_dir_moved, S_IXUSR), 0);

            // Move the cache
            Assert.IsFalse(cache.MoveDefaultCache(cache_dir_moved), "The cache shouldn't be moved if we don't have read/write permission.");

            // Change the permissions (Enable read)
            Assert.AreEqual(Sys_chmod(cache_dir_moved, S_IRUSR), 0);

            // Move the cache
            Assert.IsFalse(cache.MoveDefaultCache(cache_dir_moved), "The cache shouldn't be moved if we don't have write permission.");

            // Change the permissions (Enable write)
            Assert.AreEqual(Sys_chmod(cache_dir_moved, S_IWUSR), 0);

            // Move the cache
            Assert.IsFalse(cache.MoveDefaultCache(cache_dir_moved), "The cache shouldn't be moved if we don't have read permission.");

            // Enable all permissions
            Assert.AreEqual(Sys_chmod(cache_dir_moved, S_IRUSR | S_IWUSR | S_IXUSR), 0);

            // Move the cache
            Assert.IsTrue(cache.MoveDefaultCache(cache_dir_moved), "The cache should be moved if we have full permission.");
        }

        [Test]
        public void MoveCache()
        {
            Uri url = new Uri("http://example.com/");
            string file = TestData.DogeCoinFlagZip();

            Assert.IsFalse(cache.IsCached(url));

            // Cache the file
            string path = cache.Store(url, file);

            // Check that file exists in the old cache dir
            Assert.IsTrue(File.Exists(path));

            // Move the cache
            Assert.IsTrue(cache.MoveDefaultCache(cache_dir_moved));

            // Make sure we still have the file cached
            string newPath = cache.GetCachedFilename(url);
            Assert.IsTrue(File.Exists(newPath));
            Assert.IsFalse(File.Exists(path));
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
                    "/DoesNotExist.zip"
                )
            );

            // Try to store the LZMA-format DogeCoin zip into a NetModuleCache
            // and expect an InvalidModuleFileKraken
            Assert.Throws<InvalidModuleFileKraken>(() =>
                module_cache.Store(
                    TestData.DogeCoinFlag_101_LZMA_module,
                    TestData.DogeCoinFlagZipLZMA
                )
            );

            // Try to store the normal DogeCoin zip into a NetModuleCache
            // using the WRONG metadata (file size and hashes)
            // and expect an InvalidModuleFileKraken
            Assert.Throws<InvalidModuleFileKraken>(() =>
                module_cache.Store(
                    TestData.DogeCoinFlag_101_LZMA_module,
                    TestData.DogeCoinFlagZip()
                )
            );
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

            Assert.IsFalse(cache.IsCachedZip(url));

            // Store a bad zip.
            cache.Store(url, TestData.DogeCoinFlagZipCorrupt());

            // Make sure it's stored, but not valid as a zip
            Assert.IsTrue(cache.IsCached(url));
            Assert.IsFalse(cache.IsCachedZip(url));

            // Store a good zip.
            cache.Store(url, TestData.DogeCoinFlagZip());

            // Make sure it's stored, and valid.
            Assert.IsTrue(cache.IsCached(url));
            Assert.IsTrue(cache.IsCachedZip(url));
        }
    }
}
