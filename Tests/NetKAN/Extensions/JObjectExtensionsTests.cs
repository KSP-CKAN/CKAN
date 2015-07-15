using CKAN.NetKAN.Extensions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Tests.NetKAN.Extensions
{
    [TestFixture]
    public sealed class JObjectExtensionsTests
    {
        [Test]
        public void SafeAddAddsPropertyWhenItDoesNotExist()
        {
            // Arrange
            var sut = new JObject();

            // Act
            sut.SafeAdd("foo", "bar");

            // Assert
            Assert.That((string)sut["foo"], Is.EqualTo("bar"),
                "SafeAdd() should add property if it doesn't exist."
            );
        }

        [Test]
        public void SafeAddDoesNotAddPropertyWhenItAlreadyExists()
        {
            // Arrange
            var sut = new JObject();
            sut["foo"] = "bar";

            // Act
            sut.SafeAdd("foo", "baz");

            // Assert
            Assert.That((string)sut["foo"], Is.EqualTo("bar"),
                "SafeAdd() should not add property if it already exists."
            );
        }

        [Test]
        public void SafeAddDoesNotAddPropertyWhenValueIsNull()
        {
            // Arrange
            var sut = new JObject();
            sut["foo"] = "bar";

            // Act
            sut.SafeAdd("foo", null);

            // Assert
            Assert.That((string)sut["foo"], Is.EqualTo("bar"),
                "SafeAdd() should not add property if value is null."
            );
        }
    }
}
