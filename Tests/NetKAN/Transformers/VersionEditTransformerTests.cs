using System;
using System.Linq;

using Newtonsoft.Json.Linq;
using NUnit.Framework;

using CKAN;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Transformers;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class VersionEditTransformerTests
    {
        private readonly TransformOptions opts = new TransformOptions(1, null, null, null, false, null);

        [Test]
        public void Transform_NoMatch_DoesNothing()
        {
            // Arrange
            var sut = new VersionEditTransformer();

            var json = new JObject();
            json["spec_version"] = 1;
            json["version"] = "1.2.3";

            // Act
            var result = sut.Transform(new Metadata(json), opts).First();
            var transformedJson = result.Json();

            // Assert
            Assert.That(transformedJson, Is.EqualTo(json),
                "VersionEditTransformer should not modify metadata if it does not match."
            );
        }

        [Test]
        public void Transform_WithString_EditsCorrectly()
        {
            // Arrange
            var sut = new VersionEditTransformer();

            var json = new JObject();
            json["spec_version"] = 1;
            json["version"] = "v1.2.3";
            json["x_netkan_version_edit"] = "^v?(?<version>.+)$";

            // Act
            var result = sut.Transform(new Metadata(json), opts).First();
            var transformedJson = result.Json();

            // Assert
            Assert.That((string?)transformedJson["version"], Is.EqualTo("1.2.3"));
        }

        [Test]
        public void Transform_WithOnlyFind_EditsCorrectly()
        {
            // Arrange
            var sut = new VersionEditTransformer();

            var edit = new JObject();
            edit["find"] = "^v?(?<version>.+)$";

            var json = new JObject();
            json["spec_version"] = 1;
            json["version"] = "v1.2.3";
            json["x_netkan_version_edit"] = edit;

            // Act
            var result = sut.Transform(new Metadata(json), opts).First();
            var transformedJson = result.Json();

            // Assert
            Assert.That((string?)transformedJson["version"], Is.EqualTo("1.2.3"));
        }

        [Test]
        public void Transform_WithFindAndReplace_EditsCorrectly()
        {
            // Arrange
            var sut = new VersionEditTransformer();

            var edit = new JObject();
            edit["find"] = "^v?(?<version>.+)$";
            edit["replace"] = "FOO-${version}-BAR";

            var json = new JObject();
            json["spec_version"] = 1;
            json["version"] = "v1.2.3";
            json["x_netkan_version_edit"] = edit;

            // Act
            var result = sut.Transform(new Metadata(json), opts).First();
            var transformedJson = result.Json();

            // Assert
            Assert.That((string?)transformedJson["version"], Is.EqualTo("FOO-1.2.3-BAR"));
        }

        [Test]
        public void Transform_NoMatchInStrictMode_Throws()
        {
            // Arrange
            var sut = new VersionEditTransformer();

            var edit = new JObject();
            edit["find"] = "^v(?<version>.+)$";

            var json = new JObject();
            json["spec_version"] = 1;
            json["version"] = "1.2.3";
            json["x_netkan_version_edit"] = edit;

            // Act
            TestDelegate act = () => sut.Transform(new Metadata(json), opts).First();

            // Assert
            Assert.That(act, Throws.Exception.TypeOf<Kraken>());
        }

        [Test]
        public void Transform_NoMatchInNonStrictMode_DoesNotThrow()
        {
            // Arrange
            var sut = new VersionEditTransformer();

            var edit = new JObject();
            edit["find"] = "^v(?<version>.+)$";
            edit["strict"] = false;

            var json = new JObject();
            json["spec_version"] = 1;
            json["version"] = "1.2.3";
            json["x_netkan_version_edit"] = edit;

            // Act
            TestDelegate act = () => sut.Transform(new Metadata(json), opts).First();

            // Assert
            Assert.That(act, Throws.Nothing);
        }

        [Test]
        public void Transform_NeitherObjectNorValue_Throws()
        {
            // Arrange
            var sut  = new VersionEditTransformer();
            var json = new JObject()
            {
                { "spec_version",          "1" },
                { "version",               "1.0" },
                { "x_netkan_version_edit", new JArray() { "Junk value" } },
            };

            // Act / Assert
            var exc = Assert.Throws<Kraken>(() =>
            {
                var result = sut.Transform(new Metadata(json), opts).First();
            })!;

            // Assert
            CollectionAssert.AreEqual(
                new string[]
                {
                    @"Unrecognized `x_netkan_version_edit` value: [",
                    @"  ""Junk value""",
                    @"]",
                },
                exc.Message.Split(new string[] { Environment.NewLine },
                                  StringSplitOptions.None));
        }
    }
}
