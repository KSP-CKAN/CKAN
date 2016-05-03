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
        
        [Test]
        public void GetsFileHashSha1Correctly()
        {
            // Arrange
            var sut = new FileService();

            // Act
            var result = sut.GetFileHashSha1(TestData.DogeCoinFlagZip());

            // Assert
            Assert.That(result, Is.EqualTo("47B6ED5F502AD914744882858345BE030A29E1AA"),
                "FileService should return the correct file SHA1 hash."
            );
        }
        
        [Test]
        public void GetsFileHashSha256Correctly()
        {
            // Arrange
            var sut = new FileService();

            // Act
            var result = sut.GetFileHashSha256(TestData.DogeCoinFlagZip());

            // Assert
            Assert.That(result, Is.EqualTo("EC955DB772FBA8CAA62BF61C180D624C350D792C6F573D35A5EAEE3898DCF7C1"),
                "FileService should return the correct file SHA256 hash."
            );
        }
    }
}
