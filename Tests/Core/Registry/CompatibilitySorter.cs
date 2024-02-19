using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using CKAN;
using CKAN.Versioning;
#if NET45
using CKAN.Extensions;
#endif

using Tests.Data;

namespace Tests.Core.Registry
{
    [TestFixture]
    public class CompatibilitySorterTests
    {
        [Test,
            TestCase(new string[]
                     {
                         @"{
                             ""spec_version"":    ""v1.4"",
                             ""identifier"":      ""kOS-EVA"",
                             ""name"":            ""kOS-EVA"",
                             ""abstract"":        ""Addon for kOS that allows controlling a kerbal while on EVA"",
                             ""version"":         ""0.2.0.0"",
                             ""ksp_version_min"": ""1.8.0"",
                             ""ksp_version_max"": ""1.12.99"",
                             ""license"":         ""GPL-3.0"",
                             ""download"":        ""https://github.com/"",
                             ""depends"": [
                                 { ""name"": ""Harmony2"" }
                             ]
                         }",
                         @"{
                             ""spec_version"": ""v1.4"",
                             ""identifier"":   ""Harmony2"",
                             ""name"":         ""Harmony 2"",
                             ""abstract"":     ""A library for patching, replacing and decorating .NET and Mono methods during runtime"",
                             ""version"":      ""2.2.1.0"",
                             ""ksp_version_min"": ""1.8.0"",
                             ""ksp_version_max"": ""1.12.99"",
                             ""license"":         ""MIT"",
                             ""download"":        ""https://spacedockinfo/""
                         }",
                     },
                     new string[]
                     {
                         @"{
                             ""spec_version"":    ""v1.4"",
                             ""identifier"":      ""kOS-EVA"",
                             ""name"":            ""kOS-EVA"",
                             ""abstract"":        ""Addon for kOS that allows controlling a kerbal while on EVA"",
                             ""version"":         ""0.2.0.0"",
                             ""ksp_version_min"": ""1.8.0"",
                             ""ksp_version_max"": ""1.12.99"",
                             ""license"":         ""GPL-3.0"",
                             ""download"":        ""https://github.com/""
                         }",
                     },
                     "kOS-EVA"),
        ]
        public void Constructor_OverlappingModules_HigherPriorityOverrides(string[] modules1,
                                                                           string[] modules2,
                                                                           string   identifier)
        {
            // Arrange
            var user = new NullUser();
            using (var repo1 = new TemporaryRepository(0, modules1))
            using (var repo2 = new TemporaryRepository(1, modules2))
            using (var repoData = new TemporaryRepositoryData(user, repo1.repo,
                                                                    repo2.repo))
            {
                var versCrit  = new GameVersionCriteria(GameVersion.Parse("1.12.5"));
                var repos     = new Repository[] { repo1.repo, repo2.repo };
                var providers = repoData.Manager
                                        .GetAllAvailableModules(repos)
                                        .GroupBy(am => am.AllAvailable().First().identifier)
                                        .ToDictionary(grp => grp.Key,
                                                      grp => grp.ToHashSet());
                var installed = new Dictionary<string, InstalledModule>();
                var dlls      = new HashSet<string>();
                var dlcs      = new Dictionary<string, ModuleVersion>();
                var highPrio  = repoData.Manager
                                        .GetAvailableModules(Enumerable.Repeat(repo1.repo, 1),
                                                             identifier)
                                        .First()
                                        .Latest();

                // Act
                var sorter = new CompatibilitySorter(
                    versCrit,
                    repoData.Manager.GetAllAvailDicts(repos),
                    providers, installed, dlls, dlcs);

                // Assert
                Assert.AreEqual(0, sorter.LatestIncompatible.Count);
                Assert.AreEqual(2, sorter.LatestCompatible.Count);
                Assert.AreEqual(CkanModule.ToJson(highPrio),
                                CkanModule.ToJson(sorter.LatestCompatible.First(m => m.identifier == identifier)));
            }
        }
    }
}
