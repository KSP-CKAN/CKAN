using System;
using System.Linq;
using System.Resources;
using System.ComponentModel;
using System.Globalization;
using System.Collections;

using NUnit.Framework;

namespace Tests.GUI
{
    [TestFixture]
    public class ResourcesTests
    {
        /// <summary>
        /// .resx files should never have a $this.Language property because it
        /// does not deserialize properly in Windows when serialized on Mono 6+
        ///
        /// This test covers the GUI/Properties/Resources.resx files.
        /// </summary>
        [Test]
        public void PropertiesResources_AllLocales_LanguageNotSetAndAllStrings()
        {
            // Arrange
            ResourceManager resources = new CKAN.GUI.SingleAssemblyResourceManager(
                "CKAN.GUI.Properties.Resources", typeof(CKAN.GUI.Properties.Resources).Assembly);

            // Act/Assert
            Assert.Multiple(() =>
            {
                foreach (CultureInfo resourceCulture in cultures)
                {
                    Assert.IsNull(resources.GetObject("$this.Language", resourceCulture));

                    var resSet = resources.GetResourceSet(resourceCulture, false, false);
                    if (resSet != null)
                    {
                        foreach (DictionaryEntry entry in resSet)
                        {
                            Assert.IsInstanceOf<string>(entry.Value,
                                $"Resource '{entry.Key}' in locale '{resourceCulture.Name}' is not a string");
                        }
                    }
                }
            });
        }

        /// <summary>
        /// .resx files should never have a $this.Language property because it
        /// does not deserialize properly in Windows when serialized on Mono 6+
        ///
        /// This test covers the controls and dialogs.
        /// </summary>
        [Test,
            // Controls
            TestCase(typeof(CKAN.GUI.Metadata)),
            TestCase(typeof(CKAN.GUI.Relationships)),
            TestCase(typeof(CKAN.GUI.Contents)),
            TestCase(typeof(CKAN.GUI.Versions)),
            TestCase(typeof(CKAN.GUI.Changeset)),
            TestCase(typeof(CKAN.GUI.ChooseProvidedMods)),
            TestCase(typeof(CKAN.GUI.ChooseRecommendedMods)),
            TestCase(typeof(CKAN.GUI.DeleteDirectories)),
            TestCase(typeof(CKAN.GUI.EditModpack)),
            TestCase(typeof(CKAN.GUI.EditModSearch)),
            TestCase(typeof(CKAN.GUI.InstallationHistory)),
            TestCase(typeof(CKAN.GUI.ManageMods)),
            TestCase(typeof(CKAN.GUI.ModInfo)),
            TestCase(typeof(CKAN.GUI.PlayTime)),
            TestCase(typeof(CKAN.GUI.UnmanagedFiles)),
            TestCase(typeof(CKAN.GUI.Wait)),

            // Dialogs
            TestCase(typeof(CKAN.GUI.Main)),
            TestCase(typeof(CKAN.GUI.AboutDialog)),
            TestCase(typeof(CKAN.GUI.AskUserForAutoUpdatesDialog)),
            TestCase(typeof(CKAN.GUI.CloneGameInstanceDialog)),
            TestCase(typeof(CKAN.GUI.CompatibleGameVersionsDialog)),
            TestCase(typeof(CKAN.GUI.DownloadsFailedDialog)),
            TestCase(typeof(CKAN.GUI.EditLabelsDialog)),
            TestCase(typeof(CKAN.GUI.ErrorDialog)),
            TestCase(typeof(CKAN.GUI.GameCommandLineOptionsDialog)),
            TestCase(typeof(CKAN.GUI.InstallFiltersDialog)),
            TestCase(typeof(CKAN.GUI.ManageGameInstancesDialog)),
            TestCase(typeof(CKAN.GUI.NewRepoDialog)),
            TestCase(typeof(CKAN.GUI.NewUpdateDialog)),
            TestCase(typeof(CKAN.GUI.PluginsDialog)),
            TestCase(typeof(CKAN.GUI.PreferredHostsDialog)),
            TestCase(typeof(CKAN.GUI.RenameInstanceDialog)),
            TestCase(typeof(CKAN.GUI.SelectionDialog)),
            TestCase(typeof(CKAN.GUI.YesNoDialog)),
        ]
        public void ControlOrDialog_AllLocales_LanguageNotSetAndAllStrings(Type t)
        {
            // Arrange
            ComponentResourceManager resources = new CKAN.GUI.SingleAssemblyComponentResourceManager(t);

            // Act/Assert
            Assert.Multiple(() =>
            {
                foreach (CultureInfo resourceCulture in cultures)
                {
                    Assert.IsNull(resources.GetObject("$this.Language", resourceCulture));

                    var resSet = resources.GetResourceSet(resourceCulture, false, false);
                    if (resSet != null)
                    {
                        foreach (DictionaryEntry entry in resSet)
                        {
                            Assert.IsInstanceOf<string>(entry.Value,
                                $"Resource '{t.Name} {entry.Key}' in locale '{resourceCulture.Name}' is not a string");
                        }
                    }
                }
            });
        }

        // The cultures to test
        private static readonly CultureInfo[] cultures = CKAN.Utilities.AvailableLanguages
            .Select(l => new CultureInfo(l))
            .ToArray();
    }
}
