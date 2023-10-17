using Newtonsoft.Json.Linq;
using NUnit.Framework;

using CKAN;
using CKAN.NetKAN.Services;
using CKAN.Versioning;
using CKAN.Games.KerbalSpaceProgram;

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
            json["install"][0]["file"] = "DOES_NOT_EXIST";

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
            GameInstance inst = new GameInstance(new KerbalSpaceProgram(), "/", "dummy", new NullUser());

            // Act
            var result = sut.GetInternalCkan(mod, TestData.DogeCoinFlagZip(), inst);

            // Assert
            Assert.That(result, Is.Not.Null,
                "ModuleService should get an internal CKAN file."
            );
            Assert.That((string)result["identifier"], Is.EqualTo("DogeCoinFlag"),
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
            json["version"] = "1.0.0";
            json["download"] = "https://awesomemod.example/AwesomeMod.zip";

            var sut = new ModuleService(new KerbalSpaceProgram());

            // Act
            var result = sut.GetInternalAvc(CkanModule.FromJson(json.ToString()), TestData.DogeCoinFlagAvcZip());

            // Assert
            Assert.That(result, Is.Not.Null,
                "ModuleService should get an internal AVC file."
            );
            Assert.That(result.version, Is.EqualTo(new ModuleVersion("1.1.0.0")),
                "ModuleService should get correct version from the internal AVC file."
            );
            Assert.That(result.ksp_version, Is.EqualTo(GameVersion.Parse("0.24.2")),
                "ModuleService should get correct ksp_version from the internal AVC file."
            );
            Assert.That(result.ksp_version_min, Is.EqualTo(GameVersion.Parse("0.24.0")),
                "ModuleService should get correct ksp_version_min from the internal AVC file."
            );
            Assert.That(result.ksp_version_max, Is.EqualTo(GameVersion.Parse("0.24.2")),
                "ModuleService should get correct ksp_version_max from  the internal AVC file."
            );
        }
    }
}
