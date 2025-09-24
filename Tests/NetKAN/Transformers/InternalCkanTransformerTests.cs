using System.Linq;

using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

using CKAN;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Transformers;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class InternalCkanTransformerTests
    {
        private readonly TransformOptions opts = new TransformOptions(1, null, null, null, false, null);

        [Test]
        public void AddsMissingProperties()
        {
            // Arrange
            const string filePath = "/DoesNotExist.zip";

            var internalCkan = new JObject();
            internalCkan["spec_version"] = 1;
            internalCkan["foo"] = "bar";

            var mHttp = new Mock<IHttpService>();
            var mModuleService = new Mock<IModuleService>();

            mHttp.Setup(i => i.DownloadModule(It.IsAny<Metadata>()))
                .Returns(filePath);

            mModuleService.Setup(i => i.GetInternalCkan(
                    It.IsAny<CkanModule>(), It.IsAny<string>()))
                .Returns(internalCkan);

            var sut = new InternalCkanTransformer(mHttp.Object, mModuleService.Object);

            var json = new JObject();
            json["spec_version"] = 1;
            json["identifier"] = "DoesNotExist";
            json["author"] = "DidNotCreate";
            json["version"] = "1.0";
            json["download"] = "https://awesomemod.example/AwesomeMod.zip";

            // Act
            var result = sut.Transform(new Metadata(json), opts).First();
            var transformedJson = result.Json();

            // Assert
            Assert.That((string?)transformedJson["foo"], Is.EqualTo("bar"),
                "InternalCkanTransformer should add properties from the internal ckan that don't exist on the original."
            );
        }

        [Test]
        public void DoesNotOverrideExistingProperties()
        {
            // Arrange
            const string filePath = "/DoesNotExist.zip";

            var internalCkan = new JObject();
            internalCkan["spec_version"] = 1;
            internalCkan["foo"] = "bar";

            var mHttp = new Mock<IHttpService>();
            var mModuleService = new Mock<IModuleService>();

            mHttp.Setup(i => i.DownloadModule(It.IsAny<Metadata>()))
                .Returns(filePath);

            mModuleService.Setup(i => i.GetInternalCkan(
                    It.IsAny<CkanModule>(), It.IsAny<string>()))
                .Returns(internalCkan);

            var sut = new InternalCkanTransformer(mHttp.Object, mModuleService.Object);

            var json = new JObject();
            json["spec_version"] = 1;
            json["identifier"] = "DoesNotExist";
            json["author"] = "DidNotCreate";
            json["version"] = "1.0";
            json["foo"] = "baz";
            json["download"] = "https://awesomemod.example/AwesomeMod.zip";

            // Act
            var result = sut.Transform(new Metadata(json), opts).First();
            var transformedJson = result.Json();

            // Assert
            Assert.That((string?)transformedJson["foo"], Is.EqualTo("baz"),
                "InternalCkanTransformer should not override existing properties."
            );
        }
    }
}
