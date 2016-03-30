using CKAN;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Sources.Avc;
using CKAN.NetKAN.Transformers;
using CKAN.Versioning;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class AvcTransformerTests
    {
        [Test]
        public void AddsMissingVersionInfo()
        {
            // Arrange
            var avcVersion = new AvcVersion
            {
                version = new Version("1.0.0"),
                ksp_version = KspVersion.Parse("1.0.4")
            };

            var mHttp = new Mock<IHttpService>();
            var mModuleService = new Mock<IModuleService>();

            mModuleService.Setup(i => i.GetInternalAvc(It.IsAny<CkanModule>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(avcVersion);

            var sut = new AvcTransformer(mHttp.Object, mModuleService.Object);

            var json = new JObject();
            json["spec_version"] = 1;
            json["identifier"] = "AwesomeMod";
            json["$vref"] = "#/ckan/ksp-avc";
            json["download"] = "https://awesomemod.example/AwesomeMod.zip";

            // Act
            var result = sut.Transform(new Metadata(json));
            var transformedJson = result.Json();

            // Assert
            Assert.That((string)transformedJson["version"], Is.EqualTo("1.0.0"),
                "AvcTransformer should add the version specified in the AVC version file."
            );
            Assert.That((string)transformedJson["ksp_version"], Is.EqualTo("1.0.4"),
                "AvcTransformer should add the KSP version specified in the AVC version file."
            );
        }

        [Test]
        public void PreferentiallyAddsRangedKspVersionInfo()
        {
            // Arrange
            var avcVersion = new AvcVersion
            {
                ksp_version = KspVersion.Parse("1.0.4"),
                ksp_version_min = KspVersion.Parse("0.90"),
                ksp_version_max  = KspVersion.Parse("1.0.3")
            };

            var mHttp = new Mock<IHttpService>();
            var mModuleService = new Mock<IModuleService>();

            mModuleService.Setup(i => i.GetInternalAvc(It.IsAny<CkanModule>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(avcVersion);

            var sut = new AvcTransformer(mHttp.Object, mModuleService.Object);

            var json = new JObject();
            json["spec_version"] = 1;
            json["identifier"] = "AwesomeMod";
            json["$vref"] = "#/ckan/ksp-avc";
            json["download"] = "https://awesomemod.example/AwesomeMod.zip";

            // Act
            var result = sut.Transform(new Metadata(json));
            var transformedJson = result.Json();

            // Assert
            Assert.That((string)transformedJson["ksp_version_min"], Is.EqualTo("0.90"),
                "AvcTransformer should add the KSP min version specified in the AVC version file."
            );
            Assert.That((string)transformedJson["ksp_version_max"], Is.EqualTo("1.0.3"),
                "AvcTransformer should add the KSP min version specified in the AVC version file."
            );
            Assert.That(transformedJson["ksp_version"], Is.Null,
                "AvcTransformer should not add a KSP version if min or max versions are specified."
            );
        }

        [TestCase(
            "1.0.4", null, null,
            "1.0.4", null, null,
            "1.0.4", null, null
        )]
        [TestCase(
            null, "1.0.4", "1.0.4",
            null, "1.0.4", "1.0.4",
            "1.0.4", null, null
        )]
        [TestCase(
            "1.0.4", null, null,
            null, "1.0.4", "1.0.4",
            "1.0.4", null, null
        )]
        [TestCase(
            null, "1.0.2", "1.0.4",
            null, "1.0.2", "1.0.4",
            null, "1.0.2", "1.0.4"
        )]
        [TestCase(
            "1.0.4", null, null,
            null, "1.0.0", "1.0.2",
            null, "1.0.0", "1.0.4"
        )]
        [TestCase(
            "1.0.4", null, null,
            "0.90", null, null,
            null, "0.90", "1.0.4"
        )]
        [TestCase(
            null, "1.0.4", "1.0.4",
            "1.0.0", null, null,
            null, "1.0.0", "1.0.4"
        )]
        [TestCase(
            null, "1.0.0", "1.0.4",
            "1.0.2", null, null,
            null, "1.0.0", "1.0.4"
        )]
        [TestCase(
            null, null, null,
            "1.0.2", null, null,
            "1.0.2", null, null
        )]
        [TestCase(
            "1.0.2", null, null,
            null, null, null,
            "1.0.2", null, null
        )]
        [TestCase(
            null, null, null,
            null, null, null,
            null, null, null
        )]
        [TestCase(
            null, "1.0.0", "1.0.1",
            "1.0.2", "1.0.3", "1.0.4",
            null, "1.0.0", "1.0.4"
        )]
        public void CorrectlyCalculatesKspVersionInfo(
            string existingKsp, string existingKspMin, string existingKspMax,
            string avcKsp, string avcKspMin, string avcKspMax,
            string expectedKsp, string expectedKspMin, string expectedKspMax
        )
        {
            // Arrange
            var json = new JObject();
            json["spec_version"] = 1;
            json["identifier"] = "AwesomeMod";
            json["$vref"] = "#/ckan/ksp-avc";
            json["download"] = "https://awesomemod.example/AwesomeMod.zip";

            if (!string.IsNullOrWhiteSpace(existingKsp))
                json["ksp_version"] = existingKsp;

            if (!string.IsNullOrWhiteSpace(existingKspMin))
                json["ksp_version_min"] = existingKspMin;

            if (!string.IsNullOrWhiteSpace(existingKspMax))
                json["ksp_version_max"] = existingKspMax;

            var avcVersion = new AvcVersion();

            if (!string.IsNullOrWhiteSpace(avcKsp))
                avcVersion.ksp_version = KspVersion.Parse(avcKsp);

            if (!string.IsNullOrWhiteSpace(avcKspMin))
                avcVersion.ksp_version_min = KspVersion.Parse(avcKspMin);

            if (!string.IsNullOrWhiteSpace(avcKspMax))
                avcVersion.ksp_version_max = KspVersion.Parse(avcKspMax);

            var mHttp = new Mock<IHttpService>();
            var mModuleService = new Mock<IModuleService>();

            mModuleService.Setup(i => i.GetInternalAvc(It.IsAny<CkanModule>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(avcVersion);

            var sut = new AvcTransformer(mHttp.Object, mModuleService.Object);

            // Act
            var result = sut.Transform(new Metadata(json));
            var transformedJson = result.Json();

            // Assert
            Assert.That((string)transformedJson["ksp_version"], Is.EqualTo(expectedKsp),
                "AvcTransformer should calculate ksp_version correctly"
            );

            Assert.That((string)transformedJson["ksp_version_min"], Is.EqualTo(expectedKspMin),
                "AvcTransformer should calculate ksp_version_min correctly"
            );

            Assert.That((string)transformedJson["ksp_version_max"], Is.EqualTo(expectedKspMax),
                "AvcTransformer should calculate ksp_version_max correctly"
            );
        }

        [Test]
        public void DoesNotOverrideExistingVersionInfo()
        {
            // Arrange
            var avcVersion = new AvcVersion { version = new Version("1.2.3") };

            var mHttp = new Mock<IHttpService>();
            var mModuleService = new Mock<IModuleService>();

            mModuleService.Setup(i => i.GetInternalAvc(It.IsAny<CkanModule>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(avcVersion);

            var sut = new AvcTransformer(mHttp.Object, mModuleService.Object);

            var json = new JObject();
            json["spec_version"] = 1;
            json["identifier"] = "AwesomeMod";
            json["$vref"] = "#/ckan/ksp-avc";
            json["download"] = "https://awesomemod.example/AwesomeMod.zip";
            json["version"] = "9001";

            // Act
            var result = sut.Transform(new Metadata(json));
            var transformedJson = result.Json();

            // Assert
            Assert.That((string)transformedJson["version"], Is.EqualTo("9001"),
                "AvcTransformer should not override an existing version."
            );
        }
    }
}
