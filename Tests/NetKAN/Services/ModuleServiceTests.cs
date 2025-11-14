using System;
using System.Linq;

using Newtonsoft.Json.Linq;
using NUnit.Framework;
using ICSharpCode.SharpZipLib.Zip;
using Moq;

using CKAN;
using CKAN.NetKAN.Services;
using CKAN.Versioning;
using CKAN.Games.KerbalSpaceProgram;
using CKAN.Games.KerbalSpaceProgram2;
using CKAN.NetKAN.Sources.Github;
using Tests.Data;

namespace Tests.NetKAN.Services
{
    [TestFixture]
    public sealed class ModuleServiceTests
    {
        [Test]
        public void HasInstallableFilesReturnsFalseWhenNoInstallableFiles()
        {
            // Arrange
            var zip = TestData.DogeCoinFlagZip();
            var json = JObject.Parse(TestData.DogeCoinFlag_101());
            json["install"]![0]!["file"] = "DOES_NOT_EXIST";

            var sut = new ModuleService(new KerbalSpaceProgram());

            // Act
            var result = sut.HasInstallableFiles(CkanModule.FromJson(json.ToString()), zip);

            // Assert
            Assert.IsFalse(result,
                "HasInstallableFiles() should return false when there are no installable files."
            );
        }

        [Test]
        public void HasInstallableFilesReturnsTrueWhenInstallableFiles()
        {
            // Arrange
            var zip = TestData.DogeCoinFlagZip();
            var json = JObject.Parse(TestData.DogeCoinFlag_101());

            var sut = new ModuleService(new KerbalSpaceProgram());

            // Act
            var result = sut.HasInstallableFiles(CkanModule.FromJson(json.ToString()), zip);

            // Assert
            Assert.IsTrue(result,
                "HasInstallableFiles() should return true when there are installable files."
            );
        }

        [Test]
        public void GetsInternalCkanCorrectly()
        {
            // Arrange
            var sut = new ModuleService(new KerbalSpaceProgram());
            CkanModule mod = CkanModule.FromJson(TestData.DogeCoinFlag_101());

            // Act
            var result = sut.GetInternalCkan(mod, TestData.DogeCoinFlagZip());

            // Assert
            Assert.That(result, Is.Not.Null,
                "ModuleService should get an internal CKAN file."
            );
            Assert.That((string?)result?["identifier"], Is.EqualTo("DogeCoinFlag"),
                "ModuleService should get correct data for the internal CKAN file."
            );
        }

        [Test]
        public void GetsInternalAvcCorrectly()
        {
            //Arrange
            var json = new JObject();
            json["spec_version"] = 1;
            json["identifier"] = "DogeCoinFlag";
            json["author"] = "DogeAuthor";
            json["version"] = "1.0.0";
            json["download"] = "https://awesomemod.example/AwesomeMod.zip";

            var sut = new ModuleService(new KerbalSpaceProgram());

            // Act
            var result = sut.GetInternalAvc(CkanModule.FromJson(json.ToString()), TestData.DogeCoinFlagAvcZip());

            // Assert
            Assert.That(result, Is.Not.Null,
                "ModuleService should get an internal AVC file."
            );
            Assert.That(result?.version, Is.EqualTo(new ModuleVersion("1.1.0.0")),
                "ModuleService should get correct version from the internal AVC file."
            );
            Assert.That(result?.ksp_version, Is.EqualTo(GameVersion.Parse("0.24.2")),
                "ModuleService should get correct ksp_version from the internal AVC file."
            );
            Assert.That(result?.ksp_version_min, Is.EqualTo(GameVersion.Parse("0.24.0")),
                "ModuleService should get correct ksp_version_min from the internal AVC file."
            );
            Assert.That(result?.ksp_version_max, Is.EqualTo(GameVersion.Parse("0.24.2")),
                "ModuleService should get correct ksp_version_max from  the internal AVC file."
            );
        }

        [Test]
        public void GetInternalSpaceWarpInfo_ModuleWithSwinfo_Works()
        {
            // Arrange
            using (var gameDir = new TemporaryDirectory())
            {
                var            game   = new KerbalSpaceProgram2();
                var            http   = new Mock<IHttpService>();
                var            ghApi  = new Mock<IGithubApi>();
                var            loader = new SpaceWarpInfoLoader(http.Object, ghApi.Object);
                IModuleService sut    = new ModuleService(game);

                // Act
                var result = sut.GetInternalSpaceWarpInfos(TestData.BurnControllerModule(),
                                                           new ZipFile(TestData.BurnControllerZip()))
                                .Select(loader.Load)
                                .FirstOrDefault();

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual("BurnController",                result?.mod_id);
                Assert.AreEqual("Burn Controller",               result?.name);
                Assert.AreEqual("Lets you set up engine burns.", result?.description);
                Assert.AreEqual("JohnsterSpaceProgram",          result?.author);
                Assert.AreEqual("0.8.1",                         result?.version);
                Assert.AreEqual("0.1.1",                         result?.ksp2_version?.min);
                Assert.AreEqual("any",                           result?.ksp2_version?.max);
                Assert.AreEqual(new Uri("https://github.com/JohnsterSpaceProgramOfficial/BurnController/raw/main/Burn Controller/swinfo.json"),
                                result!.version_check);
                CollectionAssert.AreEquivalent(new string[] { "SpaceWarp" }, result!.dependencies!.Select(d => d.id));
            }
        }
    }
}
