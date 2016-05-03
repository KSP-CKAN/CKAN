using System;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Transformers;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class DownloadHashTransformerTests
    {
        [Test]
        public void AddsDownloadHash()
        {
            // Arrange
            const string downloadFilePath = "/DoesNotExist.zip";
            const string downloadHash = "47B6ED5F502AD914744882858345BE030A29E1AA";

            var mHttp = new Mock<IHttpService>();
            var mFileHash = new Mock<IFileHash>();

            mHttp.Setup(i => i.DownloadPackage(It.IsAny<Uri>(), It.IsAny<string>()))
                .Returns(downloadFilePath);

            mFileHash.Setup(i => i.GetFileHash(downloadFilePath))
                .Returns(downloadHash);

            var sut = new DownloadHashTransformer(mHttp.Object, mFileHash.Object);

            var json = new JObject();
            json["spec_version"] = 1;
            json["download"] = "https://awesomemod.example/AwesomeMod.zip";

            // Act
            var result = sut.Transform(new Metadata(json));
            var transformedJson = result.Json();

            // Assert
            Assert.That((string)transformedJson["download_hash"], Is.EqualTo(downloadHash),
                "DownloadHashTransformer should add a download_hash property equal to the calculated hash of the file."
            );
        }

        [Test]
        public void DoesNothingIfFileDoesNotExist()
        {
            // Arrange
            var mHttp = new Mock<IHttpService>();
            var mFileHash = new Mock<IFileHash>();

            mHttp.Setup(i => i.DownloadPackage(It.IsAny<Uri>(), It.IsAny<string>()))
                .Returns((string)null);

            var sut = new DownloadHashTransformer(mHttp.Object, mFileHash.Object);

            var json = new JObject();
            json["spec_version"] = 1;
            json["download"] = "https://awesomemod.example/AwesomeMod.zip";

            // Act
            var result = sut.Transform(new Metadata(json));
            var transformedJson = result.Json();

            // Assert
            Assert.That(transformedJson, Is.EqualTo(json),
                "DownloadHashTransformer should do nothing if the file does not exist."
            );
        }

        [Test]
        public void DoesNothingIfDownloadDoesNotExist()
        {
            // Arrange
            var mHttp = new Mock<IHttpService>();
            var mFileHash = new Mock<IFileHash>();

            var sut = new DownloadHashTransformer(mHttp.Object, mFileHash.Object);

            var json = new JObject();
            json["spec_version"] = 1;

            // Act
            var result = sut.Transform(new Metadata(json));
            var transformedJson = result.Json();

            // Assert
            Assert.That(transformedJson, Is.EqualTo(json),
                "DownloadHashTransformer should do nothing if the download property does not exist."
            );
        }
    }
}
