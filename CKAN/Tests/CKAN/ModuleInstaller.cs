using NUnit.Framework;
using System;
using System.IO;
using System.Transactions;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using CKAN;

namespace CKANTests
{
    [TestFixture()]
    public class ModuleInstaller
    {
        private static readonly string flag_path = "DogeCoinFlag-1.01/GameData/DogeCoinFlag/Flags/dogecoin.png";

        [Test()]
        public void GenerateDefaultInstall()
        {
            string filename = Tests.TestData.DogeCoinFlagZip();
            using (var zipfile = new ZipFile(filename))
            {
                CKAN.ModuleInstallDescriptor stanza = CKAN.ModuleInstaller.GenerateDefaultInstall("DogeCoinFlag", zipfile);

                TestDogeCoinStanza(stanza);

                // Same again, but screwing up the case (we see this *all the time*)
                CKAN.ModuleInstallDescriptor stanza2 = CKAN.ModuleInstaller.GenerateDefaultInstall("DogecoinFlag", zipfile);

                TestDogeCoinStanza(stanza2);

                // Now what happens if we can't find what to install?

                Assert.Throws<FileNotFoundKraken>(delegate
                {
                    CKAN.ModuleInstaller.GenerateDefaultInstall("Xyzzy", zipfile);
                });

                // Make sure the FNFKraken looks like what we expect.
                try
                {
                    CKAN.ModuleInstaller.GenerateDefaultInstall("Xyzzy", zipfile);
                }
                catch (FileNotFoundKraken kraken)
                {
                    Assert.AreEqual("Xyzzy", kraken.file);
                }
            }
        }

        [Test()]
        public void FindInstallableFiles()
        {
            string dogezip = Tests.TestData.DogeCoinFlagZip();
            CkanModule dogemod = Tests.TestData.DogeCoinFlag_101_module();

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

        [Test()]
        // Make sure all our filters work.
        public void FindInstallableFIilesWithFilter()
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

        [Test()]
        public void No_Installable_Files()
        {
            // This tests GH #93

            string dogezip = Tests.TestData.DogeCoinFlagZip();
            CkanModule bugged_mod = Tests.TestData.DogeCoinFlag_101_bugged_module();

            Assert.Throws<BadMetadataKraken>(delegate {
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

        [Test()]
        // GH #205, make sure we write in *binary*, not text.
        public void BinaryNotText_205()
        {
            // Use CopyZipEntry (via CopyDogeFromZip) and make sure it
            // comes out the right size.
            string tmpfile = CopyDogeFromZip();
            long size = new System.IO.FileInfo(tmpfile).Length;

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

        [Test()]
        // Make sure when we roll-back a transaction, files written with CopyZipEntry go
        // back to their pre-transaction state.
        public void FileSysRollBack()
        {
            string file;

            using (var scope = new TransactionScope())
            {
                file = CopyDogeFromZip();
                Assert.IsTrue(new System.IO.FileInfo(file).Length > 0);
                scope.Dispose(); // Rollback
            }

            // And now, our file should be gone!
            Assert.IsFalse(File.Exists (file));
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

		[Test()]
		// Test how we handle corrupt data
		public void CorruptZip_242()
		{
			string corrupt_dogezip = Tests.TestData.DogeCoinFlagZipCorrupt();

			// GenerateDefault Install
			using (var zipfile = new ZipFile(corrupt_dogezip))
			{
				CKAN.ModuleInstaller.GenerateDefaultInstall("DogeCoinFlag", zipfile);
			}

			// FindInstallableFiles
			CkanModule dogemod = Tests.TestData.DogeCoinFlag_101_module();
			CKAN.ModuleInstaller.FindInstallableFiles(dogemod, corrupt_dogezip, null);

		}

        private static string CopyDogeFromZip()
        {
            string dogezip = Tests.TestData.DogeCoinFlagZip();
            ZipFile zipfile = new ZipFile(dogezip);

            ZipEntry entry = zipfile.GetEntry(flag_path);
            string tmpfile = Path.GetTempFileName();

            // We have to delete our temporary file, as CZE refuses to overwrite; huzzah!
            File.Delete (tmpfile);
            CKAN.ModuleInstaller.CopyZipEntry(zipfile, entry, tmpfile, false);

            return tmpfile;
        }

        private void TestDogeCoinStanza(CKAN.ModuleInstallDescriptor stanza)
        {
            Assert.AreEqual("GameData", stanza.install_to);
            Assert.AreEqual("DogeCoinFlag-1.01/GameData/DogeCoinFlag",stanza.file);
        }

    }


}

