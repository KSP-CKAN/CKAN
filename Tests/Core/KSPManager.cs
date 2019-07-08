using System;
using System.Collections.Generic;
using System.Linq;
using CKAN;
using CKAN.Versioning;
using NUnit.Framework;
using Tests.Core.Win32Registry;
using Tests.Data;

namespace Tests.Core
{
    [TestFixture] public class KSPManagerTests
    {
        private DisposableKSP tidy;
        private const string nameInReg = "testing";
        private FakeWin32Registry win32_reg;
        KSPManager manager;

        [SetUp]
        public void SetUp()
        {
            tidy = new DisposableKSP();
            win32_reg = GetTestWin32Reg(nameInReg);
            manager = new KSPManager(new NullUser(), win32_reg);
        }

        [TearDown]
        public void TearDown()
        {
            manager.Dispose();
            tidy.Dispose();
        }

        [Test]
        public void HasInstance_ReturnsFalseIfNoInstanceByThatName()
        {
            const string anyNameNotInReg = "Games";
            Assert.That(manager.HasInstance(anyNameNotInReg), Is.EqualTo(false));
        }

        [Test]
        public void HasInstance_ReturnsTrueIfInstanceByThatName()
        {
            Assert.That(manager.HasInstance(nameInReg), Is.EqualTo(true));
        }

        [Test]
        public void SetAutoStart_ValidName_SetsAutoStart()
        {
            Assert.That(manager.AutoStartInstance, Is.EqualTo(null));

            manager.SetAutoStart(nameInReg);
            Assert.That(manager.AutoStartInstance, Is.EqualTo(nameInReg));
        }

        [Test]
        public void SetAutoStart_InvalidName_DoesNotChangeAutoStart()
        {
            manager.SetAutoStart(nameInReg);
            Assert.Throws<InvalidKSPInstanceKraken>(() => manager.SetAutoStart("invalid"));
            Assert.That(manager.AutoStartInstance, Is.EqualTo(nameInReg));
        }

        [Test]
        public void RemoveInstance_HasInstanceReturnsFalse()
        {
            manager.RemoveInstance(nameInReg);
            Assert.False(manager.HasInstance(nameInReg));
        }

        [Test]
        public void RenameInstance_HasInstanceOriginalName_ReturnsFalse()
        {
            manager.RenameInstance(nameInReg,"newname");
            Assert.False(manager.HasInstance(nameInReg));
        }

        [Test]
        public void RenameInstance_HasInstanceNewName()
        {
            const string newname = "newname";
            manager.RenameInstance(nameInReg, newname);
            Assert.True(manager.HasInstance(newname));
        }

        [Test]
        public void ClearAutoStart_UpdatesValueInWin32Reg()
        {

            Assert.That(win32_reg.AutoStartInstance, Is.Null.Or.Empty);

        }

        [Test]
        public void GetNextValidInstanceName_ManagerDoesNotHaveResult()
        {
            var name = manager.GetNextValidInstanceName(nameInReg);
            Assert.That(manager.HasInstance(name),Is.False);

        }

        [Test]
        public void AddInstance_ManagerHasInstance()
        {
            using (var tidy2 = new DisposableKSP())
            {
                const string newInstance = "tidy2";
                tidy2.KSP.Name = newInstance;
                Assert.IsFalse(manager.HasInstance(newInstance));
                manager.AddInstance(tidy2.KSP);
                Assert.IsTrue(manager.HasInstance(newInstance));
            }
        }

        // CloneInstance

        [Test]
        public void CloneInstance_BadInstance_ThrowsNotKSPDirKraken()
        {
            string badName = "badInstance";
            string tempdir = TestData.NewTempDir();
            CKAN.KSP badKSP = new CKAN.KSP(TestData.bad_ksp_dirs().First(), "badDir", new NullUser());

            Assert.Throws<NotKSPDirKraken>(() =>
                manager.CloneInstance(badKSP, badName, tempdir));
            Assert.IsFalse(manager.HasInstance(badName));

            // Tidy up
            System.IO.Directory.Delete(tempdir, true);
        }

        [Test]
        public void CloneInstance_ToNotEmptyFolder_ThrowsPathErrorKraken()
        {
            using (var KSP = new DisposableKSP())
            {
                string instanceName = "newInstance";
                string tempdir = TestData.NewTempDir();
                System.IO.File.Create(System.IO.Path.Combine(tempdir, "shouldntbehere.txt")).Close();

                Assert.Throws<PathErrorKraken>(() =>
                    manager.CloneInstance(KSP.KSP, instanceName, tempdir));
                Assert.IsFalse(manager.HasInstance(instanceName));

                // Tidy up.
                System.IO.Directory.Delete(tempdir, true);
            }
        }

        [Test]
        public void CloneInstance_GoodInstance_ManagerHasValidInstance()
        {
            using (var KSP = new DisposableKSP())
            {
                string instanceName = "newInstance";
                string tempdir = TestData.NewTempDir();

                manager.CloneInstance(KSP.KSP, instanceName, tempdir);
                Assert.IsTrue(manager.HasInstance(instanceName));

                // Tidy up.
                System.IO.Directory.Delete(tempdir, true);
            }
        }

        // FakeInstance

        [Test]
        public void FakeInstance_InvalidVersion_ThrowsBadKSPVersionKraken()
        {
            string name = "testname";
            string tempdir = TestData.NewTempDir();
            KspVersion version = KspVersion.Parse("1.1.99");

            Assert.Throws<BadKSPVersionKraken>(() =>
                manager.FakeInstance(name, tempdir, version));
            Assert.IsFalse(manager.HasInstance(name));

            // Tidy up.
            System.IO.Directory.Delete(tempdir, true);
        }

        [Test,
            TestCase("1.4.0"),
            TestCase("1.6.1")]
        public void FakeInstance_DlcsWithWrongBaseVersion_ThrowsWrongKSPVersionKraken(string baseVersion)
        {
            string name = "testname";
            KspVersion mhVersion = KspVersion.Parse("1.1.0");
            KspVersion bgVersion = KspVersion.Parse("1.0.0");
            string tempdir = TestData.NewTempDir();
            KspVersion version = KspVersion.Parse(baseVersion);

            Dictionary<CKAN.DLC.IDlcDetector, KspVersion> dlcs = new Dictionary<CKAN.DLC.IDlcDetector, KspVersion>() {
                    { new CKAN.DLC.MakingHistoryDlcDetector(), mhVersion },
                    { new CKAN.DLC.BreakingGroundDlcDetector(), bgVersion }
                };

            Assert.Throws<WrongKSPVersionKraken>(() =>
                manager.FakeInstance(name, tempdir, version, dlcs));
            Assert.IsFalse(manager.HasInstance(name));

            // Tidy up.
            System.IO.Directory.Delete(tempdir, true);
        }

        [Test]
        public void FakeInstance_InNotEmptyFolder_ThrowsBadInstallLocationKraken()
        {
            string name = "testname";
            string tempdir = TestData.NewTempDir();
            KspVersion version = KspVersion.Parse("1.5.1");
            System.IO.File.Create(System.IO.Path.Combine(tempdir, "shouldntbehere.txt")).Close();

            Assert.Throws<BadInstallLocationKraken>(() =>
                manager.FakeInstance(name, tempdir, version));
            Assert.IsFalse(manager.HasInstance(name));

            // Tidy up.
            System.IO.Directory.Delete(tempdir, true);
        }

        [Test]
        public void FakeInstance_ValidArgumentsWithDLCs_ManagerHasValidInstance()
        {
            string name = "testname";
            KspVersion mhVersion = KspVersion.Parse("1.1.0");
            KspVersion bgVersion = KspVersion.Parse("1.0.0");
            string tempdir = TestData.NewTempDir();
            KspVersion version = KspVersion.Parse("1.7.1");

            Dictionary<CKAN.DLC.IDlcDetector, KspVersion> dlcs = new Dictionary<CKAN.DLC.IDlcDetector, KspVersion>() {
                    { new CKAN.DLC.MakingHistoryDlcDetector(), mhVersion },
                    { new CKAN.DLC.BreakingGroundDlcDetector(), bgVersion }
                };

            manager.FakeInstance(name, tempdir, version, dlcs);
            CKAN.KSP newKSP = new CKAN.KSP(tempdir, name, new NullUser());
            CKAN.DLC.MakingHistoryDlcDetector mhDetector = new CKAN.DLC.MakingHistoryDlcDetector();
            CKAN.DLC.BreakingGroundDlcDetector bgDetector = new CKAN.DLC.BreakingGroundDlcDetector();

            Assert.IsTrue(manager.HasInstance(name));
            Assert.IsTrue(mhDetector.IsInstalled(newKSP, out string _, out UnmanagedModuleVersion detectedMhVersion));
            Assert.IsTrue(bgDetector.IsInstalled(newKSP, out string _, out UnmanagedModuleVersion detectedBgVersion));
            Assert.IsTrue(detectedMhVersion == new UnmanagedModuleVersion(mhVersion.ToString()));
            Assert.IsTrue(detectedBgVersion == new UnmanagedModuleVersion(bgVersion.ToString()));

            // Tidy up.
            CKAN.RegistryManager.Instance(newKSP).ReleaseLock();
            System.IO.Directory.Delete(tempdir, true);
        }

        // GetPreferredInstance

        [Test]
        public void GetPreferredInstance_WithAutoStart_ReturnsAutoStart()
        {
            Assert.That(manager.GetPreferredInstance(),Is.EqualTo(tidy.KSP));
        }

        [Test]
        public void GetPreferredInstance_WithEmptyAutoStartAndMultipleInstances_ReturnsNull()
        {
            using (var tidy2 = new DisposableKSP())
            {
                win32_reg.Instances.Add(new Tuple<string, string>("tidy2",tidy2.KSP.GameDir()));
                manager.LoadInstancesFromRegistry();
                manager.ClearAutoStart();
                Assert.That(manager.GetPreferredInstance(), Is.Null);
            }

        }

        [Test]
        public void GetPreferredInstance_OnlyOneAvailable_ReturnsAvailable()
        {
            manager.ClearAutoStart();
            Assert.That(manager.GetPreferredInstance(), Is.EqualTo(tidy.KSP));
        }

        [Test]
        public void SetCurrentInstance_NameNotInRepo_Throws()
        {
            Assert.Throws<InvalidKSPInstanceKraken>(() => manager.SetCurrentInstance("invalid"));
        }

        [Test] //37a33
        public void Ctor_InvalidAutoStart_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new KSPManager(new NullUser(),new FakeWin32Registry(tidy.KSP, "invalid")
                ));
        }


        //TODO Test FindAndRegisterDefaultInstance

        private FakeWin32Registry GetTestWin32Reg(string name)
        {
            return new FakeWin32Registry(
                new List<Tuple<string, string>>
                {
                    new Tuple<string, string>(name, tidy.KSP.GameDir())
                },
                null
            );
        }
    }
}
