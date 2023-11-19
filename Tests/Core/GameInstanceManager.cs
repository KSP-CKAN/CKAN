using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using CKAN;
using CKAN.Versioning;
using CKAN.Games.KerbalSpaceProgram;
using CKAN.Games.KerbalSpaceProgram.DLC;

using Tests.Core.Configuration;
using Tests.Data;

namespace Tests.Core
{
    [TestFixture] public class GameInstanceManagerTests
    {
        private DisposableKSP tidy;
        private const string nameInReg = "testing";
        private FakeConfiguration cfg;
        private GameInstanceManager manager;

        [SetUp]
        public void SetUp()
        {
            tidy = new DisposableKSP();
            cfg = GetTestCfg(nameInReg);
            manager = new GameInstanceManager(new NullUser(), cfg);
        }

        [TearDown]
        public void TearDown()
        {
            manager.Dispose();
            tidy.Dispose();
            cfg.Dispose();
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

            Assert.That(cfg.AutoStartInstance, Is.Null.Or.Empty);

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
            GameInstance badKSP = new GameInstance(new KerbalSpaceProgram(), TestData.bad_ksp_dirs().First(), "badDir", new NullUser());

            Assert.Throws<NotKSPDirKraken>(() =>
                manager.CloneInstance(badKSP, badName, tempdir));
            Assert.IsFalse(manager.HasInstance(badName));

            // Tidy up
            Directory.Delete(tempdir, true);
        }

        [Test]
        public void CloneInstance_ToNotEmptyFolder_ThrowsPathErrorKraken()
        {
            using (var KSP = new DisposableKSP())
            {
                string instanceName = "newInstance";
                string tempdir = TestData.NewTempDir();
                File.Create(Path.Combine(tempdir, "shouldntbehere.txt")).Close();

                Assert.Throws<PathErrorKraken>(() =>
                    manager.CloneInstance(KSP.KSP, instanceName, tempdir));
                Assert.IsFalse(manager.HasInstance(instanceName));

                // Tidy up.
                Directory.Delete(tempdir, true);
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
                Directory.Delete(tempdir, true);
            }
        }

        // FakeInstance

        [Test]
        public void FakeInstance_InvalidVersion_ThrowsBadGameVersionKraken()
        {
            string name = "testname";
            string tempdir = TestData.NewTempDir();
            GameVersion version = GameVersion.Parse("1.1.99");

            Assert.Throws<BadGameVersionKraken>(() =>
                manager.FakeInstance(new KerbalSpaceProgram(), name, tempdir, version));
            Assert.IsFalse(manager.HasInstance(name));

            // Tidy up.
            Directory.Delete(tempdir, true);
        }

        [Test,
            TestCase("1.4.0"),
            TestCase("1.6.1")]
        public void FakeInstance_DlcsWithWrongBaseVersion_ThrowsWrongGameVersionKraken(string baseVersion)
        {
            string name = "testname";
            GameVersion mhVersion = GameVersion.Parse("1.1.0");
            GameVersion bgVersion = GameVersion.Parse("1.0.0");
            string tempdir = TestData.NewTempDir();
            GameVersion version = GameVersion.Parse(baseVersion);

            Dictionary<CKAN.DLC.IDlcDetector, GameVersion> dlcs = new Dictionary<CKAN.DLC.IDlcDetector, GameVersion>() {
                    { new MakingHistoryDlcDetector(), mhVersion },
                    { new BreakingGroundDlcDetector(), bgVersion }
                };

            Assert.Throws<WrongGameVersionKraken>(() =>
                manager.FakeInstance(new KerbalSpaceProgram(), name, tempdir, version, dlcs));
            Assert.IsFalse(manager.HasInstance(name));

            // Tidy up.
            Directory.Delete(tempdir, true);
        }

        [Test]
        public void FakeInstance_InNotEmptyFolder_ThrowsBadInstallLocationKraken()
        {
            string name = "testname";
            string tempdir = TestData.NewTempDir();
            GameVersion version = GameVersion.Parse("1.5.1");
            File.Create(Path.Combine(tempdir, "shouldntbehere.txt")).Close();

            Assert.Throws<BadInstallLocationKraken>(() =>
                manager.FakeInstance(new KerbalSpaceProgram(), name, tempdir, version));
            Assert.IsFalse(manager.HasInstance(name));

            // Tidy up.
            Directory.Delete(tempdir, true);
        }

        [Test]
        public void FakeInstance_ValidArgumentsWithDLCs_ManagerHasValidInstance()
        {
            string name = "testname";
            GameVersion mhVersion = GameVersion.Parse("1.1.0");
            GameVersion bgVersion = GameVersion.Parse("1.0.0");
            string tempdir = TestData.NewTempDir();
            GameVersion version = GameVersion.Parse("1.7.1");

            Dictionary<CKAN.DLC.IDlcDetector, GameVersion> dlcs = new Dictionary<CKAN.DLC.IDlcDetector, GameVersion>() {
                    { new MakingHistoryDlcDetector(), mhVersion },
                    { new BreakingGroundDlcDetector(), bgVersion }
                };

            manager.FakeInstance(new KerbalSpaceProgram(), name, tempdir, version, dlcs);
            GameInstance newKSP = new GameInstance(new KerbalSpaceProgram(), tempdir, name, new NullUser());
            MakingHistoryDlcDetector mhDetector = new MakingHistoryDlcDetector();
            BreakingGroundDlcDetector bgDetector = new BreakingGroundDlcDetector();

            Assert.IsTrue(manager.HasInstance(name));
            Assert.IsTrue(mhDetector.IsInstalled(newKSP, out string _, out UnmanagedModuleVersion detectedMhVersion));
            Assert.IsTrue(bgDetector.IsInstalled(newKSP, out string _, out UnmanagedModuleVersion detectedBgVersion));
            Assert.IsTrue(detectedMhVersion == new UnmanagedModuleVersion(mhVersion.ToString()));
            Assert.IsTrue(detectedBgVersion == new UnmanagedModuleVersion(bgVersion.ToString()));

            // Tidy up.
            RegistryManager.DisposeInstance(newKSP);
            Directory.Delete(tempdir, true);
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
                cfg.Instances.Add(new Tuple<string, string, string>("tidy2", tidy2.KSP.GameDir(), "KSP"));
                // Make a new manager with the updated config
                var multiMgr = new GameInstanceManager(new NullUser(), cfg);
                multiMgr.ClearAutoStart();
                Assert.That(multiMgr.GetPreferredInstance(), Is.Null);
                multiMgr.Dispose();
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
            using (var config = new FakeConfiguration(tidy.KSP, "invalid"))
            {
                Assert.DoesNotThrow(() => new GameInstanceManager(new NullUser(), config));
            }
        }


        //TODO Test FindAndRegisterDefaultInstance

        private FakeConfiguration GetTestCfg(string name)
        {
            return new FakeConfiguration(
                new List<Tuple<string, string, string>>
                {
                    new Tuple<string, string, string>(name, tidy.KSP.GameDir(), "KSP")
                },
                null
            );
        }
    }
}
