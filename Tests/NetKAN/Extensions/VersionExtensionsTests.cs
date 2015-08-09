using CKAN;
using CKAN.NetKAN.Extensions;
using NUnit.Framework;

namespace Tests.NetKAN.Extensions
{
    [TestFixture]
    public sealed class VersionExtensionsTests
    {
        [Test]
        public void ToSpecVersionJsonReturnsIntegerForVersion1()
        {
            // Arrange
            var version = new Version("v1.0");

            // Act
            var result = version.ToSpecVersionJson();

            // Assert
            Assert.That((int)result == 1, Is.True,
                "ToSpecVersionJson() should return the integer 1 for version 1"
            );
        }

        [Test]
        public void ToSpecVersionJsonReturnsStringForHigherVersions()
        {
            // Arrange
            var version = new Version("v1.2");

            // Act
            var result = version.ToSpecVersionJson();

            // Assert
            Assert.That((string)result == "v1.2", Is.True,
                "ToSpecVersionJson() should return a string for versions higher than 1."
            );
        }
    }
}
