using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;

using CKAN;
using Tests.Core.Configuration;
using Tests.Data;

namespace Tests.Core
{
    [TestFixture]
    public class ModuleInstaller
    {
        private string flag_path;
        private string dogezip;
        private CkanModule dogemod;

        private string mm_zip;
        private CkanModule mm_mod;

        private string mission_zip;
        private CkanModule mission_mod;

        private IUser nullUser;

        private DisposableKSP ksp = new DisposableKSP();

        [SetUp]
        public void Setup()
        {
            // By setting these for every test, we can make sure our tests can change
            // them any way they like without harming other tests.

            flag_path = "DogeCoinFlag-1.01/GameData/DogeCoinFlag/Flags/dogecoin.png";
            dogezip = TestData.DogeCoinFlagZip();
            dogemod = TestData.DogeCoinFlag_101_module();

            mm_zip = TestData.ModuleManagerZip();
            mm_mod = TestData.ModuleManagerModule();

            mission_zip = TestData.MissionZip();
            mission_mod = TestData.MissionModule();

            nullUser = new NullUser();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            ksp.Dispose();
        }

        [Test]
        public void Sanity()
        {
            // Test our assumptions are right with the data we're using.

            // Our _find mod should have a find section, but not a file section.
            CkanModule mod = TestData.DogeCoinFlag_101_module_find();
            Assert.IsNull(mod.install[0].file);
            Assert.IsNotNull(mod.install[0].find);
        }

        [Test]
        public void GenerateDefaultInstall()
        {
            string filename = TestData.DogeCoinFlagZip();
            using (var zipfile = new ZipFile(filename))
            {
                ModuleInstallDescriptor stanza = ModuleInstallDescriptor.DefaultInstallStanza("DogeCoinFlag");

                Assert.AreEqual("GameData", stanza.install_to);
                Assert.AreEqual("DogeCoinFlag", stanza.find);

                // Same again, but screwing up the case (we see this *all the time*)
                ModuleInstallDescriptor stanza2 = ModuleInstallDescriptor.DefaultInstallStanza("DogecoinFlag");

                Assert.AreEqual("GameData", stanza2.install_to);
                Assert.AreEqual("DogecoinFlag", stanza2.find);
            }
        }

        // Test data: different ways to install the same file.
        public static CkanModule[] doge_mods =
            {
                TestData.DogeCoinFlag_101_module(),
                TestData.DogeCoinFlag_101_module_find()
            };

        [Test]
        [TestCaseSource("doge_mods")]
        public void FindInstallableFiles(CkanModule mod)
        {
            List<InstallableFile> contents = CKAN.ModuleInstaller.FindInstallableFiles(mod, dogezip, ksp.KSP);
            List<string> filenames = new List<string>();

            Assert.IsNotNull(contents);

            // Make sure it's actually got files!
            Assert.IsTrue(contents.Count > 0);

            foreach (var file in contents)
            {
                // Make sure the source paths are not null, that would be silly!
                Assert.IsNotNull(file.source);

                // And make sure our makeDir info is filled in.
                Assert.IsNotNull(file.makedir);

                filenames.Add(file.source.Name);
            }

            // Ensure we've got an expected file
            Assert.Contains("DogeCoinFlag-1.01/GameData/DogeCoinFlag/Flags/dogecoin.png", filenames);
        }

        [Test]
        [TestCaseSource("doge_mods")]
        public void FindInstallableFilesWithKSP(CkanModule mod)
        {
            using (var tidy = new DisposableKSP())
            {
                List<InstallableFile> contents = CKAN.ModuleInstaller.FindInstallableFiles(mod, dogezip, tidy.KSP);

                // See if we can find an expected destination path in the right place.
                string file = contents
                    .Select(x => x.destination).FirstOrDefault(
                        x => Regex.IsMatch(x, "GameData/DogeCoinFlag/Flags/dogecoin\\.png$"));

                Assert.IsNotNull(file);
            }
        }

