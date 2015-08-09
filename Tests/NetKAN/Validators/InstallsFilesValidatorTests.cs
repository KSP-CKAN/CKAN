using CKAN;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Validators;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Tests.NetKAN.Validators
{
    [TestFixture]
    public sealed class InstallsFilesValidatorTests
    {
        [Test]
        public void DoesNotThrowWhenInstallableFiles()
        {
            // Arrange
             var mHttp = new Mock<IHttpService>();
            var mModuleService = new Mock<IModuleService>();

            mModuleService.Setup(i => i.HasInstallableFiles(It.IsAny<CkanModule>(), It.IsAny<string>()))
                .Returns(true);

            var json = new JObject();
            json["spec_version"] = 1;
            json["identifier"] = "AmazingMod";
            json["version"] = "1.0.0";
            json["download"] = "https://www.awesome-mod.example/AwesomeMod.zip";

            var sut = new InstallsFilesValidator(mHttp.Object, mModuleService.Object);

            // Act
            TestDelegate act = () => sut.Validate(new Metadata(json));

            // Assert
            Assert.That(act, Throws.Nothing,
                "InstallsFilesValidator should not throw when there are files to install."
            );
        }

        [Test]
        public void DoesThrowWhenNoInstallableFiles()
        {
            // Arrange
            var mHttp = new Mock<IHttpService>();
            var mModuleService = new Mock<IModuleService>();

            mModuleService.Setup(i => i.HasInstallableFiles(It.IsAny<CkanModule>(), It.IsAny<string>()))
                .Returns(false);

            var json = new JObject();
            json["spec_version"] = 1;
            json["identifier"] = "AmazingMod";
            json["version"] = "1.0.0";
            json["download"] = "https://www.awesome-mod.example/AwesomeMod.zip";

            var sut = new InstallsFilesValidator(mHttp.Object, mModuleService.Object);

            // Act
            TestDelegate act = () => sut.Validate(new Metadata(json));

            // Assert
            Assert.That(act, Throws.Exception,
                "InstallsFilesValidator should throw when there are no files to install."
            );
        }
    }
}
