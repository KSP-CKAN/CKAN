using Newtonsoft.Json.Linq;
using NUnit.Framework;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Validators;

namespace Tests.NetKAN.Validators
{
    [TestFixture]
    public sealed class OverrideValidatorTests
    {
        [Test,
            TestCase(null),
            TestCase(@"[ { ""version"": ""1.0"", ""delete"": ""ksp_version"" } ]"),
            TestCase(@"[ { ""version"": ""1.0"", ""override"": { ""ksp_version"": ""0.90"" } } ]"),
        ]
        public void Validate_ValidOverride_DoesNotThrow(string json)
        {
            Assert.DoesNotThrow(() => TryOverride(json));
        }

        [Test,
            TestCase(@"{ ""version"": ""1.0"", ""delete"": ""ksp_version"" }"),
            TestCase(@"[ { ""version"": ""1.0"" } ]"),
            TestCase(@"[ { ""delete"": ""identifier"" } ]"),
        ]
        public void Validate_BadOverride_Throws(string json)
        {
            Assert.Throws<CKAN.Kraken>(() => TryOverride(json));
        }

        private void TryOverride(string ovr)
        {
            // Arrange
            var json = new JObject();
            json["spec_version"] = 1;
            json["identifier"]   = "AwesomeMod";
            if (ovr != null)
            {
                json["x_netkan_override"] = JToken.Parse(ovr);
            }

            // Act
            var val = new OverrideValidator();
            val.Validate(new Metadata(json));
        }

    }
}