        // GH #315, all of these should result in the same output.
        // Even though they're not necessarily all spec-valid, we should accept them
        // nonetheless.
        public static readonly string[] SuchPaths =
            {
                "GameData/SuchTest",
                "GameData/SuchTest/",
                "GameData\\SuchTest",
                "GameData\\SuchTest\\",
                "GameData\\SuchTest/",
                "GameData/SuchTest\\"
            };

        [Test]
        [TestCaseSource("SuchPaths")]
        public void FindInstallableFilesWithBonusPath(string path)
        {
            dogemod.install[0].install_to = path;
            using (var tidy = new DisposableKSP())
            {
                IEnumerable<InstallableFile> contents = CKAN.ModuleInstaller.FindInstallableFiles(
                                                            dogemod, dogezip, tidy.KSP
                                                        );

                string file = contents
                    .Select(x => x.destination).FirstOrDefault(
                        x => Regex.IsMatch(x, "GameData/SuchTest/DogeCoinFlag/Flags/dogecoin\\.png$"));

                Assert.IsNotNull(file);
            }
        }

        [Test]
        public void ModuleManagerInstall()
        {
            using (var tidy = new DisposableKSP())
            {
                List<InstallableFile> contents = CKAN.ModuleInstaller.FindInstallableFiles(mm_mod, mm_zip, tidy.KSP);

                string file = contents
                    .Select(x => x.destination).FirstOrDefault(
                    x => Regex.IsMatch(x, @"ModuleManager\.2\.5\.1\.dll$"));

                Assert.IsNotNull(file, "ModuleManager install");
            }
        }

        [Test]
        public void MissionInstall()
        {
            using (var tidy = new DisposableKSP())
            {
                List<InstallableFile> contents = CKAN.ModuleInstaller.FindInstallableFiles(mission_mod, mission_zip, tidy.KSP);

                string failBanner = contents.Select(x => x.destination).FirstOrDefault(
                    x => Regex.IsMatch(x, "Missions/AwesomeMission/Banners/Fail/default\\.png$"));
                string menuBanner = contents.Select(x => x.destination).FirstOrDefault(
                    x => Regex.IsMatch(x, "Missions/AwesomeMission/Banners/Menu/default\\.png$"));
                string successBanner = contents.Select(x => x.destination).FirstOrDefault(
                    x => Regex.IsMatch(x, "Missions/AwesomeMission/Banners/Success/default\\.png$"));
                string metaFile = contents.Select(x => x.destination).FirstOrDefault(
                    x => Regex.IsMatch(x, "Missions/AwesomeMission/persistent\\.loadmeta$"));
                string missionFile = contents.Select(x => x.destination).FirstOrDefault(
                    x => Regex.IsMatch(x, "Missions/AwesomeMission/persistent\\.mission$"));

                Assert.IsNotNull(failBanner, "There is no fail banner in MissionInstall");
                Assert.IsNotNull(menuBanner, "There is no menu banner in MissionInstall");
                Assert.IsNotNull(successBanner, "There is no success banner in MissionInstall");
                Assert.IsNotNull(metaFile, "There is no .loadmeta file in MissionInstall");
                Assert.IsNotNull(missionFile, "There is no .mission file in MissionInstall");
            }
        }

        [Test]
        [TestCaseSource("doge_mods")]
        // Make sure all our filters work.
        public void FindInstallableFilesWithFilter(CkanModule mod)
        {
            string extra_doge = TestData.DogeCoinFlagZipWithExtras();

            List<InstallableFile> contents = CKAN.ModuleInstaller.FindInstallableFiles(mod, extra_doge, ksp.KSP);

            var files = contents.Select(x => x.source.Name);

            Assert.IsTrue(files.Contains("DogeCoinFlag-1.01/GameData/DogeCoinFlag/Flags/dogecoin.png"), "dogecoin.png");
            Assert.IsFalse(files.Contains("DogeCoinFlag-1.01/GameData/DogeCoinFlag/README.md"), "Filtered README 1");
            Assert.IsFalse(files.Contains("DogeCoinFlag-1.01/GameData/DogeCoinFlag/Flags/README.md"), "Filtered README 2");
            Assert.IsFalse(files.Contains("DogeCoinFlag-1.01/GameData/DogeCoinFlag/notes.txt.bak"), "Filtered .bak file");
        }

