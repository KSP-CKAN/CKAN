using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using Tests.Data;

using CKAN;
using CKAN.Configuration;
using CKAN.Versioning;

// We're exercising FindReverseDependencies in here, because:
// - We need a registry
// - It calls the sanity checker code to do the heavy lifting.

namespace Tests.Core.Relationships
{
    [TestFixture]
    public class SanityChecker
    {
        private RegistryManager?         manager;
        private CKAN.Registry?           registry;
        private DisposableKSP?           ksp;
        private TemporaryRepositoryData? repoData;
        private readonly StabilityToleranceConfig stabilityTolerance = new StabilityToleranceConfig("");

        private readonly string[] dlls = Array.Empty<string>();
        private readonly IDictionary<string, UnmanagedModuleVersion> dlc = new Dictionary<string, UnmanagedModuleVersion>();

        [OneTimeSetUp]
        public void Setup()
        {
            ksp = new DisposableKSP();

            var repos = new SortedDictionary<string, Repository>()
            {
                {
                    "testRepo", new Repository("testRepo", TestData.TestKANZip())
                }
            };
            var user = new NullUser();
            repoData = new TemporaryRepositoryData(user, repos.Values);

            manager = RegistryManager.Instance(ksp.KSP, repoData.Manager);
            registry = manager.registry;
            registry.Installed().Clear();

            registry.RepositoriesSet(repos);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            manager?.Dispose();
            ksp?.Dispose();
            repoData?.Dispose();
        }

        [Test]
        public void Empty()
        {
            Assert.IsTrue(CKAN.SanityChecker.IsConsistent(new List<CkanModule>(), dlls, dlc));
        }

        [Test]
        public void DogeCoin()
        {
            // Test with a module that depends and conflicts with nothing.
            var mods = new List<CkanModule?> { registry?.LatestAvailable("DogeCoinFlag", stabilityTolerance, null) }.OfType<CkanModule>();

            Assert.IsTrue(CKAN.SanityChecker.IsConsistent(mods, dlls, dlc), "DogeCoinFlag");
        }

        [Test]
        public void CustomBiomes()
        {
            var mods = Enumerable.Repeat(registry?.LatestAvailable("CustomBiomes", stabilityTolerance, null), 1).OfType<CkanModule>().ToList();

            Assert.IsFalse(CKAN.SanityChecker.IsConsistent(mods, dlls, dlc), "CustomBiomes without data");

            mods.Add(registry?.LatestAvailable("CustomBiomesKerbal", stabilityTolerance, null)!);
            Assert.IsTrue(CKAN.SanityChecker.IsConsistent(mods, dlls, dlc), "CustomBiomes with stock data");

            mods.Add(registry?.LatestAvailable("CustomBiomesRSS", stabilityTolerance, null)!);
            Assert.IsFalse(CKAN.SanityChecker.IsConsistent(mods, dlls, dlc), "CustomBiomes with conflicting data");
        }

        [Test]
        public void CustomBiomesWithDlls()
        {
            var mods = new List<CkanModule>();
            var dlls = new Dictionary<string, string> { { "CustomBiomes", "" } }.Keys;

            Assert.IsTrue(CKAN.SanityChecker.IsConsistent(mods, dlls, dlc), "CustomBiomes dll by itself");

            // This would actually be a terrible thing for users to have, but it tests the
            // relationship we want.
            mods.Add(registry?.LatestAvailable("CustomBiomesKerbal", stabilityTolerance, null)!);
            Assert.IsTrue(CKAN.SanityChecker.IsConsistent(mods, dlls, dlc), "CustomBiomes DLL, with config added");

            mods.Add(registry?.LatestAvailable("CustomBiomesRSS", stabilityTolerance, null)!);
            Assert.IsFalse(CKAN.SanityChecker.IsConsistent(mods, dlls, dlc), "CustomBiomes with conflicting data");
        }

        [Test]
        public void ConflictWithDll()
        {
            var mods = new List<CkanModule> { registry?.LatestAvailable("SRL", stabilityTolerance, null)! };
            var dlls = new Dictionary<string, string> { { "QuickRevert", "" } }.Keys;

            Assert.IsTrue(CKAN.SanityChecker.IsConsistent(mods, this.dlls, dlc), "SRL can be installed by itself");
            Assert.IsFalse(CKAN.SanityChecker.IsConsistent(mods, dlls, dlc), "SRL conflicts with QuickRevert DLL");
        }

