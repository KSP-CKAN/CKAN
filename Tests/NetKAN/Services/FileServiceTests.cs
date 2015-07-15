using CKAN.NetKAN.Services;
using NUnit.Framework;
using Tests.Data;

namespace Tests.NetKAN.Services
{
    [TestFixture]
    public sealed class FileServiceTests
    {
        [Test]
        public void GetsFileSizeCorrectly()
        {
            // Arrange
            var sut = new FileService();

            // Act
            var result = sut.GetSizeBytes(TestData.DogeCoinFlagZip());

            // Assert
            Assert.That(result, Is.EqualTo(53647),
                "FileService should return the correct file size."
            );
        }
    }
}
