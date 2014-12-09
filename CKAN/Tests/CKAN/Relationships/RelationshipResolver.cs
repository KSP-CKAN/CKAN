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
            registry = Registry.Empty();
            options = RelationshipResolver.DefaultOpts();
            Assert.DoesNotThrow(() => new RelationshipResolver(new List<string>(),
                options,
                registry,
                null));
        }

        [Test]
        public void Constructor_WithConflictingModules_Throws()
        {
            var list = new List<string>();
            var mod_a = generator.GeneratorRandomModule();
            var mod_b = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new RelationshipDescriptor {name=mod_a.identifier}
            });

            list.Add(mod_a.identifier);
            list.Add(mod_b.identifier);
            AddToRegistry(mod_a, mod_b);

            Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                list,
                options,
                registry,
                null));
        }

        [Test]
        public void Constructor_WithMultipleModulesProviding_Throws()
        {
            options.without_toomanyprovides_kraken = false;

            var list = new List<string>();
            var mod_a = generator.GeneratorRandomModule();
            var mod_b = generator.GeneratorRandomModule(provides: new List<string>
            {
                mod_a.identifier
            });
            var mod_c = generator.GeneratorRandomModule(provides: new List<string>
            {
                mod_a.identifier
            });
            var mod_d = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new RelationshipDescriptor {name=mod_a.identifier}
            });

            list.Add(mod_d.identifier);
            AddToRegistry(mod_b, mod_c, mod_d);
            Assert.Throws<TooManyModsProvideKraken>(() => new RelationshipResolver(
                list,
                options,
                registry,
                null));

        }

        [Test]
        public void Constructor_WithMissingModules_Throws()
        {
            var list = new List<string>();
            var mod_a = generator.GeneratorRandomModule();
            list.Add(mod_a.identifier);

            Assert.Throws<ModuleNotFoundKraken>(() => new RelationshipResolver(
                list,
                options,
                registry,
                null));

        }

        // Right now our RR always returns the modules it was provided. However
        // if we've already got the same version(s) installed, it should be able to
        // return a list *without* them. This isn't a hard error at the moment,
        // since ModuleInstaller.InstallList will ignore already installed mods, but
        // it would be nice to have. Discussed a little in GH #521.
        [Test][Category("TODO")][Explicit]
        public void ModList_WithInstalledModules_DoesNotContainThem()
        {
            var list = new List<string>();
            var mod_a = generator.GeneratorRandomModule();
            list.Add(mod_a.identifier);
            AddToRegistry(mod_a);
            registry.Installed().Add(mod_a.identifier, mod_a.version);

            var relationship_resolver = new RelationshipResolver(list, options, registry, null);
            CollectionAssert.IsEmpty(relationship_resolver.ModList());
        }

        [Test]
        public void ModList_WithInstalledModulesSugested_DoesNotContainThem()
        {
            options.with_all_suggests = true;
            var list = new List<string>();
            var sugested = generator.GeneratorRandomModule();
            var sugester = generator.GeneratorRandomModule(sugests: new List<RelationshipDescriptor>
            {
                new RelationshipDescriptor {name = sugested.identifier}
            });

            list.Add(sugester.identifier);
            AddToRegistry(sugester, sugested);
            registry.Installed().Add(sugested.identifier, sugested.version);

            var relationship_resolver = new RelationshipResolver(list, options, registry, null);
            CollectionAssert.Contains(relationship_resolver.ModList(), sugested);
        }

        [Test]
        public void ModList_WithSugestedModulesThatWouldConflict_DoesNotContainThem()
        {
            options.with_all_suggests = true;
            var list = new List<string>();
            var sugested = generator.GeneratorRandomModule();
            var mod = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new RelationshipDescriptor {name = sugested.identifier}
            });
            var sugester = generator.GeneratorRandomModule(sugests: new List<RelationshipDescriptor>
            {
                new RelationshipDescriptor {name = sugested.identifier}
            });

            list.Add(sugester.identifier);
            list.Add(mod.identifier);
            AddToRegistry(sugester, sugested, mod);

            var relationship_resolver = new RelationshipResolver(list, options, registry, null);
            CollectionAssert.DoesNotContain(relationship_resolver.ModList(), sugested);
        }

        [Test]
        public void Constructor_WithConflictingModulesInDependancies_ThrowUnderDefaultSettings()
        {
            var list = new List<string>();
            var dependant = generator.GeneratorRandomModule();
            var depender = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new RelationshipDescriptor {name = dependant.identifier}
            });
            var conflicts_with_dependant = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new RelationshipDescriptor {name=dependant.identifier}
            });


            list.Add(depender.identifier);
            list.Add(conflicts_with_dependant.identifier);
            AddToRegistry(depender, dependant, conflicts_with_dependant);

            Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                list,
                options,
                registry,
                null));
        }
        [Test]
        public void Constructor_WithSuggests_HasSugestedInModlist()
        {
            options.with_all_suggests = true;
            var list = new List<string>();
            var sugested = generator.GeneratorRandomModule();
            var sugester = generator.GeneratorRandomModule(sugests: new List<RelationshipDescriptor>
            {
                new RelationshipDescriptor {name = sugested.identifier}
            });

            list.Add(sugester.identifier);
            AddToRegistry(sugester, sugested);

            var relationship_resolver = new RelationshipResolver(list, options, registry, null);
            CollectionAssert.Contains(relationship_resolver.ModList(), sugested);
        }

        [Test]
        public void Constructor_ProvidesSatisfyDependencies()
        {
            var list = new List<string>();
            var mod_a = generator.GeneratorRandomModule();
            var mod_b = generator.GeneratorRandomModule(provides: new List<string>
            {
                mod_a.identifier
            });
            var depender = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new RelationshipDescriptor {name = mod_a.identifier}
            });
            list.Add(depender.identifier);
            AddToRegistry(mod_b, depender);
            var relationship_resolver = new RelationshipResolver(list, options, registry, null);

            CollectionAssert.AreEquivalent(relationship_resolver.ModList(), new List<CkanModule>
            {
                mod_b,
                depender
            });

        }


        [Test]
        public void Constructor_WithMissingDependants_Throws()
        {
            var list = new List<string>();
            var dependant = generator.GeneratorRandomModule();
            var depender = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new RelationshipDescriptor {name = dependant.identifier}
            });
            list.Add(depender.identifier);
            registry.AddAvailable(depender);

            Assert.Throws<ModuleNotFoundKraken>(() => new RelationshipResolver(
                list,
                options,
                registry,
                null));

        }

        [Test]
        public void Constructor_WithRegisteryThatHasRequiredModuleRemoved_Throws()
        {
            var list = new List<string>();
            var mod = generator.GeneratorRandomModule();
            mod.ksp_version = new KSPVersion("0.10");
            list.Add(mod.identifier);
            registry.AddAvailable(mod);
            registry.RemoveAvailable(mod);

            Assert.Throws<ModuleNotFoundKraken>(() => new RelationshipResolver(
                list,
                options,
                registry,
                null));

        }

        [Test]
        public void Constructor_RecommendsAddedInbreadthFirstOrder_Throws()
        {
            options.without_toomanyprovides_kraken = false;
            // If a conflicts with c choose c as it is shallower.
            // D->b->a
            //  \  
            //   >c
            //
            var list = new List<string>();
            var mod_a = generator.GeneratorRandomModule();
            var mod_b = generator.GeneratorRandomModule(recommends: new List<RelationshipDescriptor>
            {
                new RelationshipDescriptor {name = mod_a.identifier}
            });
            var mod_c = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new RelationshipDescriptor {name = mod_a.identifier}
            });
            var mod_d = generator.GeneratorRandomModule(recommends: new List<RelationshipDescriptor>
            {
                new RelationshipDescriptor {name = mod_b.identifier},
                new RelationshipDescriptor {name = mod_c.identifier}
            });

            list.Add(mod_d.identifier);
            AddToRegistry(mod_a, mod_b, mod_c, mod_d);

            options.with_recommends = true;
            var modlist = new RelationshipResolver(list, options, registry,null).ModList();
            CollectionAssert.Contains(modlist, mod_c);
        }

        [Test]
        public void Constructor_SugestedModulesHaveDependancesAdded()
        {
            var list = new List<string>();
            var mod_a = generator.GeneratorRandomModule();
            var mod_b = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new RelationshipDescriptor {name = mod_a.identifier}
            });
            var mod_c = generator.GeneratorRandomModule(sugests: new List<RelationshipDescriptor>
            {
                new RelationshipDescriptor {name = mod_b.identifier}
            });


            list.Add(mod_c.identifier);
            AddToRegistry(mod_a, mod_b, mod_c);

            options.with_recommends = true;
            options.with_all_suggests = true;
            var modlist = new RelationshipResolver(list, options, registry, null).ModList();
            CollectionAssert.Contains(modlist, mod_a);
        }
        [Test]
        public void Constructor_SugestedodulesHaveConflictingDependances_ThrowsIfWithAllSuggests()
        {
            var list = new List<string>();
            var mod_a = generator.GeneratorRandomModule();
            var mod_b = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new RelationshipDescriptor {name = mod_a.identifier}
            });
            var mod_c = generator.GeneratorRandomModule(
                sugests: new List<RelationshipDescriptor>
            {
                new RelationshipDescriptor {name = mod_b.identifier}
            }, conflicts: new List<RelationshipDescriptor>
            {
                new RelationshipDescriptor {name = mod_a.identifier}
            });


            list.Add(mod_c.identifier);
            AddToRegistry(mod_a, mod_b, mod_c);

            options.with_recommends = true;
            options.with_all_suggests = true;
            Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(list, options, registry, null));
        }

        private void AddToRegistry(params CkanModule[] modules)
        {
            foreach (var module in modules)
            {
                registry.AddAvailable(module);
            }
        }
    }
}
