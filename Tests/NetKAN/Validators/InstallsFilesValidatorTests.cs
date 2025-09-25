using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

using CKAN;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Validators;
using CKAN.Games.KerbalSpaceProgram;
using Tests.Data;
using System;

namespace Tests.NetKAN.Validators
{
    [TestFixture]
    public sealed class InstallsFilesValidatorTests
    {
        [Test]
        public void Validate_InstallableFiles_DoesNotThrow()
        {
            // Arrange
            var mHttp = new Mock<IHttpService>();

            var mModuleService = new Mock<IModuleService>();
            mModuleService.Setup(i => i.HasInstallableFiles(It.IsAny<CkanModule>(), It.IsAny<string>()))
                          .Returns(true);

            var json = new JObject()
            {
                { "spec_version", 1 },
                { "identifier",   "AmazingMod" },
                { "author",       "AmazingModder" },
                { "version",      "1.0.0" },
                { "download",     "https://www.awesome-mod.example/AwesomeMod.zip" },
            };

            var sut = new InstallsFilesValidator(mHttp.Object, mModuleService.Object,
                                                 new KerbalSpaceProgram());

            // Act
            TestDelegate act = () => sut.Validate(new Metadata(json));

            // Assert
            Assert.That(act, Throws.Nothing,
                        "InstallsFilesValidator should not throw when there are files to install.");
        }

        [Test]
        public void Validate_NoInstallableFiles_Throws()
        {
            // Arrange
            var mHttp = new Mock<IHttpService>();
            mHttp.Setup(i => i.DownloadModule(It.IsAny<Metadata>()))
                 .Returns("");

            var mModuleService = new Mock<IModuleService>();
            mModuleService.Setup(i => i.HasInstallableFiles(It.IsAny<CkanModule>(), It.IsAny<string>()))
                          .Returns(false);

            var json = new JObject()
            {
                { "spec_version", 1 },
                { "identifier",   "AmazingMod" },
                { "author",       "AmazingModder" },
                { "version",      "1.0.0" },
                { "download",     "https://www.awesome-mod.example/AwesomeMod.zip" },
            };

            var sut = new InstallsFilesValidator(mHttp.Object, mModuleService.Object,
                                                 new KerbalSpaceProgram());

            // Act
            TestDelegate act = () => sut.Validate(new Metadata(json));

            // Assert
            Assert.That(act, Throws.Exception,
                        "InstallsFilesValidator should throw when there are no files to install.");
        }

        [Test]
        public void Validate_GameDataWithinGameData_Throws()
        {
            // Arrange
            var game   = new KerbalSpaceProgram();
            var http   = new Mock<IHttpService>();
            http.Setup(i => i.DownloadModule(It.IsAny<Metadata>()))
                .Returns(TestData.DogeCoinFlagZip());
            var modSvc = new ModuleService(game);
            var sut    = new InstallsFilesValidator(http.Object, modSvc, game);
            var jobj   = new JObject()
            {
                { "identifier", "DumbCoinFlag" },
                { "version",    "1.0" },
                { "download",   "https://dumbcoinflag.com/download" },
                {
                    "install",
                    new JArray()
                    {
                        new JObject()
                        {
                            { "find",       "DogeCoinFlag" },
                            { "install_to", "GameData/DogeCoinFlag/GameData" },
                        },
                    }
                },
            };

            // Act / Assert
            var exc = Assert.Throws<Kraken>(() => sut.Validate(new Metadata(jobj)))!;
            CollectionAssert.AreEqual(new string[]
                                      {
                                          "GameData directory found within GameData:",
                                          "GameData/DogeCoinFlag/GameData/DogeCoinFlag/Flags/dogecoin.png",
                                      },
                                      exc.Message.Split(new string[] { Environment.NewLine },
                                                        StringSplitOptions.None));
        }

        [Test]
        public void Validate_DuplicateDestinations_Throws()
        {
            // Arrange
            var game   = new KerbalSpaceProgram();
            var http   = new Mock<IHttpService>();
            http.Setup(i => i.DownloadModule(It.IsAny<Metadata>()))
                .Returns(TestData.DogeCoinFlagZip());
            var modSvc = new ModuleService(game);
            var sut    = new InstallsFilesValidator(http.Object, modSvc, game);
            var jobj   = new JObject()
            {
                { "identifier", "DumbCoinFlag" },
                { "version",    "1.0" },
                { "download",   "https://dumbcoinflag.com/download" },
                {
                    "install",
                    new JArray()
                    {
                        new JObject()
                        {
                            { "find",       "DogeCoinFlag" },
                            { "install_to", "GameData" },
                        },
                        new JObject()
                        {
                            { "find",       "DogeCoinFlag" },
                            { "install_to", "GameData" },
                        },
                    }
                },
            };

            // Act / Assert
            var exc = Assert.Throws<Kraken>(() => sut.Validate(new Metadata(jobj)))!;
            CollectionAssert.AreEqual(new string[]
                                      {
                                          "Multiple files attempted to install to:",
                                          "GameData/DogeCoinFlag/Flags/dogecoin.png",
                                      },
                                      exc.Message.Split(new string[] { Environment.NewLine },
                                                        StringSplitOptions.None));
        }
    }
}
