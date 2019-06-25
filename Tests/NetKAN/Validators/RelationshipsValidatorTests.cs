using Newtonsoft.Json.Linq;
using NUnit.Framework;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Validators;

namespace Tests.NetKAN.Validators
{
    [TestFixture]
    public sealed class RelationshipsValidatorTests
    {
        [Test,
            TestCase("v1.4",  null,      null),
            TestCase("v1.4",  "depends", @"[ { ""name"": ""ModuleManager"" } ]"),
            TestCase("v1.26", "depends", @"[ { ""any_of"": [ { ""name"": ""ModuleManager"" } ] } ]"),
        ]
        public void Validate_ValidRelationships_DoesNotThrow(string spec_version, string relationName, string relationValue)
        {
            Assert.DoesNotThrow(() => TryRelationships(spec_version, relationName, relationValue));
        }

        [Test,
            TestCase("v1.4",  "depends", @"[ { ""name"": ""Module Manager"" } ]"),
            TestCase("v1.25", "depends", @"[ { ""any_of"": [ { ""name"": ""ModuleManager"" } ] } ]"),
            TestCase("v1.26", "depends", @"[ { ""any_of"": [ { ""name"": ""Module Manager"" } ] } ]"),
        ]
        public void Validate_BadRelationships_Throws(string spec_version, string relationName, string relationValue)
        {
            Assert.Throws<CKAN.Kraken>(() => TryRelationships(spec_version, relationName, relationValue));
        }

        private void TryRelationships(string spec_version, string relationName, string relationValue)
        {
            // Arrange
            var json = new JObject();
            json["spec_version"] = spec_version;
            json["identifier"]   = "AwesomeMod";
            if (relationName != null && relationValue != null)
            {
                json[relationName] = JToken.Parse(relationValue);
            }

            // Act
            var val = new RelationshipsValidator();
            val.Validate(new Metadata(json));
        }
    }
}