        [Test]
        public void FindUnsatisfiedDepends()
        {
            var mods = new List<CkanModule>();
            var dlls = new Dictionary<string, string>().Keys;
            var dlc = new Dictionary<string, UnmanagedModuleVersion>();

            Assert.IsEmpty(CKAN.SanityChecker.FindUnsatisfiedDepends(mods, dlls, dlc), "Empty list");

            mods.Add(registry?.LatestAvailable("DogeCoinFlag", stabilityTolerance, null)!);
            Assert.IsEmpty(CKAN.SanityChecker.FindUnsatisfiedDepends(mods, dlls, dlc), "DogeCoinFlag");

            mods.Add(registry?.LatestAvailable("CustomBiomes", stabilityTolerance, null)!);
            Assert.Contains(
                "CustomBiomesData",
                CKAN.SanityChecker.FindUnsatisfiedDepends(mods, dlls, dlc).Select(kvp => kvp.Item2.ToString()).ToList(),
                "Missing CustomBiomesData"
            );

            mods.Add(registry?.LatestAvailable("CustomBiomesKerbal", stabilityTolerance, null)!);
            Assert.IsEmpty(CKAN.SanityChecker.FindUnsatisfiedDepends(mods, dlls, dlc), "CBD+CBK");

            mods.RemoveAll(x => x.identifier == "CustomBiomes");
            Assert.AreEqual(2, mods.Count, "Checking removed CustomBiomes");

            Assert.Contains(
                "CustomBiomes",
                CKAN.SanityChecker.FindUnsatisfiedDepends(mods, dlls, dlc).Select(kvp => kvp.Item2.ToString()).ToList(),
                "Missing CustomBiomes"
            );
        }

        [Test]
        public void ReverseDepends()
        {
            var mods = new CkanModule?[]
            {
                registry?.LatestAvailable("CustomBiomes",       stabilityTolerance, null),
                registry?.LatestAvailable("CustomBiomesKerbal", stabilityTolerance, null),
                registry?.LatestAvailable("DogeCoinFlag",       stabilityTolerance, null)
            }.OfType<CkanModule>().ToHashSet();

            // Make sure some of our expectations regarding dependencies are correct.
            Assert.Contains("CustomBiomes", registry?.LatestAvailable("CustomBiomesKerbal", stabilityTolerance, null)?.depends?.Select(x => x.ToString()).ToList());
            Assert.Contains("CustomBiomesData", registry?.LatestAvailable("CustomBiomes", stabilityTolerance, null)?.depends?.Select(x => x.ToString()).ToList());

            // Removing DCF should only remove itself.
            var to_remove = new List<string> {"DogeCoinFlag"};
            TestDepends(to_remove, mods, dlls, dlc, to_remove, "DogeCoin Removal");

            // Removing CB should remove its data, and vice-versa.
            to_remove.Clear();
            to_remove.Add("CustomBiomes");
            var expected = new List<string> {"CustomBiomes", "CustomBiomesKerbal"};
            TestDepends(to_remove, mods, dlls, dlc, expected, "CustomBiomes removed");

            // We expect the same result removing CBK
            to_remove.Clear();
            to_remove.Add("CustomBiomesKerbal");
            TestDepends(to_remove, mods, dlls, dlc, expected, "CustomBiomesKerbal removed");

            // And we expect the same result if we try to remove both.
            to_remove.Add("CustomBiomes");
            TestDepends(to_remove, mods, dlls, dlc, expected, "CustomBiomesKerbal and data removed");

            // Finally, if we try to remove nothing, we shold get back the empty set.
            expected.Clear();
            to_remove.Clear();
            TestDepends(to_remove, mods, dlls, dlc, expected, "Removing nothing");
        }

        [Test]
        public void IsConsistent_MismatchedDependencyVersion_Inconsistent()
        {
            // Arrange
            List<CkanModule> modules = new List<CkanModule>()
            {
                CkanModule.FromJson(@"{
                    ""spec_version"": 1,
                    ""identifier"": ""depender"",
                    ""author"":     ""modder"",
                    ""version"":    ""1.0.0"",
                    ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                    ""depends"": [ {
                        ""name"":    ""dependency"",
                        ""version"": ""1.2.3""
                    } ]
                }"),
                CkanModule.FromJson(@"{
                    ""spec_version"": 1,
                    ""identifier"": ""dependency"",
                    ""author"":     ""modder"",
                    ""version"":    ""1.1.0"",
                    ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01""
                }")
            };

            // Act & Assert
            Assert.IsFalse(CKAN.SanityChecker.IsConsistent(modules, dlls, dlc));
        }

        [Test]
        public void IsConsistent_MatchedDependencyVersion_Consistent()
        {
            // Arrange
            List<CkanModule> modules = new List<CkanModule>()
            {
                CkanModule.FromJson(@"{
                    ""spec_version"": 1,
                    ""identifier"": ""depender"",
                    ""author"":     ""modder"",
                    ""version"":    ""1.0.0"",
                    ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                    ""depends"": [ {
                        ""name"":    ""dependency"",
                        ""version"": ""1.2.3""
                    } ]
                }"),
                CkanModule.FromJson(@"{
                    ""spec_version"": 1,
                    ""identifier"": ""dependency"",
                    ""author"":     ""modder"",
                    ""version"":    ""1.2.3"",
                    ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01""
                }")
            };

            // Act & Assert
            Assert.IsTrue(CKAN.SanityChecker.IsConsistent(modules, dlls, dlc));
        }

