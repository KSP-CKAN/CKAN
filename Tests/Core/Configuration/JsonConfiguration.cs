using CKAN.Configuration;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Tests.Data;

namespace Tests.Core.Configuration
{
    [TestFixture]
    public class JsonConfigurationTests
    {
        [Test]
        public void CreatesNewConfig()
        {
            string tmpFile = Path.GetTempFileName();
            File.Delete(tmpFile);

            _ = new JsonConfiguration(tmpFile);

            Assert.IsTrue(File.Exists(tmpFile));

            File.Delete(tmpFile);
        }

        [Test]
        public void CreatesNewConfigAndDirectory()
        {
            string tmpDir = Path.GetTempFileName();
            File.Delete(tmpDir);

            string tmpFile = Path.Combine(tmpDir, "config.json");

            _ = new JsonConfiguration(tmpFile);

            Assert.IsTrue(File.Exists(tmpFile));

            Directory.Delete(tmpDir, true);
        }

        [Test]
        public void LoadsGoodConfig()
        {
            string tmpFile = Path.GetTempFileName();
            File.WriteAllText(tmpFile, TestData.GoodJsonConfig());

            var reg = new JsonConfiguration(tmpFile);

            CollectionAssert.AreEquivalent(new List<Tuple<string, string, string>>()
            {
                new Tuple<string, string, string>("instance1", "instance1_path", "KSP"),
                new Tuple<string, string, string>("instance2", "instance2_path", "KSP")
            }, reg.GetInstances());

            CollectionAssert.AreEquivalent(new List<string>()
            {
                "host1",
                "host2",
                "host3"
            }, reg.GetAuthTokenHosts());

            Assert.IsTrue(reg.TryGetAuthToken("host1", out string token));
            Assert.AreEqual("token1", token);
            Assert.IsTrue(reg.TryGetAuthToken("host2", out token));
            Assert.AreEqual("token2", token);
            Assert.IsTrue(reg.TryGetAuthToken("host3", out token));
            Assert.AreEqual("token3", token);

            Assert.AreEqual("asi", reg.AutoStartInstance);
            Assert.AreEqual("dci", reg.DownloadCacheDir);
            Assert.AreEqual(2, reg.CacheSizeLimit);
            Assert.AreEqual(4, reg.RefreshRate);

            File.Delete(tmpFile);
        }

        [Test]
        public void LoadsMissingJsonConfig()
        {
            string tmpFile = Path.GetTempFileName();
            File.WriteAllText(tmpFile, TestData.MissingJsonConfig());

            var reg = new JsonConfiguration(tmpFile);

            CollectionAssert.AreEquivalent(new List<Tuple<string, string, string>>()
            {
                new Tuple<string, string, string>("instance1", "instance1_path", "KSP"),
                new Tuple<string, string, string>("instance2", "instance2_path", "KSP")
            }, reg.GetInstances());

            CollectionAssert.AreEquivalent(new List<string>(), reg.GetAuthTokenHosts());

            Assert.AreEqual("", reg.AutoStartInstance);
            Assert.AreEqual(JsonConfiguration.DefaultDownloadCacheDir, reg.DownloadCacheDir);
            Assert.AreEqual(null, reg.CacheSizeLimit);
            Assert.AreEqual(4, reg.RefreshRate);

            File.Delete(tmpFile);
        }

        [Test]
        public void LoadsEmptyConfig()
        {
            string tmpFile = Path.GetTempFileName();
            var reg = new JsonConfiguration(tmpFile);

            CollectionAssert.AreEquivalent(new List<Tuple<string, string>>(), reg.GetInstances());
            CollectionAssert.AreEquivalent(new List<string>(), reg.GetAuthTokenHosts());

            Assert.AreEqual("", reg.AutoStartInstance);
            Assert.AreEqual(JsonConfiguration.DefaultDownloadCacheDir, reg.DownloadCacheDir);
            Assert.AreEqual(null, reg.CacheSizeLimit);
            Assert.AreEqual(0, reg.RefreshRate);

            File.Delete(tmpFile);
        }

