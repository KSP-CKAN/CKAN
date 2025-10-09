#if NETFRAMEWORK || WINDOWS

using NUnit.Framework;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using CKAN.GUI;

namespace Tests.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    [TestFixture]
    public class EmbeddedImagesTests
    {
        [Test]
        public void AllProperties_Get_ReturnBitmaps()
        {
            // Assert
            Assert.IsNotNull(EmbeddedImages.alert);
            Assert.IsNotNull(EmbeddedImages.apply);
            Assert.IsNotNull(EmbeddedImages.backward);
            Assert.IsNotNull(EmbeddedImages.ballot);
            Assert.IsNotNull(EmbeddedImages.checkAll);
            Assert.IsNotNull(EmbeddedImages.checkRecommendations);
            Assert.IsNotNull(EmbeddedImages.checkSuggestions);
            Assert.IsNotNull(EmbeddedImages.collapseAll);
            Assert.IsNotNull(EmbeddedImages.delete);
            Assert.IsNotNull(EmbeddedImages.expandAll);
            Assert.IsNotNull(EmbeddedImages.file);
            Assert.IsNotNull(EmbeddedImages.filter);
            Assert.IsNotNull(EmbeddedImages.folder);
            Assert.IsNotNull(EmbeddedImages.folderZip);
            Assert.IsNotNull(EmbeddedImages.forward);
            Assert.IsNotNull(EmbeddedImages.info);
            Assert.IsNotNull(EmbeddedImages.ksp);
            Assert.IsNotNull(EmbeddedImages.refresh);
            Assert.IsNotNull(EmbeddedImages.refreshStale);
            Assert.IsNotNull(EmbeddedImages.refreshVeryStale);
            Assert.IsNotNull(EmbeddedImages.resetCollapse);
            Assert.IsNotNull(EmbeddedImages.search);
            Assert.IsNotNull(EmbeddedImages.settings);
            Assert.IsNotNull(EmbeddedImages.smile);
            Assert.IsNotNull(EmbeddedImages.star);
            Assert.IsNotNull(EmbeddedImages.stop);
            Assert.IsNotNull(EmbeddedImages.textClear);
            Assert.IsNotNull(EmbeddedImages.thumbup);
            Assert.IsNotNull(EmbeddedImages.triToggleBoth);
            Assert.IsNotNull(EmbeddedImages.triToggleNo);
            Assert.IsNotNull(EmbeddedImages.triToggleYes);
            Assert.IsNotNull(EmbeddedImages.uncheckAll);
            Assert.IsNotNull(EmbeddedImages.update);
        }
    }
}

#endif
