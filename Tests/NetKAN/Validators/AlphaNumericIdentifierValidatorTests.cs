using Newtonsoft.Json.Linq;
using NUnit.Framework;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Validators;

namespace Tests.NetKAN.Validators
{
    [TestFixture]
    public sealed class AlphaNumericIdentifierValidatorTests
    {
        [Test,
            TestCase("Normal"),
            TestCase("Has-Dash"),
            TestCase("ALLUPPER"),
            TestCase("alllower"),
        ]
        public void Validate_ValidIdentifier_DoesNotThrow(string identifier)
        {
            Assert.DoesNotThrow(() => TryId(identifier));
        }

        [Test,
            TestCase("#HashTag"),
            TestCase("Under_Score"),
            TestCase("Dot.Dot"),
        ]
        public void Validate_BadIdentifier_Throws(string identifier)
        {
            Assert.Throws<CKAN.Kraken>(() => TryId(identifier));
        }

        private void TryId(string identifier)
        {
            // Arrange
            var json = new JObject();
            json["spec_version"] = 1;
            json["identifier"]   = identifier;

            // Act
            var val = new AlphaNumericIdentifierValidator();
            val.Validate(new Metadata(json));
        }
    }
}
