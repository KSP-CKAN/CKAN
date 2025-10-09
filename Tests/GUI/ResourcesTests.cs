#if NETFRAMEWORK || WINDOWS

using System;
using System.Linq;
using System.Resources;
using System.ComponentModel;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

using NUnit.Framework;

using CKAN;

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
            ResourceManager resources = new SingleAssemblyResourceManager(
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
        [TestCaseSource(nameof(DialogsAndControls))]
        public void ControlOrDialog_AllLocales_LanguageNotSetAndAllStrings(Type t)
        {
            try
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
            catch (MissingManifestResourceException)
            {
                // A few controls don't have resources, and that's OK
            }
        }

        // The types to test
        private static IEnumerable<Type> DialogsAndControls =>
            Assembly.GetAssembly(typeof(CKAN.GUI.Main))
                    ?.GetTypes()
                     .Where(t => !t.IsAbstract && baseTypesToCheck.Any(t.IsSubclassOf))
                    ?? Enumerable.Empty<Type>();

        private static readonly Type[] baseTypesToCheck = new Type[]
        {
            typeof(UserControl),
            typeof(Form),
        };

        // The cultures to test
        private static readonly CultureInfo[] cultures =
            Utilities.AvailableLanguages
                          .Select(l => new CultureInfo(l))
                          .ToArray();
    }
}

#endif
