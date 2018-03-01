using System;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Transformers;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class DownloadAttributeTransformerTests
    {
        [Test]
        public void AddsDownloadAttributes()
        {
            // Arrange
            const string downloadFilePath = "/DoesNotExist.zip";
            const string downloadHashSha1 = "47B6ED5F502AD914744882858345BE030A29E1AA";
            const string downloadHashSha256 = "EC955DB772FBA8CAA62BF61C180D624C350D792C6F573D35A5EAEE3898DCF7C1";
            const string downloadMimetype = "application/zip";
            const long downloadSize = 9001;

            var mHttp = new Mock<IHttpService>();
            var mFileService = new Mock<IFileService>();

            mHttp.Setup(i => i.DownloadPackage(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<DateTime?>()))
                .Returns(downloadFilePath);

            mFileService.Setup(i => i.GetFileHashSha1(downloadFilePath))
                .Returns(downloadHashSha1);

            mFileService.Setup(i => i.GetFileHashSha256(downloadFilePath))
                .Returns(downloadHashSha256);

            mFileService.Setup(i => i.GetSizeBytes(downloadFilePath))
                .Returns(downloadSize);

            mFileService.Setup(i => i.GetMimetype(downloadFilePath))
                .Returns(downloadMimetype);

            var sut = new DownloadAttributeTransformer(mHttp.Object, mFileService.Object);

            var json = new JObject();
            json["spec_version"] = 1;
            json["download"] = "https://awesomemod.example/AwesomeMod.zip";

            // Act
            var result = sut.Transform(new Metadata(json));
            var transformedJson = result.Json();

            // Assert
            Assert.That((string)transformedJson["download_hash"]["sha1"], Is.EqualTo(downloadHashSha1),
                "DownloadAttributeTransformer should add a 'sha1' property withing 'download_hash' equal to the sha1 of the file."
            );
            Assert.That((string)transformedJson["download_hash"]["sha256"], Is.EqualTo(downloadHashSha256),
                "DownloadAttributeTransformer should add a 'sha256' property withing 'download_hash' equal to the sha256 of the file."
            );
            Assert.That((long)transformedJson["download_size"], Is.EqualTo(downloadSize),
                "DownloadAttributeTransformer should add a download_size property equal to the size of the file in bytes."
            );
            Assert.That((string)transformedJson["download_content_type"], Is.EqualTo(downloadMimetype),
                "DownloadAttributeTransformer should add a download_content_type property equal to the Mimetype of the file."
            );
        }

        [Test]
        public void DoesNothingIfFileDoesNotExist()
        {
            // Arrange
            var mHttp = new Mock<IHttpService>();
            var mFileService = new Mock<IFileService>();

            mHttp.Setup(i => i.DownloadPackage(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<DateTime?>()))
                .Returns((string)null);

            var sut = new DownloadAttributeTransformer(mHttp.Object, mFileService.Object);

            var json = new JObject();
            json["spec_version"] = 1;
            json["download"] = "https://awesomemod.example/AwesomeMod.zip";

            // Act
            var result = sut.Transform(new Metadata(json));
            var transformedJson = result.Json();

            // Assert
            Assert.That(transformedJson, Is.EqualTo(json),
                "DownloadAttributeTransformer should do nothing if the file does not exist."
            );
        }

        [Test]
        public void DoesNothingIfDownloadDoesNotExist()
        {
            // Arrange
            var mHttp = new Mock<IHttpService>();
            var mFileService = new Mock<IFileService>();

            var sut = new DownloadAttributeTransformer(mHttp.Object, mFileService.Object);

            var json = new JObject();
            json["spec_version"] = 1;

            // Act
            var result = sut.Transform(new Metadata(json));
            var transformedJson = result.Json();

            // Assert
            Assert.That(transformedJson, Is.EqualTo(json),
                "DownloadAttributeTransformer should do nothing if the download property does not exist."
            );
        }
    }
}
