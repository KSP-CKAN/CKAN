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

        [Test]
        public void ScanDlls()
        {
            string path = Path.Combine(ksp.GameData(), "Example.dll");

            Assert.IsFalse(ksp.Registry.IsInstalled("Example"), "Example should start uninstalled");

            File.WriteAllText(path, "Not really a DLL, are we?");

            ksp.ScanGameData();

            Assert.IsTrue(ksp.Registry.IsInstalled("Example"), "Example installed");

            CKAN.Version version = ksp.Registry.InstalledVersion("Example");
            Assert.IsInstanceOf<CKAN.DllVersion>(version, "DLL detected as a DLL, not full mod");

            // Now let's do the same with different case.

            string path2 = Path.Combine(ksp.GameData(), "NewMod.DLL");

            Assert.IsFalse(ksp.Registry.IsInstalled("NewMod"));
            File.WriteAllText(path2, "This text is irrelevant. You will be assimilated");

            ksp.ScanGameData();

            Assert.IsTrue(ksp.Registry.IsInstalled("NewMod"));
        }

        [Test]
        public void ToAbsolute()
        {
            Assert.AreEqual(
                CKAN.KSPPathUtils.NormalizePath(
                    Path.Combine(ksp_dir, "GameData/HydrazinePrincess")
                ),
                ksp.ToAbsolute("GameData/HydrazinePrincess")
            );
        }

        [Test]
        public void ToRelative()
        {
            string absolute = Path.Combine(ksp_dir, "GameData/HydrazinePrincess");

            Assert.AreEqual(
                "GameData/HydrazinePrincess",
                ksp.ToRelative(absolute)
            );
        }

    }
}