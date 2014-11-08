using NUnit.Framework;
using System;
using System.IO;
using CKAN;

namespace CKANTests
{
    [TestFixture()]
    public class KSP
    {
        private CKAN.KSP ksp;
        private string ksp_dir;

        [SetUp()]
        public void Setup()
        {
            ksp_dir = Tests.TestData.NewTempDir();
            Tests.TestData.CopyDirectory(Tests.TestData.good_ksp_dir(), ksp_dir);
            ksp = new CKAN.KSP(ksp_dir);
        }

        [TearDown()]
        public void TearDown()
        {
            Directory.Delete(ksp_dir, true);
        }

        [Test()]
        public void IsGameDir()
        {
            // Our test data directory should be good.
            Assert.IsTrue(CKAN.KSP.IsKspDir(Tests.TestData.good_ksp_dir()));

            // As should our copied folder.
            Assert.IsTrue(CKAN.KSP.IsKspDir(ksp_dir));

            // And the one from our KSP instance.
            Assert.IsTrue(CKAN.KSP.IsKspDir(ksp.GameDir()));

            // All these ones should be bad.
            foreach (string dir in Tests.TestData.bad_ksp_dirs())
            {
                Assert.IsFalse(CKAN.KSP.IsKspDir(dir));
            }
        }

        [Test()]
        public void Training()
        {
            Assert.AreEqual(Path.Combine(ksp_dir, "saves", "training"), ksp.Tutorial());
        }
    }
}

