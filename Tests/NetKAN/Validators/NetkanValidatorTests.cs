using CKAN.NetKAN.Model;
using CKAN.NetKAN.Validators;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Tests.NetKAN.Validators
{
    [TestFixture]
    public sealed class NetkanValidatorTests
    {
        [Test]
        public void DoesNotThrowWhenIdentifierPresent()
        {
            // Arrange
            var sut = new NetkanValidator();
            var json = new JObject();
            json["spec_version"] = 1;
            json["identifier"] = "AwesomeMod";

            // Act
            TestDelegate act = () => sut.Validate(new Metadata(json));

            // Assert
            Assert.That(act, Throws.Nothing,
                "NetkanValidator should not throw when identifier is present."
            );
        }

        [Test]
        public void DoesThrowWhenIdentifierMissing()
        {
            // Arrange
            var sut = new NetkanValidator();
            var json = new JObject();
            json["spec_version"] = 1;

            // Act
            TestDelegate act = () => sut.Validate(new Metadata(json));

            // Assert
            Assert.That(act, Throws.Exception,
                "NetkanValidator should throw when identifier is missing."
            );
        }
    }
}
