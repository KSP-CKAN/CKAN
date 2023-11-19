using System.Linq;
using System.Resources;
using System.Globalization;
using NUnit.Framework;

namespace Tests.Core
{
    [TestFixture]
    public class ResourcesTests
    {
        /// <summary>
        /// .resx files should never have a $this.Language property because it
        /// does not deserialize properly in Windows when serialized on Mono 6+
        ///
        /// This test covers the Core/Properties/Resources.resx files.
        /// </summary>
        [Test]
        public void PropertiesResources_LanguageResource_NotSet()
        {
            // Arrange
            ResourceManager resources = new CKAN.SingleAssemblyResourceManager(
                "CKAN.Properties.Resources", typeof(CKAN.Properties.Resources).Assembly);

            // Act/Assert
            foreach (CultureInfo resourceCulture in cultures)
            {
                Assert.IsNull(resources.GetObject("$this.Language", resourceCulture));
            }
        }

        // The cultures to test
        private static readonly CultureInfo[] cultures = CKAN.Utilities.AvailableLanguages
            .Select(l => new CultureInfo(l))
            .ToArray();
    }
}
