using Newtonsoft.Json.Linq;
using NUnit.Framework;

using CKAN;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Validators;

namespace Tests.NetKAN.Validators
{
    [TestFixture]
    public sealed class DownloadArrayValidatorTests
    {
        private static readonly DownloadArrayValidator validator = new DownloadArrayValidator();

        [Test]
        public void Validate_OldSpecNoArray_DoesNotThrow()
        {
            // Arrange
            var jobj = JObject.Parse(@"{
                ""spec_version"": ""v1.33"",
                ""download"": ""https://github.com/""
            }");

            // Act / Assert
            validator.Validate(new Metadata(jobj));
        }

        [Test]
        public void Validate_OldSpecArray_Throws()
        {
            // Arrange
            var jobj = JObject.Parse(@"{
                ""spec_version"": ""v1.33"",
                ""download"": [ ""https://github.com/"" ]
            }");

            // Act / Assert
            var exception = Assert.Throws<Kraken>(() =>
                validator.Validate(new Metadata(jobj)));
            Assert.AreEqual(DownloadArrayValidator.ErrorMessage, exception.Message);
        }

        [Test]
        public void Validate_NewSpecNoArray_DoesNotThrow()
        {
            // Arrange
            var jobj = JObject.Parse(@"{
                ""spec_version"": ""v1.34"",
                ""download"": ""https://github.com/""
            }");

            // Act / Assert
            validator.Validate(new Metadata(jobj));
        }

        [Test]
        public void Validate_NewSpecArray_DoesNotThrow()
        {
            // Arrange
            var jobj = JObject.Parse(@"{
                ""spec_version"": ""v1.34"",
                ""download"": [ ""https://github.com/"" ]
            }");

            // Act / Assert
            validator.Validate(new Metadata(jobj));
        }
    }
}
