using CKAN.NetKAN.Model;
using CKAN.NetKAN.Validators;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Tests.NetKAN.Validators
{
    [TestFixture]
    public sealed class MatchingIdentifiersValidatorTests
    {
        [Test]
        public void DoesNotThrowWhenIdentifiersMatch()
        {
            // Arrange
            var sut = new MatchingIdentifiersValidator("AwesomeMod");
            var json = new JObject();
            json["spec_version"] = 1;
            json["identifier"] = "AwesomeMod";

            // Act
            TestDelegate act = () => sut.Validate(new Metadata(json));

            // Assert
            Assert.That(act, Throws.Nothing,
                "MatchingIdentifiersValidator should not throw when identifiers match."
            );
        }

        [Test]
        public void DoesThrowWhenIdentifiersDoNotMatch()
        {
            // Arrange
            var sut = new MatchingIdentifiersValidator("AwesomeMod");
            var json = new JObject();
            json["spec_version"] = 1;
            json["identifier"] = "AmazingMod";

            // Act
            TestDelegate act = () => sut.Validate(new Metadata(json));

            // Assert
            Assert.That(act, Throws.Exception,
                "MatchingIdentifiersValidator should throw when identifiers don't match."
            );
        }
    }
}
