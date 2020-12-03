using System;
using System.Linq;
using System.ComponentModel;
using System.Globalization;
using NUnit.Framework;

namespace Tests.GUI
{
    [TestFixture]
    public class ResourcesTests
    {
        /// <summary>
        /// .resx files should never have a $this.Language property because it
        /// does not deserialize properly in Windows when serialized on Mono 6+
        /// </summary>
        [Test,
            // Controls
            TestCase(typeof(CKAN.AllModVersions)),
            TestCase(typeof(CKAN.Changeset)),
            TestCase(typeof(CKAN.ChooseProvidedMods)),
            TestCase(typeof(CKAN.ChooseRecommendedMods)),
            TestCase(typeof(CKAN.DeleteDirectories)),
            TestCase(typeof(CKAN.EditModpack)),
            TestCase(typeof(CKAN.EditModSearch)),
            TestCase(typeof(CKAN.HintTextBox)),
            TestCase(typeof(CKAN.ManageMods)),
            TestCase(typeof(CKAN.ModInfo)),
            TestCase(typeof(CKAN.Wait)),

            // Dialogs
            TestCase(typeof(CKAN.Main)),
            TestCase(typeof(CKAN.AboutDialog)),
            TestCase(typeof(CKAN.AskUserForAutoUpdatesDialog)),
            TestCase(typeof(CKAN.CloneFakeGameDialog)),
            TestCase(typeof(CKAN.CompatibleGameVersionsDialog)),
            TestCase(typeof(CKAN.EditLabelsDialog)),
            TestCase(typeof(CKAN.ErrorDialog)),
            TestCase(typeof(CKAN.GameCommandLineOptionsDialog)),
            TestCase(typeof(CKAN.ManageGameInstancesDialog)),
            TestCase(typeof(CKAN.NewRepoDialog)),
            TestCase(typeof(CKAN.NewUpdateDialog)),
            TestCase(typeof(CKAN.PluginsDialog)),
            TestCase(typeof(CKAN.RenameInstanceDialog)),
            TestCase(typeof(CKAN.SelectionDialog)),
            TestCase(typeof(CKAN.YesNoDialog)),
        ]
        public void ControlOrDialog_LanguageResource_NotSet(Type t)
        {
            // Arrange
            ComponentResourceManager resources = new CKAN.SingleAssemblyComponentResourceManager(t);

            // Act/Assert
            foreach (CultureInfo resourceCulture in cultures)
            {
                Assert.IsNull(resources.GetObject("$this.Language", resourceCulture));
            }
        }

        // The cultures to test
        private static CultureInfo[] cultures = CKAN.Utilities.AvailableLanguages
            .Select(l => new CultureInfo(l))
            .ToArray();
    }
}
