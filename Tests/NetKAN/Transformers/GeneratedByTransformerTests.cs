using CKAN.NetKAN.Model;
using CKAN.NetKAN.Transformers;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class GeneratedByTransformerTests
    {
        [Test]
        public void AddsGeneratedByProperty()
        {
            // Arrange
            var sut = new GeneratedByTransformer();
            var json = new JObject();
            json["spec_version"] = 1;

            // Act
            var result = sut.Transform(new Metadata(json));
            var transformedJson = result.Json();

            // Assert
            Assert.That((string)transformedJson["x_generated_by"], Does.Contain("netkan"),
                "GeneratedByTransformer should add an x_generated_by property containing the string 'netkan'"
            );
        }
    }
}
