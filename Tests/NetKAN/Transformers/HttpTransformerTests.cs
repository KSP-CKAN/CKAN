using CKAN.NetKAN.Model;
using CKAN.NetKAN.Transformers;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class HttpTransformerTests
    {
        [TestCase("#/ckan/github/foo/bar")]
        [TestCase("#/ckan/netkan/http://awesomemod.example/awesomemod.netkan")]
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
