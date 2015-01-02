using System.Collections.Generic;
using System.Linq;
using CKAN;
using NUnit.Framework;
using Tests;

// We're exercising FindReverseDependencies in here, because:
// - We need a registry
// - It calls the sanity checker code to do the heavy lifting.

namespace CKANTests
{
    [TestFixture]
    public class SanityChecker
    {
        private CKAN.Registry registry;
        private DisposableKSP ksp;

        [TestFixtureSetUp]
        public void Setup()
        {
            ksp = new DisposableKSP();

            registry = ksp.KSP.Registry;
            registry.ClearAvailable();
            registry.ClearDlls();
            registry.Installed().Clear();

            CKAN.Repo.UpdateRegistry(TestData.TestKAN(), registry, ksp.KSP, new NullUser());
        }

        [Test]
        public void Empty()
        {
            var list = new List<CkanModule>();
            Assert.IsTrue(CKAN.SanityChecker.IsConsistent(list));
        }

        [Test]
        public void Void()
        {
            Assert.IsTrue(CKAN.SanityChecker.IsConsistent(null));
        }

        [Test]
        public void DogeCoin()
        {
            // Test with a module that depends and conflicts with nothing.
            var mods = new List<CkanModule> {registry.LatestAvailable("DogeCoinFlag",null)};

            Assert.IsTrue(CKAN.SanityChecker.IsConsistent(mods), "DogeCoinFlag");
        }

        [Test]
        public void CustomBiomes()
        {
            var mods = new List<CkanModule>();

            mods.Add(registry.LatestAvailable("CustomBiomes",null));
            Assert.IsFalse(CKAN.SanityChecker.IsConsistent(mods), "CustomBiomes without data");

            mods.Add(registry.LatestAvailable("CustomBiomesKerbal",null));
            Assert.IsTrue(CKAN.SanityChecker.IsConsistent(mods), "CustomBiomes with stock data");

            mods.Add(registry.LatestAvailable("CustomBiomesRSS",null));
            Assert.IsFalse(CKAN.SanityChecker.IsConsistent(mods), "CustomBiomes with conflicting data");
        }

        [Test]
        public void CustomBiomesWithDlls()
        {
            var mods = new List<CkanModule>();
            var dlls = new List<string>();

            dlls.Add("CustomBiomes");
            Assert.IsTrue(CKAN.SanityChecker.IsConsistent(mods, dlls), "CustomBiomes dll by itself");

            // This would actually be a terrible thing for users to have, but it tests the
            // relationship we want.
            mods.Add(registry.LatestAvailable("CustomBiomesKerbal",null));
            Assert.IsTrue(CKAN.SanityChecker.IsConsistent(mods, dlls), "CustomBiomes DLL, with config added");

            mods.Add(registry.LatestAvailable("CustomBiomesRSS",null));
            Assert.IsFalse(CKAN.SanityChecker.IsConsistent(mods, dlls), "CustomBiomes with conflicting data");
        }

        [Test]
        public void ConflictWithDll()
        {
            var mods = new List<CKAN.Module> { registry.LatestAvailable("SRL",null) };
            var dlls = new List<string> { "QuickRevert" };

            Assert.IsTrue(CKAN.SanityChecker.IsConsistent(mods), "SRL can be installed by itself");
            Assert.IsFalse(CKAN.SanityChecker.IsConsistent(mods, dlls), "SRL conflicts with QuickRevert DLL");
        }

        [Test]
        public void ModulesToProvides()
        {
            var mods = new List<CKAN.Module>
            {
                registry.LatestAvailable("CustomBiomes",null),
                registry.LatestAvailable("CustomBiomesKerbal",null),
                registry.LatestAvailable("DogeCoinFlag",null)
            };

            var provides = CKAN.SanityChecker.ModulesToProvides(mods);
            Assert.Contains("CustomBiomes", provides.Keys);
            Assert.Contains("CustomBiomesData", provides.Keys);
            Assert.Contains("CustomBiomesKerbal", provides.Keys);
            Assert.Contains("DogeCoinFlag", provides.Keys);
            Assert.AreEqual(4, provides.Keys.Count);
        }

