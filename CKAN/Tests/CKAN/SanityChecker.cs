using NUnit.Framework;
using System;
using CKAN;
using System.Collections.Generic;
using Tests;

namespace CKANTests
{
    [TestFixture()]
    public class SanityChecker
    {
        private CKAN.Registry registry;

        [TestFixtureSetUp]
        public void Setup()
        {
            registry = CKAN.Registry.Empty();
            CKAN.Repo.UpdateRegistry(TestData.TestKAN(), registry);
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
            var mods = new List<CKAN.CkanModule>();
            mods.Add(registry.LatestAvailable("DogeCoinFlag"));

            Assert.IsTrue(CKAN.SanityChecker.IsConsistent(mods), "DogeCoinFlag");
        }

        [Test]
        public void CustomBiomes()
        {
            var mods = new List<CKAN.CkanModule>();

            mods.Add(registry.LatestAvailable("CustomBiomes"));
            Assert.IsFalse(CKAN.SanityChecker.IsConsistent(mods), "CustomBiomes without data");

            mods.Add(registry.LatestAvailable("CustomBiomesKerbal"));
            Assert.IsTrue(CKAN.SanityChecker.IsConsistent(mods), "CustomBiomes with stock data");

            mods.Add(registry.LatestAvailable("CustomBiomesRSS"));
            Assert.IsFalse(CKAN.SanityChecker.IsConsistent(mods), "CustomBiomes with conflicting data");
        }

        [Test]
        public void CustomBiomesWithDlls()
        {
            var mods = new List<CKAN.CkanModule>();
            var dlls = new List<string>();

            dlls.Add("CustomBiomes");
            Assert.IsTrue(CKAN.SanityChecker.IsConsistent(mods, dlls), "CustomBiomes dll by itself");

            // This would actually be a terrible thing for users to have, but it tests the
            // relationship we want.
            mods.Add(registry.LatestAvailable("CustomBiomesKerbal"));
            Assert.IsTrue(CKAN.SanityChecker.IsConsistent(mods, dlls), "CustomBiomes DLL, with config added");

            mods.Add(registry.LatestAvailable("CustomBiomesRSS"));
            Assert.IsFalse(CKAN.SanityChecker.IsConsistent(mods, dlls), "CustomBiomes with conflicting data");
        }
    }
}

