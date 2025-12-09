using System.Linq;

using Newtonsoft.Json.Linq;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;
using Moq;

using CKAN;
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
            modSvc.Setup(ms => ms.GetInternalSpaceWarpInfos(It.IsAny<CkanModule>(),
                                                            It.IsAny<ZipFile>(),
                                                            It.IsAny<string?>()))
                  .Returns(Enumerable.Repeat(
                      @"{
                          ""name"":        ""Mod with swinfo"",
                          ""author"":      ""Mod author"",
                          ""description"": ""A mod that contains a swinfo.json and gets many properties from it"",
                          ""version"":     ""1.0.0"",
                          ""dependencies"": [
                              { ""id"": ""Missing1"" },
                              { ""id"": ""Present1"" },
                              { ""id"": ""With.Name.Space.Prefix.Missing2"" },
                              { ""id"": ""With.Name.Space.Prefix.Present2"" },
                              { ""id"": ""missing3"" },
                              { ""id"": ""present3"" }
                          ]
                      }", 1));
            var game = new KerbalSpaceProgram();
            var loader = new SpaceWarpInfoLoader(http.Object, github.Object);
            var sut  = new SpaceWarpInfoValidator(http.Object,
                                                  loader,
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
