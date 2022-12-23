using System;
using System.Linq;
using CKAN;
using CKAN.Versioning;
using NUnit.Framework;
using Tests.Data;
using Newtonsoft.Json.Linq;

namespace Tests.Core.Types
{
    [TestFixture]
    public class CkanModuleTests
    {
        [Test]
        public void CompatibleWith()
        {
            CkanModule module = CkanModule.FromJson(TestData.kOS_014());

            Assert.IsTrue(module.IsCompatibleKSP(new GameVersionCriteria(GameVersion.Parse("0.24.2"))));
        }

        [Test]
        public void StandardName()
        {
            CkanModule module = CkanModule.FromJson(TestData.kOS_014());
            Assert.AreEqual("kOS-0.14.zip", module.StandardName());

            CkanModule module2 = CkanModule.FromJson(TestData.kOS_014_with_invalid_version_characters());
            Assert.AreEqual("kOS-0-14-0.zip", module2.StandardName());
        }

        [Test]
        public void MetaData()
        {
            CkanModule module = CkanModule.FromJson(TestData.kOS_014());

            Assert.AreEqual("kOS - Kerbal OS", module.name);
            Assert.AreEqual("kOS", module.identifier);
            Assert.AreEqual("A programming and automation environment for KSP craft.", module.@abstract);
            Assert.AreEqual("https://github.com/KSP-KOS/KOS/releases/download/v0.14/kOS.v14.zip", module.download.ToString());
            Assert.AreEqual("GPL-3.0", module.license.First().ToString());
            Assert.AreEqual("0.14", module.version.ToString());
            Assert.AreEqual("stable", module.release_status.ToString());
            Assert.AreEqual("0.24.2", module.ksp_version.ToString());

            Assert.That(module.install.First().file, Is.EqualTo("GameData/kOS"));
            Assert.That(module.install.First().install_to, Is.EqualTo("GameData"));

            Assert.AreEqual("http://forum.kerbalspaceprogram.com/threads/68089-0-23-kOS-Scriptable-Autopilot-System-v0-11-2-13", module.resources.homepage.ToString());
            Assert.AreEqual("https://github.com/KSP-KOS/KOS/issues", module.resources.bugtracker.ToString());
            Assert.AreEqual("https://github.com/KSP-KOS/KOS", module.resources.repository.ToString());

            Assert.AreEqual("C5A224AC4397770C0B19B4A6417F6C5052191608", module.download_hash.sha1.ToString());
            Assert.AreEqual("E0FB79C81D8FCDA8DB6E38B104106C3B7D078FDC06ACA2BC7834973B43D789CB", module.download_hash.sha256.ToString());
        }

        /// <summary>
        /// There's a condition where some mods won't download if the server is presented with
        /// an unescaped string, but *will* if passed an escaped string. This isn't the case with
        /// all servers and mods, but in any case we check that our original string is always
        /// available after Url-ification so we can use it.
        /// </summary>
        [Test]
        public void SpacesPreservedInDownload()
        {
            CkanModule module = CkanModule.FromJson(TestData.DogeCoinFlag_101());
            Assert.AreEqual("https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01", module.download.OriginalString);
        }

        [Test]
        public void FilterRead()
        {
            CkanModule module = CkanModule.FromJson(TestData.DogeCoinFlag_101());

            // Assert known things about this mod.
            Assert.IsNotNull(module.install[0].filter);
            Assert.IsNotNull(module.install[0].filter_regexp);

            Assert.AreEqual(2, module.install[0].filter.Count);
        }

        [Test]
        public void SpecCompareAssumptions()
        {
            // These are checks to make sure our assumptions regarding
            // spec versions hold.

            // The *old* CKAN spec had a version number of "1".
            // It should be accepted by any client with an old version number,
            // as well as any with a new version number.
            var old_spec = new ModuleVersion("1");
            var old_dev = new ModuleVersion("v0.23");
            var new_dev = new ModuleVersion("v1.2.3");

            Assert.IsTrue(old_dev.IsGreaterThan(old_spec));
            Assert.IsTrue(new_dev.IsGreaterThan(old_spec));

            // The new spec requires a minimum number (v1.2, v1.4)
            // Make sure our assumptions here hold, too.

            var readable_spec = new ModuleVersion("v1.2");
            var unreadable_spec = new ModuleVersion("v1.4");

            Assert.IsTrue(new_dev.IsGreaterThan(readable_spec));
            Assert.IsFalse(new_dev.IsGreaterThan(unreadable_spec));
        }

