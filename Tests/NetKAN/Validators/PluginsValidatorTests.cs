using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Moq;

using CKAN.Games.KerbalSpaceProgram;
using CKAN.NetKAN.Validators;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Model;
using Tests.Data;

namespace Tests.NetKAN.Validators
{
    [TestFixture]
    public class PluginsValidatorTests
    {
        [Test]
        public void Validate_PluginModule_Warns()
        {
            // Arrange
            var jobj = new JObject()
            {
                { "identifier", "NotDogeCoinPlugin" },
                { "version",    "1.0"               },
                { "download",   "https://dogecoin.com/download" },
                { "install",    new JArray(new JObject() { { "find", "DogeCoinPlugin" },
                                                           { "install_to", "GameData" } }) },
            };
            var game   = new KerbalSpaceProgram();
            var modSvc = new ModuleService(game);
            var http   = new Mock<IHttpService>();
            http.Setup(h => h.DownloadModule(It.IsAny<Metadata>()))
                .Returns(TestData.DogeCoinPluginZip());
            var sut    = new PluginsValidator(http.Object, modSvc, game);
            using (var appender = new TemporaryWarningCapturer(nameof(PluginsValidator)))
            {
                // Act
                sut.Validate(new Metadata(jobj));

                // Assert
                CollectionAssert.AreEqual(
                    new string[]
                    {
                        "No plugin matching the identifier, manual installations won't be detected: GameData/DogeCoinPlugin/Plugins/DogeCoinPlugin.dll",
                        "Unbounded future compatibility for module with a plugin, consider setting $vref or ksp_version or ksp_version_max"
                    },
                    appender.Warnings);
            }
        }
    }
}
