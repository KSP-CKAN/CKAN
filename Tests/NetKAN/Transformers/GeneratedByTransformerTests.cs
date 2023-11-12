using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Transformers;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class GeneratedByTransformerTests
    {
        private readonly TransformOptions opts = new TransformOptions(1, null, null, false, null);

        [Test]
        public void AddsGeneratedByProperty()
        {
            // Arrange
            var sut = new GeneratedByTransformer();
            var json = new JObject();
            json["spec_version"] = 1;

            // Act
            var result = sut.Transform(new Metadata(json), opts).First();
            var transformedJson = result.Json();

            // Assert
            Assert.That((string)transformedJson["x_generated_by"], Does.Contain("netkan"),
                "GeneratedByTransformer should add an x_generated_by property containing the string 'netkan'"
            );
        }
    }
}
