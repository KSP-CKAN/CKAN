using Newtonsoft.Json.Linq;
using NUnit.Framework;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Validators;

namespace Tests.NetKAN.Validators
{
    [TestFixture]
    public sealed class LicensesValidatorTests
    {
        [Test,
            TestCase("v1.2",  @"""WTFPL"""),
            TestCase("v1.18", @"""Unlicense"""),
            TestCase("v1.8",  @"[ ""GPL-3.0"", ""MIT"" ]"),
        ]
        public void Validate_GoodSpecVersionLicense_DoesNotThrow(string spec_version, string license)
        {
            Assert.DoesNotThrow(() => TryLicense(spec_version, license));
        }

        [Test,
            TestCase("1",     @"""WTFPL"""),
            TestCase("v1.17", @"""Unlicense"""),
            TestCase("v1.4",  @"""NotARealLicense"""),
            TestCase("v1.4",  @"[ ""GPL-3.0"", ""MIT"" ]"),
            TestCase("v1.4",  @"[ ""GPL-3.0"", ""Unlicense"" ]"),
            TestCase("v1.4",  @"[ ""GPL-3.0"", ""NotARealLicense"" ]"),
        ]
        public void Validate_BadSpecVersionLicense_Throws(string spec_version, string license)
        {
            Assert.Throws<CKAN.Kraken>(() => TryLicense(spec_version, license));
        }

        private void TryLicense(string spec_version, string license)
        {
            // Arrange
            var json = new JObject();
            json["spec_version"] = spec_version;
            json["identifier"]   = "AwesomeMod";
            json["license"]      = JToken.Parse(license);

            // Act
            var val = new LicensesValidator();
            val.Validate(new Metadata(json));
        }
    }
}