        [Test]
        // Test includes_only and includes_only_regexp
        public void FindInstallableFilesWithInclude()
        {
            string extra_doge = TestData.DogeCoinFlagZipWithExtras();
            CkanModule mod = TestData.DogeCoinFlag_101_module_include();

            List<InstallableFile> contents = CKAN.ModuleInstaller.FindInstallableFiles(mod, extra_doge, ksp.KSP);

            var files = contents.Select(x => x.source.Name);

            Assert.IsTrue(files.Contains("DogeCoinFlag-1.01/GameData/DogeCoinFlag/Flags/dogecoin.png"), "dogecoin.png");
            Assert.IsFalse(files.Contains("DogeCoinFlag-1.01/GameData/DogeCoinFlag/README.md"), "Filtered README 1");
            Assert.IsFalse(files.Contains("DogeCoinFlag-1.01/GameData/DogeCoinFlag/Flags/README.md"), "Filtered README 2");
            Assert.IsTrue(files.Contains("DogeCoinFlag-1.01/GameData/DogeCoinFlag/notes.txt.bak"), ".bak file");
        }

        [Test]
        public void No_Installable_Files()
        {
            // This tests GH #93

            string dogezip = TestData.DogeCoinFlagZip();
            CkanModule bugged_mod = TestData.DogeCoinFlag_101_bugged_module();

            Assert.Throws<BadMetadataKraken>(delegate
                {
                    CKAN.ModuleInstaller.FindInstallableFiles(bugged_mod, dogezip, ksp.KSP);
                });

            try
            {
                CKAN.ModuleInstaller.FindInstallableFiles(bugged_mod, dogezip, ksp.KSP);
            }
            catch (BadMetadataKraken ex)
            {
                // Make sure our module information is attached.
                Assert.IsNotNull(ex.module);
                Assert.AreEqual(bugged_mod.identifier, ex.module.identifier);
            }
        }

#pragma warning disable 0414

        // All of these targets should fail.
        public static readonly string[] BadTargets = {
            "GameDataIsTheBestData", "Shups", "GameData/../../../../etc/pwned",
            "Ships/Foo", "GameRoot/saves", "GameRoot/CKAN", "GameData/..",
            @"GameData\..\..\etc\pwned", @"GameData\.."
        };

#pragma warning restore 0414

        [Test]
        [TestCaseSource("BadTargets")]
        public void FindInstallableFilesWithBadTarget(string location)
        {
            // This install location? It shouldn't be valid.
            dogemod.install[0].install_to = location;

            Assert.Throws<BadInstallLocationKraken>(delegate
            {
                CKAN.ModuleInstaller.FindInstallableFiles(dogemod, dogezip, ksp.KSP);
            });
        }

        [Test]
        // GH #205, make sure we write in *binary*, not text.
        public void BinaryNotText_205()
        {
            // Use CopyZipEntry (via CopyDogeFromZip) and make sure it
            // comes out the right size.
            string tmpfile = CopyDogeFromZip();
            long size = new FileInfo(tmpfile).Length;

            try
            {
                // Compare recorded length against what we expect.
                Assert.AreEqual(52043, size);
            }
            finally
            {
                // Tidy up.
                File.Delete(tmpfile);
            }
        }

        [Test]
        // Make sure when we roll-back a transaction, files written with CopyZipEntry go
        // back to their pre-transaction state.
        public void FileSysRollBack()
        {
            string file;

            using (var scope = new TransactionScope())
            {
                file = CopyDogeFromZip();
                Assert.IsTrue(new FileInfo(file).Length > 0);
                scope.Dispose(); // Rollback
            }

            // And now, our file should be gone!
            Assert.IsFalse(File.Exists(file));
        }

