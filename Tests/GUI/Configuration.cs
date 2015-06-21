using System.IO;
using CKAN;
using NUnit.Framework;
using Tests.Data;

namespace Tests.GUI
{
    [TestFixture]
    public class ConfigurationTests
    {
        string tempDir;

        [TestFixtureSetUp]
        public void Setup()
        {
            tempDir = TestData.NewTempDir();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            Directory.Delete(tempDir, true);
        }

        [Test]
        public void LoadConfiguration_MalformedXMLFile_ThrowsKraken()
        {
            string tempFile = Path.Combine(tempDir, "invalid.xml");

            using (var stream = new StreamWriter(tempFile))
            {
                stream.Write("This is not a valid XML file.");
            }

            Assert.Throws<Kraken>(() => Configuration.LoadConfiguration(tempFile));
        }

        [Test]
        public void LoadConfiguration_CorrectConfigurationFile_Loaded()
        {
            string tempFile = Path.Combine(tempDir, "valid.xml");

            using (var stream = new StreamWriter(tempFile))
            {
                stream.Write(TestData.ConfigurationFile());
            }

            var result = Configuration.LoadConfiguration(tempFile);

            Assert.IsNotNull(result);
        }
    }
}
