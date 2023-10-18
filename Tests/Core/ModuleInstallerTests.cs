using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;

using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;

using CKAN;
using CKAN.Versioning;
using CKAN.Games.KerbalSpaceProgram;

using Tests.Core.Configuration;
using Tests.Data;

namespace Tests.Core
{
    [TestFixture]
    public class ModuleInstallerTests
    {
        private const string flag_path = "DogeCoinFlag-1.01/GameData/DogeCoinFlag/Flags/dogecoin.png";
        private IUser nullUser = new NullUser();

        private DisposableKSP ksp = new DisposableKSP();

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
                ModuleInstallDescriptor stanza = ModuleInstallDescriptor.DefaultInstallStanza(new KerbalSpaceProgram(), "DogeCoinFlag");

                Assert.AreEqual("GameData", stanza.install_to);
                Assert.AreEqual("DogeCoinFlag", stanza.find);

                // Same again, but screwing up the case (we see this *all the time*)
                ModuleInstallDescriptor stanza2 = ModuleInstallDescriptor.DefaultInstallStanza(new KerbalSpaceProgram(), "DogecoinFlag");

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
            List<InstallableFile> contents = CKAN.ModuleInstaller.FindInstallableFiles(mod, TestData.DogeCoinFlagZip(), ksp.KSP);
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
                List<InstallableFile> contents = CKAN.ModuleInstaller.FindInstallableFiles(mod, TestData.DogeCoinFlagZip(), tidy.KSP);

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
            var dogemod = TestData.DogeCoinFlag_101_module();
            dogemod.install[0].install_to = path;
            using (var tidy = new DisposableKSP())
            {
                IEnumerable<InstallableFile> contents = CKAN.ModuleInstaller.FindInstallableFiles(
                                                            dogemod, TestData.DogeCoinFlagZip(), tidy.KSP
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
                List<InstallableFile> contents = CKAN.ModuleInstaller.FindInstallableFiles(TestData.ModuleManagerModule(), TestData.ModuleManagerZip(), tidy.KSP);

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
                List<InstallableFile> contents = CKAN.ModuleInstaller.FindInstallableFiles(TestData.MissionModule(), TestData.MissionZip(), tidy.KSP);

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

            CkanModule bugged_mod = TestData.DogeCoinFlag_101_bugged_module();

            Assert.Throws<BadMetadataKraken>(delegate
                {
                    CKAN.ModuleInstaller.FindInstallableFiles(bugged_mod, TestData.DogeCoinFlagZip(), ksp.KSP);
                });

            try
            {
                CKAN.ModuleInstaller.FindInstallableFiles(bugged_mod, TestData.DogeCoinFlagZip(), ksp.KSP);
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
            var dogemod = TestData.DogeCoinFlag_101_module();
            dogemod.install[0].install_to = location;

            Assert.Throws<BadInstallLocationKraken>(delegate
            {
                CKAN.ModuleInstaller.FindInstallableFiles(dogemod, TestData.DogeCoinFlagZip(), ksp.KSP);
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
                ModuleInstallDescriptor.DefaultInstallStanza(new KerbalSpaceProgram(), "DogeCoinFlag");

                // FindInstallableFiles
                CKAN.ModuleInstaller.FindInstallableFiles(TestData.DogeCoinFlag_101_module(), corrupt_dogezip, ksp.KSP);
            }
        }

        private string CopyDogeFromZip()
        {
            ZipFile zipfile = new ZipFile(TestData.DogeCoinFlagZip());

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
            using (var config = new FakeConfiguration(tidy.KSP, tidy.KSP.Name))
            using (var repoData = new TemporaryRepositoryData(nullUser))
            using (var manager = new GameInstanceManager(nullUser, config)
                {
                    CurrentInstance = tidy.KSP
                })
            {
                Assert.Throws<ModNotInstalledKraken>(delegate
                {
                    HashSet<string> possibleConfigOnlyDirs = null;
                    // This should throw, as our tidy KSP has no mods installed.
                    new CKAN.ModuleInstaller(manager.CurrentInstance, manager.Cache, nullUser)
                        .UninstallList(new List<string> {"Foo"},
                                       ref possibleConfigOnlyDirs,
                                       CKAN.RegistryManager.Instance(manager.CurrentInstance, repoData.Manager));
                });

                // I weep even more.
                manager.CurrentInstance = null;
            }
        }

        [Test]
        public void CanInstallMod()
        {
            string mod_file_name = "DogeCoinFlag/Flags/dogecoin.png";

            // Create a new disposable KSP instance to run the test on.
            using (var repo = new TemporaryRepository(TestData.DogeCoinFlag_101()))
            using (var repoData = new TemporaryRepositoryData(nullUser, repo.repo))
            using (var ksp = new DisposableKSP())
            using (var config = new FakeConfiguration(ksp.KSP, ksp.KSP.Name))
            using (var manager = new GameInstanceManager(nullUser, config)
                {
                    CurrentInstance = ksp.KSP
                })
            {
                // Make sure the mod is not installed.
                string mod_file_path = Path.Combine(ksp.KSP.game.PrimaryModDirectory(ksp.KSP), mod_file_name);

                Assert.IsFalse(File.Exists(mod_file_path));

                // Copy the zip file to the cache directory.
                Assert.IsFalse(manager.Cache.IsCached(TestData.DogeCoinFlag_101_module()));

                string cache_path = manager.Cache.Store(TestData.DogeCoinFlag_101_module(),
                                                        TestData.DogeCoinFlagZip(),
                                                        new Progress<long>(bytes => {}));

                Assert.IsTrue(manager.Cache.IsCached(TestData.DogeCoinFlag_101_module()));
                Assert.IsTrue(File.Exists(cache_path));

                var registry = CKAN.RegistryManager.Instance(manager.CurrentInstance, repoData.Manager).registry;
                registry.RepositoriesClear();
                registry.RepositoriesAdd(repo.repo);

                Assert.AreEqual(1, registry.CompatibleModules(ksp.KSP.VersionCriteria()).Count());

                // Attempt to install it.
                var modules = new List<CkanModule> { TestData.DogeCoinFlag_101_module() };

                HashSet<string> possibleConfigOnlyDirs = null;
                new CKAN.ModuleInstaller(ksp.KSP, manager.Cache, nullUser)
                    .InstallList(modules,
                                 new RelationshipResolverOptions(),
                                 CKAN.RegistryManager.Instance(manager.CurrentInstance, repoData.Manager),
                                 ref possibleConfigOnlyDirs);

                // Check that the module is installed.
                Assert.IsTrue(File.Exists(mod_file_path));
            }
        }

        [Test]
        public void CanUninstallMod()
        {
            const string mod_file_name = "DogeCoinFlag/Flags/dogecoin.png";

            // Create a new disposable KSP instance to run the test on.
            using (var repo = new TemporaryRepository(TestData.DogeCoinFlag_101()))
            using (var repoData = new TemporaryRepositoryData(nullUser, repo.repo))
            using (var ksp = new DisposableKSP())
            using (var config = new FakeConfiguration(ksp.KSP, ksp.KSP.Name))
            using (var manager = new GameInstanceManager(nullUser, config)
                {
                    CurrentInstance = ksp.KSP
                })
            {
                string mod_file_path = Path.Combine(ksp.KSP.game.PrimaryModDirectory(ksp.KSP), mod_file_name);

                // Install the test mod.
                var registry = CKAN.RegistryManager.Instance(ksp.KSP, repoData.Manager).registry;
                registry.RepositoriesClear();
                registry.RepositoriesAdd(repo.repo);
                manager.Cache.Store(TestData.DogeCoinFlag_101_module(),
                                    TestData.DogeCoinFlagZip(),
                                    new Progress<long>(bytes => {}));

                var modules = new List<CkanModule> { TestData.DogeCoinFlag_101_module() };

                HashSet<string> possibleConfigOnlyDirs = null;
                new CKAN.ModuleInstaller(manager.CurrentInstance, manager.Cache, nullUser)
                    .InstallList(modules, new RelationshipResolverOptions(),
                                 CKAN.RegistryManager.Instance(manager.CurrentInstance, repoData.Manager),
                                 ref possibleConfigOnlyDirs);

                // Check that the module is installed.
                Assert.IsTrue(File.Exists(mod_file_path));

                // Attempt to uninstall it.
                new CKAN.ModuleInstaller(manager.CurrentInstance, manager.Cache, nullUser)
                    .UninstallList(modules.Select(m => m.identifier),
                                   ref possibleConfigOnlyDirs,
                                   CKAN.RegistryManager.Instance(manager.CurrentInstance, repoData.Manager));

                // Check that the module is not installed.
                Assert.IsFalse(File.Exists(mod_file_path));
            }
        }

        [Test]
        public void UninstallEmptyDirs()
        {
            const string emptyFolderName = "DogeCoinFlag";

            using (var repo = new TemporaryRepository(TestData.DogeCoinFlag_101(),
                                                      TestData.DogeCoinPlugin()))
            using (var repoData = new TemporaryRepositoryData(nullUser, repo.repo))
            // Create a new disposable KSP instance to run the test on.
            using (var ksp = new DisposableKSP())
            using (var config = new FakeConfiguration(ksp.KSP, ksp.KSP.Name))
            using (var manager = new GameInstanceManager(new NullUser(), config)
                {
                    CurrentInstance = ksp.KSP
                })
            {
                string directoryPath = Path.Combine(ksp.KSP.game.PrimaryModDirectory(ksp.KSP), emptyFolderName);

                // Install the base test mod.
                var registry = CKAN.RegistryManager.Instance(ksp.KSP, repoData.Manager).registry;
                registry.RepositoriesClear();
                registry.RepositoriesAdd(repo.repo);
                manager.Cache.Store(TestData.DogeCoinFlag_101_module(),
                                    TestData.DogeCoinFlagZip(),
                                    new Progress<long>(bytes => {}));

                var modules = new List<CkanModule> { TestData.DogeCoinFlag_101_module() };

                HashSet<string> possibleConfigOnlyDirs = null;
                new CKAN.ModuleInstaller(manager.CurrentInstance, manager.Cache, nullUser)
                    .InstallList(modules,
                                 new RelationshipResolverOptions(),
                                 CKAN.RegistryManager.Instance(manager.CurrentInstance, repoData.Manager),
                                 ref possibleConfigOnlyDirs);

                modules.Clear();

                // Install the plugin test mod.
                manager.Cache.Store(TestData.DogeCoinPlugin_module(),
                                    TestData.DogeCoinPluginZip(),
                                    new Progress<long>(bytes => {}));

                modules.Add(TestData.DogeCoinPlugin_module());

                new CKAN.ModuleInstaller(manager.CurrentInstance, manager.Cache, nullUser)
                    .InstallList(modules,
                                 new RelationshipResolverOptions(),
                                 CKAN.RegistryManager.Instance(manager.CurrentInstance, repoData.Manager),
                                 ref possibleConfigOnlyDirs);

                modules.Clear();

                // Check that the directory is installed.
                Assert.IsTrue(Directory.Exists(directoryPath));

                // Uninstall both mods.

                modules.Add(TestData.DogeCoinFlag_101_module());
                modules.Add(TestData.DogeCoinPlugin_module());

                new CKAN.ModuleInstaller(manager.CurrentInstance, manager.Cache, nullUser)
                    .UninstallList(modules.Select(m => m.identifier),
                                   ref possibleConfigOnlyDirs,
                                   CKAN.RegistryManager.Instance(manager.CurrentInstance, repoData.Manager));

                // Check that the directory has been deleted.
                Assert.IsFalse(Directory.Exists(directoryPath));
            }
        }

        [Test,
            // Empty dir
            TestCase("GameData/SomeMod/Parts",
                     new string[] {},
                     new string[] {},
                     new string[] {},
                     new string[] {}),
            // A few regular files and some thumbnails
            TestCase("GameData/SomeMod/Parts",
                     new string[] {},
                     new string[]
                     {
                         "GameData/SomeMod/Parts/userfile.cfg",
                         "GameData/SomeMod/Parts/userfile2.cfg",
                         "GameData/SomeMod/Parts/@thumbs",
                         "GameData/SomeMod/Parts/@thumbs/part1.png",
                         "GameData/SomeMod/Parts/@thumbs/part3.png",
                         "GameData/SomeMod/Parts/@thumbs/part4.png",
                     },
                     new string[]
                     {
                         "GameData/SomeMod/Parts/@thumbs/part1.png",
                         "GameData/SomeMod/Parts/@thumbs/part3.png",
                         "GameData/SomeMod/Parts/@thumbs/part4.png",
                         "GameData/SomeMod/Parts/@thumbs",
                     },
                     new string[]
                     {
                         "GameData/SomeMod/Parts/userfile2.cfg",
                         "GameData/SomeMod/Parts/userfile.cfg",
                     }),
            // Just regular files
            TestCase("GameData/SomeMod/Parts",
                     new string[] {},
                     new string[]
                     {
                         "GameData/SomeMod/Parts/userfile.cfg",
                         "GameData/SomeMod/Parts/userfile2.cfg",
                     },
                     new string[] {},
                     new string[]
                     {
                         "GameData/SomeMod/Parts/userfile2.cfg",
                         "GameData/SomeMod/Parts/userfile.cfg",
                     }),
            // Just thumbnails
            TestCase("GameData/SomeMod/Parts",
                     new string[] {},
                     new string[]
                     {
                         "GameData/SomeMod/Parts/@thumbs",
                         "GameData/SomeMod/Parts/@thumbs/part1.png",
                         "GameData/SomeMod/Parts/@thumbs/part3.png",
                         "GameData/SomeMod/Parts/@thumbs/part4.png",
                     },
                     new string[]
                     {
                         "GameData/SomeMod/Parts/@thumbs/part1.png",
                         "GameData/SomeMod/Parts/@thumbs/part3.png",
                         "GameData/SomeMod/Parts/@thumbs/part4.png",
                         "GameData/SomeMod/Parts/@thumbs",
                     },
                     new string[] {}),
            // A few regular files and some thumbnails, some of which are owned by another mod
            TestCase("GameData/SomeMod/Parts",
                     new string[]
                     {
                         "GameData/SomeMod/Parts/userfile2.cfg",
                         "GameData/SomeMod/Parts/@thumbs/part1.png",
                     },
                     new string[]
                     {
                         "GameData/SomeMod/Parts/userfile.cfg",
                         "GameData/SomeMod/Parts/userfile2.cfg",
                         "GameData/SomeMod/Parts/@thumbs",
                         "GameData/SomeMod/Parts/@thumbs/part1.png",
                         "GameData/SomeMod/Parts/@thumbs/part3.png",
                         "GameData/SomeMod/Parts/@thumbs/part4.png",
                     },
                     new string[]
                     {
                         "GameData/SomeMod/Parts/@thumbs/part3.png",
                         "GameData/SomeMod/Parts/@thumbs/part4.png",
                         "GameData/SomeMod/Parts/@thumbs",
                     },
                     new string[]
                     {
                         "GameData/SomeMod/Parts/@thumbs/part1.png",
                         "GameData/SomeMod/Parts/userfile2.cfg",
                         "GameData/SomeMod/Parts/userfile.cfg",
                     }),
        ]
        public void GroupFilesByRemovable_WithFiles_CorrectOutput(string   relRoot,
                                                                  string[] registeredFiles,
                                                                  string[] relPaths,
                                                                  string[] correctRemovable,
                                                                  string[] correctNotRemovable)
        {
            // Arrange
            using (var inst = new DisposableKSP())
            using (var repoData = new TemporaryRepositoryData(nullUser))
            {
                var game     = new KerbalSpaceProgram();
                var registry = CKAN.RegistryManager.Instance(inst.KSP, repoData.Manager).registry;
                // Make files to be registered to another mod
                var absFiles = registeredFiles.Select(f => inst.KSP.ToAbsoluteGameDir(f))
                                              .ToArray();
                foreach (var absPath in absFiles)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(absPath));
                    File.Create(absPath).Dispose();
                }
                // Register the other mod
                registry.RegisterModule(CkanModule.FromJson(@"{
                                            ""spec_version"": 1,
                                            ""identifier"":   ""otherMod"",
                                            ""version"":      ""1.0"",
                                            ""download"":     ""https://github.com/""
                                        }"),
                                        absFiles, inst.KSP, false);

                // Act
                CKAN.ModuleInstaller.GroupFilesByRemovable(relRoot,
                                                           registry,
                                                           new string[] {},
                                                           game,
                                                           relPaths,
                                                           out string[] removable,
                                                           out string[] notRemovable);

                // Assert
                Assert.AreEqual(correctRemovable,    removable);
                Assert.AreEqual(correctNotRemovable, notRemovable);
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
                    using (var repo = new TemporaryRepository(TestData.DogeCoinFlag_101()))
                    using (var repoData = new TemporaryRepositoryData(nullUser, repo.repo))
                    using (var ksp = new DisposableKSP())
                    using (var config = new FakeConfiguration(ksp.KSP, ksp.KSP.Name))
                    using (var manager = new GameInstanceManager(nullUser, config)
                        {
                            CurrentInstance = ksp.KSP
                        })
                    {
                        var regMgr = CKAN.RegistryManager.Instance(manager.CurrentInstance, repoData.Manager);
                        var registry = regMgr.registry;
                        registry.RepositoriesClear();
                        registry.RepositoriesAdd(repo.repo);

                        // Copy the zip file to the cache directory.
                        manager.Cache.Store(TestData.DogeCoinFlag_101_module(),
                                            TestData.DogeCoinFlagZip(),
                                            new Progress<long>(bytes => {}));

                        // Attempt to install it.
                        var modules = new List<CkanModule> { TestData.DogeCoinFlag_101_module() };

                        HashSet<string> possibleConfigOnlyDirs = null;
                        new CKAN.ModuleInstaller(ksp.KSP, manager.Cache, nullUser)
                            .InstallList(modules,
                                         new RelationshipResolverOptions(),
                                         CKAN.RegistryManager.Instance(manager.CurrentInstance, repoData.Manager),
                                         ref possibleConfigOnlyDirs);

                        // Check that the module is installed.
                        string mod_file_path = Path.Combine(ksp.KSP.game.PrimaryModDirectory(ksp.KSP), mod_file_name);

                        Assert.IsTrue(File.Exists(mod_file_path));
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

            using (var ksp = new DisposableKSP())
            {
                var results = mod.install.First().FindInstallableFiles(zip, ksp.KSP);

                Assert.AreEqual(
                    CKAN.CKANPathUtils.NormalizePath(
                        Path.Combine(ksp.KSP.GameDir(), "saves/scenarios/AwesomeRace.sfs")),
                    results.First().destination);
            }
        }

        [Test]
        public void Replace_WithCompatibleModule_Succeeds()
        {
            // Arrange
            using (var inst = new DisposableKSP())
            using (var repo = new TemporaryRepository(
                @"{
                    ""spec_version"": ""v1.4"",
                    ""identifier"":   ""replaced"",
                    ""version"":      ""1.0"",
                    ""ksp_version"":  ""1.12"",
                    ""replaced_by"": {
                        ""name"": ""replacer""
                    },
                    ""download"":     ""https://awesomemod.example/AwesomeMod.zip"",
                }",
                @"{
                    ""spec_version"": ""v1.4"",
                    ""identifier"":   ""replacer"",
                    ""version"":      ""1.0"",
                    ""ksp_version"":  ""1.12"",
                    ""download"":     ""https://awesomemod.example/AwesomeMod.zip"",
                    ""install"": [
                        {
                            ""file"": ""DogeCoinFlag-1.01/GameData/DogeCoinFlag"",
                            ""install_to"": ""GameData"",
                            ""filter"" : [ ""Thumbs.db"", ""README.md"" ],
                            ""filter_regexp"" : ""\\.bak$""
                        }
                    ]
                }"))
            using (var repoData = new TemporaryRepositoryData(nullUser, repo.repo))
            using (var config = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(nullUser, config)
                {
                    CurrentInstance = inst.KSP
                })
            {
                var regMgr = CKAN.RegistryManager.Instance(manager.CurrentInstance, repoData.Manager);
                var registry = regMgr.registry;
                IRegistryQuerier querier = registry;
                registry.RepositoriesAdd(repo.repo);
                var replaced = registry.GetModuleByVersion("replaced", "1.0");
                Assert.IsNotNull(replaced, "Replaced module should exist");
                var replacer = registry.GetModuleByVersion("replacer", "1.0");
                Assert.IsNotNull(replacer, "Replacer module should exist");
                var installer = new CKAN.ModuleInstaller(inst.KSP, manager.Cache, nullUser);
                HashSet<string> possibleConfigOnlyDirs = null;
                var downloader = new NetAsyncModulesDownloader(nullUser, manager.Cache);

                // Act
                registry.RegisterModule(replaced, Enumerable.Empty<string>(), inst.KSP, false);
                manager.Cache.Store(replaced, TestData.DogeCoinFlagZip(), new Progress<long>(bytes => {}));
                var replacement = querier.GetReplacement(replaced.identifier,
                                                         new GameVersionCriteria(new GameVersion(1, 12)));
                installer.Replace(Enumerable.Repeat<ModuleReplacement>(replacement, 1),
                                  new RelationshipResolverOptions(),
                                  downloader, ref possibleConfigOnlyDirs, regMgr,
                                  false);

                // Assert
                CollectionAssert.AreEqual(
                    Enumerable.Repeat<CkanModule>(replacer, 1),
                    registry.InstalledModules.Select(im => im.Module));
            }
        }

        [Test]
        public void Replace_WithIncompatibleModule_Fails()
        {
            // Arrange
            using (var inst = new DisposableKSP())
            using (var repo = new TemporaryRepository(
                @"{
                    ""spec_version"": ""v1.4"",
                    ""identifier"":   ""replaced"",
                    ""version"":      ""1.0"",
                    ""ksp_version"":  ""1.12"",
                    ""replaced_by"": {
                        ""name"": ""replacer""
                    },
                    ""download"":     ""https://awesomemod.example/AwesomeMod.zip"",
                }",
                @"{
                    ""spec_version"": ""v1.4"",
                    ""identifier"":   ""replacer"",
                    ""version"":      ""1.0"",
                    ""ksp_version"":  ""1.12"",
                    ""download"":     ""https://awesomemod.example/AwesomeMod.zip"",
                    ""install"": [
                        {
                            ""file"": ""DogeCoinFlag-1.01/GameData/DogeCoinFlag"",
                            ""install_to"": ""GameData"",
                            ""filter"" : [ ""Thumbs.db"", ""README.md"" ],
                            ""filter_regexp"" : ""\\.bak$""
                        }
                    ]
                }"))
            using (var repoData = new TemporaryRepositoryData(nullUser, repo.repo))
            using (var config = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(nullUser, config)
                {
                    CurrentInstance = inst.KSP
                })
            {
                var regMgr = CKAN.RegistryManager.Instance(manager.CurrentInstance, repoData.Manager);
                var registry = regMgr.registry;
                IRegistryQuerier querier = registry;
                registry.RepositoriesAdd(repo.repo);
                var replaced = registry.GetModuleByVersion("replaced", "1.0");
                Assert.IsNotNull(replaced, "Replaced module should exist");
                var replacer = registry.GetModuleByVersion("replacer", "1.0");
                Assert.IsNotNull(replacer, "Replacer module should exist");
                var installer = new CKAN.ModuleInstaller(inst.KSP, manager.Cache, nullUser);
                var downloader = new NetAsyncModulesDownloader(nullUser, manager.Cache);

                // Act
                registry.RegisterModule(replaced, Enumerable.Empty<string>(), inst.KSP, false);
                manager.Cache.Store(replaced, TestData.DogeCoinFlagZip(), new Progress<long>(bytes => {}));
                var replacement = querier.GetReplacement(replaced.identifier,
                                                         new GameVersionCriteria(new GameVersion(1, 11)));

                // Assert
                Assert.IsNull(replacement);
                CollectionAssert.AreEqual(
                    Enumerable.Repeat<CkanModule>(replaced, 1),
                    registry.InstalledModules.Select(im => im.Module));
            }
        }

        [Test,
            // No mods, nothing installed
            TestCase(new string[] { },
                     new string[] { },
                     new string[] { },
                     new string[] { }),
            // Uninstalling a mod that depends on an auto-installed mod
            TestCase(new string[]
                     {
                         @"{
                            ""spec_version"": 1,
                            ""identifier"":   ""RemovingMod"",
                            ""version"":      ""1.0"",
                            ""depends"": [
                                { ""name"": ""AutoRemovableMod"" }
                            ],
                            ""download"":     ""https://github.com/""
                         }",
                     },
                     new string[]
                     {
                         @"{
                            ""spec_version"": 1,
                            ""identifier"":   ""AutoRemovableMod"",
                            ""version"":      ""1.0"",
                            ""download"":     ""https://github.com/""
                         }",
                     },
                     new string[] { "RemovingMod" },
                     new string[] { }),
        ]
        public void UninstallList_WithAutoInst_RemovesAutoRemovable(string[] regularMods,
                                                                    string[] autoInstMods,
                                                                    string[] removeIdentifiers,
                                                                    string[] correctRemainingIdentifiers)
        {
            // Arrange
            using (var inst    = new DisposableKSP())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(nullUser, config)
                {
                    CurrentInstance = inst.KSP
                })
            using (var repo = new TemporaryRepository(regularMods.Concat(autoInstMods).ToArray()))
            using (var repoData = new TemporaryRepositoryData(nullUser, repo.repo))
            {
                var installer = new CKAN.ModuleInstaller(inst.KSP, manager.Cache, nullUser);
                var regMgr    = CKAN.RegistryManager.Instance(manager.CurrentInstance, repoData.Manager);
                var registry  = regMgr.registry;
                var possibleConfigOnlyDirs = new HashSet<string>();
                foreach (var m in regularMods)
                {
                    registry.RegisterModule(CkanModule.FromJson(m),
                                            Enumerable.Empty<string>(),
                                            inst.KSP,
                                            false);
                }
                foreach (var m in autoInstMods)
                {
                    registry.RegisterModule(CkanModule.FromJson(m),
                                            Enumerable.Empty<string>(),
                                            inst.KSP,
                                            true);
                }

                // Act
                installer.UninstallList(removeIdentifiers, ref possibleConfigOnlyDirs, regMgr, false, null);

                // Assert
                CollectionAssert.AreEquivalent(correctRemainingIdentifiers,
                                               registry.InstalledModules.Select(im => im.identifier).ToArray());
            }
        }

