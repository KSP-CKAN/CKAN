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
    public sealed class CkanValidatorTests
    {
        private static readonly JObject ValidCkan = new JObject();

        [SetUp]
        public void SetUp()
        {
            ValidCkan["spec_version"] = 1;
            ValidCkan["identifier"] = "AwesomeMod";
            ValidCkan["version"] = "1.0.0";
            ValidCkan["download"] = "https://www.awesome-mod.example/AwesomeMod.zip";
        }

        [Test]
        public void DoesNotThrowOnValidCkan()
        {
            // Arrange
            var mHttp = new Mock<IHttpService>();
            var mModuleService = new Mock<IModuleService>();

            mModuleService.Setup(i => i.HasInstallableFiles(It.IsAny<CkanModule>(), It.IsAny<string>()))
                .Returns(true);

            var netkan = new JObject();
            netkan["spec_version"] = 1;
            netkan["identifier"] = "AwesomeMod";

            var sut = new CkanValidator(new Metadata(netkan), mHttp.Object, mModuleService.Object);
            var json = (JObject)ValidCkan.DeepClone();

            // Act
            TestDelegate act = () => sut.Validate(new Metadata(json));

            // Assert
            Assert.That(act, Throws.Nothing,
                "CkanValidator should not throw when passed valid metadata."
            );
        }

        [TestCase("spec_version")]
        [TestCase("identifier")]
        [TestCase("version")]
        [TestCase("download")]
        public void DoesThrowWhenMissingProeprty(string propertyName)
        {
            // Arrange
            var mHttp = new Mock<IHttpService>();
            var mModuleService = new Mock<IModuleService>();

            mModuleService.Setup(i => i.HasInstallableFiles(It.IsAny<CkanModule>(), It.IsAny<string>()))
                .Returns(true);

            var sut = new CkanValidator(new Metadata(ValidCkan), mHttp.Object, mModuleService.Object);
            var json = (JObject)ValidCkan.DeepClone();
            json.Remove(propertyName);

            // Act
            TestDelegate act = () => sut.Validate(new Metadata(json));

            // Assert
            Assert.That(act, Throws.Exception,
                string.Format("CkanValidator should throw when {0} is missing.", propertyName)
            );
        }

        [Test]
        public void DoesThrowWhenIdentifiersDoNotMatch()
        {
            // Arrange
            var mHttp = new Mock<IHttpService>();
            var mModuleService = new Mock<IModuleService>();

            mModuleService.Setup(i => i.HasInstallableFiles(It.IsAny<CkanModule>(), It.IsAny<string>()))
                .Returns(true);

            var sut = new CkanValidator(new Metadata(ValidCkan), mHttp.Object, mModuleService.Object);
            var json = new JObject();
            json["spec_version"] = 1;
            json["identifier"] = "AmazingMod";

            // Act
            TestDelegate act = () => sut.Validate(new Metadata(json));

            // Assert
            Assert.That(act, Throws.Exception,
                "CkanValidator should throw when identifiers don't match."
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

            var netkan = new JObject();
            netkan["spec_version"] = 1;
            netkan["identifier"] = "AwesomeMod";

            var sut = new CkanValidator(new Metadata(netkan), mHttp.Object, mModuleService.Object);
            var json = (JObject)ValidCkan.DeepClone();

            // Act
            TestDelegate act = () => sut.Validate(new Metadata(json));

            // Assert
            Assert.That(act, Throws.Exception,
                "CkanValidator should throw when there are no files to install."
            );
        }
    }
}
