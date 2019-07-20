﻿using System.IO;
using CKAN;
using NUnit.Framework;
using Tests.Data;

namespace Tests.GUI
{
    [TestFixture]
    public class GuiConfigurationTests
    {
        string tempDir;

        [OneTimeSetUp]
        public void Setup()
        {
            tempDir = TestData.NewTempDir();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Directory.Delete(tempDir, true);
        }

        [Test]
        public void LoadOrCreateConfiguration_MalformedXMLFile_ThrowsKraken()
        {
            string tempFile = Path.Combine(tempDir, "invalid.xml");

            using (var stream = new StreamWriter(tempFile))
            {
                stream.Write("This is not a valid XML file.");
            }

            Assert.Throws<Kraken>(() => GUIConfiguration.LoadOrCreateConfiguration(tempFile));
        }

        [Test]
        public void LoadOrCreateConfiguration_CorrectConfigurationFile_Loaded()
        {
            string tempFile = Path.Combine(tempDir, "valid.xml");

            using (var stream = new StreamWriter(tempFile))
            {
                stream.Write(TestData.ConfigurationFile());
            }

            var result = GUIConfiguration.LoadOrCreateConfiguration(tempFile);

            Assert.IsNotNull(result);
        }
    }
}