        [Test]
        public void LoadsExtraConfig()
        {
            string tmpFile = Path.GetTempFileName();
            File.WriteAllText(tmpFile, TestData.ExtraJsonConfig());

            var reg = new JsonConfiguration(tmpFile);

            CollectionAssert.AreEquivalent(new List<Tuple<string, string, string>>()
            {
                new Tuple<string, string, string>("instance1", "instance1_path", "KSP"),
                new Tuple<string, string, string>("instance2", "instance2_path", "KSP")
            }, reg.GetInstances());

            CollectionAssert.AreEquivalent(new List<string>()
            {
                "host1",
                "host2",
                "host3"
            }, reg.GetAuthTokenHosts());

            Assert.IsTrue(reg.TryGetAuthToken("host1", out string token));
            Assert.AreEqual("token1", token);
            Assert.IsTrue(reg.TryGetAuthToken("host2", out token));
            Assert.AreEqual("token2", token);
            Assert.IsTrue(reg.TryGetAuthToken("host3", out token));
            Assert.AreEqual("token3", token);

            Assert.AreEqual("asi", reg.AutoStartInstance);
            Assert.AreEqual("dci", reg.DownloadCacheDir);
            Assert.AreEqual(2, reg.CacheSizeLimit);
            Assert.AreEqual(4, reg.RefreshRate);

            File.Delete(tmpFile);
        }

        [Test]
        public void FailsToLoadBadConfig()
        {
            string tmpFile = Path.GetTempFileName();
            File.WriteAllText(tmpFile, TestData.BadJsonConfig());

            Assert.Catch<JsonException>(delegate
            {
                _ = new JsonConfiguration(tmpFile);
            });

            File.Delete(tmpFile);
        }

#if !NETCOREAPP
        // TODO: Migration tests.

        // I don't see any good way to do these without overwriting the
        // registry values, which is fine for the build server but may be
        // annoying for devs.
#endif

        [Test,
            TestCase("asi-test", "asi-test"),
            TestCase("", ""),
            TestCase(null, "")]
        public void AutoStartInstancePersists(string val, string expected)
        {
            string tmpFile1 = Path.GetTempFileName();
            _ = new JsonConfiguration(tmpFile1)
            {
                AutoStartInstance = val
            };

            string tmpFile2 = Path.GetTempFileName();
            File.Copy(tmpFile1, tmpFile2, true);
            JsonConfiguration reg = new JsonConfiguration(tmpFile2);

            Assert.AreEqual(expected, reg.AutoStartInstance);

            File.Delete(tmpFile1);
            File.Delete(tmpFile2);
        }

        [Test]
        public void DownloadCacheDirPersistsRooted()
        {
            string tmpFile1 = Path.GetTempFileName();
            var reg = new JsonConfiguration(tmpFile1);

            var file = Path.GetFullPath("test_path");

            reg.DownloadCacheDir = file;

            string tmpFile2 = Path.GetTempFileName();
            File.Copy(tmpFile1, tmpFile2, true);
            reg = new JsonConfiguration(tmpFile2);

            Assert.AreEqual(file, reg.DownloadCacheDir);

            File.Delete(tmpFile1);
            File.Delete(tmpFile2);
        }

        [Test]
        public void DownloadCacheDirPersistsUnrooted()
        {
            string tmpFile1 = Path.GetTempFileName();
            _ = new JsonConfiguration(tmpFile1)
            {
                DownloadCacheDir = "file"
            };

            string tmpFile2 = Path.GetTempFileName();
            File.Copy(tmpFile1, tmpFile2, true);
            JsonConfiguration reg = new JsonConfiguration(tmpFile2);

            Assert.AreEqual(Path.GetFullPath("file"), reg.DownloadCacheDir);

            File.Delete(tmpFile1);
            File.Delete(tmpFile2);
        }


        [Test]
        public void DownloadCacheDirPersistsNull()
        {
            string tmpFile1 = Path.GetTempFileName();
            _ = new JsonConfiguration(tmpFile1)
            {
                DownloadCacheDir = null
            };

            string tmpFile2 = Path.GetTempFileName();
            File.Copy(tmpFile1, tmpFile2, true);
            JsonConfiguration reg = new JsonConfiguration(tmpFile2);

            Assert.AreEqual(JsonConfiguration.DefaultDownloadCacheDir, reg.DownloadCacheDir);

            File.Delete(tmpFile1);
            File.Delete(tmpFile2);
        }

