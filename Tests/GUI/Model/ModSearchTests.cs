using System.Collections.Generic;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using NUnit.Framework;

using CKAN;
using CKAN.GUI;
using Tests.Data;

namespace Tests.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    [TestFixture]
    public class ModSearchTests
    {
        [Test]
        public void Constructor_AllProperties_CombinedCorrect()
        {
            // Arrange
            var labels = ModuleLabelList.GetDefaultLabels();
            using (var inst = new DisposableKSP())
            {
                // Act
                var sut = new ModSearch(labels, inst.KSP,
                                        "modname",
                                        new List<string> { "author1" },
                                        "description",
                                        new List<string> { "MIT" },
                                        new List<string> { "en-us" },
                                        new List<string> { "ModuleManager" },
                                        new List<string> { "Kopernicus" },
                                        new List<string> { "Scatterer" },
                                        new List<string> { "JNSQ" },
                                        new List<string> { "RasterPropMonitor" },
                                        new List<string> { "planet-pack" },
                                        new List<string> { "Favorites" },
                                        true, true, false, null, null, null);

                // Assert
                Assert.AreEqual("modname @author1 desc:description lic:MIT lang:en-us dep:ModuleManager rec:Kopernicus sug:Scatterer conf:JNSQ sup:RasterPropMonitor tag:planet-pack label:Favorites is:compatible is:installed not:cached",
                                sut.Combined);
            }
        }

        [Test]
        public void Constructor_GUIModFilter_CombinedCorrect()
        {
            // Arrange
            var labels = ModuleLabelList.GetDefaultLabels();
            using (var inst = new DisposableKSP())
            {
                // Act
                var sut = new ModSearch(labels, inst.KSP, GUIModFilter.Compatible);

                // Assert
                Assert.AreEqual("is:compatible", sut.Combined);
            }
        }

        [Test]
        public void FromAuthors_Multiple_CombinedCorrect()
        {
            // Arrange
            var labels = ModuleLabelList.GetDefaultLabels();
            using (var inst = new DisposableKSP())
            {
                // Act
                var sut = ModSearch.FromAuthors(labels, inst.KSP,
                                                new string[] { "Nertea", "linuxgurugamer", "JonnyOThan" });

                // Assert
                Assert.AreEqual("@Nertea @linuxgurugamer @JonnyOThan", sut.Combined);
            }
        }

        [Test]
        public void Parse_EmptySearchString_Null()
        {
            // Arrange
            var labels = ModuleLabelList.GetDefaultLabels();
            using (var inst = new DisposableKSP())
            {

                // Act / Assert
                Assert.IsNull(ModSearch.Parse(labels, inst.KSP, ""));
            }
        }

        [Test]
        public void Parse_SearchString_ParsedCorrectly()
        {
            // Arrange
            var labels = ModuleLabelList.GetDefaultLabels();
            using (var inst = new DisposableKSP())
            {

                // Act
                var sut = ModSearch.Parse(labels, inst.KSP,
                                          "modname @author1 desc:description lic:MIT lang:en-us dep:ModuleManager rec:Kopernicus sug:Scatterer conf:JNSQ sup:RasterPropMonitor tag:planet-pack label:Favorites is:compatible is:installed not:cached")!;

                // Assert
                Assert.AreEqual("modname", sut.Name);
                Assert.AreEqual("description", sut.Description);
                CollectionAssert.AreEquivalent(new string[] { "author1" },
                                               sut.Authors);
                CollectionAssert.AreEquivalent(new string[] { "ModuleManager" },
                                               sut.DependsOn);
                CollectionAssert.AreEquivalent(new string[] { "Kopernicus" },
                                               sut.Recommends);
                CollectionAssert.AreEquivalent(new string[] { "Scatterer" },
                                               sut.Suggests);
                CollectionAssert.AreEquivalent(new string[] { "JNSQ" },
                                               sut.ConflictsWith);
                CollectionAssert.AreEquivalent(new string[] { "RasterPropMonitor" },
                                               sut.Supports);
                CollectionAssert.AreEquivalent(new string[] { "planet-pack" },
                                               sut.TagNames);
                CollectionAssert.AreEquivalent(new string[] { "Favorites" },
                                               sut.LabelNames);
                Assert.IsTrue(sut.Compatible);
                Assert.IsTrue(sut.Installed);
                Assert.IsFalse(sut.Cached);
                Assert.IsNull(sut.NewlyCompatible);
                Assert.IsNull(sut.Upgradeable);
                Assert.IsNull(sut.Replaceable);
            }
        }

        [Test]
        public void MergedWith_AnotherSearch_Correct()
        {
            // Arrange
            var labels = ModuleLabelList.GetDefaultLabels();
            using (var inst = new DisposableKSP())
            {
                var search1 = ModSearch.Parse(labels, inst.KSP,
                                           "modname desc:description lang:en-us rec:Kopernicus conf:JNSQ tag:planet-pack is:compatible not:cached")!;
                var search2 = ModSearch.Parse(labels, inst.KSP,
                                           "@author1 lic:MIT dep:ModuleManager sug:Scatterer sup:RasterPropMonitor label:Favorites is:installed")!;

                // Act
                var sut = search1.MergedWith(labels, search2);

                // Assert
                Assert.AreEqual("modname @author1 desc:description lic:MIT lang:en-us dep:ModuleManager rec:Kopernicus sug:Scatterer conf:JNSQ sup:RasterPropMonitor tag:planet-pack label:Favorites is:compatible is:installed not:cached",
                                sut.Combined);
            }
        }

        [Test]
        public void Equals_Same_True()
        {
            // Arrange
            var labels = ModuleLabelList.GetDefaultLabels();
            using (var inst = new DisposableKSP())
            {
                var searchString = "modname @author1 desc:description lic:MIT lang:en-us dep:ModuleManager rec:Kopernicus sug:Scatterer conf:JNSQ sup:RasterPropMonitor tag:planet-pack label:Favorites is:compatible is:installed not:cached";
                var search1 = ModSearch.Parse(labels, inst.KSP, searchString)!;
                var search2 = ModSearch.Parse(labels, inst.KSP, searchString)!;

                // Act / Assert
                Assert.IsTrue(search1.Equals(search2));
                Assert.AreEqual(search1.GetHashCode(), search2.GetHashCode());
            }
        }

        [Test]
        public void Matches_ReplaceableSearches_Correct()
        {
            // Arrange
            var user   = new NullUser();
            var labels = ModuleLabelList.GetDefaultLabels();
            using (var inst     = new DisposableKSP())
            using (var repo     = new TemporaryRepository(
                                      @"{
                                          ""spec_version"": 1,
                                          ""identifier"":   ""ReplacementMod"",
                                          ""author"":       ""author"",
                                          ""version"":      ""1.0"",
                                          ""download"":     ""https://github.com/download""
                                      }"))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            using (var cacheDir = new TemporaryDirectory())
            {
                var cache = new NetModuleCache(cacheDir);
                var registry = new Registry(repoData.Manager, repo.repo);
                var search1  = ModSearch.Parse(labels, inst.KSP, "is:replaceable")!;
                var search2  = ModSearch.Parse(labels, inst.KSP, "not:replaceable")!;
                registry.SetDlls(new Dictionary<string, string> { { "ReplacedMod", "" } });
                var mod1 = new GUIMod(CkanModule.FromJson(
                                          @"{
                                              ""spec_version"": 1,
                                              ""identifier"":   ""ReplacedMod"",
                                              ""author"":       ""author"",
                                              ""version"":      ""1.0"",
                                              ""download"":     ""https://github.com/download"",
                                              ""replaced_by"":  { ""name"": ""ReplacementMod"" }
                                          }"),
                                      repoData.Manager, registry,
                                      inst.KSP.StabilityToleranceConfig,
                                      inst.KSP, cache,
                                      null, false, false);
                var mod2 = new GUIMod(CkanModule.FromJson(
                                          @"{
                                              ""spec_version"": 1,
                                              ""identifier"":   ""NotReplacedMod"",
                                              ""author"":       ""author"",
                                              ""version"":      ""1.0"",
                                              ""download"":     ""https://github.com/download""
                                          }"),
                                      repoData.Manager, registry,
                                      inst.KSP.StabilityToleranceConfig,
                                      inst.KSP, cache,
                                      null, false, false);

                // Act / Assert
                Assert.IsTrue(search1.Matches(mod1),  "search1 should match mod1");
                Assert.IsFalse(search1.Matches(mod2), "search1 should not match mod2");
                Assert.IsTrue(search2.Matches(mod2),  "search2 should match mod2");
                Assert.IsFalse(search2.Matches(mod1), "search2 should not match mod1");
            }
        }
    }
}
