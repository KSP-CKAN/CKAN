using Newtonsoft.Json.Linq;
using NUnit.Framework;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Validators;

namespace Tests.NetKAN.Validators
{
    [TestFixture]
    public sealed class InstallValidatorTests
    {
        [Test,
            TestCase("v1.2",  "GameData/something"),
            TestCase("v1.12", "Ships/something"),
            TestCase("v1.16", "Ships/@thumbs"),
        ]
        public void Validate_GoodSpecVersionInstallTo_DoesNotThrow(string spec_version, string install_to)
        {
            Assert.DoesNotThrow(() => TryInstallTo(spec_version, install_to));
        }

        [Test,
            TestCase("1",     "GameData/something"),
            TestCase("v1.11", "Ships/something"),
            TestCase("v1.15", "Ships/@thumbs"),
        ]
        public void Validate_BadSpecVersionInstallTo_Throws(string spec_version, string install_to)
        {
            Assert.Throws<CKAN.Kraken>(() => TryInstallTo(spec_version, install_to));
        }

        [Test,
            TestCase("v1.4", "{ \"find\": \"something\", \"install_to\": \"GameData\" }"),
            TestCase("v1.10", "{ \"find_regexp\": \"something\", \"install_to\": \"GameData\" }"),
            TestCase("v1.16", "{ \"find_matches_files\": true, \"find\": \"something\", \"install_to\": \"GameData\" }"),
            TestCase("v1.18", "{ \"as\": \"somethingelse\", \"find\": \"something\", \"install_to\": \"GameData\" }"),
        ]
        public void Validate_GoodSpecVersionInstallStanza_DoesNotThrow(string spec_version, string install_stanza)
        {
            Assert.DoesNotThrow(() => TryInstallStanza(spec_version, install_stanza));
        }

        [Test,
            TestCase("v1.3", "{ \"find\": \"something\", \"install_to\": \"GameData\" }"),
            TestCase("v1.9", "{ \"find_regexp\": \"something\", \"install_to\": \"GameData\" }"),
            TestCase("v1.15", "{ \"find_matches_files\": true, \"find\": \"something\", \"install_to\": \"GameData\" }"),
            TestCase("v1.17", "{ \"as\": \"somethingelse\", \"find\": \"something\", \"install_to\": \"GameData\" }"),
        ]
        public void Validate_BadSpecVersionInstallStanza_Throws(string spec_version, string install_stanza)
        {
            Assert.Throws<CKAN.Kraken>(() => TryInstallStanza(spec_version, install_stanza));
        }

        private void TryInstallTo(string spec_version, string install_to)
        {
            // Arrange
            var json = new JObject();
            json["spec_version"] = spec_version;
            json["identifier"]   = "AwesomeMod";
            json["install"]      = new JArray() {
                new JObject() {
                    { "file",       "something" },
                    { "install_to", install_to  }
                }
            };

            // Act
            var val = new InstallValidator();
            val.Validate(new Metadata(json));
        }

        private void TryInstallStanza(string spec_version, string install_stanza)
        {
            // Arrange
            var json = new JObject();
            json["spec_version"] = spec_version;
            json["identifier"]   = "AwesomeMod";
            json["install"]      = new JArray() {
                JObject.Parse(install_stanza)
            };

            // Act
            var val = new InstallValidator();
            val.Validate(new Metadata(json));
        }
    }
}
