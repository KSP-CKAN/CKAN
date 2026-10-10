using System.Linq;

using NUnit.Framework;

using CKAN;
using CKAN.Versioning;

using Tests.Core.Relationships;

namespace Tests.Core.Registry
{
    [TestFixture]
    public class InstallHistoryDiffTests
    {
        [Test]
        public void Between_AllKindsOfChange_ReportedSortedByIdentifier()
        {
            // Arrange
            var older = Modpack(("Same",       "1.0"),
                                ("Upgraded",   "1.0"),
                                ("Downgraded", "2.0"),
                                ("Removed",    "1.0"));
            var newer = Modpack(("Added",      "1.0"),
                                ("Same",       "1.0"),
                                ("Upgraded",   "1.1"),
                                ("Downgraded", "1.9"));

            // Act
            var changes = InstallHistoryDiff.Between(older, newer);

            // Assert
            CollectionAssert.AreEqual(new (string, HistoryChangeType, string?, string?)[]
                                      {
                                          ("Added",      HistoryChangeType.Added,      null,  "1.0"),
                                          ("Downgraded", HistoryChangeType.Downgraded, "2.0", "1.9"),
                                          ("Removed",    HistoryChangeType.Removed,    "1.0", null),
                                          ("Upgraded",   HistoryChangeType.Upgraded,   "1.0", "1.1"),
                                      },
                                      changes.Select(c => (c.Identifier,
                                                           c.Type,
                                                           c.Before?.ToString(),
                                                           c.After?.ToString())));
        }

        [Test]
        public void Between_SameMods_NoChanges()
        {
            // Arrange
            var modpack = Modpack(("ModA", "1.0"), ("ModB", "2.0"));

            // Act
            var changes = InstallHistoryDiff.Between(modpack, modpack);

            // Assert
            CollectionAssert.IsEmpty(changes);
        }

        [Test]
        public void Between_NoDepends_DoesNotThrow()
        {
            // Arrange
            var empty = Modpack();
            empty.depends = null;
            var modpack = Modpack(("ModA", "1.0"));

            // Act
            var changes = InstallHistoryDiff.Between(empty, modpack);

            // Assert
            Assert.AreEqual(HistoryChangeType.Added, changes.Single().Type);
        }

        [Test]
        public void Between_UnversionedEntry_Ignored()
        {
            // Arrange
            var older   = Modpack(("ModA", null));
            var newer   = Modpack(("ModA", "1.0"));

            // Act
            var changes = InstallHistoryDiff.Between(older, newer);

            // Assert
            Assert.AreEqual(HistoryChangeType.Added, changes.Single().Type);
            Assert.AreEqual(new ModuleVersion("1.0"), changes.Single().After);
        }

        [Test]
        public void Between_NothingBefore_EverythingAdded()
        {
            // Arrange
            var newer = Modpack(("ModA", "1.0"), ("ModB", "2.0"));

            // Act
            var changes = InstallHistoryDiff.Between(null, newer);

            // Assert
            CollectionAssert.AreEqual(new string[] { "ModA", "ModB" },
                                      changes.Select(c => c.Identifier));
            Assert.IsTrue(changes.All(c => c.Type == HistoryChangeType.Added));
        }

        [Test]
        public void Between_MixedCaseIdentifiers_SortedIgnoringCase()
        {
            // Arrange
            var newer = Modpack(("Zebra", "1.0"), ("apple", "1.0"), ("Mango", "1.0"));

            // Act
            var changes = InstallHistoryDiff.Between(null, newer);

            // Assert
            CollectionAssert.AreEqual(new string[] { "apple", "Mango", "Zebra" },
                                      changes.Select(c => c.Identifier));
        }

        [Test]
        public void Type_SameVersionOnBothSides_Unchanged()
        {
            // Arrange
            var sut = new HistoryChange("ModA", new ModuleVersion("1.0"), new ModuleVersion("1.0"));

            // Act / Assert
            Assert.AreEqual(HistoryChangeType.Unchanged, sut.Type);
        }

        private static CkanModule Modpack(params (string name, string? version)[] mods)
            => CkanModule.FromJson(RelationshipResolverTests.MergeWithDefaults(
                   $@"{{
                       ""identifier"": ""MyModPack"",
                       ""kind"":       ""metapackage"",
                       ""depends"":    [
                           {string.Join(",", mods.Select(mod => mod.version == null
                                                                    ? $@"{{ ""name"": ""{mod.name}"" }}"
                                                                    : $@"{{ ""name"": ""{mod.name}"", ""version"": ""{mod.version}"" }}"))}
                       ]
                   }}"));
    }
}
