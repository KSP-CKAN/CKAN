using System.Linq;

using Newtonsoft.Json.Linq;
using NUnit.Framework;
using ICSharpCode.SharpZipLib.Zip;
using Moq;

using CKAN;
using CKAN.SpaceWarp;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Transformers;
using CKAN.NetKAN.Sources.Github;
using Tests.Data;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class SpaceWarpInfoTransformerTests
    {
        [Test]
        public void Transform_WithSpaceWarpInfo_Works()
        {
            // Arrange
            var http     = new Mock<IHttpService>();
            http.Setup(h => h.DownloadModule(It.IsAny<Metadata>()))
                .Returns(TestData.DogeCoinFlagZip());
            var ghApi    = new Mock<IGithubApi>();
            var modSvc   = new Mock<IModuleService>();
            modSvc.Setup(ms => ms.GetInternalSpaceWarpInfo(It.IsAny<CkanModule>(),
                                                           It.IsAny<ZipFile>(),
                                                           It.IsAny<string?>()))
                  .Returns(new SpaceWarpInfo()
                           {
                               name        = "Mod with swinfo",
                               author      = "Mod author",
                               description = "A mod that contains a swinfo.json and gets many properties from it",
                               version     = "1.0.0",
                           });
            var sut      = new SpaceWarpInfoTransformer(http.Object,
                                                        ghApi.Object,
                                                        modSvc.Object);
            var opts     = new TransformOptions(1, null, null, null, false, null);
            var metadata = new Metadata(new JObject()
            {
                { "identifier", "SWInfoMod"                   },
                { "$vref",      "#/ckan/space-warp"           },
                { "download",   "https://github.com/download" },
            });

            // Act
            var result          = sut.Transform(metadata, opts).First();
            var transformedJson = result.Json();

            // Assert
            Assert.AreEqual("Mod with swinfo", (string?)transformedJson?["name"]);
            Assert.AreEqual("Mod author",      (string?)transformedJson?["author"]);
            Assert.AreEqual("A mod that contains a swinfo.json and gets many properties from it",
                            (string?)transformedJson?["abstract"]);
            Assert.AreEqual("1.0.0",           (string?)transformedJson?["version"]);
        }
    }
}
