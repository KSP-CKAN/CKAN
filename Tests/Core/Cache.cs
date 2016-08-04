using CKAN;
using NUnit.Framework;
using System;
using System.IO;
using Tests.Data;

namespace Tests.Core
{
    [TestFixture]
    public class Cache
    {
        private readonly string cache_dir = Path.Combine(TestData.DataDir(), "cache_test");

        private NetFileCache cache;

        [SetUp]
        public void MakeCache()
        {
            Directory.CreateDirectory(cache_dir);
            cache = new NetFileCache(cache_dir);
        }

        [TearDown]
        public void RemoveCache()
        {
            Directory.Delete(cache_dir, true);
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

        [Test, TestCase("cheesy.zip", "cheesy.zip"), TestCase("Foo-1-2.3", "Foo-1-2.3"),
            TestCase("Foo-1-2-3", "Foo-1-2-3"), TestCase("Foo-..-etc-passwd", "Foo-..-etc-passwd")]
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