using System.Linq;

using Newtonsoft.Json.Linq;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;
using Moq;

using CKAN;
using CKAN.SpaceWarp;
using CKAN.NetKAN.Validators;
using CKAN.Games.KerbalSpaceProgram;
using CKAN.NetKAN.Sources.Github;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Model;
using Tests.Data;

namespace Tests.NetKAN.Validators
{
    [TestFixture]
    public class SpaceWarpInfoValidatorTests
    {
        [Test]
        public void Validate_WithMismatchedDeps_Warns()
        {
            // Arrange
            var http = new Mock<IHttpService>();
            http.Setup(h => h.DownloadModule(It.IsAny<Metadata>()))
                .Returns(TestData.DogeCoinFlagZip());
            var github = new Mock<IGithubApi>();
            var modSvc = new Mock<IModuleService>();
            modSvc.Setup(ms => ms.GetSpaceWarpInfo(It.IsAny<CkanModule>(),
                                                   It.IsAny<ZipFile>(),
                                                   It.IsAny<IGithubApi>(),
                                                   It.IsAny<IHttpService>(),
                                                   It.IsAny<string?>()))
                  .Returns(new SpaceWarpInfo()
                           {
                               name        = "Mod with swinfo",
                               author      = "Mod author",
                               description = "A mod that contains a swinfo.json and gets many properties from it",
                               version     = "1.0.0",
                               dependencies = new string[]
                                              {
                                                  "Missing1",
                                                  "Present1",
                                                  "With.Name.Space.Prefix.Missing2",
                                                  "With.Name.Space.Prefix.Present2",
                                                  "missing3",
                                                  "present3",
                                              }.Select(ident => new Dependency() { id = ident })
                                               .ToArray(),
                           });
            var game = new KerbalSpaceProgram();
            var sut  = new SpaceWarpInfoValidator(http.Object,
                                                  github.Object,
                                                  modSvc.Object);
            var metadata = new Metadata(new JObject()
            {
                { "identifier", "TestMod"                     },
                { "version",    "1.0"                         },
                { "download",   "https://github.com/download" },
                { "depends",    new JArray(new JObject() { { "name", "Present1" } },
                                           new JObject() { { "name", "Present2" } },
                                           new JObject() { { "name", "Present3" } }) },
            });
            using (var appender = new TemporaryWarningCapturer(nameof(SpaceWarpInfoValidator)))
            {
                // Act
                sut.Validate(metadata);

                // Assert
                CollectionAssert.AreEquivalent(
                    new string[]
                    {
                        "Dependencies from swinfo.json missing from module: Missing1, With.Name.Space.Prefix.Missing2, missing3"
                    },
                    appender.Warnings);
            }
        }
    }
}
