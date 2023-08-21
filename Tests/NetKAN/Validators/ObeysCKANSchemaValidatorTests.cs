using Newtonsoft.Json.Linq;
using NUnit.Framework;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Validators;

namespace Tests.NetKAN.Validators
{
    [TestFixture]
    public sealed class ObeysCKANSchemaValidatorTests
    {
        [Test,
            TestCase(boringModule),
        ]
        public void Validate_Obeys_DoesNotThrow(string json)
        {
            Assert.DoesNotThrow(() => TryModule(json));
        }

        [Test,
            TestCase(boringModule, "spec_version"),
            TestCase(boringModule, "identifier"),
            TestCase(boringModule, "name"),
            TestCase(boringModule, "author"),
            TestCase(boringModule, "version"),
            TestCase(boringModule, "license"),
            TestCase(boringModule, "download"),
        ]
        public void Validate_MissingProperty_Throws(string json, string removeProperty)
        {
            Assert.Throws<CKAN.Kraken>(() => TryModule(json, removeProperty));
        }

        [Test,
            TestCase(boringModule, @"[ ""en-us"" ]"),
        ]
        public void Validate_UniqueLocalizations_DoesNotThrow(string json, string localizations)
        {
            Assert.DoesNotThrow(
                () => TryModule(json, null, "localizations", JArray.Parse(localizations))
            );
        }

        [Test,
            TestCase(boringModule, @"[ ""en-us"", ""en-us"" ]"),
            TestCase(boringModule, @"[ ""en-us"", ""es-es"", ""en-us"" ]"),
            TestCase(boringModule, @"[ ""en-us"", ""de-de"", ""fr-fr"", ""de-de"" ]"),
        ]
        public void Validate_DuplicateLocalizations_Throws(string json, string localizations)
        {
            Assert.Throws<CKAN.Kraken>(
                () => TryModule(json, null, "localizations", JArray.Parse(localizations))
            );
        }

        public void Validate_InstallToGameDataSlashmods_DoesNotThrow()
        {
            Assert.DoesNotThrow(
                () => TryModule(boringModule, null, "install",
                                JArray.Parse(@"[
                                    {
                                        ""find"":       ""BetterPartsManager"",
                                        ""install_to"": ""GameData/Mods""
                                    }
                                ]")));
        }

        private void TryModule(string json, string removeProperty = null, string addProperty = null, JToken addPropertyValue = null)
        {
            // Arrange
            var jObj = JObject.Parse(json);
            if (removeProperty != null)
            {
                jObj.Remove(removeProperty);
            }
            if (addProperty != null && addPropertyValue != null)
            {
                jObj[addProperty] = addPropertyValue;
            }

            // Act
            var val = new ObeysCKANSchemaValidator();
            val.Validate(new Metadata(jObj));
        }

        private const string boringModule = @"{
            ""spec_version"": 1,
            ""identifier"":   ""BoringModule"",
            ""name"":         ""Boring Module"",
            ""abstract"":     ""A minimal module that obeys CKAN.schema"",
            ""author"":       ""Boring Author"",
            ""version"":      ""1.0.0"",
            ""license"":      ""MIT"",
            ""download"":     ""https://mysite.org/mymod.zip""
        }";
    }
}
