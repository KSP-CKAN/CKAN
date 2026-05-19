using System;
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
        public void Transform_WithMissingProperties_Adds()
        {
            // Arrange
            const string filePath = "/DoesNotExist.zip";

            var internalCkan = new JObject()
            {
                { "spec_version", 1 },
                { "foo",          "bar" },
            };

            var mHttp = new Mock<IHttpService>();
            var mModuleService = new Mock<IModuleService>();

            mHttp.Setup(i => i.DownloadModule(It.IsAny<Metadata>()))
                .Returns(filePath);

            mModuleService.Setup(i => i.GetInternalCkans(
                    It.IsAny<CkanModule>(), It.IsAny<string>()))
                .Returns(new JObject[] { internalCkan });

            var sut = new InternalCkanTransformer(mHttp.Object, mModuleService.Object);

            var json = new JObject()
            {
                { "spec_version", 1 },
                { "identifier",   "DoesNotExist" },
                { "author",       "DidNotCreate" },
                { "version",      "1.0" },
                { "download",     "https://awesomemod.example/AwesomeMod.zip" },
            };

            // Act
            var result = sut.Transform(new Metadata(json), opts).First();
            var transformedJson = result.Json();

            // Assert
            Assert.That((string?)transformedJson["foo"], Is.EqualTo("bar"),
                "InternalCkanTransformer should add properties from the internal ckan that don't exist on the original."
            );
        }

        [Test]
        public void Transform_WithExistingProperties_DoesNotOverride()
        {
            // Arrange
            const string filePath = "/DoesNotExist.zip";

            var internalCkan = new JObject()
            {
                { "spec_version", 1 },
                { "foo",          "bar" },
                { "$kref",        "#/ckan/github/ThisShould/BeIgnored"},
            };

            var mHttp = new Mock<IHttpService>();
            var mModuleService = new Mock<IModuleService>();

            mHttp.Setup(i => i.DownloadModule(It.IsAny<Metadata>()))
                .Returns(filePath);

            mModuleService.Setup(i => i.GetInternalCkans(
                    It.IsAny<CkanModule>(), It.IsAny<string>()))
                .Returns(new JObject[] { internalCkan });

            var sut = new InternalCkanTransformer(mHttp.Object, mModuleService.Object);

            var json = new JObject()
            {
                { "spec_version", 1 },
                { "identifier",   "DoesNotExist" },
                { "author",       "DidNotCreate" },
                { "version",      "1.0" },
                { "foo",          "baz" },
                { "download",     "https://awesomemod.example/AwesomeMod.zip" },
            };

            // Act
            var result = sut.Transform(new Metadata(json), opts).First();
            var transformedJson = result.Json();

            // Assert
            Assert.That((string?)transformedJson["foo"], Is.EqualTo("baz"),
                "InternalCkanTransformer should not override existing properties."
            );
            Assert.IsFalse(transformedJson.ContainsKey("$kref"));
        }

        [Test]
        public void Transform_NoInternalCkan_DoesNothing()
        {
            // Arrange
            const string filePath = "/DoesNotExist.zip";

            var mHttp = new Mock<IHttpService>();
            var mModuleService = new Mock<IModuleService>();

            mHttp.Setup(i => i.DownloadModule(It.IsAny<Metadata>()))
                .Returns(filePath);

            mModuleService.Setup(i => i.GetInternalCkans(
                    It.IsAny<CkanModule>(), It.IsAny<string>()))
                .Returns(Array.Empty<JObject>());

            var sut = new InternalCkanTransformer(mHttp.Object, mModuleService.Object);

            var json = new JObject()
            {
                { "spec_version", 1 },
                { "identifier",   "DoesNotExist" },
                { "author",       "DidNotCreate" },
                { "version",      "1.0" },
                { "foo",          "baz" },
                { "download",     "https://awesomemod.example/AwesomeMod.zip" },
            };

            // Act
            var result = sut.Transform(new Metadata(json), opts).First();
            var transformedJson = result.Json();

            // Assert
            Assert.AreEqual(json, transformedJson);
        }
    }
}
