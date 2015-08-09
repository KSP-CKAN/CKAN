using CKAN.NetKAN.Model;
using CKAN.NetKAN.Validators;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Tests.NetKAN.Validators
{
    [TestFixture]
    public sealed class IsCkanModuleValidatorTests
    {
        private static readonly JObject ValidCkan = new JObject();

        [SetUp]
        public void SetUp()
        {
            ValidCkan["spec_version"] = 1;
            ValidCkan["identifier"] = "AwesomeMod";
            ValidCkan["version"] = "1.0.0";
            ValidCkan["download"] = "https://www.awesome-mod.example/AwesomeMod.zip";
        }

        [Test]
        public void DoesNotThrowOnValidCkan()
        {
            // Arrange
            var sut = new IsCkanModuleValidator();
            var json = (JObject)ValidCkan.DeepClone();

            // Act
            TestDelegate act = () => sut.Validate(new Metadata(json));

            // Assert
            Assert.That(act, Throws.Nothing,
                "IsCkanModuleValidator should not throw when passed valid metadata."
            );
        }

        [TestCase("spec_version")]
        [TestCase("identifier")]
        [TestCase("version")]
        [TestCase("download")]
        public void DoesThrowWhenMissingProperty(string propertyName)
        {
            // Arrange
            var sut = new IsCkanModuleValidator();
            var json = (JObject)ValidCkan.DeepClone();
            json.Remove(propertyName);

            // Act
            TestDelegate act = () => sut.Validate(new Metadata(json));

            // Assert
            Assert.That(act, Throws.Exception,
                string.Format("IsCkanModuleValidator should throw when {0} is missing.", propertyName)
            );
        }
    }
}
