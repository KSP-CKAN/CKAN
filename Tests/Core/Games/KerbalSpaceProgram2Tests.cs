using System.Linq;

using NUnit.Framework;

using CKAN.Versioning;
using CKAN.Games.KerbalSpaceProgram2;

namespace Tests.Core.Games
{
    [TestFixture]
    public class KerbalSpaceProgram2Tests
    {
        [Test, TestCase("0.1.0.0",
                        new string[] { "0.1", "0.2" }),
               TestCase("0.1.1.0",
                        new string[] { "0.1", "0.2" }),
               TestCase("0.2.0.0",
                        new string[] { "0.1", "0.2" }),
               TestCase("0.2.2.0",
                        new string[] { "0.1", "0.2" }),
        ]
        public void DefaultCompatibleVersions_RealVersion_CorrectRange(string   installedVersion,
                                                                       string[] correctCompatVersions)
        {
            // Arrange
            var game = new KerbalSpaceProgram2();

            // Act
            var compat = game.DefaultCompatibleVersions(GameVersion.Parse(installedVersion));

            // Assert
            CollectionAssert.AreEqual(correctCompatVersions.Select(GameVersion.Parse),
                                      compat);
        }
    }
}
