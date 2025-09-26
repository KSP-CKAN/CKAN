using NUnit.Framework;

using CKAN.Games.KerbalSpaceProgram.GameVersionProviders;

namespace Tests.Core.Games
{
    [TestFixture]
    public class KspBuildMapTests
    {
        [Test]
        [Category("Online")]
        public void KnownVersions_WithInstance_Works()
        {
            // Arrange
            var map = new KspBuildMap();

            // Act
            var result = map.KnownVersions;

            // Assert
            CollectionAssert.IsNotEmpty(result);
        }
    }
}
