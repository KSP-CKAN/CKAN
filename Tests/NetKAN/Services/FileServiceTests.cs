using System.IO;
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
        
        [Test]
        public void GetsAsciiMimeCorrectly()
        {
            // Arrange
            var sut = new FileService();

            // Act
            var result = sut.GetMimetype(TestData.DataDir("FileIdentifier/test_ascii.txt"));

            // Assert
            Assert.That(result, Is.EqualTo("text/plain"),
                "FileService should return the correct mimetype for a text file"
            );
        }
        
        [Test]
        public void GetsGzipMimeCorrectly()
        {
            // Arrange
            var sut = new FileService();

            // Act
            var result = sut.GetMimetype(TestData.DataDir("FileIdentifier/test_gzip.gz"));

            // Assert
            Assert.That(result, Is.EqualTo("application/x-gzip"),
                "FileService should return the correct mimetype for a gzip file."
            );
        }
        
        [Test]
        public void GetsTarMimeCorrectly()
        {
            // Arrange
            var sut = new FileService();

            // Act
            var result = sut.GetMimetype(TestData.DataDir("FileIdentifier/test_tar.tar"));

            // Assert
            Assert.That(result, Is.EqualTo("application/x-tar"),
                "FileService should return the correct mimetype for a tar file."
            );
        }
        
        [Test]
        public void GetsTarGzMimeCorrectly()
        {
            // Arrange
            var sut = new FileService();

            // Act
            var result = sut.GetMimetype(TestData.DataDir("FileIdentifier/test_targz.tar.gz"));

            // Assert
            Assert.That(result, Is.EqualTo("application/x-compressed-tar"),
                "FileService should return the correct mimetype for a tar.gz file."
            );
        }
        
        [Test]
        public void GetsZipMimeCorrectly()
        {
            // Arrange
            var sut = new FileService();

            // Act
            var result = sut.GetMimetype(TestData.DogeCoinFlagZip());

            // Assert
            Assert.That(result, Is.EqualTo("application/zip"),
                "FileService should return the correct mimetype for a zipfile."
            );
        }
        
        [Test]
        public void GetsUnknownMimeCorrectly()
        {
            // Arrange
            var sut = new FileService();
            string random_bin = TestData.DataDir("FileIdentifier/random.bin");
            Assert.IsTrue(File.Exists(random_bin));

            // Act
            var result = sut.GetMimetype(random_bin);

            // Assert
            Assert.That(result, Is.EqualTo("application/octet-stream"),
                "FileService should return 'application/octet-stream' for all other file types."
            );
        }
    }
}
