using Newtonsoft.Json.Linq;
using NUnit.Framework;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Validators;

namespace Tests.NetKAN.Validators
{
    [TestFixture]
    public sealed class ReplacedByValidatorTests
    {
        [Test,
            TestCase("v1.4",  null),
            TestCase("v1.26", @"{ ""name"": ""AwesomeModContinued"" }"),
        ]
        public void Validate_ValidReplacement_DoesNotThrow(string spec_version, string replacement)
        {
            Assert.DoesNotThrow(() => TryRelationships(spec_version, replacement));
        }

        [Test,
            TestCase("v1.25", @"{ ""name"": ""AwesomeModContinued"" }"),
        ]
        public void Validate_BadReplacement_Throws(string spec_version, string replacement)
        {
            Assert.Throws<CKAN.Kraken>(() => TryRelationships(spec_version, replacement));
        }

        private void TryRelationships(string spec_version, string replacement)
        {
            // Arrange
            var json = new JObject();
            json["spec_version"] = spec_version;
            json["identifier"]   = "AwesomeMod";
            if (replacement != null)
            {
                json["replaced_by"] = JToken.Parse(replacement);
            }

            // Act
            var val = new ReplacedByValidator();
            val.Validate(new Metadata(json));
        }
    }
}
