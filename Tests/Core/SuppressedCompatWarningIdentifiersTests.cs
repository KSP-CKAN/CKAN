using System.IO;
using System.Collections.Generic;

using NUnit.Framework;

using CKAN;
using CKAN.Versioning;
using Tests.Data;

namespace Tests.Core
{
    [TestFixture]
    public sealed class SuppressedCompatWarningIdentifiersTests
    {
        [Test]
        public void LoadFrom_GeneratedBySaveTo_Works()
        {
            // Arrange
            using (var dir = new TemporaryDirectory())
            {
                var gameVer     = new GameVersion(1, 12, 5);
                var identifiers = new HashSet<string>
                {
                    "BadVersionFileMod",
                    "NoVersionFileMod",
                    "OverlyCautiousMod",
                };
                var suppressed = new SuppressedCompatWarningIdentifiers()
                {
                    GameVersionWhenWritten = gameVer,
                    Identifiers            = identifiers,
                };
                var filename = Path.Combine(dir, "suppressed.json");

                // Act
                suppressed.SaveTo(filename);
                var deserialized = SuppressedCompatWarningIdentifiers.LoadFrom(gameVer, filename);

                // Assert
                CollectionAssert.AreEquivalent(identifiers, deserialized.Identifiers);
            }
        }

        [Test]
        public void LoadFrom_ZeroByteFile_Works()
        {
            // Arrange
            using (var dir = new TemporaryDirectory())
            {
                var gameVer  = new GameVersion(1, 12, 5);
                var filename = Path.Combine(dir, "suppressed.json");

                // Act
                File.WriteAllText(filename, "");
                var deserialized = SuppressedCompatWarningIdentifiers.LoadFrom(gameVer, filename);

                // Assert
                CollectionAssert.IsEmpty(deserialized.Identifiers);
            }
        }
    }
}
