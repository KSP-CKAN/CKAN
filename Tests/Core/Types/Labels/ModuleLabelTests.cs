using CKAN;
using CKAN.Games.KerbalSpaceProgram;
using NUnit.Framework;

namespace Tests.Core
{
    [TestFixture]
    public class ModuleLabelTests
    {
        [Test]
        public void AddRemove__Works()
        {
            // Arrange
            var lbl  = new ModuleLabel("TestLabel");
            var game = new KerbalSpaceProgram();

            // Act
            lbl.Add(game, "Mod1");
            lbl.Add(game, "Mod2");
            lbl.Add(game, "Mod3");
            lbl.Add(game, "Mod4");

            // Assert
            Assert.AreEqual(4, lbl.ModuleCount(game));
            CollectionAssert.AreEquivalent(new string[]
                                           {
                                               "Mod1",
                                               "Mod2",
                                               "Mod3",
                                               "Mod4",
                                           },
                                           lbl.IdentifiersFor(game));
            // Act
            lbl.Remove(game, "Mod2");
            lbl.Remove(game, "Mod4");

            // Assert
            Assert.AreEqual(2, lbl.ModuleCount(game));
            CollectionAssert.AreEquivalent(new string[]
                                           {
                                               "Mod1",
                                               "Mod3",
                                           },
                                           lbl.IdentifiersFor(game));

            // Act
            lbl.Remove(game, "Mod1");
            lbl.Remove(game, "Mod3");

            // Assert
            Assert.AreEqual(0, lbl.ModuleCount(game));
            CollectionAssert.IsEmpty(lbl.IdentifiersFor(game));
        }
    }
}
