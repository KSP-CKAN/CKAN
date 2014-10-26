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
            ksp_dir = NewTempDir();
            CopyDirectory(Tests.TestData.good_ksp_dir(), ksp_dir);
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

        // Ugh, this is awful.
        private void CopyDirectory(string src, string dst)
        {
            // Create directory structure
            foreach (string path in Directory.GetDirectories(src, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(path.Replace(src, dst));
            }

            // Copy files.
            foreach (string file in Directory.GetFiles(src, "*", SearchOption.AllDirectories))
            {
                File.Copy(file, file.Replace(src, dst));
            }
        }

        // Where's my mkdtemp? Instead we'll make a random file, delete it, and
        // fill its place with a directory.
        // Taken from https://stackoverflow.com/a/20445952
        private string NewTempDir()
        {
            string tempFolder = Path.GetTempFileName();
            File.Delete(tempFolder);
            Directory.CreateDirectory(tempFolder);

            return tempFolder;
        }
    }
}