        [Test]
        // We don't allow overwriting of files when doing installs. Hooray!
        public void DontOverWrite_208()
        {
            using (ZipFile zipfile = new ZipFile(TestData.DogeCoinFlagZip()))
            {
                ZipEntry entry = zipfile.GetEntry(flag_path);
                string tmpfile = Path.GetTempFileName();

                Assert.Throws<FileExistsKraken>(delegate
                {
                    CKAN.ModuleInstaller.CopyZipEntry(zipfile, entry, tmpfile, false);
                });

                // Cleanup
                File.Delete(tmpfile);
            }
        }

        [Test]
        [Category("TODO")]
        [Explicit]
        //Test how we handle corrupt data
        public void CorruptZip_242()
        {
            string corrupt_dogezip = TestData.DogeCoinFlagZipCorrupt();

            using (var zipfile = new ZipFile(corrupt_dogezip))
            {
                // GenerateDefault Install
                ModuleInstallDescriptor.DefaultInstallStanza("DogeCoinFlag");

                // FindInstallableFiles
                CkanModule dogemod = TestData.DogeCoinFlag_101_module();
                CKAN.ModuleInstaller.FindInstallableFiles(dogemod, corrupt_dogezip, ksp.KSP);
            }
        }

        private string CopyDogeFromZip()
        {
            string dogezip = TestData.DogeCoinFlagZip();
            ZipFile zipfile = new ZipFile(dogezip);

            ZipEntry entry = zipfile.GetEntry(flag_path);
            string tmpfile = Path.GetTempFileName();

            // We have to delete our temporary file, as CZE refuses to overwrite; huzzah!
            File.Delete(tmpfile);
            CKAN.ModuleInstaller.CopyZipEntry(zipfile, entry, tmpfile, false);

            return tmpfile;
        }

        [Test]
        public void UninstallModNotFound()
        {
            using (var tidy = new DisposableKSP())
            {
                var config = new FakeConfiguration(tidy.KSP, tidy.KSP.Name);

                GameInstanceManager manager = new GameInstanceManager(
                    new NullUser(),
                    config
                ) {
                    CurrentInstance = tidy.KSP
                };

                Assert.Throws<ModNotInstalledKraken>(delegate
                {
                    HashSet<string> possibleConfigOnlyDirs = null;
                    // This should throw, as our tidy KSP has no mods installed.
                    CKAN.ModuleInstaller.GetInstance(manager.CurrentInstance, manager.Cache, nullUser).UninstallList(new List<string> {"Foo"}, ref possibleConfigOnlyDirs, CKAN.RegistryManager.Instance(manager.CurrentInstance));
                });

                manager.CurrentInstance = null; // I weep even more.
                manager.Dispose();
                config.Dispose();
            }
        }

        [Test]
        public void CanInstallMod()
        {
            string mod_file_name = "DogeCoinFlag/Flags/dogecoin.png";

            // Create a new disposable KSP instance to run the test on.
            using (DisposableKSP ksp = new DisposableKSP())
            {
                var config = new FakeConfiguration(ksp.KSP, ksp.KSP.Name);

                GameInstanceManager manager = new GameInstanceManager(
                    new NullUser(),
                    config
                ) {
                    CurrentInstance = ksp.KSP
                };

                // Make sure the mod is not installed.
                string mod_file_path = Path.Combine(ksp.KSP.game.PrimaryModDirectory(ksp.KSP), mod_file_name);

                Assert.IsFalse(File.Exists(mod_file_path));

                // Copy the zip file to the cache directory.
                Assert.IsFalse(manager.Cache.IsCachedZip(TestData.DogeCoinFlag_101_module()));

                string cache_path = manager.Cache.Store(TestData.DogeCoinFlag_101_module(), TestData.DogeCoinFlagZip());

                Assert.IsTrue(manager.Cache.IsCachedZip(TestData.DogeCoinFlag_101_module()));
                Assert.IsTrue(File.Exists(cache_path));

                // Mark it as available in the registry.
                var registry = CKAN.RegistryManager.Instance(ksp.KSP).registry;
                Assert.AreEqual(0, registry.CompatibleModules(ksp.KSP.VersionCriteria()).Count());

                registry.AddAvailable(TestData.DogeCoinFlag_101_module());

                Assert.AreEqual(1, registry.CompatibleModules(ksp.KSP.VersionCriteria()).Count());

                // Attempt to install it.
                List<string> modules = new List<string> { TestData.DogeCoinFlag_101_module().identifier };

                HashSet<string> possibleConfigOnlyDirs = null;
                CKAN.ModuleInstaller.GetInstance(ksp.KSP, manager.Cache, nullUser).InstallList(modules, new RelationshipResolverOptions(), CKAN.RegistryManager.Instance(manager.CurrentInstance), ref possibleConfigOnlyDirs);

                // Check that the module is installed.
                Assert.IsTrue(File.Exists(mod_file_path));

                manager.Dispose();
                config.Dispose();
            }
        }

