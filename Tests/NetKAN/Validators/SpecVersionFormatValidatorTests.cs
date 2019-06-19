using Newtonsoft.Json.Linq;
using NUnit.Framework;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Validators;

namespace Tests.NetKAN.Validators
{
    [TestFixture]
    public sealed class SpecVersionFormatValidatorTests
    {
        [Test,
            TestCase("1"),
            TestCase("v1.4"),
            TestCase("v1.26"),
        ]
        public void Validate_ValidSpecVersion_DoesNotThrow(string spec_version)
        {
            Assert.DoesNotThrow(() => TrySpecVersion(spec_version));
        }

        [Test,
            //TestCase(null), // NetKAN.Model.Metadata can't handle this, so we can't test null
            TestCase(""),
            TestCase("0"),
            TestCase("2"),
            TestCase("1.4"),
            TestCase("v1.4.1"),
        ]
        public void Validate_BadSpecVersion_Throws(string spec_version)
        {
            Assert.Throws<CKAN.Kraken>(() => TrySpecVersion(spec_version));
        }

        private void TrySpecVersion(string spec_version)
        {
            // Arrange
            var json = new JObject();
            json["spec_version"] = spec_version;

            // Act
            var val = new SpecVersionFormatValidator();
            val.Validate(new Metadata(json));
        }
    }
}
