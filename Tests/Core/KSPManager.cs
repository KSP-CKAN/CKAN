using System;
using System.Collections.Generic;
using System.Linq;
using CKAN;
using NUnit.Framework;
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
        public void SetAutoStart_VaildName_SetsAutoStart()
        {
            Assert.That(manager.AutoStartInstance, Is.EqualTo(string.Empty));

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
        public void CloneInstance_ToNotEmptyFolder_ThrowsIOException()
        {
            using (var KSP = new DisposableKSP())
            {
                string instanceName = "newInstance";
                string tempdir = TestData.NewTempDir();
                System.IO.File.Create(System.IO.Path.Combine(tempdir, "shouldntbehere.txt"));

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
        public void FakeInstance_InvalidVersion_ThrowsArgumentOutOfRangeException()
        {
            string name = "testname";
            string tempdir = TestData.NewTempDir();
            CKAN.Versioning.KspVersion version = CKAN.Versioning.KspVersion.Parse("1.1.99");

            Assert.Throws<IncorrectKSPVersionKraken>(() =>
                manager.FakeInstance(name, tempdir, version));
            Assert.IsFalse(manager.HasInstance(name));

            // Tidy up.
            System.IO.Directory.Delete(tempdir, true);
        }

        [Test]
        public void FakeInstance_InNotEmptyFolder_ThrowsBadInstallLocationKraken()
        {
            string name = "testname";
            string tempdir = TestData.NewTempDir();
            CKAN.Versioning.KspVersion version = CKAN.Versioning.KspVersion.Parse("1.5.1");
            System.IO.File.Create(System.IO.Path.Combine(tempdir, "shouldntbehere.txt"));

            Assert.Throws<BadInstallLocationKraken>(() =>
                manager.FakeInstance(name, tempdir, version));
            Assert.IsFalse(manager.HasInstance(name));

            // Tidy up.
            System.IO.Directory.Delete(tempdir, true);
        }

        [Test]
        public void FakeInstance_ValidArgumentsWithDLC_ManagerHasValidInstance()
        {
            string name = "testname";
            string dlcVersion = "1.1.0";
            string tempdir = TestData.NewTempDir();
            CKAN.Versioning.KspVersion version = CKAN.Versioning.KspVersion.Parse("1.6.0");

            manager.FakeInstance(name, tempdir, version, dlcVersion);
            CKAN.KSP newKSP = new CKAN.KSP(tempdir, name, new NullUser());
            CKAN.DLC.MakingHistoryDlcDetector detector = new CKAN.DLC.MakingHistoryDlcDetector();

            Assert.IsTrue(manager.HasInstance(name));
            Assert.IsTrue(detector.IsInstalled(newKSP, out string _dump, out CKAN.Versioning.UnmanagedModuleVersion dlcVersionObject));
            Assert.IsTrue(dlcVersionObject.ToString().Contains(dlcVersion));

            // Tidy up.
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
