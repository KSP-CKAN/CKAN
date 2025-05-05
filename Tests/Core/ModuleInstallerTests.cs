using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;

using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;

using CKAN;
using CKAN.IO;
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
        private readonly IUser nullUser = new NullUser();

        private readonly DisposableKSP ksp = new DisposableKSP();

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
            Assert.IsNull(mod.install?[0].file);
            Assert.IsNotNull(mod.install?[0].find);
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
        private static readonly CkanModule[] doge_mods =
        {
            TestData.DogeCoinFlag_101_module(),
            TestData.DogeCoinFlag_101_module_find()
        };

        [Test]
        [TestCaseSource(nameof(doge_mods))]
        public void FindInstallableFiles(CkanModule mod)
        {
            List<InstallableFile> contents = ModuleInstaller.FindInstallableFiles(mod, TestData.DogeCoinFlagZip(), ksp.KSP);
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
        [TestCaseSource(nameof(doge_mods))]
        public void FindInstallableFiles_WithKSP(CkanModule mod)
        {
            using (var tidy = new DisposableKSP())
            {
                List<InstallableFile> contents = ModuleInstaller.FindInstallableFiles(mod, TestData.DogeCoinFlagZip(), tidy.KSP);

                // See if we can find an expected destination path in the right place.
                var file = contents
                    .Select(x => x.destination)
                    .FirstOrDefault(x => Regex.IsMatch(x, "GameData/DogeCoinFlag/Flags/dogecoin\\.png$"));

                Assert.IsNotNull(file);
            }
        }

        // GH #315, all of these should result in the same output.
        // Even though they're not necessarily all spec-valid, we should accept them
        // nonetheless.
        private static readonly string[] SuchPaths =
        {
            "GameData/SuchTest",
            "GameData/SuchTest/",
            "GameData\\SuchTest",
            "GameData\\SuchTest\\",
            "GameData\\SuchTest/",
            "GameData/SuchTest\\"
        };

        [Test]
        [TestCaseSource(nameof(SuchPaths))]
        public void FindInstallableFiles_WithBonusPath(string path)
        {
            var dogemod = TestData.DogeCoinFlag_101_module();
            dogemod.install![0].install_to = path;
            using (var tidy = new DisposableKSP())
            {
                IEnumerable<InstallableFile> contents = ModuleInstaller.FindInstallableFiles(
                                                            dogemod, TestData.DogeCoinFlagZip(), tidy.KSP
                                                        );

                var file = contents
                    .Select(x => x.destination)
                    .FirstOrDefault(x => Regex.IsMatch(x, "GameData/SuchTest/DogeCoinFlag/Flags/dogecoin\\.png$"));

                Assert.IsNotNull(file);
            }
        }

        [Test]
        public void ModuleManagerInstall()
        {
            using (var tidy = new DisposableKSP())
            {
                List<InstallableFile> contents = ModuleInstaller.FindInstallableFiles(TestData.ModuleManagerModule(), TestData.ModuleManagerZip(), tidy.KSP);

                var file = contents
                    .Select(x => x.destination)
                    .FirstOrDefault(x => Regex.IsMatch(x, @"ModuleManager\.2\.5\.1\.dll$"));

                Assert.IsNotNull(file, "ModuleManager install");
            }
        }

        [Test]
        public void MissionInstall()
        {
            using (var tidy = new DisposableKSP())
            {
                List<InstallableFile> contents = ModuleInstaller.FindInstallableFiles(TestData.MissionModule(), TestData.MissionZip(), tidy.KSP);

                var failBanner = contents.Select(x => x.destination).FirstOrDefault(
                    x => Regex.IsMatch(x, "Missions/AwesomeMission/Banners/Fail/default\\.png$"));
                var menuBanner = contents.Select(x => x.destination).FirstOrDefault(
                    x => Regex.IsMatch(x, "Missions/AwesomeMission/Banners/Menu/default\\.png$"));
                var successBanner = contents.Select(x => x.destination).FirstOrDefault(
                    x => Regex.IsMatch(x, "Missions/AwesomeMission/Banners/Success/default\\.png$"));
                var metaFile = contents.Select(x => x.destination).FirstOrDefault(
                    x => Regex.IsMatch(x, "Missions/AwesomeMission/persistent\\.loadmeta$"));
                var missionFile = contents.Select(x => x.destination).FirstOrDefault(
                    x => Regex.IsMatch(x, "Missions/AwesomeMission/persistent\\.mission$"));

                Assert.IsNotNull(failBanner, "There is no fail banner in MissionInstall");
                Assert.IsNotNull(menuBanner, "There is no menu banner in MissionInstall");
                Assert.IsNotNull(successBanner, "There is no success banner in MissionInstall");
                Assert.IsNotNull(metaFile, "There is no .loadmeta file in MissionInstall");
                Assert.IsNotNull(missionFile, "There is no .mission file in MissionInstall");
            }
        }

        [Test]
        [TestCaseSource(nameof(doge_mods))]
        // Make sure all our filters work.
        public void FindInstallableFiles_WithFilter(CkanModule mod)
        {
            string extra_doge = TestData.DogeCoinFlagZipWithExtras();

            List<InstallableFile> contents = ModuleInstaller.FindInstallableFiles(mod, extra_doge, ksp.KSP);

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

            List<InstallableFile> contents = ModuleInstaller.FindInstallableFiles(mod, extra_doge, ksp.KSP);

            var files = contents.Select(x => x.source.Name);

            Assert.IsTrue(files.Contains("DogeCoinFlag-1.01/GameData/DogeCoinFlag/Flags/dogecoin.png"), "dogecoin.png");
            Assert.IsFalse(files.Contains("DogeCoinFlag-1.01/GameData/DogeCoinFlag/README.md"), "Filtered README 1");
            Assert.IsFalse(files.Contains("DogeCoinFlag-1.01/GameData/DogeCoinFlag/Flags/README.md"), "Filtered README 2");
            Assert.IsTrue(files.Contains("DogeCoinFlag-1.01/GameData/DogeCoinFlag/notes.txt.bak"), ".bak file");
        }

        [Test]
        public void FindInstallableFiles_NoInstallableFiles()
        {
            // This tests GH #93

            CkanModule bugged_mod = TestData.DogeCoinFlag_101_bugged_module();

            var exc = Assert.Throws<BadMetadataKraken>(delegate
                {
                    ModuleInstaller.FindInstallableFiles(bugged_mod, TestData.DogeCoinFlagZip(), ksp.KSP);
                });

            // Make sure our module information is attached.
            Assert.IsNotNull(exc?.module);
            Assert.AreEqual(bugged_mod.identifier, exc?.module?.identifier);
        }

        // All of these targets should fail.
        private static readonly string[] BadTargets = {
            "GameDataIsTheBestData", "Shups", "GameData/../../../../etc/pwned",
            "Ships/Foo", "GameRoot/saves", "GameRoot/CKAN", "GameData/..",
            @"GameData\..\..\etc\pwned", @"GameData\.."
        };

        [Test]
        [TestCaseSource(nameof(BadTargets))]
        public void FindInstallableFiles_WithBadTarget(string location)
        {
            // This install location? It shouldn't be valid.
            var dogemod = TestData.DogeCoinFlag_101_module();
            dogemod.install![0].install_to = location;

            Assert.Throws<BadInstallLocationKraken>(delegate
            {
                ModuleInstaller.FindInstallableFiles(dogemod, TestData.DogeCoinFlagZip(), ksp.KSP);
            });
        }

        [Test]
        public void FindInstallableFiles_ZipSlip_Throws()
        {
            // Arrange
            // Create a ZIP file with an entry that tries to exploit Zip Slip
            var zip = ZipFile.Create(new MemoryStream());
            zip.BeginUpdate();
            zip.AddDirectory("AwesomeMod");
            zip.Add(new ZipEntry("AwesomeMod/../../../outside.txt") { Size = 0, CompressedSize = 0 });
            zip.CommitUpdate();
            // Create a mod that would install the top folder of that path
            var mod = CkanModule.FromJson(@"
                {
                    ""spec_version"": 1,
                    ""identifier"": ""AwesomeMod"",
                    ""author"": ""AwesomeModder"",
                    ""version"": ""1.0.0"",
                    ""download"": ""https://awesomemod.example/AwesomeMod.zip""
                }");

            // Act / Assert
            Assert.Throws<BadInstallLocationKraken>(
                delegate
                {
                    using (var ksp = new DisposableKSP())
                    {
                        var contents = ModuleInstaller.FindInstallableFiles(mod, zip, ksp.KSP);
                    }
                },
                "Kraken should be thrown if ZIP file attempts to exploit Zip Slip vulnerability");
        }

        [Test]
        // GH #205, make sure we write in *binary*, not text.
        public void BinaryNotText_205()
        {
            // Use InstallFile (via CopyDogeFromZip) and make sure it
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
        // Make sure when we roll-back a transaction, files written with InstallFile go
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
                    ModuleInstaller.InstallFile(zipfile, entry, tmpfile, false, Array.Empty<string>(), null);
                });

                // Cleanup
                File.Delete(tmpfile);
            }
        }

        [Test]
        //Test how we handle corrupt data
        public void CorruptZip_242()
        {
            string corrupt_dogezip = TestData.DogeCoinFlagZipCorrupt();

            var exc = Assert.Throws<ZipException>(() =>
            {
                using (var zipfile = new ZipFile(corrupt_dogezip))
                {
                    // GenerateDefault Install
                    ModuleInstallDescriptor.DefaultInstallStanza(new KerbalSpaceProgram(), "DogeCoinFlag");

                    // FindInstallableFiles
                    ModuleInstaller.FindInstallableFiles(TestData.DogeCoinFlag_101_module(),
                                                         corrupt_dogezip, ksp.KSP);
                }
            });
            Assert.AreEqual("Cannot find central directory", exc?.Message);
        }

        private static string CopyDogeFromZip()
        {
            ZipFile zipfile = new ZipFile(TestData.DogeCoinFlagZip());

            ZipEntry entry = zipfile.GetEntry(flag_path);
            string tmpfile = Path.GetTempFileName();

            // We have to delete our temporary file, as CZE refuses to overwrite; huzzah!
            File.Delete(tmpfile);
            ModuleInstaller.InstallFile(zipfile, entry, tmpfile, false, Array.Empty<string>(), null);

            return tmpfile;
        }

        [Test]
        public void UninstallModNotFound()
        {
            using (var tidy     = new DisposableKSP())
            using (var config   = new FakeConfiguration(tidy.KSP, tidy.KSP.Name))
            using (var repoData = new TemporaryRepositoryData(nullUser))
            using (var manager  = new GameInstanceManager(nullUser, config)
                {
                    CurrentInstance = tidy.KSP
                })
            using (var regMgr   = RegistryManager.Instance(tidy.KSP, repoData.Manager))
            {
                Assert.Throws<ModNotInstalledKraken>(delegate
                {
                    HashSet<string>? possibleConfigOnlyDirs = null;
                    // This should throw, as our tidy KSP has no mods installed.
                    new ModuleInstaller(manager.CurrentInstance, manager.Cache!, config, nullUser)
                        .UninstallList(new List<string> {"Foo"},
                                       ref possibleConfigOnlyDirs, regMgr);
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
            using (var repo     = new TemporaryRepository(TestData.DogeCoinFlag_101()))
            using (var repoData = new TemporaryRepositoryData(nullUser, repo.repo))
            using (var ksp      = new DisposableKSP())
            using (var config   = new FakeConfiguration(ksp.KSP, ksp.KSP.Name))
            using (var manager  = new GameInstanceManager(nullUser, config)
                {
                    CurrentInstance = ksp.KSP
                })
            using (var regMgr   = RegistryManager.Instance(manager.CurrentInstance, repoData.Manager,
                                                           new Repository[] { repo.repo }))
            {
                // Make sure the mod is not installed.
                string mod_file_path = Path.Combine(ksp.KSP.game.PrimaryModDirectory(ksp.KSP), mod_file_name);

                Assert.IsFalse(File.Exists(mod_file_path));

                // Copy the zip file to the cache directory.
                Assert.IsFalse(manager.Cache?.IsCached(TestData.DogeCoinFlag_101_module()));

                var cache_path = manager.Cache?.Store(TestData.DogeCoinFlag_101_module(),
                                                      TestData.DogeCoinFlagZip(),
                                                      new Progress<long>(bytes => {}));

                Assert.IsTrue(manager.Cache?.IsCached(TestData.DogeCoinFlag_101_module()));
                Assert.IsTrue(File.Exists(cache_path));

                var registry = regMgr.registry;
                registry.RepositoriesClear();
                registry.RepositoriesAdd(repo.repo);

                Assert.AreEqual(1, registry.CompatibleModules(ksp.KSP.StabilityToleranceConfig, ksp.KSP.VersionCriteria()).Count());

                // Attempt to install it.
                var modules = new List<CkanModule> { TestData.DogeCoinFlag_101_module() };

                HashSet<string>? possibleConfigOnlyDirs = null;
                new ModuleInstaller(ksp.KSP, manager.Cache!, config, nullUser)
                    .InstallList(modules,
                                 new RelationshipResolverOptions(ksp.KSP.StabilityToleranceConfig),
                                 regMgr,
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
            using (var repo     = new TemporaryRepository(TestData.DogeCoinFlag_101()))
            using (var repoData = new TemporaryRepositoryData(nullUser, repo.repo))
            using (var ksp      = new DisposableKSP())
            using (var config   = new FakeConfiguration(ksp.KSP, ksp.KSP.Name))
            using (var manager  = new GameInstanceManager(nullUser, config)
                {
                    CurrentInstance = ksp.KSP
                })
            using (var regMgr   = RegistryManager.Instance(ksp.KSP, repoData.Manager,
                                                           new Repository[] { repo.repo }))
            {
                string mod_file_path = Path.Combine(ksp.KSP.game.PrimaryModDirectory(ksp.KSP), mod_file_name);

                // Install the test mod.
                var registry = regMgr.registry;
                registry.RepositoriesClear();
                registry.RepositoriesAdd(repo.repo);
                manager.Cache?.Store(TestData.DogeCoinFlag_101_module(),
                                     TestData.DogeCoinFlagZip(),
                                     new Progress<long>(bytes => {}));

                var modules = new List<CkanModule> { TestData.DogeCoinFlag_101_module() };

                HashSet<string>? possibleConfigOnlyDirs = null;
                new ModuleInstaller(manager.CurrentInstance, manager.Cache!, config, nullUser)
                    .InstallList(modules, new RelationshipResolverOptions(ksp.KSP.StabilityToleranceConfig),
                                 regMgr, ref possibleConfigOnlyDirs);

                // Check that the module is installed.
                Assert.IsTrue(File.Exists(mod_file_path));

                // Attempt to uninstall it.
                new ModuleInstaller(manager.CurrentInstance, manager.Cache!, config, nullUser)
                    .UninstallList(modules.Select(m => m.identifier),
                                   ref possibleConfigOnlyDirs, regMgr);

                // Check that the module is not installed.
                Assert.IsFalse(File.Exists(mod_file_path));
            }
        }

        [Test]
        public void UninstallEmptyDirs()
        {
            const string emptyFolderName = "DogeCoinFlag";

            using (var repo     = new TemporaryRepository(TestData.DogeCoinFlag_101(),
                                                      TestData.DogeCoinPlugin()))
            using (var repoData = new TemporaryRepositoryData(nullUser, repo.repo))
            // Create a new disposable KSP instance to run the test on.
            using (var ksp      = new DisposableKSP())
            using (var config   = new FakeConfiguration(ksp.KSP, ksp.KSP.Name))
            using (var manager  = new GameInstanceManager(new NullUser(), config)
                {
                    CurrentInstance = ksp.KSP
                })
            using (var regMgr   = RegistryManager.Instance(ksp.KSP, repoData.Manager,
                                                           new Repository[] { repo.repo }))
            {
                string directoryPath = Path.Combine(ksp.KSP.game.PrimaryModDirectory(ksp.KSP), emptyFolderName);

                // Install the base test mod.
                var registry = regMgr.registry;
                registry.RepositoriesClear();
                registry.RepositoriesAdd(repo.repo);
                manager.Cache?.Store(TestData.DogeCoinFlag_101_module(),
                                     TestData.DogeCoinFlagZip(),
                                     new Progress<long>(bytes => {}));

                var modules = new List<CkanModule> { TestData.DogeCoinFlag_101_module() };

                HashSet<string>? possibleConfigOnlyDirs = null;
                new ModuleInstaller(manager.CurrentInstance, manager.Cache!, config, nullUser)
                    .InstallList(modules,
                                 new RelationshipResolverOptions(ksp.KSP.StabilityToleranceConfig),
                                 regMgr, ref possibleConfigOnlyDirs);

                modules.Clear();

                // Install the plugin test mod.
                manager.Cache?.Store(TestData.DogeCoinPlugin_module(),
                                     TestData.DogeCoinPluginZip(),
                                     new Progress<long>(bytes => {}));

                modules.Add(TestData.DogeCoinPlugin_module());

                new ModuleInstaller(manager.CurrentInstance, manager.Cache!, config, nullUser)
                    .InstallList(modules,
                                 new RelationshipResolverOptions(ksp.KSP.StabilityToleranceConfig),
                                 regMgr, ref possibleConfigOnlyDirs);

                modules.Clear();

                // Check that the directory is installed.
                Assert.IsTrue(Directory.Exists(directoryPath));

                // Uninstall both mods.

                modules.Add(TestData.DogeCoinFlag_101_module());
                modules.Add(TestData.DogeCoinPlugin_module());

                new ModuleInstaller(manager.CurrentInstance, manager.Cache!, config, nullUser)
                    .UninstallList(modules.Select(m => m.identifier),
                                   ref possibleConfigOnlyDirs, regMgr);

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
            using (var inst     = new DisposableKSP())
            using (var repoData = new TemporaryRepositoryData(nullUser))
            {
                var game     = new KerbalSpaceProgram();
                var registry = RegistryManager.Instance(inst.KSP, repoData.Manager).registry;
                // Make files to be registered to another mod
                var absFiles = registeredFiles.Select(inst.KSP.ToAbsoluteGameDir)
                                              .ToList();
                foreach (var absPath in absFiles)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(absPath)!);
                    File.Create(absPath).Dispose();
                }
                // Register the other mod
                registry.RegisterModule(CkanModule.FromJson(@"{
                                            ""spec_version"": 1,
                                            ""identifier"":   ""otherMod"",
                                            ""author"":       ""otherModder"",
                                            ""version"":      ""1.0"",
                                            ""download"":     ""https://github.com/""
                                        }"),
                                        absFiles, inst.KSP, false);

                // Act
                ModuleInstaller.GroupFilesByRemovable(relRoot,
                                                      registry,
                                                      Array.Empty<string>(),
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
            const string mod_file_name = "DogeCoinFlag/Flags/dogecoin.png";

            for (int i = 0; i < 5; i++)
            {
                // Create a new disposable KSP instance to run the test on.
                using (var repo     = new TemporaryRepository(TestData.DogeCoinFlag_101()))
                using (var repoData = new TemporaryRepositoryData(nullUser, repo.repo))
                using (var ksp      = new DisposableKSP())
                using (var config   = new FakeConfiguration(ksp.KSP, ksp.KSP.Name))
                using (var manager  = new GameInstanceManager(nullUser, config)
                    {
                        CurrentInstance = ksp.KSP
                    })
                using (var regMgr   = RegistryManager.Instance(ksp.KSP, repoData.Manager,
                                                               new Repository[] { repo.repo }))
                {
                    var registry = regMgr.registry;
                    registry.RepositoriesClear();
                    registry.RepositoriesAdd(repo.repo);

                    // Copy the zip file to the cache directory.
                    manager.Cache?.Store(TestData.DogeCoinFlag_101_module(),
                                         TestData.DogeCoinFlagZip(),
                                         new Progress<long>(bytes => {}));

                    // Attempt to install it.
                    var modules = new List<CkanModule> { TestData.DogeCoinFlag_101_module() };

                    HashSet<string>? possibleConfigOnlyDirs = null;
                    new ModuleInstaller(ksp.KSP, manager.Cache!, config, nullUser)
                        .InstallList(modules,
                                     new RelationshipResolverOptions(ksp.KSP.StabilityToleranceConfig),
                                     regMgr, ref possibleConfigOnlyDirs);

                    // Check that the module is installed.
                    Assert.IsTrue(File.Exists(Path.Combine(ksp.KSP.game.PrimaryModDirectory(ksp.KSP),
                                                           mod_file_name)));
                }
            }
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
                ""author"": ""AwesomeModder"",
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
                results = mod.install!.First().FindInstallableFiles(zip, ksp.KSP);
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
                    ""author"": ""AwesomeModder"",
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
                var results = mod.install!.First().FindInstallableFiles(zip, ksp.KSP);

                Assert.AreEqual(
                    CKANPathUtils.NormalizePath(
                        Path.Combine(ksp.KSP.GameDir(), "saves/scenarios/AwesomeRace.sfs")),
                    results.First().destination);
            }
        }

        [Test,
         TestCase(new string[] {
                      @"{
                          ""identifier"": ""MyDLC"",
                          ""kind"": ""dlc""
                      }",
                      @"{
                          ""identifier"": ""InstallingMod"",
                          ""recommends"": [ { ""name"": ""MyDLC"" } ]
                      }",
                  },
                  new string[] { "InstallingMod" },
                  new string[] { "MyDLC" })
        ]
        public void FindRecommendations_WithDLCRecommendationsUnsatisfied_DLCRecommended(
                string[] availableModules,
                string[] installIdents,
                string[] dlcIdents)
        {
            // Arrange
            var user = new NullUser();
            var crit = new GameVersionCriteria(new GameVersion(1, 12, 5));
            using (var repo     = new TemporaryRepository(availableModules.Select(Relationships.RelationshipResolverTests.MergeWithDefaults)
                                                                          .ToArray()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            using (var inst     = new DisposableKSP())
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);

                // Act
                var result = ModuleInstaller.FindRecommendations(inst.KSP,
                                                                 installIdents.Select(ident => registry.LatestAvailable(ident, ksp.KSP.StabilityToleranceConfig, crit))
                                                                              .OfType<CkanModule>()
                                                                              .ToHashSet(),
                                                                 new List<CkanModule>(),
                                                                 registry,
                                                                 out Dictionary<CkanModule, Tuple<bool, List<string>>> recommendations,
                                                                 out Dictionary<CkanModule, List<string>> suggestions,
                                                                 out Dictionary<CkanModule, HashSet<string>> supporters);

                // Assert
                Assert.IsTrue(result, "Should return something");
                CollectionAssert.IsNotEmpty(recommendations, "Should return recommendations");
                CollectionAssert.AreEquivalent(dlcIdents.Select(ident => registry.LatestAvailable(ident, ksp.KSP.StabilityToleranceConfig, crit)),
                                               recommendations.Keys,
                                               "The DLC should be recommended");
            }
        }

        [Test,
         TestCase(new string[] {
                      @"{
                          ""identifier"": ""MyDLC"",
                          ""kind"": ""dlc""
                      }",
                      @"{
                          ""identifier"": ""InstallingMod"",
                          ""recommends"": [ { ""name"": ""MyDLC"" } ]
                      }",
                  },
                  new string[] { "InstallingMod" },
                  new string[] { "MyDLC" })
        ]
        public void FindRecommendations_WithDLCRecommendationsSatisfied_DLCNotRecommended(
                string[] availableModules,
                string[] installIdents,
                string[] dlcIdents)
        {
            // Arrange
            var user = new NullUser();
            var crit = new GameVersionCriteria(new GameVersion(1, 12, 5));
            using (var repo     = new TemporaryRepository(availableModules.Select(Relationships.RelationshipResolverTests.MergeWithDefaults)
                                                                          .ToArray()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            using (var inst     = new DisposableKSP())
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                registry.SetDlcs(dlcIdents.ToDictionary(ident => ident,
                                                        ident => new UnmanagedModuleVersion("1.0.0")));

                // Act
                var result = ModuleInstaller.FindRecommendations(inst.KSP,
                                                                 installIdents.Select(ident => registry.LatestAvailable(ident, ksp.KSP.StabilityToleranceConfig, crit))
                                                                              .OfType<CkanModule>()
                                                                              .ToHashSet(),
                                                                 new List<CkanModule>(),
                                                                 registry,
                                                                 out Dictionary<CkanModule, Tuple<bool, List<string>>> recommendations,
                                                                 out Dictionary<CkanModule, List<string>> suggestions,
                                                                 out Dictionary<CkanModule, HashSet<string>> supporters);

                // Assert
                Assert.IsFalse(result, "Should return nothing");
                foreach (var mod in dlcIdents.Select(ident => registry.LatestAvailable(ident, ksp.KSP.StabilityToleranceConfig, crit)))
                {
                    CollectionAssert.DoesNotContain(recommendations, mod,
                                                    "DLC should not be recommended");
                }
            }
        }

        [Test]
        public void InstallList_RealZipSlip_Throws()
        {
            // Arrange
            using (var repo     = new TemporaryRepository(TestData.DogeCoinFlag_101ZipSlip()))
            using (var repoData = new TemporaryRepositoryData(nullUser, repo.repo))
            using (var ksp      = new DisposableKSP())
            using (var config   = new FakeConfiguration(ksp.KSP, ksp.KSP.Name))
            using (var manager  = new GameInstanceManager(nullUser, config)
                {
                    CurrentInstance = ksp.KSP
                })
            using (var regMgr   = RegistryManager.Instance(ksp.KSP, repoData.Manager,
                                                           new Repository[] { repo.repo }))
            {
                var registry = regMgr.registry;
                registry.RepositoriesClear();
                registry.RepositoriesAdd(repo.repo);

                // Copy the zip file to the cache directory.
                manager.Cache?.Store(TestData.DogeCoinFlag_101ZipSlip_module(),
                                     TestData.DogeCoinFlagZipSlipZip(),
                                     new Progress<long>(bytes => {}));

                // Attempt to install it.
                var modules = new List<CkanModule> { TestData.DogeCoinFlag_101ZipSlip_module() };

                // Act / Assert
                Assert.Throws<BadInstallLocationKraken>(
                    delegate
                    {
                        HashSet<string>? possibleConfigOnlyDirs = null;
                        new ModuleInstaller(ksp.KSP, manager.Cache!, config, nullUser)
                            .InstallList(modules,
                                         new RelationshipResolverOptions(ksp.KSP.StabilityToleranceConfig),
                                         regMgr, ref possibleConfigOnlyDirs);
                    },
                    "Kraken should be thrown if ZIP file attempts to exploit Zip Slip vulnerability");
            }
        }

        [Test]
        public void InstallList_RealZipBomb_DoesNotThrow()
        {
            // Arrange
            using (var repo     = new TemporaryRepository(TestData.DogeCoinFlag_101ZipBomb()))
            using (var repoData = new TemporaryRepositoryData(nullUser, repo.repo))
            using (var ksp      = new DisposableKSP())
            using (var config   = new FakeConfiguration(ksp.KSP, ksp.KSP.Name))
            using (var manager  = new GameInstanceManager(nullUser, config)
                {
                    CurrentInstance = ksp.KSP
                })
            using (var regMgr   = RegistryManager.Instance(ksp.KSP, repoData.Manager,
                                                           new Repository[] { repo.repo }))
            {
                var registry = regMgr.registry;
                registry.RepositoriesClear();
                registry.RepositoriesAdd(repo.repo);

                // Copy the zip file to the cache directory.
                manager.Cache?.Store(TestData.DogeCoinFlag_101ZipBomb_module(),
                                     TestData.DogeCoinFlagZipBombZip(),
                                     new Progress<long>(bytes => {}));

                // Attempt to install it.
                var modules = new List<CkanModule> { TestData.DogeCoinFlag_101ZipBomb_module() };

                // Act / Assert
                HashSet<string>? possibleConfigOnlyDirs = null;
                new ModuleInstaller(ksp.KSP, manager.Cache!, config, nullUser)
                    .InstallList(modules,
                                 new RelationshipResolverOptions(ksp.KSP.StabilityToleranceConfig),
                                 regMgr, ref possibleConfigOnlyDirs);
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
                    ""author"":       ""AwesomeModder"",
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
                    ""author"": ""AwesomeModder"",
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
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager  = new GameInstanceManager(nullUser, config)
                {
                    CurrentInstance = inst.KSP
                })
            using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                           new Repository[] { repo.repo }))
            {
                var registry = regMgr.registry;
                IRegistryQuerier querier = registry;
                var replaced = registry.GetModuleByVersion("replaced", "1.0")!;
                Assert.IsNotNull(replaced, "Replaced module should exist");
                var replacer = registry.GetModuleByVersion("replacer", "1.0");
                Assert.IsNotNull(replacer, "Replacer module should exist");
                var installer = new ModuleInstaller(inst.KSP, manager.Cache!, config, nullUser);
                HashSet<string>? possibleConfigOnlyDirs = null;
                var downloader = new NetAsyncModulesDownloader(nullUser, manager.Cache!);

                // Act
                registry.RegisterModule(replaced, new List<string>(), inst.KSP, false);
                manager.Cache?.Store(replaced, TestData.DogeCoinFlagZip(), new Progress<long>(bytes => {}));
                var replacement = querier.GetReplacement(replaced.identifier, ksp.KSP.StabilityToleranceConfig,
                                                         new GameVersionCriteria(new GameVersion(1, 12)))!;
                installer.Replace(Enumerable.Repeat(replacement, 1),
                                  new RelationshipResolverOptions(ksp.KSP.StabilityToleranceConfig),
                                  downloader, ref possibleConfigOnlyDirs, regMgr, null,
                                  false);

                // Assert
                CollectionAssert.AreEqual(
                    Enumerable.Repeat(replacer, 1),
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
                    ""author"":       ""AwesomeModder"",
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
                    ""author"":       ""AwesomeModder"",
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
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager  = new GameInstanceManager(nullUser, config)
                {
                    CurrentInstance = inst.KSP
                })
            using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                           new Repository[] { repo.repo }))
            {
                var registry = regMgr.registry;
                IRegistryQuerier querier = registry;
                var replaced = registry.GetModuleByVersion("replaced", "1.0")!;
                Assert.IsNotNull(replaced, "Replaced module should exist");
                var replacer = registry.GetModuleByVersion("replacer", "1.0");
                Assert.IsNotNull(replacer, "Replacer module should exist");
                var installer = new ModuleInstaller(inst.KSP, manager.Cache!, config, nullUser);
                var downloader = new NetAsyncModulesDownloader(nullUser, manager.Cache!);

                // Act
                registry.RegisterModule(replaced, new List<string>(), inst.KSP, false);
                manager.Cache?.Store(replaced, TestData.DogeCoinFlagZip(), new Progress<long>(bytes => {}));
                var replacement = querier.GetReplacement(replaced.identifier, ksp.KSP.StabilityToleranceConfig,
                                                         new GameVersionCriteria(new GameVersion(1, 11)));

                // Assert
                Assert.IsNull(replacement);
                CollectionAssert.AreEqual(
                    Enumerable.Repeat(replaced, 1),
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
                            ""author"":       ""AwesomeModder"",
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
                            ""author"":       ""AwesomeModder"",
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
            using (var repo     = new TemporaryRepository(regularMods.Concat(autoInstMods).ToArray()))
            using (var repoData = new TemporaryRepositoryData(nullUser, repo.repo))
            using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                           new Repository[] { repo.repo }))
            {
                var installer = new ModuleInstaller(inst.KSP, manager.Cache!, config, nullUser);
                var registry  = regMgr.registry;
                var possibleConfigOnlyDirs = new HashSet<string>();
                foreach (var m in regularMods)
                {
                    registry.RegisterModule(CkanModule.FromJson(m),
                                            new List<string>(),
                                            inst.KSP,
                                            false);
                }
                foreach (var m in autoInstMods)
                {
                    registry.RegisterModule(CkanModule.FromJson(m),
                                            new List<string>(),
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
                            ""author"":       ""AwesomeModder"",
                            ""version"":      ""1.0"",
                            ""depends"": [
                                { ""name"": ""RemovableMod"" }
                            ],
                            ""download"":     ""https://github.com/""
                         }",
                         @"{
                            ""spec_version"": 1,
                            ""identifier"":   ""DogeCoinFlag"",
                            ""author"":       ""AwesomeModder"",
                            ""version"":      ""2.0"",
                            ""download"":     ""https://awesomemod.example/AwesomeMod.zip"",
                         }",
                     },
                     new string[]
                     {
                         @"{
                            ""spec_version"": 1,
                            ""identifier"":   ""RemovableMod"",
                            ""author"":       ""AwesomeModder"",
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
            using (var repo     = new TemporaryRepository(regularMods.Concat(autoInstMods).ToArray()))
            using (var repoData = new TemporaryRepositoryData(nullUser, repo.repo))
            using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                           new Repository[] { repo.repo }))
            {
                var installer  = new ModuleInstaller(inst.KSP, manager.Cache!, config, nullUser);
                var downloader = new NetAsyncModulesDownloader(nullUser, manager.Cache!);
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
                    manager.Cache?.Store(module,
                                         TestData.DogeCoinFlagZip(),
                                         new Progress<long>(bytes => {}));
                    if (!querier.IsInstalled(module.identifier, false))
                    {
                        registry.RegisterModule(module,
                                                new List<string>(),
                                                inst.KSP,
                                                false);
                    }
                }
                foreach (var m in autoInstMods)
                {
                    registry.RegisterModule(CkanModule.FromJson(m),
                                            new List<string>(),
                                            inst.KSP,
                                            true);
                }

                // Act
                installer.Upgrade(upgradeIdentifiers.Select(ident =>
                                      registry.LatestAvailable(ident, ksp.KSP.StabilityToleranceConfig, inst.KSP.VersionCriteria()))
                                                    .OfType<CkanModule>()
                                                    .ToArray(),
                                  downloader, ref possibleConfigOnlyDirs, regMgr, null, false);

                // Assert
                CollectionAssert.AreEquivalent(correctRemainingIdentifiers,
                                               registry.InstalledModules.Select(im => im.identifier).ToArray());
            }
        }

        [TestCase]
        public void Install_WithMatchedUnmanagedDll_Throws()
        {
            const string unmanaged = "GameData/DogeCoinPlugin.1.0.0.dll";
            var kraken = Assert.Throws<DllLocationMismatchKraken>(() =>
                installTestPlugin(unmanaged,
                                  TestData.DogeCoinPlugin(),
                                  TestData.DogeCoinPluginZip()));
            Assert.AreEqual(unmanaged, kraken?.path);
        }

        [TestCase]
        public void Install_WithUnmatchedUnmanagedDll_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => installTestPlugin("GameData/DogeCoinPlugin-1-0-0.dll",
                                                        TestData.DogeCoinPlugin(),
                                                        TestData.DogeCoinPluginZip()),
                                "Unmanaged file must match identifier");
            Assert.DoesNotThrow(() => installTestPlugin("GameData/DogeCoinPlugin.dll",
                                                        TestData.DogeCoinPluginAddonFerram(),
                                                        TestData.DogeCoinPluginAddonFerramZip()),
                                "Managed file being installed must match identifier");
        }

        private void installTestPlugin(string unmanaged, string moduleJson, string zipPath)
        {
            // Arrange
            using (var repo     = new TemporaryRepository(moduleJson))
            using (var repoData = new TemporaryRepositoryData(nullUser, repo.repo))
            using (var inst     = new DisposableKSP())
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager  = new GameInstanceManager(nullUser, config)
                {
                    CurrentInstance = inst.KSP
                })
            using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                           new Repository[] { repo.repo }))
            {
                var module    = CkanModule.FromJson(moduleJson);
                var modules   = new List<CkanModule> { module };
                var installer = new ModuleInstaller(inst.KSP, manager.Cache!, config, nullUser);
                File.WriteAllText(inst.KSP.ToAbsoluteGameDir(unmanaged),
                                  "Not really a DLL, are we?");
                regMgr.ScanUnmanagedFiles();
                manager.Cache?.Store(module, zipPath, new Progress<long>(bytes => {}));

                // Act
                HashSet<string>? possibleConfigOnlyDirs = null;
                new ModuleInstaller(inst.KSP, manager.Cache!, config, nullUser)
                    .InstallList(modules,
                                 new RelationshipResolverOptions(ksp.KSP.StabilityToleranceConfig),
                                 regMgr,
                                 ref possibleConfigOnlyDirs);
            }
        }

    }
}