        [Test]
        public void FindUnmetDependencies()
        {
            var mods = new List<CKAN.Module>();
            var dlls = new List<string>();
            Assert.IsEmpty(CKAN.SanityChecker.FindUnmetDependencies(mods, dlls), "Empty list");

            mods.Add(registry.LatestAvailable("DogeCoinFlag",null));
            Assert.IsEmpty(CKAN.SanityChecker.FindUnmetDependencies(mods, dlls), "DogeCoinFlag");

            mods.Add(registry.LatestAvailable("CustomBiomes",null));
            Assert.Contains("CustomBiomesData", CKAN.SanityChecker.FindUnmetDependencies(mods, dlls).Keys, "Missing CustomBiomesData");

            mods.Add(registry.LatestAvailable("CustomBiomesKerbal",null));
            Assert.IsEmpty(CKAN.SanityChecker.FindUnmetDependencies(mods, dlls), "CBD+CBK");

            mods.RemoveAll(x => x.identifier == "CustomBiomes");
            Assert.AreEqual(2, mods.Count, "Checking removed CustomBiomes");

            Assert.Contains("CustomBiomes", CKAN.SanityChecker.FindUnmetDependencies(mods, dlls).Keys, "Missing CustomBiomes");
        }

        [Test]
        public void ReverseDepends()
        {
            var mods = new List<CKAN.Module>
            {
                registry.LatestAvailable("CustomBiomes",null),
                registry.LatestAvailable("CustomBiomesKerbal",null),
                registry.LatestAvailable("DogeCoinFlag",null)
            };

            // Make sure some of our expectations regarding dependencies are correct.
            Assert.Contains("CustomBiomes", registry.LatestAvailable("CustomBiomesKerbal",null).depends.Select(x => x.name).ToList());
            Assert.Contains("CustomBiomesData", registry.LatestAvailable("CustomBiomes",null).depends.Select(x => x.name).ToList());

            // Removing DCF should only remove itself.
            var to_remove = new List<string>();
            to_remove.Add("DogeCoinFlag");
            TestDepends(to_remove, mods, null, to_remove, "DogeCoin Removal");

            // Removing CB should remove its data, and vice-versa.
            to_remove.Clear();
            to_remove.Add("CustomBiomes");
            var expected = new List<string>();
            expected.Add("CustomBiomes");
            expected.Add("CustomBiomesKerbal");
            TestDepends(to_remove, mods, null, expected, "CustomBiomes removed");

            // We expect the same result removing CBK
            to_remove.Clear();
            to_remove.Add("CustomBiomesKerbal");
            TestDepends(to_remove, mods, null, expected, "CustomBiomesKerbal removed");

            // And we expect the same result if we try to remove both.
            to_remove.Add("CustomBiomes");
            TestDepends(to_remove, mods, null, expected, "CustomBiomesKerbal and data removed");

            // Finally, if we try to remove nothing, we shold get back the empty set.
            expected.Clear();
            to_remove.Clear();
            TestDepends(to_remove, mods, null, expected, "Removing nothing");

        }

        private void TestDepends(List<string> to_remove, List<CKAN.Module> mods, List<string> dlls, List<string> expected, string message)
        {
            dlls = dlls ?? new List<string>();

            var remove_count = to_remove.Count;
            var dll_count = dlls.Count;
            var mods_count = mods.Count;

            var results = CKAN.Registry.FindReverseDependencies(to_remove, mods, dlls);

            // Make sure nothing changed.
            Assert.AreEqual(remove_count, to_remove.Count, message + " remove count");
            Assert.AreEqual(dll_count, dlls.Count, message + " dll count");
            Assert.AreEqual(mods_count, mods.Count, message + " mods count");

            // Check our actual results.
            CollectionAssert.AreEquivalent(expected, results, message);
        }
    }
}