        [Test]
        public void InstallList_IdentifierEqualsVersionSyntax_InstallsModule()
        {
            using (DisposableKSP ksp = new DisposableKSP())
            {
                // Arrange
                var config = new FakeConfiguration(ksp.KSP, ksp.KSP.Name);

                GameInstanceManager manager = new GameInstanceManager(
                    new NullUser(),
                    config
                ) {
                    CurrentInstance = ksp.KSP
                };
                var registry = CKAN.RegistryManager.Instance(ksp.KSP).registry;
                var inst = CKAN.ModuleInstaller.GetInstance(ksp.KSP, manager.Cache, nullUser);

                const string mod_file_name = "DogeCoinFlag/Flags/dogecoin.png";
                string mod_file_path = Path.Combine(ksp.KSP.game.PrimaryModDirectory(ksp.KSP), mod_file_name);
                CkanModule mod = TestData.DogeCoinFlag_101_module();
                registry.AddAvailable(mod);
                manager.Cache.Store(mod, TestData.DogeCoinFlagZip());
                List<string> modules = new List<string>()
                {
                    $"{mod.identifier}={mod.version}"
                };

                // Act
                HashSet<string> possibleConfigOnlyDirs = null;
                inst.InstallList(modules, new RelationshipResolverOptions(), CKAN.RegistryManager.Instance(manager.CurrentInstance), ref possibleConfigOnlyDirs);

                // Assert
                Assert.IsTrue(File.Exists(mod_file_path));

                manager.Dispose();
                config.Dispose();
            }
        }

        [Test]
        public void CanUninstallMod()
        {
            string mod_file_name = "DogeCoinFlag/Flags/dogecoin.png";

            // Create a new disposable KSP instance to run the test on.
            using (var ksp = new DisposableKSP())
            {
                var config = new FakeConfiguration(ksp.KSP, ksp.KSP.Name);

                GameInstanceManager manager = new GameInstanceManager(
                    new NullUser(),
                    config
                ) {
                    CurrentInstance = ksp.KSP
                };

                string mod_file_path = Path.Combine(ksp.KSP.game.PrimaryModDirectory(ksp.KSP), mod_file_name);

                // Install the test mod.
                var registry = CKAN.RegistryManager.Instance(ksp.KSP).registry;
                manager.Cache.Store(TestData.DogeCoinFlag_101_module(), TestData.DogeCoinFlagZip());
                registry.AddAvailable(TestData.DogeCoinFlag_101_module());

                List<string> modules = new List<string> { TestData.DogeCoinFlag_101_module().identifier };

                HashSet<string> possibleConfigOnlyDirs = null;
                CKAN.ModuleInstaller.GetInstance(manager.CurrentInstance, manager.Cache, nullUser).InstallList(modules, new RelationshipResolverOptions(), CKAN.RegistryManager.Instance(manager.CurrentInstance), ref possibleConfigOnlyDirs);

                // Check that the module is installed.
                Assert.IsTrue(File.Exists(mod_file_path));

                // Attempt to uninstall it.
                CKAN.ModuleInstaller.GetInstance(manager.CurrentInstance, manager.Cache, nullUser).UninstallList(modules, ref possibleConfigOnlyDirs, CKAN.RegistryManager.Instance(manager.CurrentInstance));

                // Check that the module is not installed.
                Assert.IsFalse(File.Exists(mod_file_path));

                manager.Dispose();
                config.Dispose();
            }
        }

