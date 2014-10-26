using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using CKAN;

namespace CKANTests
{
    [TestFixture()]
    public class ModuleInstaller
    {
        [Test()]
        public void GenerateDefaultInstall()
        {
            string filename = Tests.TestData.DogeCoinFlagZip();
            var zipfile = new ZipFile(File.OpenRead(filename));

            ModuleInstallDescriptor stanza = CKAN.ModuleInstaller.GenerateDefaultInstall("DogeCoinFlag", zipfile);

            TestDogeCoinStanza(stanza);

            // Same again, but screwing up the case (we see this *all the time*)
            ModuleInstallDescriptor stanza2 = CKAN.ModuleInstaller.GenerateDefaultInstall("DogecoinFlag", zipfile);

            TestDogeCoinStanza(stanza2);

            // Now what happens if we can't find what to install?

            Assert.Throws<FileNotFoundKraken>(delegate {
                CKAN.ModuleInstaller.GenerateDefaultInstall("Xyzzy", zipfile);
            });

            // Make sure the FNFKraken looks like what we expect.
            try
            {
                CKAN.ModuleInstaller.GenerateDefaultInstall("Xyzzy",zipfile);
            }
            catch (FileNotFoundKraken kraken)
            {
                Assert.AreEqual("Xyzzy", kraken.file);
            }
        }

        [Test()]
        public void FindInstallableFiles()
        {
            string dogezip = Tests.TestData.DogeCoinFlagZip();
            CkanModule dogemod = Tests.TestData.DogeCoinFlag_101_module();

            Console.WriteLine("{0}", dogezip);

            List<InstallableFile> contents = CKAN.ModuleInstaller.FindInstallableFiles(dogemod, dogezip, null);

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
            }

            // TODO: Ensure it's got a file we expect.
        }

        private void TestDogeCoinStanza(ModuleInstallDescriptor stanza)
        {
            Assert.AreEqual("GameData", stanza.install_to);
            Assert.AreEqual("DogeCoinFlag-1.01/GameData/DogeCoinFlag",stanza.file);
        }

    }


}

