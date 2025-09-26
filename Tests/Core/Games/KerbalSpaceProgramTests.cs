using System.Linq;

using NUnit.Framework;

using CKAN.Versioning;
using CKAN.Games.KerbalSpaceProgram;

namespace Tests.Core.Games
{
    [TestFixture]
    public class KerbalSpaceProgramTests
    {
        [Test, TestCase("1.12.5",
                        new string[] { "1.8", "1.9", "1.10", "1.11", "1.12" }),
               TestCase("1.12.0",
                        new string[] { "1.8", "1.9", "1.10", "1.11", "1.12" }),
               TestCase("1.10.1",
                        new string[] { "1.8", "1.9", "1.10" }),
               TestCase("1.8.0",
                        new string[] { "1.8" }),
               TestCase("1.7.3",
                        new string[] { "1.4", "1.5", "1.6", "1.7" }),
               TestCase("1.5.1",
                        new string[] { "1.4", "1.5" }),
               TestCase("1.3.1",
                        new string[] { "1.2", "1.3" }),
               TestCase("1.2.9",
                        new string[] { }),
               TestCase("1.0.5",
                        new string[] { "1.0" })]
        public void DefaultCompatibleVersions_RealVersion_CorrectRange(string   installedVersion,
                                                                       string[] correctCompatVersions)
        {
            // Arrange
            var game = new KerbalSpaceProgram();

            // Act
            var compat = game.DefaultCompatibleVersions(GameVersion.Parse(installedVersion));

            // Assert
            CollectionAssert.AreEqual(correctCompatVersions.Select(GameVersion.Parse),
                                      compat);
        }
    }
}
