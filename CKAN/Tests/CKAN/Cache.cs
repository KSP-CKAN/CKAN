using NUnit.Framework;
using System;
using System.IO;
using CKAN;

namespace CKANTests
{
    [TestFixture()]
    public class Cache
    {
        private readonly string cache_dir = Path.Combine(Tests.TestData.DataDir(),"cache_test");

        private CKAN.Cache cache;

        [SetUp()]
        public void MakeCache()
        {
            Directory.CreateDirectory(cache_dir);
            cache = new CKAN.Cache(cache_dir);
        }

        [TearDown()]
        public void RemoveCache()
        {
            Directory.Delete(cache_dir, true);
        }

        [Test()]
        public void CacheKraken()
        {
            string dir = "/this/path/better/not/exist";

            try
            {
                new CKAN.Cache(dir);
            }
            catch (DirectoryNotFoundKraken kraken)
            {
                Assert.AreSame(dir,kraken.directory);
            }
        }

        [Test()]
        public void New()
        {
            // Not much to do here, our Setup() makes an object for us.
            // Let's make sure it's actually there.
            Assert.IsNotNull(cache);
        }

        [Test()]
        public void IsCachedFile()
        {
            string full_file  = Tests.TestData.DogeCoinFlagZip();
            string short_file = Path.GetFileName(full_file);

            Assert.IsFalse(cache.IsCached(short_file));
            Store(full_file);
            Assert.IsTrue(cache.IsCached(short_file));
        }

        [Test()]
        public void IsCachedModule()
        {
            string full_file  = Tests.TestData.DogeCoinFlagZip();
            CKAN.CkanModule module = Tests.TestData.DogeCoinFlag_101_module();

            Assert.IsFalse(cache.IsCached(module));
            Store(full_file);
            Assert.IsTrue(cache.IsCached(module));
        }

        // Stores the file in our cache.
        // This may be good to have in the actual Cache class itself.
        private void Store(string file)
        {
            string dir = cache.CachePath();
            string short_file = Path.GetFileName(file);

            File.Copy(file, Path.Combine(dir, short_file));
        }

    }
}

