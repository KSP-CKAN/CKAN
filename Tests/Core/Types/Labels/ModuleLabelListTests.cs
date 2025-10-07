using NUnit.Framework;

using CKAN;
using Tests.Data;
using System.IO;

namespace Tests.Core
{
    [TestFixture]
    public class ModuleLabelListTests
    {
        [Test]
        public void Load_TestJSON_Works()
        {
            using (var inst = new DisposableKSP())
            {
                // Arrange / Act
                var lbls = ModuleLabelList.Load(TestData.LabelListPath())!;

                // Assert
                Assert.NotNull(lbls);
                CollectionAssert.AreEquivalent(new string[] { "Mod1" },
                                               lbls.HeldIdentifiers(inst.KSP));
                CollectionAssert.AreEquivalent(new string[] { "Mod2" },
                                               lbls.IgnoreMissingIdentifiers(inst.KSP));
            }
        }

        [Test]
        public void Save_WithLabels_Works()
        {
            using (var dir = new TemporaryDirectory())
            {
                // Arrange
                var lbls = new ModuleLabelList()
                {
                    Labels = new ModuleLabel[]
                    {
                        new ModuleLabel("Label1"),
                        new ModuleLabel("Label2"),
                        new ModuleLabel("Label3"),
                    }
                };
                var path = Path.Combine(dir, "labels.json");

                // Act
                lbls.Save(path);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                              @"{",
                                              @"  ""labels"": [",
                                              @"    {",
                                              @"      ""name"": ""Label1"",",
                                              @"      ""module_identifiers_by_game"": {}",
                                              @"    },",
                                              @"    {",
                                              @"      ""name"": ""Label2"",",
                                              @"      ""module_identifiers_by_game"": {}",
                                              @"    },",
                                              @"    {",
                                              @"      ""name"": ""Label3"",",
                                              @"      ""module_identifiers_by_game"": {}",
                                              @"    }",
                                              @"  ]",
                                              @"}",
                                          },
                                          File.ReadAllLines(path));
            }
        }
    }
}
