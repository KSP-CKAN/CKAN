using CKAN.NetKAN.Model;
using CKAN.NetKAN.Transformers;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class VersionEditTransformerTests
    {
        [Test]
        public void DoesNothingWhenNoMatch()
        {
            // Arrange
            var sut = new VersionEditTransformer();

            var json = new JObject();
            json["spec_version"] = 1;
            json["version"] = "1.2.3";

            // Act
            var result = sut.Transform(new Metadata(json));
            var transformedJson = result.Json();

            // Assert
            Assert.That(transformedJson, Is.EqualTo(json),
                "VersionEditTransformer should not modify metadata if it does not match."
            );
        }

        [Test]
        public void EditsCorrectlyWithString()
        {
            // Arrange
            var sut = new VersionEditTransformer();

            var json = new JObject();
            json["spec_version"] = 1;
            json["version"] = "v1.2.3";
            json["x_netkan_version_edit"] = "^v?(?<version>.+)$";

            // Act
            var result = sut.Transform(new Metadata(json));
            var transformedJson = result.Json();

            // Assert
            Assert.That((string)transformedJson["version"], Is.EqualTo("1.2.3"));
        }

        [Test]
        public void EditsCorrectlyWithOnlyFind()
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
            var result = sut.Transform(new Metadata(json));
            var transformedJson = result.Json();

            // Assert
            Assert.That((string)transformedJson["version"], Is.EqualTo("1.2.3"));
        }

        [Test]
        public void EditsCorrectlyWithFindAndReplace()
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
            var result = sut.Transform(new Metadata(json));
            var transformedJson = result.Json();

            // Assert
            Assert.That((string)transformedJson["version"], Is.EqualTo("FOO-1.2.3-BAR"));
        }
    }
}
