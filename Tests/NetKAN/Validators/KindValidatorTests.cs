using Newtonsoft.Json.Linq;
using NUnit.Framework;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Validators;

namespace Tests.NetKAN.Validators
{
    [TestFixture]
    public sealed class KindValidatorTests
    {
        [Test,
            TestCase("1",     @"""package"""),
            TestCase("v1.2",  @"""package"""),
            TestCase("v1.6",  @"""metapackage"""),
            TestCase("v1.28", @"""dlc"""),
        ]
        public void Validate_GoodSpecVersionKind_DoesNotThrow(string spec_version, string kind)
        {
            Assert.DoesNotThrow(() => TryKind(spec_version, kind));
        }

        [Test,
            TestCase("1",     @"""metapackage"""),
            TestCase("v1.5",  @"""metapackage"""),
            TestCase("1",     @"""dlc"""),
            TestCase("v1.4",  @"""dlc"""),
            TestCase("v1.17", @"""dlc"""),
        ]
        public void Validate_BadSpecVersionKind_Throws(string spec_version, string kind)
        {
            Assert.Throws<CKAN.Kraken>(() => TryKind(spec_version, kind));
        }

        private void TryKind(string spec_version, string kind)
        {
            // Arrange
            var json = new JObject();
            json["spec_version"] = spec_version;
            json["identifier"]   = "AwesomeMod";
            json["kind"]         = JToken.Parse(kind);

            // Act
            var val = new KindValidator();
            val.Validate(new Metadata(json));
        }
    }
}
