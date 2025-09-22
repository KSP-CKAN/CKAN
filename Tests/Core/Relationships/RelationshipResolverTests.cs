using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;
using NUnit.Framework;

using Tests.Data;

using CKAN;
using CKAN.Configuration;
using CKAN.Games;
using CKAN.Games.KerbalSpaceProgram;
using CKAN.Versioning;
using RelationshipDescriptor = CKAN.RelationshipDescriptor;
using CKAN.NetKAN.Extensions;

namespace Tests.Core.Relationships
{
    [TestFixture]
    public class RelationshipResolverTests
    {
        private RelationshipResolverOptions? options;
        private RandomModuleGenerator? generator;
        private static readonly IGame               game = new KerbalSpaceProgram();
        private static readonly GameVersionCriteria crit = new GameVersionCriteria(null);
        private readonly StabilityToleranceConfig stabilityTolerance = new StabilityToleranceConfig("");

        [SetUp]
        public void Setup()
        {
            options = RelationshipResolverOptions.DefaultOpts(stabilityTolerance);
            generator = new RandomModuleGenerator(new Random(0451));
            //Sanity checker means even incorrect RelationshipResolver logic was passing
            options.without_enforce_consistency = true;
        }

        [Test]
        public void Constructor_WithoutModules_AlwaysReturns()
        {
            var user = new NullUser();
            using (var repoData = new TemporaryRepositoryData(user))
            {
                var registry = CKAN.Registry.Empty(repoData.Manager);
                options = RelationshipResolverOptions.DefaultOpts(stabilityTolerance);
                Assert.DoesNotThrow(() => new RelationshipResolver(new List<CkanModule>(),
                    null, options, registry, game, crit));
            }
        }

        [Test]
        public void Constructor_WithConflictingModules()
        {
            var mod_a = generator!.GenerateRandomModule();
            var mod_b = generator.GenerateRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor { name = mod_a.identifier }
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(mod_a.ToJson(),
                                                      mod_b.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);

                var list = new List<CkanModule> { mod_a, mod_b };
                Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                    list, null, options!, registry, game, crit));

                options!.proceed_with_inconsistencies = true;
                var resolver = new RelationshipResolver(list, null, options, registry, game, crit);

