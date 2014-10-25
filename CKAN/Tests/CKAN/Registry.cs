using NUnit.Framework;
using System;
using CKAN;
using Tests;

namespace CKANTests
{
    [TestFixture()]
    public class Registry
    {
        private static readonly CKAN.CkanModule module = TestData.kOS_014_module();
        private static readonly string identifier = module.identifier;
        private static readonly CKAN.KSPVersion v0_24_2 = new CKAN.KSPVersion("0.24.2");
        private static readonly CKAN.KSPVersion v0_25_0 = new CKAN.KSPVersion("0.25.0");

        private CKAN.Registry registry = null;

        [SetUp()]
        public void Setup()
        {
            // Provide an empty registry before each test.
            registry = CKAN.Registry.Empty();
            Assert.IsNotNull(registry);
        }

        [Test()]
        public void Empty()
        {
            CKAN.Registry registry = CKAN.Registry.Empty();
            Assert.IsInstanceOf<CKAN.Registry>(registry);

        }

        [Test()]
        public void AddAvailable()
        {
            // We shouldn't have kOS in our registry.
            Assert.IsFalse(registry.available_modules.ContainsKey(module.identifier));

            // Register
            registry.AddAvailable(module);

            // Make sure it's now there.
            Assert.IsTrue(registry.available_modules.ContainsKey(module.identifier));
        }

        [Test()]
        public void RemoveAvailable()
        {
            // Add our module and test it's there.
            registry.AddAvailable(module);
            Assert.IsNotNull(registry.LatestAvailable(identifier, v0_24_2));

            // Remove it, and make sure it's gone.
            registry.RemoveAvailable(identifier, module.version);

            Assert.IsNull(registry.LatestAvailable(identifier, v0_24_2));
        }

        [Test()]
        public void LatestAvailable()
        {

            registry.AddAvailable(module);

            // Make sure it's there for 0.24.2
            Assert.AreEqual(module.ToString(), registry.LatestAvailable(identifier, v0_24_2).ToString());

            // But not for 0.25.0
            Assert.IsNull(registry.LatestAvailable(identifier, v0_25_0));

            // And that we fail if we ask for something we don't know.
            Assert.Throws<ModuleNotFoundKraken>(delegate
            {
                registry.LatestAvailable("ToTheMun", v0_24_2);
            });
        }
    }
}