        [Test]
        public void UninstallEmptyDirs()
        {
            string emptyFolderName = "DogeCoinFlag";

            // Create a new disposable KSP instance to run the test on.
            using (var ksp = new DisposableKSP())
            {
                var config = new FakeConfiguration(ksp.KSP, ksp.KSP.Name);

                GameInstanceManager manager = new GameInstanceManager(
                    new NullUser(),
                    config
                ) {
                    CurrentInstance = ksp.KSP
                };

                string directoryPath = Path.Combine(ksp.KSP.game.PrimaryModDirectory(ksp.KSP), emptyFolderName);

                // Install the base test mod.

                var registry = CKAN.RegistryManager.Instance(ksp.KSP).registry;
                manager.Cache.Store(TestData.DogeCoinFlag_101_module(), TestData.DogeCoinFlagZip());
                registry.AddAvailable(TestData.DogeCoinFlag_101_module());

                List<string> modules = new List<string> { TestData.DogeCoinFlag_101_module().identifier };

                HashSet<string> possibleConfigOnlyDirs = null;
                CKAN.ModuleInstaller.GetInstance(manager.CurrentInstance, manager.Cache, nullUser).InstallList(modules, new RelationshipResolverOptions(), CKAN.RegistryManager.Instance(manager.CurrentInstance), ref possibleConfigOnlyDirs);

                modules.Clear();

                // Install the plugin test mod.
                manager.Cache.Store(TestData.DogeCoinPlugin_module(), TestData.DogeCoinPluginZip());
                registry.AddAvailable(TestData.DogeCoinPlugin_module());

                modules.Add(TestData.DogeCoinPlugin_module().identifier);

                CKAN.ModuleInstaller.GetInstance(manager.CurrentInstance, manager.Cache, nullUser).InstallList(modules, new RelationshipResolverOptions(), CKAN.RegistryManager.Instance(manager.CurrentInstance), ref possibleConfigOnlyDirs);

                modules.Clear();

                // Check that the directory is installed.
                Assert.IsTrue(Directory.Exists(directoryPath));

                // Uninstall both mods.

                modules.Add(TestData.DogeCoinFlag_101_module().identifier);
                modules.Add(TestData.DogeCoinPlugin_module().identifier);

                CKAN.ModuleInstaller.GetInstance(manager.CurrentInstance, manager.Cache, nullUser).UninstallList(modules, ref possibleConfigOnlyDirs, CKAN.RegistryManager.Instance(manager.CurrentInstance));

                // Check that the directory has been deleted.
                Assert.IsFalse(Directory.Exists(directoryPath));

                manager.Dispose();
                config.Dispose();
            }
        }

        [Test]
        public void ModuleManagerInstancesAreDecoupled()
        {
            string mod_file_name = "DogeCoinFlag/Flags/dogecoin.png";

            // Create a new disposable KSP instance to run the test on.
            Assert.DoesNotThrow(delegate
            {
                for (int i = 0; i < 5; i++)
                {
                    using (DisposableKSP ksp = new DisposableKSP())
                    {
                        var config = new FakeConfiguration(ksp.KSP, ksp.KSP.Name);

                        GameInstanceManager manager = new GameInstanceManager(
                            new NullUser(),
                            config
                        ) {
                            CurrentInstance = ksp.KSP
                        };

                        // Copy the zip file to the cache directory.
                        manager.Cache.Store(TestData.DogeCoinFlag_101_module(), TestData.DogeCoinFlagZip());

                        // Mark it as available in the registry.
                        var registry = CKAN.RegistryManager.Instance(ksp.KSP).registry;
                        registry.AddAvailable(TestData.DogeCoinFlag_101_module());

                        // Attempt to install it.
                        List<string> modules = new List<string> { TestData.DogeCoinFlag_101_module().identifier };

                        HashSet<string> possibleConfigOnlyDirs = null;
                        CKAN.ModuleInstaller.GetInstance(ksp.KSP, manager.Cache, nullUser).InstallList(modules, new RelationshipResolverOptions(), CKAN.RegistryManager.Instance(manager.CurrentInstance), ref possibleConfigOnlyDirs);

                        // Check that the module is installed.
                        string mod_file_path = Path.Combine(ksp.KSP.game.PrimaryModDirectory(ksp.KSP), mod_file_name);

                        Assert.IsTrue(File.Exists(mod_file_path));

                        manager.Dispose();
                        config.Dispose();
                    }
                }
            });
        }

