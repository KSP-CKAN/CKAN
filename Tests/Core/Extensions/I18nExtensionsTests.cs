using NUnit.Framework;

using CKAN;
using CKAN.Extensions;
#if NETFRAMEWORK || WINDOWS
using CKAN.GUI;
#endif

namespace Tests.Core.Extensions
{
    [TestFixture]
    public class I18nExtensionsTests
    {
        [TestCase(ReleaseStatus.testing,     ExpectedResult = "Testing")]
        #if NETFRAMEWORK || WINDOWS
        [TestCase(RelationshipType.Suggests, ExpectedResult = "Suggests")]
        [TestCase(GUIModChangeType.Install,  ExpectedResult = "Install")]
        #endif
        public string LocalizeName_WithLocalizedEnums_Works<T>(T val) where T: System.Enum
            => val.LocalizeName();

        [TestCase(ReleaseStatus.testing,     ExpectedResult = "Pre-releases for adventurous users")]
        #if NETFRAMEWORK || WINDOWS
        [TestCase(RelationshipType.Suggests, ExpectedResult = "Suggests")]
        [TestCase(GUIModChangeType.Install,  ExpectedResult = "Install")]
        #endif
        public string LocalizeDescription_WithLocalizedEnums_Works<T>(T val) where T: System.Enum
            => val.LocalizeDescription();
    }
}
