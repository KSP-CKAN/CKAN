using CKAN.NetKAN.Services;
using NUnit.Framework;
using Tests.Data;

namespace Tests.NetKAN.Services
{
    [TestFixture]
    public sealed class FileHashTests
    {
        [Test]
        public void GetsFileHashCorrectly()
        {
            // Arrange
            var sut = new FileHash();

            // Act
            var result = sut.GetFileHash(TestData.DogeCoinFlagZip());

            // Assert
            Assert.That(result, Is.EqualTo("47B6ED5F502AD914744882858345BE030A29E1AA"),
                "FileService should return the correct file hash."
            );
        }
    }
}
