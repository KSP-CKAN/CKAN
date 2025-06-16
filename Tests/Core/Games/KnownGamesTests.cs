using System.Linq;

using NUnit.Framework;

using CKAN.Games;
using CKAN.Games.KerbalSpaceProgram;
using CKAN.Games.KerbalSpaceProgram2;

namespace Tests.Core.Games
{
    [TestFixture]
    public sealed class KnownGamesTests
    {
        [Test]
        public void AllShortGameNames_Called_ReturnsKSPAndKSP2()
        {
            // Arrange / Act
            var names = KnownGames.AllGameShortNames().ToArray();

            // Act / Assert
            CollectionAssert.AreEquivalent(new string[] { "KSP", "KSP2"},
                                           names);
        }

        [Test]
        public void GameByShortName_EachGame_CorrectType()
        {
            // Arrange / Act
            var ksp  = KnownGames.GameByShortName("KSP");
            var ksp2 = KnownGames.GameByShortName("KSP2");
            var ksp3 = KnownGames.GameByShortName("KSP3");

            // Act/ Assert
            Assert.IsTrue(ksp  is KerbalSpaceProgram);
            Assert.IsTrue(ksp2 is KerbalSpaceProgram2);
            Assert.IsNull(ksp3);
        }
    }
}
