using System;
using System.Linq;

using NUnit.Framework;
using Moq;

using CKAN;
using CKAN.Extensions;
using CKAN.Games.KerbalSpaceProgram;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Processors;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Transformers;
using Tests.Data;

namespace Tests.NetKAN.Processors
{
    [TestFixture]
    public class InflatorTests
    {
        [Test]
        public void Inflate_WithTestNetkan_Inflates()
        {
            // Arrange
            using (var cacheDir = new TemporaryDirectory())
            {
                var http     = new Mock<IHttpService>();
                var game     = new KerbalSpaceProgram();
                var cache    = new NetFileCache(cacheDir.Directory.FullName);
                var modSvc   = new ModuleService(game);
                var fileSvc  = new FileService(cache);
                var sut      = new Inflator(null, null, null, null,
                                            game, cache, http.Object, modSvc, fileSvc);
                var filename = TestData.TestNetkanPath();
                var netkans  = YamlExtensions.Parse(TestData.TestNetkanContents())
                                             .Select(yaml => new Metadata(yaml))
                                             .ToArray();
                var opts     = new TransformOptions(1, 0, null, null, false, null);
                http.Setup(h => h.DownloadText(It.Is<Uri>(u => u.AbsolutePath.StartsWith("/repos/")
                                                               && !u.AbsolutePath.EndsWith("/releases")),
                                               It.IsAny<string?>(), It.IsAny<string?>()))
                    .Returns(@"{
                                 ""name"": ""Example Project"",
                                 ""owner"": {
                                     ""login"": ""authoruser"",
                                     ""type"":  ""User""
                                 },
                                 ""license"": {
                                     ""spdx_id"": ""GPL-3.0""
                                 },
                                 ""html_url"": ""https://github.com/ExampleAccount/ExampleProject"",
                             }");
                http.Setup(h => h.DownloadText(It.Is<Uri>(u => u.AbsolutePath.StartsWith("/repos/")
                                                               && u.AbsolutePath.EndsWith("/releases")),
                                               It.IsAny<string?>(), It.IsAny<string?>()))
                    .Returns(@"[
                                 {
                                     ""author"": {
                                         ""login"": ""ExampleProject""
                                     },
                                     ""tag_name"": ""1.0"",
                                     ""published_at"": ""2025-01-05T00:00:00Z"",
                                     ""assets"": [
                                         {
                                             ""name"":                 ""download.zip"",
                                             ""browser_download_url"": ""http://github.example/download/1.0""
                                         }
                                     ]
                                 }
                             ]");
                http.Setup(h => h.DownloadModule(It.IsAny<Metadata>()))
                    .Returns(TestData.DogeCoinFlagImportableZip());

                // Act
                var metadatas = sut.Inflate(filename, netkans, opts)
                                   .ToArray();

                // Assert
                CollectionAssert.IsNotEmpty(metadatas);
            }
        }

        [Test]
        public void ValidateCkan_ValidModule_DoesNotThrow()
        {
            // Arrange
            using (var cacheDir = new TemporaryDirectory())
            {
                var game    = new KerbalSpaceProgram();
                var cache   = new NetFileCache(cacheDir.Directory.FullName);
                var http    = new Mock<IHttpService>();
                var modSvc  = new ModuleService(game);
                var fileSvc = new FileService(cache);
                var sut     = new Inflator(null, null, null, null,
                                           game, cache, http.Object, modSvc, fileSvc);
                var ckans   = YamlExtensions.Parse(TestData.DogeCoinPlugin())
                                            .Select(yaml => new Metadata(yaml))
                                            .ToArray();
                http.Setup(h => h.DownloadModule(It.IsAny<Metadata>()))
                    .Returns(TestData.DogeCoinPluginZip());

                // Act / Assert
                Assert.DoesNotThrow(() => sut.ValidateCkan(ckans.Single()));
            }
        }
    }
}
