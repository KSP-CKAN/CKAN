using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using CKAN;
using CKAN.Configuration;
using CKAN.DLC;
using CKAN.IO;
using CKAN.Versioning;
using CKAN.Games.KerbalSpaceProgram;
using CKAN.Games.KerbalSpaceProgram.DLC;

using Tests.Core.Configuration;
using Tests.Data;

namespace Tests.Core
{
    [TestFixture] public class GameInstanceManagerTests
    {
        private const string nameInReg = "testing";
        private DisposableKSP?       tidy;
        private FakeConfiguration?   cfg;
        private GameInstanceManager? manager;

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
            manager?.Dispose();
            tidy?.Dispose();
            cfg?.Dispose();
        }

        [Test]
        public void HasInstance_ReturnsFalseIfNoInstanceByThatName()
        {
            const string anyNameNotInReg = "Games";
            Assert.That(manager?.HasInstance(anyNameNotInReg), Is.EqualTo(false));
        }

        [Test]
        public void HasInstance_ReturnsTrueIfInstanceByThatName()
        {
            Assert.That(manager?.HasInstance(nameInReg), Is.EqualTo(true));
        }

        [Test]
        public void SetAutoStart_ValidName_SetsAutoStart()
        {
            Assert.That(manager?.AutoStartInstance, Is.EqualTo(null));

            manager?.SetAutoStart(nameInReg);
            Assert.That(manager?.AutoStartInstance, Is.EqualTo(nameInReg));
        }

        [Test]
        public void SetAutoStart_InvalidName_DoesNotChangeAutoStart()
        {
            manager?.SetAutoStart(nameInReg);
            Assert.Throws<InvalidGameInstanceKraken>(() => manager?.SetAutoStart("invalid"));
            Assert.That(manager?.AutoStartInstance, Is.EqualTo(nameInReg));
        }

        [Test]
        public void RemoveInstance_HasInstance_ReturnsFalse()
        {
            manager?.RemoveInstance(nameInReg);
            Assert.False(manager?.HasInstance(nameInReg));
        }

        [Test]
        public void RenameInstance_NewName_Works()
        {
            const string newname = "newname";
            manager!.RenameInstance(nameInReg, newname);
            Assert.False(manager.HasInstance(nameInReg));
            Assert.True(manager.HasInstance(newname));
        }

        [Test]
        public void RenameInstance_SameName_Throws()
        {
            var fakeName = "fake";
            using (var tidy2 = new DisposableKSP(fakeName, tidy!.KSP.Game))
            {
                manager!.AddInstance(tidy2.KSP);
                Assert.Throws<InstanceNameTakenKraken>(() =>
                {
                    manager!.RenameInstance(nameInReg, fakeName);
                });
            }
        }

        [Test]
        public void ClearAutoStart_UpdatesValueInWin32Reg()
        {
            Assert.That(cfg?.AutoStartInstance, Is.Null.Or.Empty);
        }

        [Test]
        public void GetNextValidInstanceName_ManagerDoesNotHaveResult()
        {
            var name = manager?.GetNextValidInstanceName(nameInReg)!;
            Assert.That(manager?.HasInstance(name), Is.False);
        }

        [Test]
        public void AddInstance_ManagerHasInstance()
        {
            using (var tidy2 = new DisposableKSP())
            {
                const string newInstance = "tidy2";
                tidy2.KSP.Name = newInstance;
                Assert.IsFalse(manager?.HasInstance(newInstance));
                manager?.AddInstance(tidy2.KSP);
                Assert.IsTrue(manager?.HasInstance(newInstance));
            }
        }

        // CloneInstance

        [Test]
        public void CloneInstance_BadInstance_ThrowsNotKSPDirKraken()
        {
            string badName = "badInstance";
            string tempdir = TestData.NewTempDir();
            GameInstance badKSP = new GameInstance(new KerbalSpaceProgram(), TestData.bad_ksp_dirs().First(), "badDir", new NullUser());

            Assert.Throws<NotGameDirKraken>(() =>
                manager?.CloneInstance(badKSP, badName, tempdir));
            Assert.IsFalse(manager?.HasInstance(badName));

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
                    manager?.CloneInstance(KSP.KSP, instanceName, tempdir));
                Assert.IsFalse(manager?.HasInstance(instanceName));

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

                manager?.CloneInstance(KSP.KSP, instanceName, tempdir);
                Assert.IsTrue(manager?.HasInstance(instanceName));

