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
    public sealed class MetaNetkanTransformerTests
    {
        private readonly TransformOptions opts = new TransformOptions(1, null, null, null, false, null);

        [Test]
        public void DoesNothingWhenNoMatch()
        {
            // Arrange
            var mHttp = new Mock<IHttpService>();

            var sut = new MetaNetkanTransformer(mHttp.Object, null);

            var json = new JObject();
            json["spec_version"] = 1;
            json["$kref"] = "#/ckan/foo";

            // Act
            var result = sut.Transform(new Metadata(json), opts).First();
            var transformedJson = result.Json();

            // Assert
            Assert.That(transformedJson, Is.EqualTo(json),
                "MetaNetkanTransformer should not modify metadata if it does not match."
            );
        }

        [Test]
        public void ThrowsWhenTargetIsAlsoMetaNetkan()
        {
            // Arrange
            var targetJson = new JObject();
            targetJson["spec_version"] = 1;
            targetJson["$kref"] = "#/ckan/netkan/http://awesomemod.example/AwesomeMod.netkan";

            var mHttp = new Mock<IHttpService>();

            mHttp.Setup(i => i.DownloadText(It.IsAny<Uri>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .Returns(targetJson.ToString());

            var sut = new MetaNetkanTransformer(mHttp.Object, null);

            var json = new JObject();
            json["spec_version"] = 1;
            json["$kref"] = "#/ckan/netkan/http://awesomemod.example/AwesomeMod.netkan";

            // Act
            TestDelegate act = () => sut.Transform(new Metadata(json), opts).First();

            // Assert
            Assert.That(act, Throws.Exception.InstanceOf<Kraken>(),
                "MetaNetkanTransformer should throw if target is also a metanetkan."
            );
        }

        [Test]
        public void TargetMetadataAddsMissingProperties()
        {
            // Arrange
            var targetJson = new JObject();
            targetJson["spec_version"] = 1;
            targetJson["foo"] = "bar";

            var mHttp = new Mock<IHttpService>();

            mHttp.Setup(i => i.DownloadText(It.IsAny<Uri>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .Returns(targetJson.ToString());

            var sut = new MetaNetkanTransformer(mHttp.Object, null);

            var json = new JObject();
            json["spec_version"] = 1;
            json["$kref"] = "#/ckan/netkan/http://awesomemod.example/AwesomeMod.netkan";

            // Act
            var result = sut.Transform(new Metadata(json), opts).First();
            var transformedJson = result.Json();

            // Assert
            Assert.That((string?)transformedJson["foo"], Is.EqualTo("bar"),
                "MetaNetkanTransformer add properties from target netkan that do not already exist."
            );
        }

        [Test]
        public void TargetMetadataDoesNotOverrideExistingProperty()
        {
            // Arrange
            var targetJson = new JObject();
            targetJson["spec_version"] = 1;
            targetJson["foo"] = "bar";

            var mHttp = new Mock<IHttpService>();

            mHttp.Setup(i => i.DownloadText(It.IsAny<Uri>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .Returns(targetJson.ToString());

            var sut = new MetaNetkanTransformer(mHttp.Object, null);

            var json = new JObject();
            json["spec_version"] = 1;
            json["$kref"] = "#/ckan/netkan/http://awesomemod.example/AwesomeMod.netkan";
            json["foo"] = "baz";

            // Act
            var result = sut.Transform(new Metadata(json), opts).First();
            var transformedJson = result.Json();

            // Assert
            Assert.That((string?)transformedJson["foo"], Is.EqualTo("baz"),
                "MetaNetkanTransformer should not override existing properties."
            );
        }
    }
}