                Assert.That(resolver.ConflictList.Any(s => Equals(s.Key, mod_a)));
                Assert.That(resolver.ConflictList.Any(s => Equals(s.Key, mod_b)));
                Assert.That(resolver.ConflictList, Has.Count.EqualTo(2));
            }
        }

        [Test]
        [Category("Version")]
        public void Constructor_WithConflictingModulesVersion_Throws()
        {
            var mod_a = generator!.GenerateRandomModule();
            var mod_b = generator.GenerateRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor
                {
                    name = mod_a.identifier,
                    version = mod_a.version
                }
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(mod_a.ToJson(),
                                                      mod_b.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod_a, mod_b };

                Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                    list, null, options!, registry, game, crit));
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "0.5")]
        [TestCase("1.0", "1.0")]
        public void Constructor_WithConflictingModulesVersionMin_Throws(string ver, string conf_min)
        {
            var mod_a = generator!.GenerateRandomModule(version: new ModuleVersion(ver));
            var mod_b = generator.GenerateRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor
                {
                    name = mod_a.identifier,
                    min_version = new ModuleVersion(conf_min)
                }
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(mod_a.ToJson(),
                                                      mod_b.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod_a, mod_b };

                Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                    list, null, options!, registry, game, crit));
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "2.0")]
        [TestCase("1.0", "1.0")]
        public void Constructor_WithConflictingModulesVersionMax_Throws(string ver, string conf_max)
        {
            var mod_a = generator!.GenerateRandomModule(version: new ModuleVersion(ver));
            var mod_b = generator.GenerateRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor
                {
                    name = mod_a.identifier,
                    max_version = new ModuleVersion(conf_max)
                }
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(mod_a.ToJson(),
                                                      mod_b.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod_a, mod_b };

                Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                    list, null, options!, registry, game, crit));
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "0.5", "2.0")]
        [TestCase("1.0", "1.0", "2.0")]
        [TestCase("1.0", "0.5", "1.0")]
        public void Constructor_WithConflictingModulesVersionMinMax_Throws(string ver, string conf_min, string conf_max)
        {
            var mod_a = generator!.GenerateRandomModule(version: new ModuleVersion(ver));
            var mod_b = generator.GenerateRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor
                {
                    name = mod_a.identifier,
                    min_version = new ModuleVersion(conf_min),
                    max_version = new ModuleVersion(conf_max)
                }
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(mod_a.ToJson(),
                                                      mod_b.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod_a, mod_b };

                Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                    list, null, options!, registry, game, crit));
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "0.5")]
        [TestCase("1.0", "2.0")]
        public void Constructor_WithNonConflictingModulesVersion_DoesNotThrow(string ver, string conf)
        {
            var mod_a = generator!.GenerateRandomModule(version: new ModuleVersion(ver));
            var mod_b = generator.GenerateRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor
                {
                    name = mod_a.identifier,
                    version = new ModuleVersion(conf)
                }
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(mod_a.ToJson(),
                                                      mod_b.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod_a, mod_b };

                Assert.DoesNotThrow(() => new RelationshipResolver(
                    list, null, options!, registry, game, crit));
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "2.0")]
        public void Constructor_WithConflictingModulesVersionMin_DoesNotThrow(string ver, string conf_min)
        {
            var mod_a = generator!.GenerateRandomModule(version: new ModuleVersion(ver));
            var mod_b = generator.GenerateRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor
                {
                    name = mod_a.identifier,
                    min_version = new ModuleVersion(conf_min)
                }
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(mod_a.ToJson(),
                                                      mod_b.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod_a, mod_b };

                Assert.DoesNotThrow(() => new RelationshipResolver(
                    list, null, options!, registry, game, crit));
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("2.0", "1.0")]
        public void Constructor_WithConflictingModulesVersionMax_DoesNotThrow(string ver, string conf_max)
        {
            var mod_a = generator!.GenerateRandomModule(version: new ModuleVersion(ver));
            var mod_b = generator.GenerateRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor
                {
                    name = mod_a.identifier,
                    max_version = new ModuleVersion(conf_max)
                }
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(mod_a.ToJson(),
                                                      mod_b.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod_a, mod_b };

                Assert.DoesNotThrow(() => new RelationshipResolver(
                    list, null, options!, registry, game, crit));
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "2.0", "3.0")]
        [TestCase("4.0", "2.0", "3.0")]
        public void Constructor_WithConflictingModulesVersionMinMax_DoesNotThrow(string ver, string conf_min, string conf_max)
        {
            var mod_a = generator!.GenerateRandomModule(version: new ModuleVersion(ver));
            var mod_b = generator.GenerateRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor
                {
                    name = mod_a.identifier,
                    min_version = new ModuleVersion(conf_min),
                    max_version = new ModuleVersion(conf_max)
                }
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(mod_a.ToJson(),
                                                      mod_b.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod_a, mod_b };

                Assert.DoesNotThrow(() => new RelationshipResolver(
                    list, null, options!, registry, game, crit));
            }
        }

        [Test]
        public void Constructor_WithMultipleModulesProviding_Throws()
        {
            options!.without_toomanyprovides_kraken = false;

            var mod_a = generator!.GenerateRandomModule();
            var mod_b = generator.GenerateRandomModule(provides: new List<string>
            {
                mod_a.identifier
            });
            var mod_c = generator.GenerateRandomModule(provides: new List<string>
            {
                mod_a.identifier
            });
            var mod_d = generator.GenerateRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = mod_a.identifier}
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(mod_b.ToJson(),
                                                      mod_c.ToJson(),
                                                      mod_d.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod_d };

                Assert.Throws<TooManyModsProvideKraken>(() => new RelationshipResolver(
                    list, null, options, registry, game, crit));
            }
        }

        [Test]
        public void ModList_WithInstalledModules_ContainsThemWithReasonInstalled()
        {
            var user = new NullUser();
            var mod_a = generator!.GenerateRandomModule();
            using (var ksp = new DisposableKSP())
            using (var repo = new TemporaryRepository(mod_a.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod_a };

                registry.RegisterModule(mod_a, new List<string>(), ksp.KSP, false);

                var relationship_resolver = new RelationshipResolver(
                    list, null, options!, registry, game, crit);
                CollectionAssert.Contains(relationship_resolver.ModList(), mod_a);
                CollectionAssert.AreEquivalent(new List<SelectionReason>
                                          {
                                              new SelectionReason.Installed(),
                                              new SelectionReason.UserRequested(),
                                          },
                                          relationship_resolver.ReasonsFor(mod_a));
            }
        }

        [Test]
        public void ModList_WithInstalledModulesSuggested_DoesNotContainThem()
        {
            options!.with_all_suggests = true;
            var suggested = generator!.GenerateRandomModule();
            var suggester = generator.GenerateRandomModule(suggests: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = suggested.identifier}
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(suggested.ToJson(),
                                                      suggester.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                registry.Installed().Add(suggested.identifier, suggested.version);
                var list = new List<CkanModule> { suggester };

                var relationship_resolver = new RelationshipResolver(list, null, options, registry, game, crit);
                CollectionAssert.Contains(relationship_resolver.ModList(), suggested);
            }
        }

        [Test]
        public void ModList_WithSuggestedModulesThatWouldConflict_DoesNotContainThem()
        {
            options!.with_all_suggests = true;
            var suggested = generator!.GenerateRandomModule();
            var mod = generator.GenerateRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = suggested.identifier}
            });
            var suggester = generator.GenerateRandomModule(suggests: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = suggested.identifier}
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(suggested.ToJson(),
                                                      suggester.ToJson(),
                                                      mod.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { suggester, mod };

                var relationship_resolver = new RelationshipResolver(list, null, options, registry, game, crit);
                CollectionAssert.DoesNotContain(relationship_resolver.ModList(), suggested);
            }
        }

        [Test]
        public void Constructor_WithConflictingModulesInDependencies_ThrowUnderDefaultSettings()
        {
            var dependent = generator!.GenerateRandomModule();
            var depender = generator.GenerateRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = dependent.identifier}
            });
            var conflicts_with_dependent = generator.GenerateRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = dependent.identifier}
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(depender.ToJson(),
                                                      dependent.ToJson(),
                                                      conflicts_with_dependent.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { depender, conflicts_with_dependent };

                Assert.Throws<DependenciesNotSatisfiedKraken>(() => new RelationshipResolver(
                    list, null, options!, registry, game, crit));
            }
        }

        [Test]
        public void Constructor_WithSuggests_HasSuggestedInModlist()
        {
            options!.with_all_suggests = true;
            var suggested = generator!.GenerateRandomModule();
            var suggester = generator.GenerateRandomModule(suggests: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = suggested.identifier}
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(suggester.ToJson(),
                                                      suggested.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { suggester };

                var relationship_resolver = new RelationshipResolver(list, null, options, registry, game, crit);
                CollectionAssert.Contains(relationship_resolver.ModList(), suggested);
            }
        }

        [Test]
        public void Constructor_WithAllSuggests_ContainsSuggestedOfSuggested()
        {
            var suggested2 = generator!.GenerateRandomModule();
            var suggested = generator.GenerateRandomModule(
                suggests: new List<RelationshipDescriptor>
                {
                    new ModuleRelationshipDescriptor { name = suggested2.identifier }
                }
            );
            var suggester = generator.GenerateRandomModule(
                suggests: new List<RelationshipDescriptor>
                {
                    new ModuleRelationshipDescriptor { name = suggested.identifier }
                }
            );

            var user = new NullUser();
            using (var repo = new TemporaryRepository(suggester.ToJson(),
                                                      suggested.ToJson(),
                                                      suggested2.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { suggester };

                options!.with_all_suggests = true;
                var relationship_resolver = new RelationshipResolver(list, null, options, registry, game, crit);
                CollectionAssert.Contains(relationship_resolver.ModList(), suggested2);

                options.with_all_suggests = false;

                relationship_resolver = new RelationshipResolver(list, null, options, registry, game, crit);
                CollectionAssert.DoesNotContain(relationship_resolver.ModList(), suggested2);
            }
        }

        [Test]
        public void Constructor_ProvidesSatisfyDependencies()
        {
            var mod_a = generator!.GenerateRandomModule();
            var mod_b = generator.GenerateRandomModule(provides: new List<string>
            {
                mod_a.identifier
            });
            var depender = generator.GenerateRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = mod_a.identifier}
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(mod_b.ToJson(),
                                                      depender.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { depender };
                var relationship_resolver = new RelationshipResolver(list, null, options!, registry, game, crit);

                CollectionAssert.AreEquivalent(relationship_resolver.ModList(), new List<CkanModule>
                {
                    mod_b,
                    depender
                });
            }
        }

        [Test]
        public void Constructor_WithMissingDependents_Throws()
        {
            var dependent = generator!.GenerateRandomModule();
            var depender = generator.GenerateRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = dependent.identifier}
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(depender.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);

                Assert.Throws<DependenciesNotSatisfiedKraken>(() =>
                    new RelationshipResolver(new List<CkanModule> { depender },
                                             null, options!, registry, game, crit));
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "2.0")]
        [TestCase("1.0", "0.2")]
        [TestCase("0",   "0.2")]
        [TestCase("1.0", "0")]
        public void Constructor_WithMissingDependentsVersion_Throws(string ver, string dep)
        {
            var dependent = generator!.GenerateRandomModule(version: new ModuleVersion(ver));
            var depender = generator.GenerateRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor
                {
                    name = dependent.identifier,
                    version = new ModuleVersion(dep)
                }
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(depender.ToJson(),
                                                      dependent.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);

                var list = new List<CkanModule> { depender };

                Assert.Throws<DependenciesNotSatisfiedKraken>(() => new RelationshipResolver(
                    list, null, options!, registry, game, crit));
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "2.0")]
        public void Constructor_WithMissingDependentsVersionMin_Throws(string ver, string dep_min)
        {
            var dependent = generator!.GenerateRandomModule(version: new ModuleVersion(ver));
            var depender = generator.GenerateRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor
                {
                    name = dependent.identifier,
                    min_version = new ModuleVersion(dep_min)
                }
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(depender.ToJson(),
                                                      dependent.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule>() { depender };

                Assert.Throws<DependenciesNotSatisfiedKraken>(() => new RelationshipResolver(
                    list, null, options!, registry, game, crit));
                list.Add(dependent);
                Assert.Throws<DependenciesNotSatisfiedKraken>(() => new RelationshipResolver(
                    list, null, options!, registry, game, crit));
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "0.5")]
        public void Constructor_WithMissingDependentsVersionMax_Throws(string ver, string dep_max)
        {
            var dependent = generator!.GenerateRandomModule(version: new ModuleVersion(ver));
            var depender = generator.GenerateRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor
                {
                    name = dependent.identifier,
                    max_version = new ModuleVersion(dep_max)
                }
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(depender.ToJson(),
                                                      dependent.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { depender, dependent };

                Assert.Throws<DependenciesNotSatisfiedKraken>(() => new RelationshipResolver(
                    list, null, options!, registry, game, crit));
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "2.0", "3.0")]
        [TestCase("4.0", "2.0", "3.0")]
        public void Constructor_WithMissingDependentsVersionMinMax_Throws(string ver, string dep_min, string dep_max)
        {
            var dependent = generator!.GenerateRandomModule(version: new ModuleVersion(ver));
            var depender = generator.GenerateRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor
                {
                    name = dependent.identifier,
                    min_version = new ModuleVersion(dep_min),
                    max_version = new ModuleVersion(dep_max)
                }
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(depender.ToJson(),
                                                      dependent.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { depender, dependent };

                Assert.Throws<DependenciesNotSatisfiedKraken>(() => new RelationshipResolver(
                    list, null, options!, registry, game, crit));
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "1.0", "2.0")]
        [TestCase("1.0", "1.0", "0.5")]//what to do if a mod is present twice with the same version ?
        public void Constructor_WithDependentVersion_ChooseCorrectly(string ver, string dep, string other)
        {
            var dependent = generator!.GenerateRandomModule(version: new ModuleVersion(ver));
            var other_dependent = generator.GenerateRandomModule(identifier: dependent.identifier, version: new ModuleVersion(other));

            var depender = generator.GenerateRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor
                {
                    name = dependent.identifier,
                    version = new ModuleVersion(dep)
                }
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(depender.ToJson(),
                                                      dependent.ToJson(),
                                                      other_dependent.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { depender };

                var relationship_resolver = new RelationshipResolver(list, null, options!, registry, game, crit);
                CollectionAssert.AreEquivalent(relationship_resolver.ModList(), new List<CkanModule>
                {
                    dependent,
                    depender
                });
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("2.0", "1.0", "0.5")]
        [TestCase("2.0", "1.0", "1.5")]
        [TestCase("2.0", "2.0", "0.5")]
        public void Constructor_WithDependentVersionMin_ChooseCorrectly(string ver, string dep_min, string other)
        {
            var dependent = generator!.GenerateRandomModule(version: new ModuleVersion(ver));
            var other_dependent = generator.GenerateRandomModule(identifier: dependent.identifier, version: new ModuleVersion(other));

            var depender = generator.GenerateRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor
                {
                    name = dependent.identifier,
                    min_version = new ModuleVersion(dep_min)
                }
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(depender.ToJson(),
                                                      dependent.ToJson(),
                                                      other_dependent.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { depender };

                var relationship_resolver = new RelationshipResolver(list, null, options!, registry, game, crit);
                CollectionAssert.AreEquivalent(relationship_resolver.ModList(), new List<CkanModule>
                {
                    dependent,
                    depender
                });
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("2.0", "2.0", "0.5")]
        [TestCase("2.0", "3.0", "0.5")]
        [TestCase("2.0", "3.0", "4.0")]
        public void Constructor_WithDependentVersionMax_ChooseCorrectly(string ver, string dep_max, string other)
        {
            var dependent = generator!.GenerateRandomModule(version: new ModuleVersion(ver));
            var other_dependent = generator.GenerateRandomModule(identifier: dependent.identifier, version: new ModuleVersion(other));

            var depender = generator.GenerateRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor
                {
                    name = dependent.identifier,
                    max_version = new ModuleVersion(dep_max)
                }
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(depender.ToJson(),
                                                      dependent.ToJson(),
                                                      other_dependent.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { depender };

                var relationship_resolver = new RelationshipResolver(list, null, options!, registry, game, crit);
                CollectionAssert.AreEquivalent(relationship_resolver.ModList(), new List<CkanModule>
                {
                    dependent,
                    depender
                });
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("2.0", "1.0", "3.0", "0.5")]
        [TestCase("2.0", "1.0", "3.0", "1.5")]
        [TestCase("2.0", "1.0", "3.0", "3.5")]
        public void Constructor_WithDependentVersionMinMax_ChooseCorrectly(string ver, string dep_min, string dep_max, string other)
        {
            var dependent = generator!.GenerateRandomModule(version: new ModuleVersion(ver));
            var other_dependent = generator.GenerateRandomModule(identifier: dependent.identifier, version: new ModuleVersion(other));

            var depender = generator.GenerateRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor
                {
                    name = dependent.identifier,
                    min_version = new ModuleVersion(dep_min),
                    max_version = new ModuleVersion(dep_max)
                }
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(depender.ToJson(),
                                                      dependent.ToJson(),
                                                      other_dependent.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { depender };

                var relationship_resolver = new RelationshipResolver(list, null, options!, registry, game, crit);
                CollectionAssert.AreEquivalent(relationship_resolver.ModList(), new List<CkanModule>
                {
                    dependent,
                    depender
                });
            }
        }

        [Test]
        public void Constructor_WithDependsAnyOf_ChooseCorrectly()
        {
            var dependent = generator!.GenerateRandomModule();
            var depender = generator.GenerateRandomModule(depends: new List<RelationshipDescriptor>
            {
                new AnyOfRelationshipDescriptor
                {
                    any_of = new List<RelationshipDescriptor>
                    {
                        new ModuleRelationshipDescriptor
                        {
                            name = "Nonexistent"
                        },
                        new ModuleRelationshipDescriptor
                        {
                            name = dependent.identifier
                        },
                        new ModuleRelationshipDescriptor
                        {
                            name = "Absent"
                        }
                    }
                }
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(depender.ToJson(),
                                                      dependent.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { depender };

                var relationship_resolver = new RelationshipResolver(list, null, options!, registry, game, crit);
                CollectionAssert.AreEquivalent(relationship_resolver.ModList(),
                                               new List<CkanModule>
                                               {
                                                   dependent,
                                                   depender,
                                               });
            }
        }

        [Test]
        public void Constructor_ReverseDependencyDoesntMatchLatest_ChoosesOlderVersion()
        {
            // Arrange
            CkanModule depender = CkanModule.FromJson(@"{
                ""spec_version"": 1,
                ""identifier"":   ""depender"",
                ""author"":       ""modder"",
                ""version"":      ""1.0"",
                ""download"":     ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""depends"": [ { ""name"": ""dependency"" } ]
            }");

            CkanModule olderDependency = CkanModule.FromJson(@"{
                ""spec_version"": 1,
                ""identifier"":   ""dependency"",
                ""author"":       ""modder"",
                ""version"":      ""1.0"",
                ""download"":     ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""depends"": [ {
                    ""name"":        ""depender"",
                    ""min_version"": ""1.0""
                } ]
            }");

            CkanModule newerDependency = CkanModule.FromJson(@"{
                ""spec_version"": 1,
                ""identifier"":   ""dependency"",
                ""author"":       ""modder"",
                ""version"":      ""2.0"",
                ""download"":     ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""depends"": [ {
                    ""name"":        ""depender"",
                    ""min_version"": ""2.0""
                } ]
            }");

            var user = new NullUser();
            using (var repo = new TemporaryRepository(depender.ToJson(),
                                                      olderDependency.ToJson(),
                                                      newerDependency.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);

                // Act
                RelationshipResolver rr = new RelationshipResolver(
                    new CkanModule[] { depender }, null,
                    options!, registry, game, crit);

                // Assert
                CollectionAssert.Contains(      rr.ModList(), olderDependency);
                CollectionAssert.DoesNotContain(rr.ModList(), newerDependency);
            }
        }

        [Test]
        public void Constructor_ReverseDependencyConflictsLatest_ChoosesOlderVersion()
        {
            // Arrange
            CkanModule depender = CkanModule.FromJson(@"{
                ""spec_version"": 1,
                ""identifier"": ""depender"",
                ""author"":     ""dependerModder"",
                ""version"":    ""1.0"",
                ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""depends"": [ { ""name"": ""dependency"" } ]
            }");

            CkanModule olderDependency = CkanModule.FromJson(@"{
                ""spec_version"": 1,
                ""identifier"": ""dependency"",
                ""author"":     ""dependencyModder"",
                ""version"":    ""1.0"",
                ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01""
            }");

            CkanModule newerDependency = CkanModule.FromJson(@"{
                ""spec_version"": 1,
                ""identifier"": ""dependency"",
                ""author"":     ""dependencyModder"",
                ""version"":    ""2.0"",
                ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""conflicts"": [ {
                    ""name"": ""depender""
                } ]
            }");

            var user = new NullUser();
            using (var repo = new TemporaryRepository(depender.ToJson(),
                                                      olderDependency.ToJson(),
                                                      newerDependency.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);

                // Act
                RelationshipResolver rr = new RelationshipResolver(
                    new CkanModule[] { depender }, null,
                    options!, registry, game, crit);

                // Assert
                CollectionAssert.Contains(      rr.ModList(), olderDependency);
                CollectionAssert.DoesNotContain(rr.ModList(), newerDependency);
            }
        }

        [Test]
        public void ReasonFor_WithModsNotInList_Empty()
        {
            var mod = generator!.GenerateRandomModule();

            var user = new NullUser();
            using (var repo = new TemporaryRepository(mod.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod };
                var relationship_resolver = new RelationshipResolver(list, null, options!, registry, game, crit);

                var mod_not_in_resolver_list = generator.GenerateRandomModule();
                CollectionAssert.DoesNotContain(relationship_resolver.ModList(), mod_not_in_resolver_list);
                Assert.IsEmpty(relationship_resolver.ReasonsFor(mod_not_in_resolver_list));
            }
        }

        [Test]
        public void ReasonFor_WithUserAddedMods_GivesReasonUserAdded()
        {
            var mod = generator!.GenerateRandomModule();

            var user = new NullUser();
            using (var repo = new TemporaryRepository(mod.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod };

                var relationship_resolver = new RelationshipResolver(list, null, options!, registry, game, crit);
                var reasons = relationship_resolver.ReasonsFor(mod);
                Assert.That(reasons[0], Is.AssignableTo<SelectionReason.UserRequested>());
            }
        }

        [Test]
        public void ReasonFor_WithSuggestedMods_GivesCorrectParent()
        {
            var suggested = generator!.GenerateRandomModule();
            var mod = generator.GenerateRandomModule(suggests:
                new List<RelationshipDescriptor>
                {
                    new ModuleRelationshipDescriptor
                    {
                        name = suggested.identifier
                    }
                });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(mod.ToJson(),
                                                      suggested.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod };

                options!.with_all_suggests = true;
                var relationship_resolver = new RelationshipResolver(list, null, options, registry, game, crit);
                var reasons = relationship_resolver.ReasonsFor(suggested);

                Assert.IsTrue(reasons[0] is SelectionReason.Suggested sug
                              && sug.Parent == mod);
            }
        }

        [Test]
        public void ReasonFor_WithTreeOfMods_GivesCorrectParents()
        {
            var suggested = generator!.GenerateRandomModule();
            var recommendedA = generator.GenerateRandomModule();
            var recommendedB = generator.GenerateRandomModule();
            var mod = generator.GenerateRandomModule(
                suggests: new List<RelationshipDescriptor>
                {
                    new ModuleRelationshipDescriptor { name = suggested.identifier }
                }
            );
            suggested.recommends = new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor { name = recommendedA.identifier },
                new ModuleRelationshipDescriptor { name = recommendedB.identifier }
            };

            var user = new NullUser();
            using (var repo = new TemporaryRepository(mod.ToJson(),
                                                      suggested.ToJson(),
                                                      recommendedA.ToJson(),
                                                      recommendedB.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod };

                options!.with_all_suggests = true;
                options.with_recommends = true;
                var relationship_resolver = new RelationshipResolver(list, null, options, registry, game, crit);
                var reasons = relationship_resolver.ReasonsFor(recommendedA);
                Assert.IsTrue(reasons[0] is SelectionReason.Recommended rec
                              && rec.Parent.Equals(suggested));
                reasons = relationship_resolver.ReasonsFor(recommendedB);
                Assert.IsTrue(reasons[0] is SelectionReason.Recommended rec2
                              && rec2.Parent.Equals(suggested));
            }
        }

        // The whole point of autodetected mods is they can participate in relationships.
        // This makes sure they can (at least for dependencies). It may overlap with other
        // tests, but that's cool, beacuse it's a test. :D
        [Test]
        public void AutodetectedCanSatisfyRelationships()
        {
            var user = new NullUser();
            using (var repoData = new TemporaryRepositoryData(user))
            using (var ksp = new DisposableKSP())
            {
                var registry = CKAN.Registry.Empty(repoData.Manager);
                registry.SetDlls(new Dictionary<string, string>()
                {
                    {
                        "ModuleManager",
                        ksp.KSP.ToRelativeGameDir(Path.Combine(ksp.KSP.Game.PrimaryModDirectory(ksp.KSP),
                                                               "ModuleManager.dll"))
                    }
                });

                var depends = new List<RelationshipDescriptor>
                {
                    new ModuleRelationshipDescriptor { name = "ModuleManager" }
                };

                CkanModule mod = generator!.GenerateRandomModule(depends: depends);

                new RelationshipResolver(
                    new CkanModule[] { mod }, null, RelationshipResolverOptions.DefaultOpts(stabilityTolerance),
                    registry, game, new GameVersionCriteria(GameVersion.Parse("1.0.0")));
            }
        }

        [Test,
         TestCase(new string[] {
                      @"{
                          ""identifier"": ""Deferred"",
                          ""conflicts"": [
                              {
                                  ""name"":        ""RasterPropMonitor"",
                                  ""max_version"": ""1:v0.31.13.4""
                              }
                          ]
                      }",
                      @"{
                          ""identifier"": ""RasterPropMonitor"",
                          ""version"":    ""1:v1.0.2""
                      }",
                  },
                  new string[] { "Deferred", "RasterPropMonitor" })]
        public void Constructor_UpgradeVersionSpecificConflictedAutoDetected_DoesNotThrow(
                string[] availableModules,
                string[] installIdents)
        {
            var user = new NullUser();
            var crit = new GameVersionCriteria(new GameVersion(1, 12, 5));
            using (var repo     = new TemporaryRepository(availableModules.Select(MergeWithDefaults)
                                                                          .ToArray()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            using (var inst     = new DisposableKSP())
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                registry.SetDlls(new Dictionary<string, string>()
                {
                    {
                        "RasterPropMonitor",
                        inst.KSP.ToRelativeGameDir(Path.Combine(inst.KSP.Game.PrimaryModDirectory(inst.KSP),
                                                               "RasterPropMonitor.dll"))
                    }
                });
                Assert.DoesNotThrow(() =>
                {
                    var rr = new RelationshipResolver(
                        installIdents.Select(ident => registry.LatestAvailable(ident, stabilityTolerance, crit))
                                     .OfType<CkanModule>(),
                        null,
                        RelationshipResolverOptions.DependsOnlyOpts(stabilityTolerance),
                        registry, inst.KSP.Game, crit);
                });
            }
        }

        // Models the EVE - EVE-Config - AVP - AVP-Textures relationship
        [Test]
        public void UninstallingConflictingModule_InstallingRecursiveDependencies_ResolvesSuccessfully()
        {
            // Arrange: create dummy modules that resemble the relationship entanglement, and make them available
            var eve = generator!.GenerateRandomModule(
                identifier: "EnvironmentalVisualEnhancements",
                depends: new List<RelationshipDescriptor>
                    {new ModuleRelationshipDescriptor {name = "EnvironmentalVisualEnhancements-Config"}}
            );
            var eveDefaultConfig = generator.GenerateRandomModule(
                identifier: "EnvironmentalVisualEnhancements-Config-stock",
                provides: new List<string> {"EnvironmentalVisualEnhancements-Config"},
                conflicts: new List<RelationshipDescriptor>
                    {new ModuleRelationshipDescriptor {name = "EnvironmentalVisualEnhancements-Config"}}
            );
            var avp = generator.GenerateRandomModule(
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
            var avp2kTextures = generator.GenerateRandomModule(
                identifier: "AVP-2kTextures",
                provides: new List<string> {"AVP-Textures"},
                depends: new List<RelationshipDescriptor>
                    {new ModuleRelationshipDescriptor {name = "AstronomersVisualPack"}},
                conflicts: new List<RelationshipDescriptor>
                    {new ModuleRelationshipDescriptor {name = "AVP-Textures"}}
            );
            var user = new NullUser();
            using (var ksp = new DisposableKSP())
            using (var repo = new TemporaryRepository(eve.ToJson(),
                                                      eveDefaultConfig.ToJson(),
                                                      avp.ToJson(),
                                                      avp2kTextures.ToJson()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);

                // Start with eve and eveDefaultConfig installed
                registry.RegisterModule(eve, new List<string>(), ksp.KSP, false);
                registry.RegisterModule(eveDefaultConfig, new List<string>(), ksp.KSP, false);

                Assert.DoesNotThrow(registry.CheckSanity);

                List<CkanModule> modulesToInstall;
                List<CkanModule> modulesToRemove;
                RelationshipResolver? resolver;

                // Act and assert: play through different possible user interactions
                // Scenario 1 - Try installing AVP, expect an exception for proceed_with_inconsistencies=false

                modulesToInstall = new List<CkanModule> { avp };
                modulesToRemove = new List<CkanModule>();

                options!.proceed_with_inconsistencies = false;
                var exception = Assert.Throws<InconsistentKraken>(() =>
                {
                    resolver = new RelationshipResolver(modulesToInstall, modulesToRemove, options, registry, game, crit);
                });
                Assert.AreEqual($"{avp} conflicts with {eveDefaultConfig}",
                                exception?.ShortDescription);

                // Scenario 2 - Try installing AVP, expect no exception for proceed_with_inconsistencies=true, but a conflict list

                resolver = null;
                options.proceed_with_inconsistencies = true;
                Assert.DoesNotThrow(() =>
                {
                    resolver = new RelationshipResolver(modulesToInstall, modulesToRemove, options, registry, game, crit);
                });
                CollectionAssert.AreEquivalent(modulesToInstall,
                                               resolver?.ConflictList.Keys);
                CollectionAssert.AreEquivalent(new List<string> {$"{avp} conflicts with {eveDefaultConfig}"},
                                               resolver?.ConflictList.Values);

                // Scenario 3 - Try uninstalling eveDefaultConfig and installing avp, should work and result in no conflicts

                modulesToInstall = new List<CkanModule> { avp };
                modulesToRemove = new List<CkanModule> { eveDefaultConfig };

                resolver = null;
                options.proceed_with_inconsistencies = false;
                Assert.DoesNotThrow(() =>
                {
                    resolver = new RelationshipResolver(modulesToInstall, modulesToRemove, options, registry, game, crit);
                });
                Assert.IsEmpty(resolver!.ConflictList);
                CollectionAssert.AreEquivalent(new List<CkanModule> {avp, avp2kTextures}, resolver.ModList());
            }
        }

        [Test,
         TestCase(new string[] {
                      @"{
                          ""identifier"": ""MyDLC"",
                          ""kind"": ""dlc""
                      }",
                      @"{
                          ""identifier"": ""InstallingMod"",
                          ""recommends"": [ { ""name"": ""MyDLC"" } ]
                      }",
                  },
                  new string[] { "InstallingMod" },
                  new string[] { "MyDLC" }),
        ]
        public void Recommendations_WithDLCRecommendationUnsatisfied_ContainsDLC(string[] availableModules,
                                                                                 string[] installIdents,
                                                                                 string[] dlcIdents)
        {
            // Arrange
            var user    = new NullUser();
            var game    = new KerbalSpaceProgram();
            var crit    = new GameVersionCriteria(new GameVersion(1, 12, 5));
            using (var repo     = new TemporaryRepository(availableModules.Select(MergeWithDefaults)
                                                                          .ToArray()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);

                // Act
                var rr = new RelationshipResolver(
                    installIdents.Select(ident => registry.LatestAvailable(ident, stabilityTolerance, crit))
                                 .OfType<CkanModule>(),
                    null,
                    RelationshipResolverOptions.KitchenSinkOpts(stabilityTolerance),
                    registry, game, crit);
                var deps = rr.Dependencies().ToHashSet();
                var recs = rr.Recommendations(deps).ToArray();
                var dlcs = dlcIdents.Select(ident => registry.LatestAvailable(ident, stabilityTolerance, crit)).ToArray();

                // Assert
                CollectionAssert.IsSubsetOf(dlcs, rr.ModList(false));
                CollectionAssert.IsSubsetOf(dlcs, recs);
            }
        }

        [Test,
         TestCase(new string[] {
                      @"{
                          ""identifier"": ""MyDLC"",
                          ""kind"": ""dlc""
                      }",
                      @"{
                          ""identifier"": ""InstallingMod"",
                          ""recommends"": [ { ""name"": ""MyDLC"" } ]
                      }",
                  },
                  new string[] { "InstallingMod" },
                  new string[] { "MyDLC" }),
        ]
        public void Recommendations_WithDLCRecommendationSatisfied_OmitsDLC(string[] availableModules,
                                                                            string[] installIdents,
                                                                            string[] dlcIdents)
        {
            // Arrange
            var user    = new NullUser();
            var game    = new KerbalSpaceProgram();
            var crit    = new GameVersionCriteria(new GameVersion(1, 12, 5));
            using (var repo     = new TemporaryRepository(availableModules.Select(MergeWithDefaults)
                                                                          .ToArray()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                registry.SetDlcs(dlcIdents.ToDictionary(ident => ident,
                                                        ident => new UnmanagedModuleVersion("1.0.0")));

                // Act
                var rr = new RelationshipResolver(
                    installIdents.Select(ident => registry.LatestAvailable(ident, stabilityTolerance, crit))
                                 .OfType<CkanModule>(),
                    null,
                    RelationshipResolverOptions.KitchenSinkOpts(stabilityTolerance),
                    registry, game, crit);
                var deps = rr.Dependencies().ToHashSet();
                var recs = rr.Recommendations(deps).ToArray();

                // Assert
                foreach (var mod in dlcIdents.Select(ident => registry.LatestAvailable(ident, stabilityTolerance, crit)))
                {
                    CollectionAssert.DoesNotContain(rr.ModList(false), mod);
                    CollectionAssert.DoesNotContain(recs,              mod);
                }
            }
        }

        [Test,
         TestCase(new string[] {
                      @"{
                          ""identifier"": ""MyDLC"",
                          ""kind"": ""dlc""
                      }",
                      @"{
                          ""identifier"": ""InstallingMod"",
                          ""depends"": [ { ""name"": ""MyDLC"" } ]
                      }",
                  },
                  new string[] { "InstallingMod" }),
        ]
        public void Constructor_WithDLCDependsUnsatisfied_Throws(string[] availableModules,
                                                                 string[] installIdents)
        {
            // Arrange
            var user    = new NullUser();
            var game    = new KerbalSpaceProgram();
            var crit    = new GameVersionCriteria(new GameVersion(1, 12, 5));
            using (var repo     = new TemporaryRepository(availableModules.Select(MergeWithDefaults)
                                                                          .ToArray()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);

                // Act / Assert
                Assert.Throws<DependenciesNotSatisfiedKraken>(() =>
                {
                    var rr = new RelationshipResolver(
                        installIdents.Select(ident => registry.LatestAvailable(ident, stabilityTolerance, crit))
                                     .OfType<CkanModule>(),
                        null,
                        RelationshipResolverOptions.DependsOnlyOpts(stabilityTolerance),
                        registry, game, crit);
                });
            }
        }

        [Test,
         TestCase(new string[] {
                      @"{
                          ""identifier"": ""MyDLC"",
                          ""kind"": ""dlc""
                      }",
                      @"{
                          ""identifier"": ""InstallingMod"",
                          ""depends"": [ { ""name"": ""MyDLC"" } ]
                      }",
                  },
                  new string[] { "InstallingMod" },
                  new string[] { "MyDLC" }),
        ]
        public void Constructor_WithDLCDependsSatisfied_DoesNotThrow(string[] availableModules,
                                                                     string[] installIdents,
                                                                     string[] dlcIdents)
        {
            // Arrange
            var user    = new NullUser();
            var game    = new KerbalSpaceProgram();
            var crit    = new GameVersionCriteria(new GameVersion(1, 12, 5));
            using (var repo     = new TemporaryRepository(availableModules.Select(MergeWithDefaults)
                                                                          .ToArray()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                registry.SetDlcs(dlcIdents.ToDictionary(ident => ident,
                                                        ident => new UnmanagedModuleVersion("1.0.0")));

                // Act / Assert
                Assert.DoesNotThrow(() =>
                {
                    var rr = new RelationshipResolver(
                        installIdents.Select(ident => registry.LatestAvailable(ident, stabilityTolerance, crit))
                                     .OfType<CkanModule>(),
                        null,
                        RelationshipResolverOptions.DependsOnlyOpts(stabilityTolerance),
                        registry, game, crit);
                });
            }
        }

        [Test,
         TestCase(new string[] {
                      @"{
                          ""identifier"":  ""PopularMod"",
                          ""ksp_version"": ""1.10.0""
                      }",
                  },
                  @"{
                      ""identifier"": ""MyModpack"",
                      ""kind"":       ""metapackage"",
                      ""depends"":    [ { ""name"": ""PopularMod"" } ]
                  }",
                  "Unsatisfied dependency PopularMod (KSP 1.10.0) needed for: MyModpack 1.0"),
         TestCase(new string[] {
                      @"{
                          ""identifier"":  ""IncompatibleDependency"",
                          ""ksp_version"": ""1.11.0""
                      }",
                      @"{
                          ""identifier"":  ""CompatibleDepending1"",
                          ""ksp_version"": ""1.12"",
                          ""depends"":     [ { ""name"": ""IncompatibleDependency"" } ]
                      }",
                      @"{
                          ""identifier"":  ""CompatibleDepending2"",
                          ""ksp_version"": ""1.12"",
                          ""depends"":     [ { ""name"": ""IncompatibleDependency"" } ]
                      }",
                      @"{
                          ""identifier"":  ""CompatibleDepending3"",
                          ""ksp_version"": ""1.12"",
                          ""depends"":     [ { ""name"": ""CompatibleDepending2"" } ]
                      }",
                  },
                  @"{
                      ""identifier"": ""MyModpack"",
                      ""kind"":       ""metapackage"",
                      ""depends"":    [ { ""name"": ""CompatibleDepending1"" },
                                        { ""name"": ""CompatibleDepending3"" } ]
                  }",
                  "Unsatisfied dependency IncompatibleDependency (KSP 1.11.0) needed for: CompatibleDepending1 1.0 (needed for MyModpack 1.0); CompatibleDepending2 1.0 (needed for CompatibleDepending3 1.0, needed for MyModpack 1.0)"),
        ]
        public void Constructor_ModpackWithIncompatibleDepends_Throws(string[] availableModules,
                                                                      string   modpackModule,
                                                                      string   exceptionMessage)
        {
            // Arrange
            var user    = new NullUser();
            var game    = new KerbalSpaceProgram();
            var crit    = new GameVersionCriteria(new GameVersion(1, 12, 5));
            var modpack = CkanModule.FromJson(MergeWithDefaults(modpackModule));
            using (var repo     = new TemporaryRepository(availableModules.Select(MergeWithDefaults)
                                                                          .ToArray()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);

                // Act / Assert
                var exc = Assert.Throws<DependenciesNotSatisfiedKraken>(() =>
                {
                    var rr = new RelationshipResolver(
                        Enumerable.Repeat(modpack, 1), null,
                        RelationshipResolverOptions.DependsOnlyOpts(stabilityTolerance),
                        registry, game, crit);
                });
                Assert.AreEqual(exceptionMessage, exc?.Message);
            }
        }

        [Test,
         TestCase(new string[] {
                      @"{
                          ""identifier"":  ""WildBlueTools"",
                          ""depends"":     [ { ""name"": ""WildBlue-PlayMode"" } ]
                      }",
                      @"{
                          ""identifier"":  ""WildBlue-PlayMode-CRP"",
                          ""provides"":    [ ""WildBlue-PlayMode"" ],
                          ""conflicts"":   [ { ""name"": ""WildBlue-PlayMode"" } ],
                          ""depends"":     [ { ""name"": ""WildBlueTools"" } ]
                      }",
                      @"{
                          ""identifier"":  ""WildBlue-PlayMode-ClassicStock"",
                          ""provides"":    [ ""WildBlue-PlayMode"" ],
                          ""conflicts"":   [ { ""name"": ""WildBlue-PlayMode"" } ],
                          ""depends"":     [ { ""name"": ""WildBlueTools"" } ]
                      }",
                      @"{
                          ""identifier"":  ""Heisenberg"",
                          ""depends"":     [ { ""name"": ""Heisenberg-PlayMode"" },
                                             { ""name"": ""WildBlueTools"" } ]
                      }",
                      @"{
                          ""identifier"":  ""Heisenberg-PlayMode-CRP"",
                          ""provides"":    [ ""Heisenberg-PlayMode"" ],
                          ""conflicts"":   [ { ""name"": ""Heisenberg-PlayMode"" } ],
                          ""depends"":     [ { ""name"": ""WildBlue-PlayMode-CRP"" },
                                             { ""name"": ""Heisenberg"" } ]
                      }",
                      @"{
                          ""identifier"":  ""Heisenberg-PlayMode-ClassicStock"",
                          ""provides"":    [ ""Heisenberg-PlayMode"" ],
                          ""conflicts"":   [ { ""name"": ""Heisenberg-PlayMode"" } ],
                          ""depends"":     [ { ""name"": ""WildBlue-PlayMode-ClassicStock"" },
                                             { ""name"": ""Heisenberg"" } ]
                      }",
                      @"{
                          ""identifier"":  ""DSEV"",
                          ""depends"":     [ { ""name"": ""DSEV-PlayMode"" },
                                             { ""name"": ""WildBlueTools"" } ]
                      }",
                      @"{
                          ""identifier"":  ""DSEV-PlayMode-CRP"",
                          ""provides"":    [ ""DSEV-PlayMode"" ],
                          ""conflicts"":   [ { ""name"": ""DSEV-PlayMode"" } ],
                          ""depends"":     [ { ""name"": ""WildBlue-PlayMode-CRP"" },
                                             { ""name"": ""DSEV"" } ]
                      }",
                      @"{
                          ""identifier"":  ""DSEV-PlayMode-ClassicStock"",
                          ""provides"":    [ ""DSEV-PlayMode"" ],
                          ""conflicts"":   [ { ""name"": ""DSEV-PlayMode"" } ],
                          ""depends"":     [ { ""name"": ""WildBlue-PlayMode-ClassicStock"" },
                                             { ""name"": ""DSEV"" } ]
                      }",
                      @"{
                          ""identifier"":  ""Pathfinder"",
                          ""depends"":     [ { ""name"": ""Pathfinder-PlayMode"" },
                                             { ""name"": ""WildBlueTools"" } ]
                      }",
                      @"{
                          ""identifier"":  ""Pathfinder-PlayMode-CRP"",
                          ""provides"":    [ ""Pathfinder-PlayMode"" ],
                          ""conflicts"":   [ { ""name"": ""Pathfinder-PlayMode"" } ],
                          ""depends"":     [ { ""name"": ""WildBlue-PlayMode-CRP"" },
                                             { ""name"": ""Pathfinder"" } ]
                      }",
                      @"{
                          ""identifier"":  ""Pathfinder-PlayMode-ClassicStock"",
                          ""provides"":    [ ""Pathfinder-PlayMode"" ],
                          ""conflicts"":   [ { ""name"": ""Pathfinder-PlayMode"" } ],
                          ""depends"":     [ { ""name"": ""WildBlue-PlayMode-ClassicStock"" },
                                             { ""name"": ""Pathfinder"" } ]
                      }",
                  },
                  new string[] { "Heisenberg-PlayMode-ClassicStock", "DSEV", "Pathfinder" },
                  new string[] { "DSEV-PlayMode-", "Pathfinder-PlayMode-", "WildBlue-PlayMode-" },
                  new string[] { "CRP" }),
        ]
        public void Constructor_WithPlayModes_DoesNotThrow(string[] availableModules,
                                                           string[] installIdents,
                                                           string[] goodSubstrings,
                                                           string[] badSubstrings)
        {
            var user = new NullUser();
            var game = new KerbalSpaceProgram();
            var crit = new GameVersionCriteria(new GameVersion(1, 12, 5));
            using (var repo     = new TemporaryRepository(availableModules.Select(MergeWithDefaults)
                                                                          .ToArray()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);

                // Act / Assert
                Assert.DoesNotThrow(() =>
                {
                    var rr = new RelationshipResolver(
                        installIdents.Select(ident => registry.LatestAvailable(ident, stabilityTolerance, crit))
                                     .OfType<CkanModule>(),
                        null,
                        RelationshipResolverOptions.DependsOnlyOpts(stabilityTolerance),
                        registry, game, crit);
                    var idents = rr.ModList().Select(m => m.identifier).ToArray();
                    foreach (var goodSubstring in goodSubstrings)
                    {
                        Assert.IsTrue(idents.Any(ident => ident.Contains(goodSubstring)),
                                      $"Some identifier containing {goodSubstring} must be in resolver");
                    }
                    foreach (var ident in idents)
                    {
                        foreach (var badSubstring in badSubstrings)
                        {
                            Assert.IsFalse(ident.Contains(badSubstring),
                                           $"No identifiers containing {badSubstring} should be in resolver");
                        }
                    }
                });
            }
        }

        public static string MergeWithDefaults(string json)
        {
            var incoming = JObject.Parse(json);
            incoming.SafeMerge(moduleDefaults);
            return incoming.ToString();
        }

        public static IEnumerable<string> MergeWithDefaults(params string[] jsons)
            => jsons.Select(MergeWithDefaults);

        // Unimportant required fields that we don't want to duplicate
        private static readonly JObject moduleDefaults = JObject.Parse(
            @"{
                ""spec_version"": ""v1.34"",
                ""name"":         ""A mod or modpack"",
                ""author"":       ""An author"",
                ""version"":      ""1.0"",
                ""download"":     ""https://www.nonexistent.com/download""
            }");

    }
}