                // Tidy up.
                Directory.Delete(tempdir, true);
            }
        }

        // FakeInstance

        [Test]
        public void FakeInstance_InvalidVersion_ThrowsBadGameVersionKraken()
        {
            // Arrange
            var name = "testname";
            var version = GameVersion.Parse("1.1.99");

            using (var tempdir = new TemporaryDirectory())
            {
                // Act / Assert
                Assert.Throws<BadGameVersionKraken>(() =>
                    manager?.FakeInstance(new KerbalSpaceProgram(), name,
                                          tempdir.Directory.FullName, version));
                Assert.IsFalse(manager?.HasInstance(name));
            }
        }

        [TestCase("1.4.0"),
         TestCase("1.6.1")]
        public void FakeInstance_DlcsWithWrongBaseVersion_ThrowsWrongGameVersionKraken(string baseVersion)
        {
            // Arrange
            var name = "testname";
            var mhVersion = GameVersion.Parse("1.1.0");
            var bgVersion = GameVersion.Parse("1.0.0");
            var version = GameVersion.Parse(baseVersion);
            var dlcs = new Dictionary<IDlcDetector, GameVersion>()
            {
                { new MakingHistoryDlcDetector(),  mhVersion },
                { new BreakingGroundDlcDetector(), bgVersion },
            };
            using (var tempdir = new TemporaryDirectory())
            {
                // Act / Assert
                Assert.Throws<WrongGameVersionKraken>(() =>
                    manager?.FakeInstance(new KerbalSpaceProgram(), name,
                                          tempdir.Directory.FullName, version, dlcs));
                Assert.IsFalse(manager?.HasInstance(name));
            }
        }

        [Test]
        public void FakeInstance_InNotEmptyFolder_ThrowsBadInstallLocationKraken()
        {
            // Arrange
            var name = "testname";
            var version = GameVersion.Parse("1.5.1");

            using (var tempdir = new TemporaryDirectory())
            {
                File.Create(Path.Combine(tempdir.Directory.FullName,
                                         "shouldntbehere.txt"))
                    .Close();

                // Act / Assert
                Assert.Throws<BadInstallLocationKraken>(() =>
                    manager?.FakeInstance(new KerbalSpaceProgram(), name,
                                          tempdir.Directory.FullName, version));
                Assert.IsFalse(manager?.HasInstance(name));
            }
        }

        [Test]
        public void FakeInstance_ValidArgumentsWithDLCs_ManagerHasValidInstance()
        {
            // Arrange
            string name = "testname";
            var mhVersion = GameVersion.Parse("1.1.0");
            var unmanagedMhVersion = new UnmanagedModuleVersion(mhVersion.ToString());
            var bgVersion = GameVersion.Parse("1.0.0");
            var unmanagedBgVersion = new UnmanagedModuleVersion(bgVersion.ToString());
            var version = GameVersion.Parse("1.7.1.2539");
            var mhDetector = new MakingHistoryDlcDetector();
            var bgDetector = new BreakingGroundDlcDetector();

            var dlcs = new Dictionary<IDlcDetector, GameVersion>()
            {
                { mhDetector, mhVersion },
                { bgDetector, bgVersion },
            };

            using (var tempdir = new TemporaryDirectory())
            {
                // Act
                var newKSP = manager!.FakeInstance(new KerbalSpaceProgram(), name,
                                                   tempdir.Directory.FullName, version, dlcs);

                Assert.IsTrue(manager?.HasInstance(name));
                Assert.IsTrue(mhDetector.IsInstalled(newKSP, out string? _, out UnmanagedModuleVersion? detectedMhVersion));
                Assert.AreEqual(unmanagedMhVersion, detectedMhVersion);
                Assert.IsTrue(bgDetector.IsInstalled(newKSP, out string? _, out UnmanagedModuleVersion? detectedBgVersion));
                Assert.AreEqual(unmanagedBgVersion, detectedBgVersion);
                FileAssert.Exists(Path.Combine(tempdir.Directory.FullName, "buildID.txt"));
                FileAssert.Exists(Path.Combine(tempdir.Directory.FullName, "buildID64.txt"));
            }
        }

        // GetPreferredInstance

        [Test]
        public void GetPreferredInstance_WithAutoStart_ReturnsAutoStart()
        {
            Assert.That(manager?.GetPreferredInstance(),Is.EqualTo(tidy?.KSP));
        }

        [Test]
        public void GetPreferredInstance_WithEmptyAutoStartAndMultipleInstances_ReturnsNull()
        {
            using (var tidy2 = new DisposableKSP())
            {
                cfg!.Instances.Add(new Tuple<string, string, string>("tidy2", tidy2.KSP.GameDir, "KSP"));
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
            manager?.ClearAutoStart();
            Assert.That(manager?.GetPreferredInstance(), Is.EqualTo(tidy?.KSP));
        }

        [Test]
        public void SetCurrentInstance_NameNotInRepo_Throws()
        {
            Assert.Throws<InvalidGameInstanceKraken>(() => manager?.SetCurrentInstance("invalid"));
        }

        [Test] //37a33
        public void Ctor_InvalidAutoStart_DoesNotThrow()
        {
            using (var config = new FakeConfiguration(tidy!.KSP, "invalid"))
            {
                Assert.DoesNotThrow(() =>
                {
                    using (var mgr = new GameInstanceManager(new NullUser(), config))
                    {
                    }
                });
            }
        }

        [Test]
        public void SetCurrentInstanceByPath_WithInstance_Works()
        {
            // Act
            manager!.SetCurrentInstanceByPath(tidy!.KSP.GameDir);

            // Assert
            Assert.AreEqual(tidy!.KSP, manager.CurrentInstance);
        }

        [Test]
        public void InstanceAt_WithInstance_Found()
        {
            // Arrange
            var instFromMgr = manager!.InstanceAt(tidy!.KSP.GameDir);

            // Assert
            Assert.IsNotNull(instFromMgr);
        }

        [Test]
        public void IsGameInstanceDir_WithInstance_True()
        {
            // Arrange
            var di = new DirectoryInfo(tidy!.KSP.GameDir);

            // Act / Assert
            Assert.IsTrue(GameInstanceManager.IsGameInstanceDir(di));
        }


        [Test]
        public void FindAndRegisterDefaultInstances_WithMockedSteam_FindsInstances()
        {
            // Arrange
            using (var config = new FakeConfiguration(new List<Tuple<string, string, string>>(), null, null))
            using (var dir = new TemporarySteamDirectory(
                                 new (string acfFileName, int appId, string appName)[]
                                 {
                                     (acfFileName: "appmanifest_220200.acf",
                                      appId:       220200,
                                      appName:     "Kerbal Space Program"),
                                     (acfFileName: "appmanifest_954850.acf",
                                      appId:       954850,
                                      appName:     "Kerbal Space Program 2"),
                                 },
                                 new (string name, string absPath)[]
                                 {
                                     (name:    "Test Instance",
                                      absPath: Path.GetFullPath(TestData.good_ksp_dir())),
                                 }))
            {
                var steamLib = new SteamLibrary(dir.Directory.FullName);
                using (var mgr = new GameInstanceManager(new NullUser(), config, steamLib))
                {
                    foreach (var g in steamLib.Games.OfType<SteamGame>())
                    {
                        Utilities.CopyDirectory(TestData.good_ksp_dir(),
                                                g.GameDir!.FullName,
                                                Array.Empty<string>(),
                                                Array.Empty<string>());
                    }

                    // Act
                    mgr.FindAndRegisterDefaultInstances();

                    // Assert
                    CollectionAssert.AreEquivalent(new string[]
                                                   {
                                                       "Kerbal Space Program",
                                                       "Kerbal Space Program 2",
                                                       "Test Instance",
                                                   },
                                                   mgr.Instances.Keys);
                }
            }
        }

        [Test]
        public void Constructor_WithCachePathDefined_Creates()
        {
            // Arrange
            using (var dir = new TemporaryDirectory())
            {
                var cachePath = Path.Combine(dir.Directory.FullName, "cachetest");
                DirectoryAssert.DoesNotExist(cachePath);

                var configPath = Path.Combine(dir.Directory.FullName, "config.json");
                File.WriteAllText(configPath, "{}");
                var config = new JsonConfiguration(configPath)
                {
                    DownloadCacheDir = cachePath,
                };

                // Act
                using (var mgr = new GameInstanceManager(new NullUser(), config))
                {
                    // Assert
                    DirectoryAssert.Exists(cachePath, $"{cachePath} should exist");
                }
            }
        }

        private FakeConfiguration GetTestCfg(string name)
            => new FakeConfiguration(
                   new List<Tuple<string, string, string>>
                   {
                       new Tuple<string, string, string>(name,
                                                         tidy!.KSP.GameDir,
                                                         tidy!.KSP.Game.ShortName)
                   },
                   null, null);
    }
}
