using CKAN;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Sources.Avc;
using CKAN.NetKAN.Transformers;
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
                ksp_version = new KSPVersion("1.0.4")
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
                ksp_version = new KSPVersion("1.0.4"),
                ksp_version_min = new KSPVersion("0.90"),
                ksp_version_max  = new KSPVersion("1.0.3")
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

        [TestCase("version")]
        [TestCase("ksp_version")]
        [TestCase("ksp_version_min")]
        [TestCase("ksp_version_max")]
        public void OverridesExistingVersionInfo(string propertyName)
        {
            // Arrange
            var avcVersion = new AvcVersion();

            switch(propertyName)
            {
                case "version":
                    avcVersion.version = new Version("1.2.3");
                    break;
                case "ksp_version":
                    avcVersion.ksp_version = new KSPVersion("1.2.3");
                    break;
                case "ksp_version_min":
                    avcVersion.ksp_version_min = new KSPVersion("1.2.3");
                    break;
                case "ksp_version_max":
                    avcVersion.ksp_version_max = new KSPVersion("1.2.3");
                    break;
            }

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
            json[propertyName] = "9001";

            // Act
            var result = sut.Transform(new Metadata(json));
            var transformedJson = result.Json();

            // Assert
            Assert.That((string)transformedJson[propertyName], Is.EqualTo("1.2.3"),
                string.Format("AvcTransformer should override an existing {0}.", propertyName)
            );
        }
    }
}
