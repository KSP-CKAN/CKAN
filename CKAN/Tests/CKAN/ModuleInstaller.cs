using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;
using CKAN;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;

namespace CKANTests
{
    [TestFixture]
    public class ModuleInstaller
    {
        private string flag_path;
        private string dogezip;
        private CkanModule dogemod;

        private string mm_zip;
        private CkanModule mm_mod;

        [SetUp]
        public void Setup()
        {
            // By setting these for every test, we can make sure our tests can change
            // them any way they like without harming other tests.

            flag_path = "DogeCoinFlag-1.01/GameData/DogeCoinFlag/Flags/dogecoin.png";
            dogezip = Tests.TestData.DogeCoinFlagZip();
            dogemod = Tests.TestData.DogeCoinFlag_101_module();

            mm_zip = Tests.TestData.ModuleManagerZip();
            mm_mod = Tests.TestData.ModuleManagerModule();
        }

        [Test]
        public void GenerateDefaultInstall()
        {
            string filename = Tests.TestData.DogeCoinFlagZip();
            using (var zipfile = new ZipFile(filename))
            {
                CKAN.ModuleInstallDescriptor stanza = CKAN.ModuleInstallDescriptor.DefaultInstallStanza("DogeCoinFlag", zipfile);

                TestDogeCoinStanza(stanza);

                // Same again, but screwing up the case (we see this *all the time*)
                CKAN.ModuleInstallDescriptor stanza2 = CKAN.ModuleInstallDescriptor.DefaultInstallStanza("DogecoinFlag", zipfile);

                TestDogeCoinStanza(stanza2);

                // Now what happens if we can't find what to install?

                Assert.Throws<FileNotFoundKraken>(delegate
                {
                    CKAN.ModuleInstallDescriptor.DefaultInstallStanza("Xyzzy", zipfile);
                });

                // Make sure the FNFKraken looks like what we expect.
                try
                {
                    CKAN.ModuleInstallDescriptor.DefaultInstallStanza("Xyzzy", zipfile);
                }
                catch (FileNotFoundKraken kraken)
                {
                    Assert.AreEqual("Xyzzy", kraken.file);
                }
            }
        }

