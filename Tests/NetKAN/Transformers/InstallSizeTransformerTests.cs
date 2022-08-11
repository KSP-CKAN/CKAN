using System.Linq;

using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Moq;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Transformers;
using Tests.Data;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class InstallSizeTransformerTests
    {
        [Test]
        public void Transform_NormalModule_CorrectInstallSize()
        {
            // Arrange
            var json = JObject.Parse(TestData.DogeCoinFlag_101());

            var mHttp = new Mock<IHttpService>();
            mHttp.Setup(i => i.DownloadModule(It.IsAny<Metadata>()))
                 .Returns(TestData.DogeCoinFlagZip());

            var modSvc = new ModuleService();

            ITransformer sut = new InstallSizeTransformer(mHttp.Object, modSvc);

            // Act
            var result = sut.Transform(new Metadata(json), opts).First();
            var transformedJson = result.Json();

            // Assert
            Assert.AreEqual(52291, (int)transformedJson["install_size"]);
        }

        private TransformOptions opts = new TransformOptions(1, null, null, false, null);
    }
}
