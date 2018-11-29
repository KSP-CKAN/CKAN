using NUnit.Framework;
using Newtonsoft.Json.Linq;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Validators;

namespace Tests.NetKAN.Validators
{
    [TestFixture]
    public sealed class KrefValidatorTests
    {
        [Test,
            TestCase(null),
            TestCase("#/ckan/curse/12345"),
            TestCase("#/ckan/github/AGoodModder/AGoodMod"),
            TestCase("#/ckan/http/https://mysite.org/AGoodMod.zip"),
            TestCase("#/ckan/ksp-avc/https://mysite.org/AGoodMod.version"),
            TestCase("#/ckan/netkan/https://mysite.org/AGoodMod.netkan"),
            TestCase("#/ckan/jenkins/https://mysite.org/AGoodMod/"),
            TestCase("#/ckan/spacedock/12345")
        ]
        public void Validate_KnownKref_Valid(string kref)
        {
            Assert.DoesNotThrow(() => TryKref(kref));
        }

        [Test,
            TestCase("#/ckan/techman/iscool"),
            TestCase("#/ckan/spaceport/305")
        ]
        public void Validate_TechManIsCool_Invalid(string kref)
        {
            Assert.Throws<CKAN.Kraken>(() => TryKref(kref));
        }

        private void TryKref(string kref)
        {
            // Arrange
            var json = new JObject();
            json["spec_version"] = 1;
            json["identifier"]   = "AwesomeMod";
            if (kref != null)
            {
                json["$kref"] = kref;
            }

            // Act
            var val = new KrefValidator();
            val.Validate(new Metadata(json));
        }

    }
}
