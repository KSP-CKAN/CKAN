using CKAN.NetKAN.Model;
using CKAN.NetKAN.Transformers;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class HttpTransformerTests
    {
        [Test]
        public void AddsDownloadProperty()
        {
            // Arrange
            const string url = "https://awesomemod.example/download/AwesomeMod.zip";

            var sut = new HttpTransformer();
            var json = new JObject();
            json["spec_version"] = 1;
            json["$kref"] = string.Format("#/ckan/http/{0}", url);

            // Act
            var result = sut.Transform(new Metadata(json));
            var transformedJson = result.Json();

            // Assert
            Assert.That((string)transformedJson["download"], Is.EqualTo(url),
                "HttpTransformer should add a download property equal to the $kref ID."
            );
        }

        [TestCase("#/ckan/github/foo/bar")]
        [TestCase("#/ckan/netkan/http://awesomemod.example/awesomemod.netkan")]
        [TestCase("#/ckan/kerbalstuff/1")]
        [TestCase("#/ckan/spacedock/1")]
        [TestCase("#/ckan/curse/1")]
        [TestCase("#/ckan/foo")]
        public void DoesNotAlterMetadataWhenNonMatching(string kref)
        {
            // Arrange
            var sut = new HttpTransformer();
            var json = new JObject();
            json["spec_version"] = 1;
            json["$kref"] = kref;

            // Act
            var result = sut.Transform(new Metadata(json));
            var transformedJson = result.Json();

            // Assert
            Assert.That(transformedJson, Is.EqualTo(json),
                "HttpTransformed should not alter the metatadata when it does not match the $kref."
            );
        }
    }
}
