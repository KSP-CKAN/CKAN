using System;
using System.Collections.Generic;
using CKAN;
using NUnit.Framework;

namespace Tests.CKAN.Relationships
{
    [TestFixture]
    public class RelationshipResolverTests
    {
        private Registry registry;
        private RelationshipResolverOptions options;
        private RandomModuleGenerator generator;

        [SetUp]
        public void Setup()
        {
            registry = Registry.Empty();
            options = RelationshipResolver.DefaultOpts();
            generator = new RandomModuleGenerator(new Random(0451));
            //Sanity checker means even incorrect RelationshipResolver logic was passing
            options.without_enforce_consistency = true;
        }

        [Test]
        public void Constructor_WithoutModules_AlwaysReturns()
        {
            var registry = Registry.Empty();
            var options = RelationshipResolver.DefaultOpts();
            Assert.DoesNotThrow(() => new RelationshipResolver(new List<string>(),
                options,
                registry));
        }

        [Test]
        public void Constructor_WithConflictingModules_Throws()
        {
            var list = new List<string>();
            var modA = generator.GeneratorRandomModule();
            var modB = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new RelationshipDescriptor {name=modA.identifier}
            });

            list.Add(modA.identifier);
            list.Add(modB.identifier);
            registry.AddAvailable(modA);
            registry.AddAvailable(modB);

            Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                list,
                options,
                registry));

        }

        [Test]
        public void Constructor_WithConflictingModulesInDependancies_ThrowUnderDefaultSettings()
        {
            var list = new List<string>();
            var dependant = generator.GeneratorRandomModule();
            var depender = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new RelationshipDescriptor(){name = dependant.identifier}
            });
            var conflicts_with_dependant = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new RelationshipDescriptor {name=dependant.identifier}
            });


            list.Add(depender.identifier);
            list.Add(conflicts_with_dependant.identifier);
            registry.AddAvailable(depender);
            registry.AddAvailable(dependant);
            registry.AddAvailable(conflicts_with_dependant);

            Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                list,
                options,
                registry));
        }

        [Test]
        public void Constructor_WithMissingDependants_Throws()
        {
            var list = new List<string>();
            var dependant = generator.GeneratorRandomModule();
            var depender = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new RelationshipDescriptor(){name = dependant.identifier}
            });
            list.Add(depender.identifier);
            registry.AddAvailable(depender);

            Assert.Throws<ModuleNotFoundKraken>(() => new RelationshipResolver(
                list,
                options,
                registry));

        }
    }
}
