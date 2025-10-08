using System.IO;

using NUnit.Framework;

using CKAN;
using Tests.Data;

namespace Tests.Core
{
    [TestFixture]
    public sealed class ModuleTagListTests
    {
        [Test]
        public void Load_FromSave_Works()
        {
            // Arrange
            using (var dir = new TemporaryDirectory())
            {
                var path = Path.Combine(dir, "mtl.json");
                var orig = new ModuleTagList();

                // Act
                orig.HiddenTags.Add("library");
                orig.HiddenTags.Add("flags");
                orig.HiddenTags.Add("agency");
                orig.Save(path);

                var loaded = ModuleTagList.Load(path)!;

                // Assert
                CollectionAssert.AreEquivalent(new string[]
                                               {
                                                   "library",
                                                   "flags",
                                                   "agency",
                                               },
                                               loaded.HiddenTags);
            }
        }
    }
}
