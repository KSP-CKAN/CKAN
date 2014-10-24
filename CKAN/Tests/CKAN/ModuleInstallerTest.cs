using NUnit.Framework;
using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using CKAN;

namespace CKANTests
{
    [TestFixture()]
    public class ModuleInstallerTest
    {
        [Test()]
        public void GenerateDefaultInstall()
        {
            string filename = Tests.TestData.DogeCoinFlagZip();
            var zipfile = new ZipFile(File.OpenRead(filename));

            ModuleInstallDescriptor stanza = ModuleInstaller.GenerateDefaultInstall("DogeCoinFlag", zipfile);

            TestDogeCoinStanza(stanza);

            // Same again, but screwing up the case (we see this *all the time*)
            ModuleInstallDescriptor stanza2 = ModuleInstaller.GenerateDefaultInstall("DogecoinFlag", zipfile);

            TestDogeCoinStanza(stanza2);

            // Now what happens if we can't find what to install?

            Assert.Throws<FileNotFoundKraken>(delegate {
                ModuleInstaller.GenerateDefaultInstall("Xyzzy", zipfile);
            });
        }

        private void TestDogeCoinStanza(ModuleInstallDescriptor stanza)
        {
            Assert.AreEqual("GameData", stanza.install_to);
            Assert.AreEqual("DogeCoinFlag-1.01/GameData/DogeCoinFlag",stanza.file);
        }

    }


}

