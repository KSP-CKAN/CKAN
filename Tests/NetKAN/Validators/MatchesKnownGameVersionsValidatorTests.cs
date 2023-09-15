using Newtonsoft.Json.Linq;
using NUnit.Framework;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Validators;
using CKAN.Games.KerbalSpaceProgram;

namespace Tests.NetKAN.Validators
{
    [TestFixture]
    public sealed class MatchesKnownGameVersionsValidatorTests
    {
        [Test,
            TestCase(null,    null,     null),
            TestCase("0.90",  null,     null),
            TestCase("1.2",   null,     null),
            TestCase("1.7.2", null,     null),
            TestCase(null,    "1.3.1",  "1.3.1"),
            TestCase(null,    "1.1.10", "1.2.20"),
            TestCase(null,    "1.6",    "1.7"),
        ]
        public void Validate_KnownVersions_DoesNotThrow(string ksp_version, string ksp_version_min, string ksp_version_max)
        {
            Assert.DoesNotThrow(() => TryVersion(ksp_version, ksp_version_min, ksp_version_max));
        }

        [Test,
            TestCase("0.26.0", null,     null),
            TestCase("1.4.99", null,     null),
            TestCase(null,     "1.0.10", "1.0.99"),
            TestCase(null,     "1.99.0", "1.99.99"),
        ]
        public void Validate_UnknownVersions_Throws(string ksp_version, string ksp_version_min, string ksp_version_max)
        {
            Assert.Throws<CKAN.Kraken>(() => TryVersion(ksp_version, ksp_version_min, ksp_version_max));
        }

        private void TryVersion(string ksp_version, string ksp_version_min, string ksp_version_max)
        {
            // Arrange
            var json = new JObject();
            json["spec_version"] = 1;
            json["version"]      = "1.0";
            json["identifier"]   = "AwesomeMod";
            json["download"]     = "https://mysite.org/mymod.zip";
            if (ksp_version != null)
            {
                json["ksp_version"] = ksp_version;
            }
            if (ksp_version_min != null)
            {
                json["ksp_version_min"] = ksp_version_min;
            }
            if (ksp_version_max != null)
            {
                json["ksp_version_max"] = ksp_version_max;
            }

            // Act
            var val = new MatchesKnownGameVersionsValidator(new KerbalSpaceProgram());
            val.Validate(new Metadata(json));
        }
    }
}
