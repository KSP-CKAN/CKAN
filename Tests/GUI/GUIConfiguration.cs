using System.IO;

using NUnit.Framework;

using CKAN;
using CKAN.IO;
using CKAN.GUI;
using Tests.Data;

namespace Tests.GUI
{
    [TestFixture]
    public class GuiConfigurationTests
    {
        [Test]
        public void LoadOrCreateConfiguration_MalformedXMLFile_ThrowsKraken()
        {
            // Arrange
            using (var inst = new DisposableKSP())
            {
                var xmlPath = Path.Combine(inst.KSP.CkanDir, "GUIConfig.xml");
                using (var stream = new StreamWriter(Path.Combine(inst.KSP.CkanDir,
                                                                  "GUIConfig.xml")))
                {
                    stream.Write("This is not a valid XML file.");
                }
                var steamLib = new SteamLibrary(null);

                // Act / Assert
                Assert.Throws<Kraken>(() => GUIConfiguration.LoadOrCreateConfiguration(inst.KSP, steamLib));
                Assert.IsTrue(File.Exists(xmlPath));
            }
        }

        [Test]
        public void LoadOrCreateConfiguration_CorrectConfigurationFile_Loaded()
        {
            // Arrange
            using (var inst = new DisposableKSP())
            {
                var xmlPath  = Path.Combine(inst.KSP.CkanDir, "GUIConfig.xml");
                var jsonPath = Path.Combine(inst.KSP.CkanDir, "GUIConfig.json");
                using (var stream = new StreamWriter(xmlPath))
                {
                    stream.Write(TestData.ConfigurationFile());
                }
                var steamLib = new SteamLibrary(null);

                // Act / Assert
                var result = GUIConfiguration.LoadOrCreateConfiguration(inst.KSP, steamLib);
                Assert.IsNotNull(result);
                Assert.AreEqual(512, result.WindowLoc.X);
                Assert.AreEqual(136, result.WindowLoc.Y);
                Assert.IsFalse(File.Exists(xmlPath));
                Assert.IsTrue(File.Exists(jsonPath));
            }
        }

        [Test]
        public void SetColumnVisibility_HideAndShow_Works()
        {
            // Arrange
            var cfg = new GUIConfiguration();

            // Act
            cfg.SetColumnVisibility("A", false);
            cfg.SetColumnVisibility("B", false);
            cfg.SetColumnVisibility("C", false);
            cfg.SetColumnVisibility("D", false);
            cfg.SetColumnVisibility("E", false);

            // Assert
            CollectionAssert.AreEquivalent(new string[] { "A", "B", "C", "D", "E" },
                                           cfg.HiddenColumnNames);

            // Act
            cfg.SetColumnVisibility("B", true);
            cfg.SetColumnVisibility("D", true);

            // Assert
            CollectionAssert.AreEquivalent(new string[] { "A", "C", "E" },
                                           cfg.HiddenColumnNames);
        }
    }
}