        [Test]
        public void IsConsistent_MismatchedConflictVersion_Consistent()
        {
            // Arrange
            List<CkanModule> modules = new List<CkanModule>()
            {
                CkanModule.FromJson(@"{
                    ""spec_version"": 1,
                    ""identifier"": ""depender"",
                    ""author"":     ""modder"",
                    ""version"":    ""1.0.0"",
                    ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                    ""conflicts"": [ {
                        ""name"":    ""dependency"",
                        ""version"": ""1.2.3""
                    } ]
                }"),
                CkanModule.FromJson(@"{
                    ""spec_version"": 1,
                    ""identifier"": ""dependency"",
                    ""author"":     ""modder"",
                    ""version"":    ""1.1.0"",
                    ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01""
                }")
            };

            // Act & Assert
            Assert.IsTrue(CKAN.SanityChecker.IsConsistent(modules, dlls, dlc));
        }

        [Test]
        public void IsConsistent_MatchedConflictVersion_Inconsistent()
        {
            // Arrange
            List<CkanModule> modules = new List<CkanModule>()
            {
                CkanModule.FromJson(@"{
                    ""spec_version"": 1,
                    ""identifier"": ""depender"",
                    ""author"":     ""modder"",
                    ""version"":    ""1.0.0"",
                    ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                    ""conflicts"": [ {
                        ""name"":    ""dependency"",
                        ""version"": ""1.2.3""
                    } ]
                }"),
                CkanModule.FromJson(@"{
                    ""spec_version"": 1,
                    ""identifier"": ""dependency"",
                    ""author"":     ""modder"",
                    ""version"":    ""1.2.3"",
                    ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01""
                }")
            };

            // Act & Assert
            Assert.IsFalse(CKAN.SanityChecker.IsConsistent(modules, dlls, dlc));
        }

        [Test]
        public void IsConsistent_MultipleVersionsOfSelfConflictingModule_Consistent()
        {
            // Arrange
            List<CkanModule> modules = new List<CkanModule>()
            {
                CkanModule.FromJson(@"{
                    ""spec_version"": 1,
                    ""identifier"": ""self-conflictor"",
                    ""author"":     ""modder"",
                    ""version"":    ""1.0.0"",
                    ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                    ""conflicts"": [ {
                        ""name"":    ""self-conflictor""
                    } ]
                }"),
                CkanModule.FromJson(@"{
                    ""spec_version"": 1,
                    ""identifier"": ""self-conflictor"",
                    ""author"":     ""modder"",
                    ""version"":    ""1.2.3"",
                    ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                    ""conflicts"": [ {
                        ""name"":    ""self-conflictor""
                    } ]
                }")
            };

            // Act & Assert
            Assert.IsTrue(CKAN.SanityChecker.IsConsistent(modules, dlls, dlc));
        }

        [Test]
        public void IsConsistent_MultipleVersionsOfSelfProvidesConflictingModule_Consistent()
        {
            // Arrange
            List<CkanModule> modules = new List<CkanModule>()
            {
                CkanModule.FromJson(@"{
                    ""spec_version"": 1,
                    ""identifier"": ""provides-conflictor"",
                    ""author"":     ""modder"",
                    ""version"":    ""1.0.0"",
                    ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                    ""provides"":   [ ""providee"" ],
                    ""conflicts"": [ {
                        ""name"":    ""providee""
                    } ]
                }"),
                CkanModule.FromJson(@"{
                    ""spec_version"": 1,
                    ""identifier"": ""provides-conflictor"",
                    ""author"":     ""modder"",
                    ""version"":    ""1.2.3"",
                    ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                    ""provides"":   [ ""providee"" ],
                    ""conflicts"": [ {
                        ""name"":    ""providee""
                    } ]
                }")
            };

            // Act & Assert
            Assert.IsTrue(CKAN.SanityChecker.IsConsistent(modules, dlls, dlc));
        }

        private static void TestDepends(List<string>                                to_remove,
                                        HashSet<CkanModule>                         mods,
                                        ICollection<string>                         dlls,
                                        IDictionary<string, UnmanagedModuleVersion> dlc,
                                        List<string>                                expected,
                                        string                                      message)
        {
            dlls ??= new Dictionary<string, string>().Keys;

            var remove_count = to_remove.Count;
            var dll_count = dlls.Count;
            var mods_count = mods.Count;

            var results = CKAN.Registry.FindReverseDependencies(to_remove, null, mods, dlls, dlc);

            // Make sure nothing changed.
            Assert.AreEqual(remove_count, to_remove.Count, message + " remove count");
            Assert.AreEqual(dll_count, dlls.Count, message + " dll count");
            Assert.AreEqual(mods_count, mods.Count, message + " mods count");

            // Check our actual results.
            CollectionAssert.AreEquivalent(expected, results, message);
        }

    }
}