        [Test,
            // No mods, nothing installed
            TestCase(new string[] { },
                     new string[] { },
                     new string[] { },
                     new string[] { }),
            // Upgrading a mod that drops a dependency on an auto-installed mod
            TestCase(new string[]
                     {
                         @"{
                            ""spec_version"": 1,
                            ""identifier"":   ""DogeCoinFlag"",
                            ""version"":      ""1.0"",
                            ""depends"": [
                                { ""name"": ""RemovableMod"" }
                            ],
                            ""download"":     ""https://github.com/""
                         }",
                         @"{
                            ""spec_version"": 1,
                            ""identifier"":   ""DogeCoinFlag"",
                            ""version"":      ""2.0"",
                            ""download"":     ""https://awesomemod.example/AwesomeMod.zip"",
                         }",
                     },
                     new string[]
                     {
                         @"{
                            ""spec_version"": 1,
                            ""identifier"":   ""RemovableMod"",
                            ""version"":      ""1.0"",
                            ""download"":     ""https://github.com/""
                         }",
                     },
                     new string[] { "DogeCoinFlag" },
                     new string[] { "DogeCoinFlag" }),
        ]
        public void Upgrade_WithAutoInst_RemovesAutoRemovable(string[] regularMods,
                                                              string[] autoInstMods,
                                                              string[] upgradeIdentifiers,
                                                              string[] correctRemainingIdentifiers)
        {
            // Arrange
            using (var inst    = new DisposableKSP())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(nullUser, config)
                {
                    CurrentInstance = inst.KSP
                })
            using (var repo = new TemporaryRepository(regularMods.Concat(autoInstMods).ToArray()))
            using (var repoData = new TemporaryRepositoryData(nullUser, repo.repo))
            {
                var installer  = new CKAN.ModuleInstaller(inst.KSP, manager.Cache, nullUser);
                var downloader = new NetAsyncModulesDownloader(nullUser, manager.Cache);
                var regMgr     = CKAN.RegistryManager.Instance(manager.CurrentInstance, repoData.Manager);
                var registry   = regMgr.registry;
                registry.RepositoriesSet(new SortedDictionary<string, Repository>()
                {
                    { "testRepo", repo.repo }
                });
                IRegistryQuerier querier = registry;
                var possibleConfigOnlyDirs = new HashSet<string>();
                foreach (var m in regularMods)
                {
                    var module = CkanModule.FromJson(m);
                    manager.Cache.Store(module,
                                        TestData.DogeCoinFlagZip(),
                                        new Progress<long>(bytes => {}));
                    if (!querier.IsInstalled(module.identifier, false))
                    {
                        registry.RegisterModule(module,
                                                Enumerable.Empty<string>(),
                                                inst.KSP,
                                                false);
                    }
                }
                foreach (var m in autoInstMods)
                {
                    registry.RegisterModule(CkanModule.FromJson(m),
                                            Enumerable.Empty<string>(),
                                            inst.KSP,
                                            true);
                }

                // Act
                installer.Upgrade(upgradeIdentifiers.Select(ident =>
                                      registry.LatestAvailable(ident, inst.KSP.VersionCriteria())),
                                  downloader, ref possibleConfigOnlyDirs, regMgr, false);

                // Assert
                CollectionAssert.AreEquivalent(correctRemainingIdentifiers,
                                               registry.InstalledModules.Select(im => im.identifier).ToArray());
            }
        }
    }
}