        [Test]
        public void DownloadCacheDirPersistsEmpty()
        {
            string tmpFile1 = Path.GetTempFileName();
            _ = new JsonConfiguration(tmpFile1)
            {
                DownloadCacheDir = ""
            };

            string tmpFile2 = Path.GetTempFileName();
            File.Copy(tmpFile1, tmpFile2, true);
            JsonConfiguration reg = new JsonConfiguration(tmpFile2);

            Assert.AreEqual(JsonConfiguration.DefaultDownloadCacheDir, reg.DownloadCacheDir);

            File.Delete(tmpFile1);
            File.Delete(tmpFile2);
        }


        [Test,
            TestCase(35, 35),
            TestCase(0, 0),
            TestCase(null, null),
            TestCase(-8, null)]
        public void CacheSizeLimitPersists(long? val, long? expected)
        {
            string tmpFile1 = Path.GetTempFileName();
            _ = new JsonConfiguration(tmpFile1)
            {
                CacheSizeLimit = val
            };

            string tmpFile2 = Path.GetTempFileName();
            File.Copy(tmpFile1, tmpFile2, true);
            JsonConfiguration reg = new JsonConfiguration(tmpFile2);

            Assert.AreEqual(expected, reg.CacheSizeLimit);

            File.Delete(tmpFile1);
            File.Delete(tmpFile2);
        }

        [Test,
            TestCase(35, 35),
            TestCase(1, 1),
            TestCase(0, 0),
            TestCase(-8, 0)]
        public void RefreshRatePersists(int val, int expected)
        {
            string tmpFile1 = Path.GetTempFileName();
            _ = new JsonConfiguration(tmpFile1)
            {
                RefreshRate = val
            };

            string tmpFile2 = Path.GetTempFileName();
            File.Copy(tmpFile1, tmpFile2, true);
            JsonConfiguration reg = new JsonConfiguration(tmpFile2);

            Assert.AreEqual(expected, reg.RefreshRate);

            File.Delete(tmpFile1);
            File.Delete(tmpFile2);
        }

        [Test]
        public void AuthTokensPersist()
        {
            string tmpFile1 = Path.GetTempFileName();
            var reg = new JsonConfiguration(tmpFile1);

            reg.SetAuthToken("test_host1", "hunter2");
            reg.SetAuthToken("test_host2", "asdf");

            string tmpFile2 = Path.GetTempFileName();
            File.Copy(tmpFile1, tmpFile2, true);
            reg = new JsonConfiguration(tmpFile2);

            CollectionAssert.Contains(reg.GetAuthTokenHosts(), "test_host1");
            CollectionAssert.Contains(reg.GetAuthTokenHosts(), "test_host2");

            Assert.IsTrue(reg.TryGetAuthToken("test_host1", out string token));
            Assert.AreEqual("hunter2", token);
            Assert.IsTrue(reg.TryGetAuthToken("test_host2", out token));
            Assert.AreEqual("asdf", token);

            File.Delete(tmpFile1);
            File.Delete(tmpFile2);
        }

        [Test]
        public void InstancesPersist()
        {
            using (var k1 = new DisposableKSP())
            using (var k2 = new DisposableKSP())
            {
                string tmpFile1 = Path.GetTempFileName();
                var reg = new JsonConfiguration(tmpFile1);

                var sl = new SortedList<string, CKAN.GameInstance>();
                sl.Add("instance_1", k1.KSP);
                sl.Add("instance_2", k2.KSP);
                reg.SetRegistryToInstances(sl);

                string tmpFile2 = Path.GetTempFileName();
                File.Copy(tmpFile1, tmpFile2, true);
                reg = new JsonConfiguration(tmpFile2);

                CollectionAssert.AreEquivalent(new List<Tuple<string, string, string>>()
                {
                    new Tuple<string, string, string>("instance_1", k1.KSP.GameDir(), "KSP"),
                    new Tuple<string, string, string>("instance_2", k2.KSP.GameDir(), "KSP")
                }, reg.GetInstances());

                File.Delete(tmpFile1);
                File.Delete(tmpFile2);
            }
        }
    }
}
