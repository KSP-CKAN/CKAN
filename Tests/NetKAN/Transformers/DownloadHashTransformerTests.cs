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
    public sealed class DownloadHashTransformerTests
    {
        [Test]
        public void AddsDownloadHash()
        {
            // Arrange
            const string downloadFilePath = "/DoesNotExist.zip";
            const string downloadHashSha1 = "47B6ED5F502AD914744882858345BE030A29E1AA";
            const string downloadHashSha256 = "EC955DB772FBA8CAA62BF61C180D624C350D792C6F573D35A5EAEE3898DCF7C1";

            var mHttp = new Mock<IHttpService>();
            var mFileService = new Mock<IFileService>();

            mHttp.Setup(i => i.DownloadPackage(It.IsAny<Uri>(), It.IsAny<string>()))
                .Returns(downloadFilePath);

            mFileService.Setup(i => i.GetFileHashSha1(downloadFilePath))
                .Returns(downloadHashSha1);

            mFileService.Setup(i => i.GetFileHashSha256(downloadFilePath))
                .Returns(downloadHashSha256);

            var sut = new DownloadHashTransformer(mHttp.Object, mFileService.Object);

            var json = new JObject();
            json["spec_version"] = 1;
            json["download"] = "https://awesomemod.example/AwesomeMod.zip";

            // Act
            var result = sut.Transform(new Metadata(json));
            var transformedJson = result.Json();

            // Collection Comparison
            var download_hashJson = new JObject();
            download_hashJson.SafeAdd("sha1", downloadHashSha1);
            download_hashJson.SafeAdd("sha256", downloadHashSha256);

            // Assert
            CollectionAssert.AreEquivalent(transformedJson["download_hash"],download_hashJson);
        }

        [Test]
        public void DoesNothingIfFileDoesNotExist()
        {
            // Arrange
            var mHttp = new Mock<IHttpService>();
            var mFileService = new Mock<IFileService>();

            mHttp.Setup(i => i.DownloadPackage(It.IsAny<Uri>(), It.IsAny<string>()))
                .Returns((string)null);

            var sut = new DownloadHashTransformer(mHttp.Object, mFileService.Object);

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
            var mFileService = new Mock<IFileService>();

            var sut = new DownloadHashTransformer(mHttp.Object, mFileService.Object);

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
