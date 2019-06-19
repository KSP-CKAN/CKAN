using Newtonsoft.Json.Linq;
using NUnit.Framework;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Validators;

namespace Tests.NetKAN.Validators
{
    [TestFixture]
    public sealed class VersionStrictValidatorTests
    {
        [Test,
            TestCase("v1.2",  false),
            TestCase("v1.16", true),
        ]
        public void Validate_GoodStrictVersion_DoesNotThrow(string spec_version, bool strict)
        {
            Assert.DoesNotThrow(() => TryVersionStrict(spec_version, strict));
        }

        [Test,
            TestCase("v1.15", true),
        ]
        public void Validate_BadStrictVersion_Throws(string spec_version, bool strict)
        {
            Assert.Throws<CKAN.Kraken>(() => TryVersionStrict(spec_version, strict));
        }

        private void TryVersionStrict(string spec_version, bool strict)
        {
            // Arrange
            var json = new JObject();
            json["spec_version"] = spec_version;
            json["identifier"]   = "AwesomeMod";
            if (strict)
            {
                json["ksp_version_strict"] = true;
            }

            // Act
            var val = new VersionStrictValidator();
            val.Validate(new Metadata(json));
        }
    }
}
