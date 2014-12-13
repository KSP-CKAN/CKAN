using System.Transactions;
using CKAN;
using NUnit.Framework;
using Tests;

namespace CKANTests
{
    [TestFixture]
    public class Registry
    {
        private static readonly CkanModule module = TestData.kOS_014_module();
        private static readonly string identifier = module.identifier;
        private static readonly CKAN.KSPVersion v0_24_2 = new CKAN.KSPVersion("0.24.2");
        private static readonly CKAN.KSPVersion v0_25_0 = new CKAN.KSPVersion("0.25.0");

        private CKAN.Registry registry;

        [SetUp]
        public void Setup()
        {
            // Provide an empty registry before each test.
            registry = CKAN.Registry.Empty();
            Assert.IsNotNull(registry);
        }

        [Test]
        public void Empty()
        {
            CKAN.Registry registry = CKAN.Registry.Empty();
            Assert.IsInstanceOf<CKAN.Registry>(registry);

        }

        [Test]
        public void AddAvailable()
        {
            // We shouldn't have kOS in our registry.
            Assert.IsFalse(registry.available_modules.ContainsKey(module.identifier));

            // Register
            registry.AddAvailable(module);

            // Make sure it's now there.
            Assert.IsTrue(registry.available_modules.ContainsKey(module.identifier));
        }

        [Test]
        public void RemoveAvailableByName()
        {
            // Add our module and test it's there.
            registry.AddAvailable(module);
            Assert.IsNotNull(registry.LatestAvailable(identifier, v0_24_2));

            // Remove it, and make sure it's gone.
            registry.RemoveAvailable(identifier, module.version);

            Assert.IsNull(registry.LatestAvailable(identifier, v0_24_2));
        }

        [Test]
        public void RemoveAvailableByModule()
        {
            // Add our module and test it's there.
            registry.AddAvailable(module);
            Assert.IsNotNull(registry.LatestAvailable(identifier, v0_24_2));

            // Remove it, and make sure it's gone.
            registry.RemoveAvailable(module);

            Assert.IsNull(registry.LatestAvailable(identifier, v0_24_2));
        }

        [Test]
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

        [Test]
        public void TxEmbeddedCommit()
        {
            // Our registry should work when we initialise it inside our Tx and commit.

            CKAN.Registry reg;

            using (var scope = new TransactionScope())
            {
                reg = CKAN.Registry.Empty();
                reg.AddAvailable(module);
                Assert.AreEqual(identifier, reg.LatestAvailable(identifier, null).identifier);
                scope.Complete();
            }
            Assert.AreEqual(identifier, reg.LatestAvailable(identifier, null).identifier);
        }

        [Test]
        public void TxCommit()
        {
            // Our registry should work fine on committed transactions.

            using (var scope = new TransactionScope())
            {
                registry.AddAvailable(module);
                Assert.AreEqual(module.identifier, registry.LatestAvailable(identifier, null).identifier);

                scope.Complete();
            }
            Assert.AreEqual(module.identifier, registry.LatestAvailable(identifier, null).identifier);
        }

        [Test]
        public void TxRollback()
        {
            // Our registry should roll-back any changes it made during a transaction.

            using (var scope = new TransactionScope())
            {
                registry.AddAvailable(module);
                Assert.AreEqual(module.identifier, registry.LatestAvailable(identifier, null).identifier);

                scope.Dispose(); // Rollback, our module should no longer be available.
            }

            Assert.Throws<ModuleNotFoundKraken>(delegate
            {
                registry.LatestAvailable(identifier);
            });
        }

        [Test]
        public void TxNested()
        {
            // Our registry doesn't understand how to do nested transactions,
            // make sure it throws on these.

            using (var scope = new TransactionScope())
            {
                registry.AddAvailable(module);

                using (var scope2 = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    Assert.Throws<TransactionalKraken>(delegate
                    {
                        registry.AddAvailable(TestData.DogeCoinFlag_101_module());
                    });
                    scope2.Complete();
                }
                scope.Complete();
            }
        }

        [Test]
        public void TxAmbient()
        {
            // Our registry should be fine with ambient transactions, which join together.
            // Note the absence of TransactionScopeOption.RequiresNew

            using (var scope = new TransactionScope())
            {
                registry.AddAvailable(module);

                using (var scope2 = new TransactionScope())
                {
                    Assert.DoesNotThrow(delegate
                    {
                        registry.AddAvailable(TestData.DogeCoinFlag_101_module());
                    });
                    scope2.Complete();
                }
                scope.Complete();
            }
        }
    }
}

