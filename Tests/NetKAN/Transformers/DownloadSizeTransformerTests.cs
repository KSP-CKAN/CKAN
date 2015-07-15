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
    public sealed class DownloadSizeTransformerTests
    {
        [Test]
        public void AddsDownloadSize()
        {
            // Arrange
            const string downloadFilePath = "/DoesNotExist.zip";
            const long downloadSize = 9001;

            var mHttp = new Mock<IHttpService>();
            var mFileService = new Mock<IFileService>();

            mHttp.Setup(i => i.DownloadPackage(It.IsAny<Uri>(), It.IsAny<string>()))
                .Returns(downloadFilePath);

            mFileService.Setup(i => i.GetSizeBytes(downloadFilePath))
                .Returns(downloadSize);

            var sut = new DownloadSizeTransformer(mHttp.Object, mFileService.Object);

            var json = new JObject();
            json["spec_version"] = 1;
            json["download"] = "https://awesomemod.example/AwesomeMod.zip";

            // Act
            var result = sut.Transform(new Metadata(json));
            var transformedJson = result.Json();

            // Assert
            Assert.That((long)transformedJson["download_size"], Is.EqualTo(downloadSize),
                "DownloadSizeTransformer should add a download_size property equal to the size of the file in bytes."
            );
        }

        [Test]
        public void DoesNothingIfFileDoesNotExist()
        {
            // Arrange
            var mHttp = new Mock<IHttpService>();
            var mFileService = new Mock<IFileService>();

            mHttp.Setup(i => i.DownloadPackage(It.IsAny<Uri>(), It.IsAny<string>()))
                .Returns((string)null);

            var sut = new DownloadSizeTransformer(mHttp.Object, mFileService.Object);

            var json = new JObject();
            json["spec_version"] = 1;
            json["download"] = "https://awesomemod.example/AwesomeMod.zip";

            // Act
            var result = sut.Transform(new Metadata(json));
            var transformedJson = result.Json();

            // Assert
            Assert.That(transformedJson, Is.EqualTo(json),
                "DownloadSizeTransformer should do nothing if the file does not exist."
            );
        }

        [Test]
        public void DoesNothingIfDownloadDoesNotExist()
        {
            // Arrange
            var mHttp = new Mock<IHttpService>();
            var mFileService = new Mock<IFileService>();

            var sut = new DownloadSizeTransformer(mHttp.Object, mFileService.Object);

            var json = new JObject();
            json["spec_version"] = 1;

            // Act
            var result = sut.Transform(new Metadata(json));
            var transformedJson = result.Json();

            // Assert
            Assert.That(transformedJson, Is.EqualTo(json),
                "DownloadSizeTransformer should do nothing if the download property does not exist."
            );
        }
    }
}