        [Test]
        public void IsSpecSupported()
        {
            // We should always support old versions, and the classic '1' version.
            Assert.IsTrue(CkanModule.IsSpecSupported(new ModuleVersion("1")));
            Assert.IsTrue(CkanModule.IsSpecSupported(new ModuleVersion("v0.02")));

            // We shouldn't support this far-in-the-future version.
            // NB: V2K bug!!!
            Assert.IsFalse(CkanModule.IsSpecSupported(new ModuleVersion("v2000.99.99")));
        }

        [Test]
        public void DottedSpecsSupported()
        {
            // We should support both two and three number dotted specs, on both
            // tagged and dev releases.
            Assert.IsTrue(CkanModule.IsSpecSupported(new ModuleVersion("v1.1")));
            Assert.IsTrue(CkanModule.IsSpecSupported(new ModuleVersion("v1.0.2")));
        }

        [Test]
        public void FutureModule()
        {
            // Modules from the future are unsupported.
            Assert.Throws<UnsupportedKraken>(delegate
            {
                CkanModule.FromJson(TestData.FutureMetaData());
            });
        }

        [Test]
        public void multilicense_986()
        {
            CkanModule mod = CkanModule.FromJson(TestData.kOS_014_multilicense());

            Assert.AreEqual(2, mod.license.Count, "Dual-license");
            Assert.AreEqual("GPL-3.0", mod.license[0].ToString());
            Assert.AreEqual("GPL-2.0", mod.license[1].ToString());
        }

        [Test]
        public void unilicense_986()
        {
            CkanModule mod = TestData.kOS_014_module();

            Assert.AreEqual(1, mod.license.Count, "Uni-license");
            Assert.AreEqual("GPL-3.0", mod.license[0].ToString());
        }

        [Test]
        public void bad_resource_1208()
        {
            JObject metadata = JObject.Parse(TestData.kOS_014());

            // Guess which string totally isn't a valid Url? This one.
            metadata["resources"]["homepage"] = "https://included%in%the%download";

            CkanModule mod = CkanModule.FromJson(metadata.ToString());

            Assert.IsNotNull(mod);
            Assert.IsNull(mod.resources.homepage);
        }

        [Test]
        public void good_resource_1208()
        {
            CkanModule mod = CkanModule.FromJson(TestData.kOS_014());

            Assert.AreEqual(
                "http://forum.kerbalspaceprogram.com/threads/68089-0-23-kOS-Scriptable-Autopilot-System-v0-11-2-13",
                mod.resources.homepage.ToString()
            );
        }

        [Test]
        public void InternetArchiveDownload_RedistributableLicense_CorrectURL()
        {
            // Arrange
            CkanModule module = TestData.kOS_014_module();
            // Act
            Uri uri = module.InternetArchiveDownload;
            // Assert
            Assert.IsNotNull(uri);
            Assert.AreEqual("https://archive.org/download/kOS-0.14/C5A224AC-kOS-0.14.zip", uri.ToString());
        }

        [Test]
        public void InternetArchiveDownload_RestrictedLicense_NullURL()
        {
            // Arrange
            CkanModule module = TestData.FireSpitterModule();
            // Act
            Uri uri = module.InternetArchiveDownload;
            // Assert
            Assert.IsNull(uri);
        }

        [Test]
        public void InternetArchiveDownload_EpochVersion_CorrectURL()
        {
            // Arrange
            CkanModule module = TestData.kOS_014_epoch_module();
            // Act
            Uri uri = module.InternetArchiveDownload;
            // Assert
            Assert.IsNotNull(uri);
            Assert.AreEqual("https://archive.org/download/kOS-3-0.14/C5A224AC-kOS-3-0.14.zip", uri.ToString());
        }

        [Test]
        public void InternetArchiveDownload_NoHash_NullURL()
        {
            // Arrange
            CkanModule module = TestData.RandSCapsuleDyneModule();
            // Act
            Uri uri = module.InternetArchiveDownload;
            // Assert
            Assert.IsNull(uri);
        }

    }
}
