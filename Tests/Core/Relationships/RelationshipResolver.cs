﻿using System;
using System.Collections.Generic;
using System.Linq;
using CKAN;
using NUnit.Framework;
using Tests.Data;
using System.IO;
using CKAN.Versioning;
using RelationshipDescriptor = CKAN.RelationshipDescriptor;

namespace Tests.Core.Relationships
{
    [TestFixture]
    public class RelationshipResolverTests
    {
        private CKAN.Registry registry;
        private RelationshipResolverOptions options;
        private RandomModuleGenerator generator;

        [SetUp]
        public void Setup()
        {
            registry = CKAN.Registry.Empty();
            options = RelationshipResolver.DefaultOpts();
            generator = new RandomModuleGenerator(new Random(0451));
            //Sanity checker means even incorrect RelationshipResolver logic was passing
            options.without_enforce_consistency = true;
        }

        [Test]
        public void Constructor_WithoutModules_AlwaysReturns()
        {
            registry = CKAN.Registry.Empty();
            options = RelationshipResolver.DefaultOpts();
            Assert.DoesNotThrow(() => new RelationshipResolver(new List<string>(),
                null,
                options,
                registry,
                null));
        }

        [Test]
        public void Constructor_WithConflictingModules()
        {
            var list = new List<string>();
            var mod_a = generator.GeneratorRandomModule();
            var mod_b = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name=mod_a.identifier}
            });

            list.Add(mod_a.identifier);
            list.Add(mod_b.identifier);
            AddToRegistry(mod_a, mod_b);

            Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                list,
                null,
                options,
                registry,
                null));


            options.proceed_with_inconsistencies = true;
            var resolver = new RelationshipResolver(list, null, options, registry, null);

            Assert.That(resolver.ConflictList.Any(s => Equals(s.Key, mod_a)));
            Assert.That(resolver.ConflictList.Any(s => Equals(s.Key, mod_b)));
            Assert.That(resolver.ConflictList, Has.Count.EqualTo(2));
        }

        [Test]
        [Category("Version")]
        public void Constructor_WithConflictingModulesVersion_Throws()
        {
            var list = new List<string>();
            var mod_a = generator.GeneratorRandomModule();
            var mod_b = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name=mod_a.identifier, version=mod_a.version}
            });

            list.Add(mod_a.identifier);
            list.Add(mod_b.identifier);
            AddToRegistry(mod_a, mod_b);

            Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                list,
                null,
                options,
                registry,
                null));
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "0.5")]
        [TestCase("1.0", "1.0")]
        public void Constructor_WithConflictingModulesVersionMin_Throws(string ver, string conf_min)
        {
            var list = new List<string>();
            var mod_a = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var mod_b = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name=mod_a.identifier, min_version=new ModuleVersion(conf_min)}
            });

            list.Add(mod_a.identifier);
            list.Add(mod_b.identifier);
            AddToRegistry(mod_a, mod_b);

            Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                list,
                null,
                options,
                registry,
                null));
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "2.0")]
        [TestCase("1.0", "1.0")]
        public void Constructor_WithConflictingModulesVersionMax_Throws(string ver, string conf_max)
        {
            var list = new List<string>();
            var mod_a = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var mod_b = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name=mod_a.identifier, max_version=new ModuleVersion(conf_max)}
            });

            list.Add(mod_a.identifier);
            list.Add(mod_b.identifier);
            AddToRegistry(mod_a, mod_b);

            Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                list,
                null,
                options,
                registry,
                null));
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "0.5", "2.0")]
        [TestCase("1.0", "1.0", "2.0")]
        [TestCase("1.0", "0.5", "1.0")]
        public void Constructor_WithConflictingModulesVersionMinMax_Throws(string ver, string conf_min, string conf_max)
        {
            var list = new List<string>();
            var mod_a = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var mod_b = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name=mod_a.identifier, min_version=new ModuleVersion(conf_min), max_version=new ModuleVersion(conf_max)}
            });

            list.Add(mod_a.identifier);
            list.Add(mod_b.identifier);
            AddToRegistry(mod_a, mod_b);

            Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                list,
                null,
                options,
                registry,
                null));
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "0.5")]
        [TestCase("1.0", "2.0")]
        public void Constructor_WithNonConflictingModulesVersion_DoesNotThrows(string ver, string conf)
        {
            var list = new List<string>();
            var mod_a = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var mod_b = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name=mod_a.identifier, version=new ModuleVersion(conf)}
            });

            list.Add(mod_a.identifier);
            list.Add(mod_b.identifier);
            AddToRegistry(mod_a, mod_b);

            Assert.DoesNotThrow(() => new RelationshipResolver(
                list,
                null,
                options,
                registry,
                null));
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "2.0")]
        public void Constructor_WithConflictingModulesVersionMin_DoesNotThrows(string ver, string conf_min)
        {
            var list = new List<string>();
            var mod_a = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var mod_b = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name=mod_a.identifier, min_version=new ModuleVersion(conf_min)}
            });

            list.Add(mod_a.identifier);
            list.Add(mod_b.identifier);
            AddToRegistry(mod_a, mod_b);

            Assert.DoesNotThrow(() => new RelationshipResolver(
                list,
                null,
                options,
                registry,
                null));
        }

        [Test]
        [Category("Version")]
        [TestCase("2.0", "1.0")]
        public void Constructor_WithConflictingModulesVersionMax_DoesNotThrows(string ver, string conf_max)
        {
            var list = new List<string>();
            var mod_a = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var mod_b = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name=mod_a.identifier, max_version=new ModuleVersion(conf_max)}
            });

            list.Add(mod_a.identifier);
            list.Add(mod_b.identifier);
            AddToRegistry(mod_a, mod_b);

            Assert.DoesNotThrow(() => new RelationshipResolver(
                list,
                null,
                options,
                registry,
                null));
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "2.0", "3.0")]
        [TestCase("4.0", "2.0", "3.0")]
        public void Constructor_WithConflictingModulesVersionMinMax_DoesNotThrows(string ver, string conf_min, string conf_max)
        {
            var list = new List<string>();
            var mod_a = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var mod_b = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name=mod_a.identifier, min_version=new ModuleVersion(conf_min), max_version=new ModuleVersion(conf_max)}
            });

            list.Add(mod_a.identifier);
            list.Add(mod_b.identifier);
            AddToRegistry(mod_a, mod_b);

            Assert.DoesNotThrow(() => new RelationshipResolver(
                list,
                null,
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
                new ModuleRelationshipDescriptor {name=mod_a.identifier}
            });

            list.Add(mod_d.identifier);
            AddToRegistry(mod_b, mod_c, mod_d);
            Assert.Throws<TooManyModsProvideKraken>(() => new RelationshipResolver(
                list,
                null,
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
                null,
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

            var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);
            CollectionAssert.IsEmpty(relationship_resolver.ModList());
        }

        [Test]
        public void ModList_WithInstalledModulesSuggested_DoesNotContainThem()
        {
            options.with_all_suggests = true;
            var list = new List<string>();
            var suggested = generator.GeneratorRandomModule();
            var suggester = generator.GeneratorRandomModule(suggests: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = suggested.identifier}
            });

            list.Add(suggester.identifier);
            AddToRegistry(suggester, suggested);
            registry.Installed().Add(suggested.identifier, suggested.version);

            var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);
            CollectionAssert.Contains(relationship_resolver.ModList(), suggested);
        }

        [Test]
        public void ModList_WithSuggestedModulesThatWouldConflict_DoesNotContainThem()
        {
            options.with_all_suggests = true;
            var list = new List<string>();
            var suggested = generator.GeneratorRandomModule();
            var mod = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = suggested.identifier}
            });
            var suggester = generator.GeneratorRandomModule(suggests: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = suggested.identifier}
            });

            list.Add(suggester.identifier);
            list.Add(mod.identifier);
            AddToRegistry(suggester, suggested, mod);

            var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);
            CollectionAssert.DoesNotContain(relationship_resolver.ModList(), suggested);
        }

        [Test]
        public void Constructor_WithConflictingModulesInDependancies_ThrowUnderDefaultSettings()
        {
            var list = new List<string>();
            var dependant = generator.GeneratorRandomModule();
            var depender = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = dependant.identifier}
            });
            var conflicts_with_dependant = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name=dependant.identifier}
            });


            list.Add(depender.identifier);
            list.Add(conflicts_with_dependant.identifier);
            AddToRegistry(depender, dependant, conflicts_with_dependant);

            Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                list,
                null,
                options,
                registry,
                null));
        }

        [Test]
        public void Constructor_WithSuggests_HasSuggestedInModlist()
        {
            options.with_all_suggests = true;
            var list = new List<string>();
            var suggested = generator.GeneratorRandomModule();
            var suggester = generator.GeneratorRandomModule(suggests: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = suggested.identifier}
            });

            list.Add(suggester.identifier);
            AddToRegistry(suggester, suggested);

            var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);
            CollectionAssert.Contains(relationship_resolver.ModList(), suggested);
        }

        [Test]
        public void Constructor_ContainsSugestedOfSuggested_When_With_all_suggests()
        {
            options.with_all_suggests = true;
            var list = new List<string>();
            var suggested2 = generator.GeneratorRandomModule();
            var suggested = generator.GeneratorRandomModule(
                suggests: new List<RelationshipDescriptor>
                {
                    new ModuleRelationshipDescriptor { name = suggested2.identifier }
                }
            );
            var suggester = generator.GeneratorRandomModule(
                suggests: new List<RelationshipDescriptor>
                {
                    new ModuleRelationshipDescriptor { name = suggested.identifier }
                }
            );

            list.Add(suggester.identifier);
            AddToRegistry(suggester, suggested, suggested2);

            var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);
            CollectionAssert.Contains(relationship_resolver.ModList(), suggested2);

            options.with_all_suggests = false;

            relationship_resolver = new RelationshipResolver(list, null, options, registry, null);
            CollectionAssert.DoesNotContain(relationship_resolver.ModList(), suggested2);
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
                new ModuleRelationshipDescriptor {name = mod_a.identifier}
            });
            list.Add(depender.identifier);
            AddToRegistry(mod_b, depender);
            var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);

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
                new ModuleRelationshipDescriptor {name = dependant.identifier}
            });
            list.Add(depender.identifier);
            registry.AddAvailable(depender);

            Assert.Throws<DependencyNotSatisfiedKraken>(() => new RelationshipResolver(
                list,
                null,
                options,
                registry,
                null));
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "2.0")]
        [TestCase("1.0", "0.2")]
        [TestCase("0",   "0.2")]
        [TestCase("1.0", "0")]
        public void Constructor_WithMissingDependantsVersion_Throws(string ver, string dep)
        {
            var list = new List<string>();
            var dependant = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var depender = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = dependant.identifier, version = new ModuleVersion(dep)}
            });
            list.Add(depender.identifier);
            AddToRegistry(depender, dependant);

            Assert.Throws<DependencyNotSatisfiedKraken>(() => new RelationshipResolver(
                list,
                null,
                options,
                registry,
                null));
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "2.0")]
        public void Constructor_WithMissingDependantsVersionMin_Throws(string ver, string dep_min)
        {
            var list = new List<string>();
            var dependant = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var depender = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = dependant.identifier, min_version = new ModuleVersion(dep_min)}
            });
            list.Add(depender.identifier);
            AddToRegistry(depender, dependant);

            Assert.Throws<DependencyNotSatisfiedKraken>(() => new RelationshipResolver(
                list,
                null,
                options,
                registry,
                null));
            list.Add(dependant.identifier);
            Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                list,
                null,
                options,
                registry,
                null));
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "0.5")]
        public void Constructor_WithMissingDependantsVersionMax_Throws(string ver, string dep_max)
        {
            var list = new List<string>();
            var dependant = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var depender = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = dependant.identifier, max_version = new ModuleVersion(dep_max)}
            });
            list.Add(depender.identifier);
            list.Add(dependant.identifier);
            AddToRegistry(depender, dependant);

            Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                list,
                null,
                options,
                registry,
                null));
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "2.0", "3.0")]
        [TestCase("4.0", "2.0", "3.0")]
        public void Constructor_WithMissingDependantsVersionMinMax_Throws(string ver, string dep_min, string dep_max)
        {
            var list = new List<string>();
            var dependant = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var depender = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor
                {
                    name = dependant.identifier,
                    min_version = new ModuleVersion(dep_min),
                    max_version = new ModuleVersion(dep_max)
                }
            });
            list.Add(depender.identifier);
            list.Add(dependant.identifier);
            AddToRegistry(depender, dependant);

            Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                list,
                null,
                options,
                registry,
                null));
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "1.0", "2.0")]
        [TestCase("1.0", "1.0", "0.5")]//what to do if a mod is present twice with the same version ?
        public void Constructor_WithDependantVersion_ChooseCorrectly(string ver, string dep, string other)
        {
            var list = new List<string>();
            var dependant = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var other_dependant = generator.GeneratorRandomModule(identifier: dependant.identifier, version: new ModuleVersion(other));

            var depender = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = dependant.identifier, version = new ModuleVersion(dep)}
            });

            list.Add(depender.identifier);
            AddToRegistry(depender, dependant, other_dependant);

            var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);
            CollectionAssert.AreEquivalent(relationship_resolver.ModList(), new List<CkanModule>
            {
                dependant,
                depender
            });
        }

        [Test]
        [Category("Version")]
        [TestCase("2.0", "1.0", "0.5")]
        [TestCase("2.0", "1.0", "1.5")]
        [TestCase("2.0", "2.0", "0.5")]
        public void Constructor_WithDependantVersionMin_ChooseCorrectly(string ver, string dep_min, string other)
        {
            var list = new List<string>();
            var dependant = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var other_dependant = generator.GeneratorRandomModule(identifier: dependant.identifier, version: new ModuleVersion(other));

            var depender = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = dependant.identifier, min_version = new ModuleVersion(dep_min)}
            });
            list.Add(depender.identifier);
            AddToRegistry(depender, dependant, other_dependant);

            var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);
            CollectionAssert.AreEquivalent(relationship_resolver.ModList(), new List<CkanModule>
            {
                dependant,
                depender
            });
        }

        [Test]
        [Category("Version")]
        [TestCase("2.0", "2.0", "0.5")]
        [TestCase("2.0", "3.0", "0.5")]
        [TestCase("2.0", "3.0", "4.0")]
        public void Constructor_WithDependantVersionMax_ChooseCorrectly(string ver, string dep_max, string other)
        {
            var list = new List<string>();
            var dependant = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var other_dependant = generator.GeneratorRandomModule(identifier: dependant.identifier, version: new ModuleVersion(other));

            var depender = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = dependant.identifier, max_version = new ModuleVersion(dep_max)}
            });
            list.Add(depender.identifier);
            AddToRegistry(depender, dependant, other_dependant);

            var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);
            CollectionAssert.AreEquivalent(relationship_resolver.ModList(), new List<CkanModule>
            {
                dependant,
                depender
            });
        }

        [Test]
        [Category("Version")]
        [TestCase("2.0", "1.0", "3.0", "0.5")]
        [TestCase("2.0", "1.0", "3.0", "1.5")]
        [TestCase("2.0", "1.0", "3.0", "3.5")]
        public void Constructor_WithDependantVersionMinMax_ChooseCorrectly(string ver, string dep_min, string dep_max, string other)
        {
            var list = new List<string>();
            var dependant = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var other_dependant = generator.GeneratorRandomModule(identifier: dependant.identifier, version: new ModuleVersion(other));

            var depender = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = dependant.identifier, min_version = new ModuleVersion(dep_min), max_version = new ModuleVersion(dep_max)}
            });
            list.Add(depender.identifier);
            AddToRegistry(depender, dependant, other_dependant);

            var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);
            CollectionAssert.AreEquivalent(relationship_resolver.ModList(), new List<CkanModule>
            {
                dependant,
                depender
            });
        }

        [Test]
        public void Constructor_WithRegistryThatHasRequiredModuleRemoved_Throws()
        {
            var list = new List<string>();
            var mod = generator.GeneratorRandomModule();
            mod.ksp_version = GameVersion.Parse("0.10");
            list.Add(mod.identifier);
            registry.AddAvailable(mod);
            registry.RemoveAvailable(mod);

            Assert.Throws<ModuleNotFoundKraken>(() => new RelationshipResolver(
                list,
                null,
                options,
                registry,
                null));
        }

        [Test]
        public void Constructor_ReverseDependencyDoesntMatchLatest_ChoosesOlderVersion()
        {
            // Arrange
            CkanModule depender = CkanModule.FromJson(@"{
                ""identifier"": ""depender"",
                ""version"":    ""1.0"",
                ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""depends"": [ { ""name"": ""dependency"" } ]
            }");

            CkanModule olderDependency = CkanModule.FromJson(@"{
                ""identifier"": ""dependency"",
                ""version"":    ""1.0"",
                ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""depends"": [ {
                    ""name"":        ""depender"",
                    ""min_version"": ""1.0""
                } ]
            }");

            CkanModule newerDependency = CkanModule.FromJson(@"{
                ""identifier"": ""dependency"",
                ""version"":    ""2.0"",
                ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""depends"": [ {
                    ""name"":        ""depender"",
                    ""min_version"": ""2.0""
                } ]
            }");

            AddToRegistry(olderDependency, newerDependency, depender);

            // Act
            RelationshipResolver rr = new RelationshipResolver(
                new CkanModule[] { depender }, null,
                options, registry, null
            );

            // Assert
            CollectionAssert.Contains(      rr.ModList(), olderDependency);
            CollectionAssert.DoesNotContain(rr.ModList(), newerDependency);
        }

        [Test]
        public void Constructor_ReverseDependencyConflictsLatest_ChoosesOlderVersion()
        {
            // Arrange
            CkanModule depender = CkanModule.FromJson(@"{
                ""identifier"": ""depender"",
                ""version"":    ""1.0"",
                ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""depends"": [ { ""name"": ""dependency"" } ]
            }");

            CkanModule olderDependency = CkanModule.FromJson(@"{
                ""identifier"": ""dependency"",
                ""version"":    ""1.0"",
                ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01""
            }");

            CkanModule newerDependency = CkanModule.FromJson(@"{
                ""identifier"": ""dependency"",
                ""version"":    ""2.0"",
                ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""conflicts"": [ {
                    ""name"": ""depender""
                } ]
            }");

            AddToRegistry(olderDependency, newerDependency, depender);

            // Act
            RelationshipResolver rr = new RelationshipResolver(
                new CkanModule[] { depender }, null,
                options, registry, null
            );

            // Assert
            CollectionAssert.Contains(      rr.ModList(), olderDependency);
            CollectionAssert.DoesNotContain(rr.ModList(), newerDependency);
        }

        [Test]
        public void ReasonFor_WithModsNotInList_ThrowsArgumentException()
        {
            var list = new List<string>();
            var mod = generator.GeneratorRandomModule();
            list.Add(mod.identifier);
            registry.AddAvailable(mod);
            AddToRegistry(mod);
            var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);

            var mod_not_in_resolver_list = generator.GeneratorRandomModule();
            CollectionAssert.DoesNotContain(relationship_resolver.ModList(),mod_not_in_resolver_list);
            Assert.Throws<ArgumentException>(() => relationship_resolver.ReasonFor(mod_not_in_resolver_list));
        }

        [Test]
        public void ReasonFor_WithUserAddedMods_GivesReasonUserAdded()
        {
            var list = new List<string>();
            var mod = generator.GeneratorRandomModule();
            list.Add(mod.identifier);
            registry.AddAvailable(mod);
            AddToRegistry(mod);

            var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);
            var reason = relationship_resolver.ReasonFor(mod);
            Assert.That(reason, Is.AssignableTo<SelectionReason.UserRequested>());
        }

        [Test]
        public void ReasonFor_WithSuggestedMods_GivesCorrectParent()
        {
            var list = new List<string>();
            var suggested = generator.GeneratorRandomModule();
            var mod =
                generator.GeneratorRandomModule(suggests:
                    new List<RelationshipDescriptor> {new ModuleRelationshipDescriptor {name = suggested.identifier}});
            list.Add(mod.identifier);
            AddToRegistry(mod, suggested);

            options.with_all_suggests = true;
            var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);
            var reason = relationship_resolver.ReasonFor(suggested);

            Assert.That(reason, Is.AssignableTo<SelectionReason.Suggested>());
            Assert.That(reason.Parent, Is.EqualTo(mod));
        }

        [Test]
        public void ReasonFor_WithTreeOfMods_GivesCorrectParents()
        {
            var list = new List<string>();
            var suggested = generator.GeneratorRandomModule();
            var recommendedA = generator.GeneratorRandomModule();
            var recommendedB = generator.GeneratorRandomModule();
            var mod = generator.GeneratorRandomModule(
                suggests: new List<RelationshipDescriptor>
                {
                    new ModuleRelationshipDescriptor { name = suggested.identifier }
                }
            );
            list.Add(mod.identifier);
            suggested.recommends = new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor { name = recommendedA.identifier },
                new ModuleRelationshipDescriptor { name = recommendedB.identifier }
            };

            AddToRegistry(mod, suggested, recommendedA, recommendedB);

            options.with_all_suggests = true;
            options.with_recommends = true;
            var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);
            var reason = relationship_resolver.ReasonFor(recommendedA);
            Assert.That(reason, Is.AssignableTo<SelectionReason.Recommended>());
            Assert.That(reason.Parent, Is.EqualTo(suggested));

            reason = relationship_resolver.ReasonFor(recommendedB);
            Assert.That(reason, Is.AssignableTo<SelectionReason.Recommended>());
            Assert.That(reason.Parent, Is.EqualTo(suggested));
        }

        // The whole point of autodetected mods is they can participate in relationships.
        // This makes sure they can (at least for dependencies). It may overlap with other
        // tests, but that's cool, beacuse it's a test. :D
        [Test]
        public void AutodetectedCanSatisfyRelationships()
        {
            using (var ksp = new DisposableKSP ())
            {
                registry.RegisterDll(ksp.KSP, Path.Combine(ksp.KSP.game.PrimaryModDirectory(ksp.KSP), "ModuleManager.dll"));

                var depends = new List<CKAN.RelationshipDescriptor>();
                depends.Add(new CKAN.ModuleRelationshipDescriptor { name = "ModuleManager" });

                CkanModule mod = generator.GeneratorRandomModule(depends: depends);

                new RelationshipResolver(
                    new CkanModule[] { mod },
                    null,
                    RelationshipResolver.DefaultOpts(),
                    registry,
                    new GameVersionCriteria (GameVersion.Parse("1.0.0"))
                );
            }
        }

        // Models the EVE - EVE-Config - AVP - AVP-Textures relationship
        [Test]
        public void UninstallingConflictingModule_InstallingRecursiveDependencies_ResolvesSuccessfully()
        {
            using (var ksp = new DisposableKSP())
            {
                // Arrange: create dummy modules that resemble the relationship entanglement, and make them available
                var eve = generator.GeneratorRandomModule(
                    identifier: "EnvironmentalVisualEnhancements",
                    depends: new List<RelationshipDescriptor>
                        {new ModuleRelationshipDescriptor {name = "EnvironmentalVisualEnhancements-Config"}}
                );
                var eveDefaultConfig = generator.GeneratorRandomModule(
                    identifier: "EnvironmentalVisualEnhancements-Config-stock",
                    provides: new List<string> {"EnvironmentalVisualEnhancements-Config"},
                    conflicts: new List<RelationshipDescriptor>
                        {new ModuleRelationshipDescriptor {name = "EnvironmentalVisualEnhancements-Config"}}
                );
                var avp = generator.GeneratorRandomModule(
                    identifier: "AstronomersVisualPack",
                    provides: new List<string> {"EnvironmentalVisualEnhancements-Config"},
                    depends: new List<RelationshipDescriptor>
                    {
                        new ModuleRelationshipDescriptor {name = "AVP-Textures"},
                        new ModuleRelationshipDescriptor {name = "EnvironmentalVisualEnhancements"}
                    },
                    conflicts: new List<RelationshipDescriptor>
                            {new ModuleRelationshipDescriptor {name = "EnvironmentalVisualEnhancements-Config"}}
                );
                var avp2kTextures = generator.GeneratorRandomModule(
                    identifier: "AVP-2kTextures",
                    provides: new List<string> {"AVP-Textures"},
                    depends: new List<RelationshipDescriptor>
                        {new ModuleRelationshipDescriptor {name = "AstronomersVisualPack"}},
                    conflicts: new List<RelationshipDescriptor>
                        {new ModuleRelationshipDescriptor {name = "AVP-Textures"}}
                );

                AddToRegistry(eve, eveDefaultConfig, avp, avp2kTextures);

                // Start with eve and eveDefaultConfig installed
                registry.RegisterModule(eve, new string[0], ksp.KSP, false);
                registry.RegisterModule(eveDefaultConfig, new string[0], ksp.KSP, false);

                Assert.DoesNotThrow(() => registry.CheckSanity());

                List<CkanModule> modulesToInstall;
                List<CkanModule> modulesToRemove;
                RelationshipResolver resolver;

                // Act and assert: play through different possible user interactions
                // Scenario 1 - Try installing AVP, expect an exception for proceed_with_inconsistencies=false

                modulesToInstall = new List<CkanModule> { avp };
                modulesToRemove = new List<CkanModule>();

                options.proceed_with_inconsistencies = false;
                Assert.Throws<InconsistentKraken>(() =>
                {
                    resolver = new RelationshipResolver(modulesToInstall, modulesToRemove, options, registry, null);
                });

                // Scenario 2 - Try installing AVP, expect no exception for proceed_with_inconsistencies=true, but a conflict list

                resolver = null;
                options.proceed_with_inconsistencies = true;
                Assert.DoesNotThrow(() =>
                {
                    resolver = new RelationshipResolver(modulesToInstall, modulesToRemove, options, registry, null);
                });
                CollectionAssert.AreEquivalent(new List<CkanModule> {avp, eveDefaultConfig}, resolver.ConflictList.Keys);

                // Scenario 3 - Try uninstalling eveDefaultConfig and installing avp, should work and result in no conflicts

                modulesToInstall = new List<CkanModule> { avp };
                modulesToRemove = new List<CkanModule> { eveDefaultConfig };

                resolver = null;
                options.proceed_with_inconsistencies = false;
                Assert.DoesNotThrow(() =>
                {
                    resolver = new RelationshipResolver(modulesToInstall, modulesToRemove, options, registry, null);
                });
                Assert.IsEmpty(resolver.ConflictList);
                CollectionAssert.AreEquivalent(new List<CkanModule> {avp, avp2kTextures}, resolver.ModList());
            }
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