        [Test]
        public void FindInstallableFiles()
        {
            List<InstallableFile> contents = CKAN.ModuleInstaller.FindInstallableFiles(dogemod, dogezip, null);
            List<string> filenames = new List<string>();

            Assert.IsNotNull(contents);

            // Make sure it's actually got files!
            Assert.IsTrue(contents.Count > 0);

            foreach (var file in contents)
            {
                // Make sure the destination paths are null, because we supplied no KSP instance.
                Assert.IsNull(file.destination);

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
        public void FindInstallableFilesWithKSP()
        {
            using (var tidy = new Tests.DisposableKSP())
            {
                List<InstallableFile> contents = CKAN.ModuleInstaller.FindInstallableFiles(dogemod, dogezip, tidy.KSP);

                // See if we can find an expected estination path in the right place.
                string file = contents
                    .Select(x => x.destination).FirstOrDefault(
                    x => Regex.IsMatch(x, "GameData/DogeCoinFlag/Flags/dogecoin\\.png$"));

                Assert.IsNotNull(file);
            }
        }

#pragma warning disable 0414

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

#pragma warning restore 0414

        [Test]
        [TestCaseSource("SuchPaths")]
        public void FindInstallbleFilesWithBonusPath(string path)
        {
            dogemod.install[0].install_to = path;
            using (var tidy = new Tests.DisposableKSP())
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
            using (var tidy = new Tests.DisposableKSP())
            {
                List<InstallableFile> contents = CKAN.ModuleInstaller.FindInstallableFiles(mm_mod, mm_zip, tidy.KSP);

                string file = contents
                    .Select(x => x.destination).FirstOrDefault(
                    x => Regex.IsMatch(x, @"ModuleManager\.2\.5\.1\.dll$"));

                Assert.IsNotNull(file, "ModuleManager install");
            }
        }

        [Test]
        // Make sure all our filters work.
        public void FindInstallableFilesWithFilter()
        {
            string extra_doge = Tests.TestData.DogeCoinFlagZipWithExtras();
            CkanModule dogemod = Tests.TestData.DogeCoinFlag_101_module();

            List<InstallableFile> contents = CKAN.ModuleInstaller.FindInstallableFiles(dogemod, extra_doge, null);

            var files = contents.Select(x => x.source.Name);

            Assert.IsTrue(files.Contains("DogeCoinFlag-1.01/GameData/DogeCoinFlag/Flags/dogecoin.png"), "dogecoin.png");
            Assert.IsFalse(files.Contains("DogeCoinFlag-1.01/GameData/DogeCoinFlag/README.md"), "Filtered README 1");
            Assert.IsFalse(files.Contains("DogeCoinFlag-1.01/GameData/DogeCoinFlag/Flags/README.md"), "Filtered README 2");
            Assert.IsFalse(files.Contains("DogeCoinFlag-1.01/GameData/DogeCoinFlag/notes.txt.bak"), "Filtered .bak file");
        }

        [Test]
        public void No_Installable_Files()
        {
            // This tests GH #93

            string dogezip = Tests.TestData.DogeCoinFlagZip();
            CkanModule bugged_mod = Tests.TestData.DogeCoinFlag_101_bugged_module();

            Assert.Throws<BadMetadataKraken>(delegate
            {
                CKAN.ModuleInstaller.FindInstallableFiles(bugged_mod, dogezip, null);
            });

            try
            {
                CKAN.ModuleInstaller.FindInstallableFiles(bugged_mod, dogezip, null);
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
                CKAN.ModuleInstaller.FindInstallableFiles(dogemod, dogezip, null);
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
            using (ZipFile zipfile = new ZipFile(Tests.TestData.DogeCoinFlagZip()))
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
            string corrupt_dogezip = Tests.TestData.DogeCoinFlagZipCorrupt();

            using (var zipfile = new ZipFile(corrupt_dogezip))
            {
                // GenerateDefault Install
                CKAN.ModuleInstallDescriptor.DefaultInstallStanza("DogeCoinFlag", zipfile);

                // FindInstallableFiles
                CkanModule dogemod = Tests.TestData.DogeCoinFlag_101_module();
                CKAN.ModuleInstaller.FindInstallableFiles(dogemod, corrupt_dogezip, null);
            }
        }

        [Test]
        public void TransformOutputName()
        {
            Assert.AreEqual("GameData/kOS/Plugins/kOS.dll", CKAN.ModuleInstaller.TransformOutputName("GameData/kOS", "GameData/kOS/Plugins/kOS.dll", "GameData"));
            Assert.AreEqual("GameData/kOS/Plugins/kOS.dll", CKAN.ModuleInstaller.TransformOutputName("kOS-1.1/GameData/kOS", "kOS-1.1/GameData/kOS/Plugins/kOS.dll", "GameData"));
            Assert.AreEqual("GameData/ModuleManager.2.5.1.dll", CKAN.ModuleInstaller.TransformOutputName("ModuleManager.2.5.1.dll", "ModuleManager.2.5.1.dll", "GameData"));
            Assert.AreEqual("SomeDir/Ships/SPH/FAR Firehound.craft", CKAN.ModuleInstaller.TransformOutputName("Ships", "Ships/SPH/FAR Firehound.craft", "SomeDir/Ships"));
        }

        private string CopyDogeFromZip()
        {
            string dogezip = Tests.TestData.DogeCoinFlagZip();
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
            using (var tidy = new Tests.DisposableKSP())
            {
                CKAN.KSPManager manager = new CKAN.KSPManager(new NullUser(), new FakeWin32Registry(tidy.KSP)){CurrentInstance = tidy.KSP};

                Assert.Throws<ModNotInstalledKraken>(delegate
                {
                    // This should throw, as our tidy KSP has no mods installed.
                    CKAN.ModuleInstaller.GetInstance(manager.CurrentInstance, NullUser.User).UninstallList("Foo");
                });

                manager.CurrentInstance = null; // I weep even more.
            }
        }




        private void TestDogeCoinStanza(CKAN.ModuleInstallDescriptor stanza)
        {
            Assert.AreEqual("GameData", stanza.install_to);
            Assert.AreEqual("DogeCoinFlag-1.01/GameData/DogeCoinFlag", stanza.file);
        }

    }
}

