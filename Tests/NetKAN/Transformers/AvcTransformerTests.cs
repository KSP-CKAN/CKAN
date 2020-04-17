using System.Linq;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using CKAN;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Sources.Avc;
using CKAN.NetKAN.Transformers;
using CKAN.Versioning;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class AvcTransformerTests
    {
        private TransformOptions opts = new TransformOptions(1, null, null);

        [Test]
        public void AddsMissingVersionInfo()
        {
            // Arrange
            var avcVersion = new AvcVersion
            {
                version = new ModuleVersion("1.0.0"),
                ksp_version = KspVersion.Parse("1.0.4")
            };

            var mHttp = new Mock<IHttpService>();
            var mModuleService = new Mock<IModuleService>();

            mModuleService.Setup(i => i.GetInternalAvc(It.IsAny<CkanModule>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(avcVersion);

            var sut = new AvcTransformer(mHttp.Object, mModuleService.Object, null);

            var json = new JObject();
            json["spec_version"] = 1;
            json["identifier"] = "AwesomeMod";
            json["$vref"] = "#/ckan/ksp-avc";
            json["download"] = "https://awesomemod.example/AwesomeMod.zip";

            // Act
            var result = sut.Transform(new Metadata(json), opts).First();
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

            var sut = new AvcTransformer(mHttp.Object, mModuleService.Object, null);

            var json = new JObject();
            json["spec_version"] = 1;
            json["identifier"] = "AwesomeMod";
            json["$vref"] = "#/ckan/ksp-avc";
            json["download"] = "https://awesomemod.example/AwesomeMod.zip";

            // Act
            var result = sut.Transform(new Metadata(json), opts).First();
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
        [TestCase(
            null, null, null,
            "1.5.1", "1.5.1", null,
            null, "1.5.1", null
        )]
        [TestCase(
            null, null, null,
            "1.5.1", null, "1.5.1",
            null, null, "1.5.1"
        )]
        [TestCase(
            "any", null, null,
            null, null, null,
            "any", null, null
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

            var sut = new AvcTransformer(mHttp.Object, mModuleService.Object, null);

            // Act
            var result = sut.Transform(new Metadata(json), opts).First();
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
            var avcVersion = new AvcVersion { version = new ModuleVersion("1.2.3") };

            var mHttp = new Mock<IHttpService>();
            var mModuleService = new Mock<IModuleService>();

            mModuleService.Setup(i => i.GetInternalAvc(It.IsAny<CkanModule>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(avcVersion);

            var sut = new AvcTransformer(mHttp.Object, mModuleService.Object, null);

            var json = new JObject();
            json["spec_version"] = 1;
            json["identifier"] = "AwesomeMod";
            json["$vref"] = "#/ckan/ksp-avc";
            json["download"] = "https://awesomemod.example/AwesomeMod.zip";
            json["version"] = "9001";

            // Act
            var result = sut.Transform(new Metadata(json), opts).First();
            var transformedJson = result.Json();

            // Assert
            Assert.That((string)transformedJson["version"], Is.EqualTo("9001"),
                "AvcTransformer should not override an existing version."
            );
        }

        [Test]
        public void Transform_TrustVersionFileTrue_OverridesExistingInfo()
        {
            // Arrange
            var mHttp          = new Mock<IHttpService>();
            var mModuleService = new Mock<IModuleService>();
            mModuleService.Setup(i => i.GetInternalAvc(It.IsAny<CkanModule>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new AvcVersion()
                {
                    version = new ModuleVersion("1.2.3")
                });

            ITransformer sut = new AvcTransformer(mHttp.Object, mModuleService.Object, null);

            JObject json = new JObject();
            json["spec_version"]                = 1;
            json["identifier"]                  = "AwesomeMod";
            json["$vref"]                       = "#/ckan/ksp-avc";
            json["download"]                    = "https://awesomemod.example/AwesomeMod.zip";
            json["version"]                     = "9001";
            json["x_netkan_trust_version_file"] = true;

            // Act
            Metadata result          = sut.Transform(new Metadata(json), opts).First();
            JObject  transformedJson = result.Json();

            // Assert
            Assert.That((string)transformedJson["version"], Is.EqualTo("1.2.3"),
                "AvcTransformer should override an existing version when x_netkan_trust_version_file is true."
            );
        }

        [Test]
        public void Transform_RemoteAvcOverrides_SameVersion()
        {
            // Arrange
            var mHttp = new Mock<IHttpService>();
            mHttp.Setup(i => i.DownloadText(It.Is<System.Uri>(u => u.OriginalString == "https://awesomemod.example/avc.version")))
                .Returns("//leading comment\n\n{\"version\":\"1.0.0\",\"ksp_version_min\":\"1.2.1\",\"ksp_version_max\":\"1.2.99\"}");

            var mModuleService = new Mock<IModuleService>();
            mModuleService.Setup(i => i.GetInternalAvc(It.IsAny<CkanModule>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new AvcVersion()
                {
                    version = new ModuleVersion("1.0.0"),
                    ksp_version = new KspVersion(1, 2, 3),
                    Url = "https://awesomemod.example/avc.version",
                }); ;

            ITransformer sut = new AvcTransformer(mHttp.Object, mModuleService.Object, null);

            JObject json = new JObject();
            json["spec_version"] = 1;
            json["identifier"] = "AwesomeMod";
            json["$vref"] = "#/ckan/ksp-avc";
            json["download"] = "https://awesomemod.example/AwesomeMod.zip";
            json["version"] = "1.0.0";
            json["ksp_version"] = "1.2.3";

            // Act
            Metadata result = sut.Transform(new Metadata(json), opts).First();
            JObject transformedJson = result.Json();

            // Assert
            Assert.That((string)transformedJson["ksp_version"], Is.Null,
                "AvcTransformer should replace local AVC info with remote AVC info if the module versions match."
            );

            Assert.That((string)transformedJson["ksp_version_min"], Is.EqualTo("1.2.1"),
                "AvcTransformer should replace local AVC info with remote AVC info if the module versions match."
            );

            Assert.That((string)transformedJson["ksp_version_max"], Is.EqualTo("1.2.99"),
                "AvcTransformer should replace local AVC info with remote AVC info if the module versions match."
            );
        }

        [Test]
        public void Transform_RemoteAvcOverrides_DifferentVersion()
        {
            // Arrange
            var mHttp = new Mock<IHttpService>();
            mHttp.Setup(i => i.DownloadText(It.Is<System.Uri>(u => u.OriginalString == "https://awesomemod.example/avc.version")))
                .Returns("//leading comment\n\n{\"version\":\"1.0.1\",\"ksp_version_min\":\"1.2.1\",\"ksp_version_max\":\"1.2.99\"}");

            var mModuleService = new Mock<IModuleService>();
            mModuleService.Setup(i => i.GetInternalAvc(It.IsAny<CkanModule>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new AvcVersion()
                {
                    version = new ModuleVersion("1.0.0"),
                    ksp_version = new KspVersion(1, 2, 3),
                    Url = "https://awesomemod.example/avc.version",
                }); ;

            ITransformer sut = new AvcTransformer(mHttp.Object, mModuleService.Object, null);

            JObject json = new JObject();
            json["spec_version"] = 1;
            json["identifier"] = "AwesomeMod";
            json["$vref"] = "#/ckan/ksp-avc";
            json["download"] = "https://awesomemod.example/AwesomeMod.zip";
            json["version"] = "1.0.0";
            json["ksp_version"] = "1.2.3";

            // Act
            Metadata result = sut.Transform(new Metadata(json), opts).First();
            JObject transformedJson = result.Json();

            // Assert
            Assert.That((string)transformedJson["ksp_version"], Is.EqualTo("1.2.3"),
                "AvcTransformer should not replace local AVC info with remote AVC info if the module versions don't match."
            );

            Assert.That((string)transformedJson["ksp_version_min"], Is.Null,
                "AvcTransformer should not replace local AVC info with remote AVC info if the module versions don't match."
            );

            Assert.That((string)transformedJson["ksp_version_max"], Is.Null,
                "AvcTransformer should not replace local AVC info with remote AVC info if the module versions don't match."
            );
        }

        [Test]
        public void Transform_RemoteAvcOverrides_FetchError()
        {
            // Arrange
            var mHttp = new Mock<IHttpService>();
            mHttp.Setup(i => i.DownloadText(It.Is<System.Uri>(u => u.OriginalString == "https://awesomemod.example/avc.version")))
                .Throws<System.Exception>();

            var mModuleService = new Mock<IModuleService>();
            mModuleService.Setup(i => i.GetInternalAvc(It.IsAny<CkanModule>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new AvcVersion()
                {
                    version = new ModuleVersion("1.0.0"),
                    ksp_version = new KspVersion(1, 2, 3),
                    Url = "https://awesomemod.example/avc.version",
                }); ;

            ITransformer sut = new AvcTransformer(mHttp.Object, mModuleService.Object, null);

            JObject json = new JObject();
            json["spec_version"] = 1;
            json["identifier"] = "AwesomeMod";
            json["$vref"] = "#/ckan/ksp-avc";
            json["download"] = "https://awesomemod.example/AwesomeMod.zip";
            json["version"] = "1.0.0";
            json["ksp_version"] = "1.2.3";

            // Act
            Metadata result = sut.Transform(new Metadata(json), opts).First();
            JObject transformedJson = result.Json();

            // Assert
            Assert.That((string)transformedJson["ksp_version"], Is.EqualTo("1.2.3"),
                "AvcTransformer should not replace local AVC info with remote AVC info if the module versions don't match."
            );

            Assert.That((string)transformedJson["ksp_version_min"], Is.Null,
                "AvcTransformer should not replace local AVC info with remote AVC info if the module versions don't match."
            );

            Assert.That((string)transformedJson["ksp_version_max"], Is.Null,
                "AvcTransformer should not replace local AVC info with remote AVC info if the module versions don't match."
            );
        }

        [Test]
        public void Transform_RemoteAvcOverrides_MultipleVersions_VersionFound()
        {
            // Arrange
            var mHttp = new Mock<IHttpService>();
            mHttp.Setup(i => i.DownloadText(It.Is<System.Uri>(u => u.OriginalString == "https://awesomemod.example/avc.version")))
                .Returns(@"//leading comment
[
    { ""version"" : ""1.0.1"", ""ksp_version"" : ""2.3.4"" },
    { ""version"" : ""1.0.0"", ""ksp_version_min"" : ""1.2.1"", ""ksp_version_max"" : ""1.2.99"" },
    { ""version"" : ""0.9.0"", ""ksp_version"" : ""1.0.0"" },
]
");

            var mModuleService = new Mock<IModuleService>();
            mModuleService.Setup(i => i.GetInternalAvc(It.IsAny<CkanModule>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new AvcVersion()
                {
                    version = new ModuleVersion("1.0.0"),
                    ksp_version = new KspVersion(1, 2, 3),
                    Url = "https://awesomemod.example/avc.version",
                }); ;

            ITransformer sut = new AvcTransformer(mHttp.Object, mModuleService.Object, null);

            JObject json = new JObject();
            json["spec_version"] = 1;
            json["identifier"] = "AwesomeMod";
            json["$vref"] = "#/ckan/ksp-avc";
            json["download"] = "https://awesomemod.example/AwesomeMod.zip";
            json["version"] = "1.0.0";
            json["ksp_version"] = "1.2.3";

            // Act
            Metadata result = sut.Transform(new Metadata(json), opts).First();
            JObject transformedJson = result.Json();

            // Assert
            Assert.That((string)transformedJson["ksp_version"], Is.Null,
                "AvcTransformer should replace local AVC info with remote AVC info if the module versions match."
            );

            Assert.That((string)transformedJson["ksp_version_min"], Is.EqualTo("1.2.1"),
                "AvcTransformer should replace local AVC info with remote AVC info if the module versions match."
            );

            Assert.That((string)transformedJson["ksp_version_max"], Is.EqualTo("1.2.99"),
                "AvcTransformer should replace local AVC info with remote AVC info if the module versions match."
            );
        }

        [Test]
        public void Transform_RemoteAvcOverrides_MultipleVersions_VersionNotFound()
        {
            // Arrange
            var mHttp = new Mock<IHttpService>();
            mHttp.Setup(i => i.DownloadText(It.Is<System.Uri>(u => u.OriginalString == "https://awesomemod.example/avc.version")))
                .Returns(@"//leading comment
[
    { ""version"" : ""1.0.2"", ""ksp_version"" : ""2.3.4"" },
    { ""version"" : ""1.0.1"", ""ksp_version_min"" : ""1.2.1"", ""ksp_version_max"" : ""1.2.99"" },
    { ""version"" : ""0.9.0"", ""ksp_version"" : ""1.0.0"" },
]
");

            var mModuleService = new Mock<IModuleService>();
            mModuleService.Setup(i => i.GetInternalAvc(It.IsAny<CkanModule>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new AvcVersion()
                {
                    version = new ModuleVersion("1.0.0"),
                    ksp_version = new KspVersion(1, 2, 3),
                    Url = "https://awesomemod.example/avc.version",
                }); ;

            ITransformer sut = new AvcTransformer(mHttp.Object, mModuleService.Object, null);

            JObject json = new JObject();
            json["spec_version"] = 1;
            json["identifier"] = "AwesomeMod";
            json["$vref"] = "#/ckan/ksp-avc";
            json["download"] = "https://awesomemod.example/AwesomeMod.zip";
            json["version"] = "1.0.0";
            json["ksp_version"] = "1.2.3";

            // Act
            Metadata result = sut.Transform(new Metadata(json), opts).First();
            JObject transformedJson = result.Json();

            // Assert
            Assert.That((string)transformedJson["ksp_version"], Is.EqualTo("1.2.3"),
                "AvcTransformer should not replace local AVC info with remote AVC info if the module versions don't match."
            );

            Assert.That((string)transformedJson["ksp_version_min"], Is.Null,
                "AvcTransformer should not replace local AVC info with remote AVC info if the module versions don't match."
            );

            Assert.That((string)transformedJson["ksp_version_max"], Is.Null,
                "AvcTransformer should not replace local AVC info with remote AVC info if the module versions don't match."
            );
        }

        [Test]
        public void Transform_RemoteAvcOverrides_MultipleVersions_MoreThanOneMatchingVersion()
        {
            // Arrange
            var mHttp = new Mock<IHttpService>();
            mHttp.Setup(i => i.DownloadText(It.Is<System.Uri>(u => u.OriginalString == "https://awesomemod.example/avc.version")))
                .Returns(@"//leading comment
[
    { ""version"" : ""1.0.1"", ""ksp_version"" : ""2.3.4"" },
    { ""version"" : ""1.0.0"", ""ksp_version_min"" : ""1.2.0"", ""ksp_version_max"" : ""1.2.9"" },
    { ""version"" : ""1.0.0"", ""ksp_version_min"" : ""1.2.1"", ""ksp_version_max"" : ""1.2.99"" },
    { ""version"" : ""0.9.0"", ""ksp_version"" : ""1.0.0"" },
]
");

            var mModuleService = new Mock<IModuleService>();
            mModuleService.Setup(i => i.GetInternalAvc(It.IsAny<CkanModule>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new AvcVersion()
                {
                    version = new ModuleVersion("1.0.0"),
                    ksp_version = new KspVersion(1, 2, 3),
                    Url = "https://awesomemod.example/avc.version",
                }); ;

            ITransformer sut = new AvcTransformer(mHttp.Object, mModuleService.Object, null);

            JObject json = new JObject();
            json["spec_version"] = 1;
            json["identifier"] = "AwesomeMod";
            json["$vref"] = "#/ckan/ksp-avc";
            json["download"] = "https://awesomemod.example/AwesomeMod.zip";
            json["version"] = "1.0.0";
            json["ksp_version"] = "1.2.3";

            // Act
            Metadata result = sut.Transform(new Metadata(json), opts).First();
            JObject transformedJson = result.Json();

            // Assert
            Assert.That((string)transformedJson["ksp_version"], Is.EqualTo("1.2.3"),
                "AvcTransformer should not replace local AVC info with remote AVC info if the module versions don't match."
            );

            Assert.That((string)transformedJson["ksp_version_min"], Is.Null,
                "AvcTransformer should not replace local AVC info with remote AVC info if the module versions don't match."
            );

            Assert.That((string)transformedJson["ksp_version_max"], Is.Null,
                "AvcTransformer should not replace local AVC info with remote AVC info if the module versions don't match."
            );
        }

        [Test]
        public void Transform_RemoteAvcOverrides_MultipleVersions_UnknownJsonRootToken()
        {
            // Arrange
            var mHttp = new Mock<IHttpService>();
            mHttp.Setup(i => i.DownloadText(It.Is<System.Uri>(u => u.OriginalString == "https://awesomemod.example/avc.version")))
                .Returns("//leading comment\n\n1.23456");

            var mModuleService = new Mock<IModuleService>();
            mModuleService.Setup(i => i.GetInternalAvc(It.IsAny<CkanModule>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new AvcVersion()
                {
                    version = new ModuleVersion("1.0.0"),
                    ksp_version = new KspVersion(1, 2, 3),
                    Url = "https://awesomemod.example/avc.version",
                }); ;

            ITransformer sut = new AvcTransformer(mHttp.Object, mModuleService.Object, null);

            JObject json = new JObject();
            json["spec_version"] = 1;
            json["identifier"] = "AwesomeMod";
            json["$vref"] = "#/ckan/ksp-avc";
            json["download"] = "https://awesomemod.example/AwesomeMod.zip";
            json["version"] = "1.0.0";
            json["ksp_version"] = "1.2.3";

            // Act
            Metadata result = sut.Transform(new Metadata(json), opts).First();
            JObject transformedJson = result.Json();

            // Assert
            Assert.That((string)transformedJson["ksp_version"], Is.EqualTo("1.2.3"),
                "AvcTransformer should not replace local AVC info with remote AVC info if the module versions don't match."
            );

            Assert.That((string)transformedJson["ksp_version_min"], Is.Null,
                "AvcTransformer should not replace local AVC info with remote AVC info if the module versions don't match."
            );

            Assert.That((string)transformedJson["ksp_version_max"], Is.Null,
                "AvcTransformer should not replace local AVC info with remote AVC info if the module versions don't match."
            );
        }

    }
}
