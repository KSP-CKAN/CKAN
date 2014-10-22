using NUnit.Framework;
using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using CKAN;

namespace Tests
{
    [TestFixture()]
    public class ModuleInstallerTest
    {
        [Test()]
        public void GenerateDefaultInstall()
        {
            string filename = DogeCoinFlagZip();
            var zipfile = new ZipFile(File.OpenRead(filename));

            ModuleInstallDescriptor stanza = ModuleInstaller.GenerateDefaultInstall("DogeCoinFlag", zipfile);

            TestDogeCoinStanza(stanza);

            // Same again, but screwing up the case (we see this *all the time*)
            ModuleInstallDescriptor stanza2 = ModuleInstaller.GenerateDefaultInstall("DogecoinFlag", zipfile);

            TestDogeCoinStanza(stanza2);

            // TODO: Now what happens if we can't find what to install?
        }

        private void TestDogeCoinStanza(ModuleInstallDescriptor stanza)
        {
            Assert.AreEqual("GameData", stanza.install_to);
            Assert.AreEqual("DogeCoinFlag-1.01/GameData/DogeCoinFlag",stanza.file);
        }

        // TODO: This would be better if it walked upwards until it
        // found a t/data/DogeCoinFlag-1.01.zip file.

        public static string DogeCoinFlagZip() {
            string current = System.IO.Directory.GetCurrentDirectory();

            string such_zip_very_currency_wow = Path.Combine(current, "../../../../t/data/DogeCoinFlag-1.01.zip");

            return such_zip_very_currency_wow;
        }
    }


}

