using NUnit.Framework;

using CKAN.Games.KerbalSpaceProgram;
using CKAN.Games.KerbalSpaceProgram.GameVersionProviders;

namespace Tests.Core.Games
{
    [TestFixture]
    public class KspBuildMapTests
    {
        [Test]
        public void KnownVersions_Uncached_ReturnsEmbedded()
        {
            // Arrange
            var map = new KspBuildMap();

            // Act
            var result = map.KnownVersions;

            // Assert
            CollectionAssert.AreEqual(new KerbalSpaceProgram().EmbeddedGameVersions, result);
        }
    }
}