        [TestCase("Ships")]
        [TestCase("Ships/VAB")]
        [TestCase("Ships/SPH")]
        [TestCase("Ships/@thumbs")]
        [TestCase("Ships/@thumbs/VAB")]
        [TestCase("Ships/@thumbs/SPH")]
        [TestCase("Ships/Script")]
        public void AllowsInstallsToShipsDirectories(string directory)
        {
            // Arrange
            var zip = ZipFile.Create(new MemoryStream());
            zip.BeginUpdate();
            zip.AddDirectory("ExampleShips");
            zip.Add(new ZipEntry("ExampleShips/AwesomeShip.craft") { Size = 0, CompressedSize = 0 });
            zip.CommitUpdate();

            var mod = CkanModule.FromJson(string.Format(@"
            {{
                ""spec_version"": 1,
                ""identifier"": ""AwesomeMod"",
                ""version"": ""1.0.0"",
                ""download"": ""https://awesomemod.example/AwesomeMod.zip"",
                ""install"": [
                    {{
                        ""file"": ""ExampleShips/AwesomeShip.craft"",
                        ""install_to"": ""{0}""
                    }}
                ]
            }}
            ", directory));

            // Act
            List<InstallableFile> results;
            using (var ksp = new DisposableKSP())
            {
                results = mod.install.First().FindInstallableFiles(zip, ksp.KSP);
            }

            // Assert
            Assert.That(
                results.Count(i => i.destination.EndsWith(string.Format("/{0}/AwesomeShip.craft", directory))) == 1,
                Is.True
            );
        }

        // TODO: It would be nice to merge this and the above function into one super
        // test.
        [Test]
        public void AllowInstallsToScenarios()
        {
            // Bogus zip with example to install.
            var zip = ZipFile.Create(new MemoryStream());
            zip.BeginUpdate();
            zip.AddDirectory("saves");
            zip.AddDirectory("saves/scenarios");
            zip.Add(new ZipEntry("saves/scenarios/AwesomeRace.sfs") { Size = 0, CompressedSize = 0 });
            zip.CommitUpdate();

            var mod = CkanModule.FromJson(@"
                {
                    ""spec_version"": ""v1.14"",
                    ""identifier"": ""AwesomeMod"",
                    ""version"": ""1.0.0"",
                    ""download"": ""https://awesomemod.example/AwesomeMod.zip"",
                    ""install"": [
                        {
                            ""file"": ""saves/scenarios/AwesomeRace.sfs"",
                            ""install_to"": ""Scenarios""
                        }
                    ]
                }");

            List<InstallableFile> results;
            using (var ksp = new DisposableKSP())
            {
                results = mod.install.First().FindInstallableFiles(zip, ksp.KSP);

                Assert.AreEqual(
                    CKAN.CKANPathUtils.NormalizePath(
                        Path.Combine(ksp.KSP.GameDir(), "saves/scenarios/AwesomeRace.sfs")
                    ),
                    results.First().destination
                );
            }
        }

        [Test]
        public void SuccessfulReplacement()
        {
            //Need to set up an installed DogeCoinFlag-101replaced mod that can validly be replaced by DogeTokenFlag-101

            // Assert that DogeCoinFlag has been removed and DogeTokenFlag has been installed
            Assert.IsTrue(true);
        }

        [Test]
        public void UnsuccessfulReplacement()
        {
            //Need to set up an installed DogeCoinFlag-101-replaced mod in a KSP version too low for DogeTokenFlag-101

            // Assert that DogeCoinFlag has not been removed and DogeTokenFlag has not been installed
            Assert.IsTrue(true);
        }
    }
}
