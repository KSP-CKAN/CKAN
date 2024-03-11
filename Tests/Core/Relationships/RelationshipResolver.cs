using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Tests.Data;

using CKAN;
using CKAN.Versioning;
using RelationshipDescriptor = CKAN.RelationshipDescriptor;

namespace Tests.Core.Relationships
{
    [TestFixture]
    public class RelationshipResolverTests
    {
        private RelationshipResolverOptions options;
        private RandomModuleGenerator generator;

        [SetUp]
        public void Setup()
        {
            options = RelationshipResolverOptions.DefaultOpts();
            generator = new RandomModuleGenerator(new Random(0451));
            //Sanity checker means even incorrect RelationshipResolver logic was passing
            options.without_enforce_consistency = true;
        }

        [Test]
        public void Constructor_WithoutModules_AlwaysReturns()
        {
            var registry = CKAN.Registry.Empty();
            options = RelationshipResolverOptions.DefaultOpts();
            Assert.DoesNotThrow(() => new RelationshipResolver(new List<CkanModule>(),
                null, options, registry, null));
        }

        [Test]
        public void Constructor_WithConflictingModules()
        {
            var mod_a = generator.GeneratorRandomModule();
            var mod_b = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor { name = mod_a.identifier }
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(mod_a),
                                                      CkanModule.ToJson(mod_b)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);

                var list = new List<CkanModule> { mod_a, mod_b };
                Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                    list, null, options, registry, null));

                options.proceed_with_inconsistencies = true;
                var resolver = new RelationshipResolver(list, null, options, registry, null);

                Assert.That(resolver.ConflictList.Any(s => Equals(s.Key, mod_a)));
                Assert.That(resolver.ConflictList.Any(s => Equals(s.Key, mod_b)));
                Assert.That(resolver.ConflictList, Has.Count.EqualTo(2));
            }
        }

        [Test]
        [Category("Version")]
        public void Constructor_WithConflictingModulesVersion_Throws()
        {
            var mod_a = generator.GeneratorRandomModule();
            var mod_b = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name=mod_a.identifier, version=mod_a.version}
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(mod_a),
                                                      CkanModule.ToJson(mod_b)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod_a, mod_b };

                Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                    list, null, options, registry, null));
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "0.5")]
        [TestCase("1.0", "1.0")]
        public void Constructor_WithConflictingModulesVersionMin_Throws(string ver, string conf_min)
        {
            var mod_a = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var mod_b = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name=mod_a.identifier, min_version=new ModuleVersion(conf_min)}
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(mod_a),
                                                      CkanModule.ToJson(mod_b)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod_a, mod_b };

                Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                    list, null, options, registry, null));
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "2.0")]
        [TestCase("1.0", "1.0")]
        public void Constructor_WithConflictingModulesVersionMax_Throws(string ver, string conf_max)
        {
            var mod_a = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var mod_b = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name=mod_a.identifier, max_version=new ModuleVersion(conf_max)}
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(mod_a),
                                                      CkanModule.ToJson(mod_b)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod_a, mod_b };

                Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                    list, null, options, registry, null));
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "0.5", "2.0")]
        [TestCase("1.0", "1.0", "2.0")]
        [TestCase("1.0", "0.5", "1.0")]
        public void Constructor_WithConflictingModulesVersionMinMax_Throws(string ver, string conf_min, string conf_max)
        {
            var mod_a = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var mod_b = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor
                {
                    name = mod_a.identifier,
                    min_version = new ModuleVersion(conf_min),
                    max_version = new ModuleVersion(conf_max)
                }
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(mod_a),
                                                      CkanModule.ToJson(mod_b)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod_a, mod_b };

                Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                    list, null, options, registry, null));
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "0.5")]
        [TestCase("1.0", "2.0")]
        public void Constructor_WithNonConflictingModulesVersion_DoesNotThrow(string ver, string conf)
        {
            var mod_a = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var mod_b = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name=mod_a.identifier, version=new ModuleVersion(conf)}
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(mod_a),
                                                      CkanModule.ToJson(mod_b)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod_a, mod_b };

                Assert.DoesNotThrow(() => new RelationshipResolver(
                    list, null, options, registry, null));
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "2.0")]
        public void Constructor_WithConflictingModulesVersionMin_DoesNotThrow(string ver, string conf_min)
        {
            var mod_a = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var mod_b = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name=mod_a.identifier, min_version=new ModuleVersion(conf_min)}
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(mod_a),
                                                      CkanModule.ToJson(mod_b)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod_a, mod_b };

                Assert.DoesNotThrow(() => new RelationshipResolver(
                    list, null, options, registry, null));
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("2.0", "1.0")]
        public void Constructor_WithConflictingModulesVersionMax_DoesNotThrow(string ver, string conf_max)
        {
            var mod_a = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var mod_b = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name=mod_a.identifier, max_version=new ModuleVersion(conf_max)}
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(mod_a),
                                                      CkanModule.ToJson(mod_b)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod_a, mod_b };

                Assert.DoesNotThrow(() => new RelationshipResolver(
                    list, null, options, registry, null));
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "2.0", "3.0")]
        [TestCase("4.0", "2.0", "3.0")]
        public void Constructor_WithConflictingModulesVersionMinMax_DoesNotThrow(string ver, string conf_min, string conf_max)
        {
            var mod_a = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var mod_b = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name=mod_a.identifier, min_version=new ModuleVersion(conf_min), max_version=new ModuleVersion(conf_max)}
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(mod_a),
                                                      CkanModule.ToJson(mod_b)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod_a, mod_b };

                Assert.DoesNotThrow(() => new RelationshipResolver(
                    list, null, options, registry, null));
            }
        }

        [Test]
        public void Constructor_WithMultipleModulesProviding_Throws()
        {
            options.without_toomanyprovides_kraken = false;

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

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(mod_b),
                                                      CkanModule.ToJson(mod_c),
                                                      CkanModule.ToJson(mod_d)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod_d };

                Assert.Throws<TooManyModsProvideKraken>(() => new RelationshipResolver(
                    list, null, options, registry, null));
            }
        }

        [Test]
        public void ModList_WithInstalledModules_ContainsThemWithReasonInstalled()
        {
            var user = new NullUser();
            var mod_a = generator.GeneratorRandomModule();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(mod_a)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod_a };

                registry.RegisterModule(mod_a, new List<string>(), null, false);

                var relationship_resolver = new RelationshipResolver(
                    list, null, options, registry, null);
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
            options.with_all_suggests = true;
            var suggested = generator.GeneratorRandomModule();
            var suggester = generator.GeneratorRandomModule(suggests: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = suggested.identifier}
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(suggested),
                                                      CkanModule.ToJson(suggester)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                registry.Installed().Add(suggested.identifier, suggested.version);
                var list = new List<CkanModule> { suggester };

                var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);
                CollectionAssert.Contains(relationship_resolver.ModList(), suggested);
            }
        }

        [Test]
        public void ModList_WithSuggestedModulesThatWouldConflict_DoesNotContainThem()
        {
            options.with_all_suggests = true;
            var suggested = generator.GeneratorRandomModule();
            var mod = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = suggested.identifier}
            });
            var suggester = generator.GeneratorRandomModule(suggests: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = suggested.identifier}
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(suggested),
                                                      CkanModule.ToJson(suggester),
                                                      CkanModule.ToJson(mod)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { suggester, mod };

                var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);
                CollectionAssert.DoesNotContain(relationship_resolver.ModList(), suggested);
            }
        }

        [Test]
        public void Constructor_WithConflictingModulesInDependencies_ThrowUnderDefaultSettings()
        {
            var dependant = generator.GeneratorRandomModule();
            var depender = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = dependant.identifier}
            });
            var conflicts_with_dependant = generator.GeneratorRandomModule(conflicts: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name=dependant.identifier}
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(depender),
                                                      CkanModule.ToJson(dependant),
                                                      CkanModule.ToJson(conflicts_with_dependant)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { depender, conflicts_with_dependant };

                Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                    list, null, options, registry, null));
            }
        }

        [Test]
        public void Constructor_WithSuggests_HasSuggestedInModlist()
        {
            options.with_all_suggests = true;
            var suggested = generator.GeneratorRandomModule();
            var suggester = generator.GeneratorRandomModule(suggests: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = suggested.identifier}
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(suggester),
                                                      CkanModule.ToJson(suggested)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { suggester };

                var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);
                CollectionAssert.Contains(relationship_resolver.ModList(), suggested);
            }
        }

        [Test]
        public void Constructor_ContainsSugestedOfSuggested_When_With_all_suggests()
        {
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

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(suggester),
                                                      CkanModule.ToJson(suggested),
                                                      CkanModule.ToJson(suggested2)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { suggester };

                options.with_all_suggests = true;
                var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);
                CollectionAssert.Contains(relationship_resolver.ModList(), suggested2);

                options.with_all_suggests = false;

                relationship_resolver = new RelationshipResolver(list, null, options, registry, null);
                CollectionAssert.DoesNotContain(relationship_resolver.ModList(), suggested2);
            }
        }

        [Test]
        public void Constructor_ProvidesSatisfyDependencies()
        {
            var mod_a = generator.GeneratorRandomModule();
            var mod_b = generator.GeneratorRandomModule(provides: new List<string>
            {
                mod_a.identifier
            });
            var depender = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = mod_a.identifier}
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(mod_b),
                                                      CkanModule.ToJson(depender)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { depender };
                var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);

                CollectionAssert.AreEquivalent(relationship_resolver.ModList(), new List<CkanModule>
                {
                    mod_b,
                    depender
                });
            }
        }

        [Test]
        public void Constructor_WithMissingDependants_Throws()
        {
            var dependant = generator.GeneratorRandomModule();
            var depender = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = dependant.identifier}
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(depender)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);

                Assert.Throws<DependencyNotSatisfiedKraken>(() =>
                    new RelationshipResolver(new List<CkanModule> { depender },
                                             null, options, registry, null));
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "2.0")]
        [TestCase("1.0", "0.2")]
        [TestCase("0",   "0.2")]
        [TestCase("1.0", "0")]
        public void Constructor_WithMissingDependantsVersion_Throws(string ver, string dep)
        {
            var dependant = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var depender = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = dependant.identifier, version = new ModuleVersion(dep)}
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(depender),
                                                      CkanModule.ToJson(dependant)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);

                var list = new List<CkanModule> { depender };

                Assert.Throws<DependencyNotSatisfiedKraken>(() => new RelationshipResolver(
                    list, null, options, registry, null));
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "2.0")]
        public void Constructor_WithMissingDependantsVersionMin_Throws(string ver, string dep_min)
        {
            var dependant = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var depender = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = dependant.identifier, min_version = new ModuleVersion(dep_min)}
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(depender),
                                                      CkanModule.ToJson(dependant)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule>() { depender };

                Assert.Throws<DependencyNotSatisfiedKraken>(() => new RelationshipResolver(
                    list, null, options, registry, null));
                list.Add(dependant);
                Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                    list, null, options, registry, null));
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "0.5")]
        public void Constructor_WithMissingDependantsVersionMax_Throws(string ver, string dep_max)
        {
            var dependant = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var depender = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = dependant.identifier, max_version = new ModuleVersion(dep_max)}
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(depender),
                                                      CkanModule.ToJson(dependant)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { depender, dependant };

                Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                    list, null, options, registry, null));
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "2.0", "3.0")]
        [TestCase("4.0", "2.0", "3.0")]
        public void Constructor_WithMissingDependantsVersionMinMax_Throws(string ver, string dep_min, string dep_max)
        {
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

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(depender),
                                                      CkanModule.ToJson(dependant)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { depender, dependant };

                Assert.Throws<InconsistentKraken>(() => new RelationshipResolver(
                    list, null, options, registry, null));
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("1.0", "1.0", "2.0")]
        [TestCase("1.0", "1.0", "0.5")]//what to do if a mod is present twice with the same version ?
        public void Constructor_WithDependantVersion_ChooseCorrectly(string ver, string dep, string other)
        {
            var dependant = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var other_dependant = generator.GeneratorRandomModule(identifier: dependant.identifier, version: new ModuleVersion(other));

            var depender = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = dependant.identifier, version = new ModuleVersion(dep)}
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(depender),
                                                      CkanModule.ToJson(dependant),
                                                      CkanModule.ToJson(other_dependant)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { depender };

                var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);
                CollectionAssert.AreEquivalent(relationship_resolver.ModList(), new List<CkanModule>
                {
                    dependant,
                    depender
                });
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("2.0", "1.0", "0.5")]
        [TestCase("2.0", "1.0", "1.5")]
        [TestCase("2.0", "2.0", "0.5")]
        public void Constructor_WithDependantVersionMin_ChooseCorrectly(string ver, string dep_min, string other)
        {
            var dependant = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var other_dependant = generator.GeneratorRandomModule(identifier: dependant.identifier, version: new ModuleVersion(other));

            var depender = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = dependant.identifier, min_version = new ModuleVersion(dep_min)}
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(depender),
                                                      CkanModule.ToJson(dependant),
                                                      CkanModule.ToJson(other_dependant)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { depender };

                var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);
                CollectionAssert.AreEquivalent(relationship_resolver.ModList(), new List<CkanModule>
                {
                    dependant,
                    depender
                });
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("2.0", "2.0", "0.5")]
        [TestCase("2.0", "3.0", "0.5")]
        [TestCase("2.0", "3.0", "4.0")]
        public void Constructor_WithDependantVersionMax_ChooseCorrectly(string ver, string dep_max, string other)
        {
            var dependant = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var other_dependant = generator.GeneratorRandomModule(identifier: dependant.identifier, version: new ModuleVersion(other));

            var depender = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor {name = dependant.identifier, max_version = new ModuleVersion(dep_max)}
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(depender),
                                                      CkanModule.ToJson(dependant),
                                                      CkanModule.ToJson(other_dependant)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { depender };

                var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);
                CollectionAssert.AreEquivalent(relationship_resolver.ModList(), new List<CkanModule>
                {
                    dependant,
                    depender
                });
            }
        }

        [Test]
        [Category("Version")]
        [TestCase("2.0", "1.0", "3.0", "0.5")]
        [TestCase("2.0", "1.0", "3.0", "1.5")]
        [TestCase("2.0", "1.0", "3.0", "3.5")]
        public void Constructor_WithDependantVersionMinMax_ChooseCorrectly(string ver, string dep_min, string dep_max, string other)
        {
            var dependant = generator.GeneratorRandomModule(version: new ModuleVersion(ver));
            var other_dependant = generator.GeneratorRandomModule(identifier: dependant.identifier, version: new ModuleVersion(other));

            var depender = generator.GeneratorRandomModule(depends: new List<RelationshipDescriptor>
            {
                new ModuleRelationshipDescriptor
                {
                    name = dependant.identifier,
                    min_version = new ModuleVersion(dep_min),
                    max_version = new ModuleVersion(dep_max)
                }
            });

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(depender),
                                                      CkanModule.ToJson(dependant),
                                                      CkanModule.ToJson(other_dependant)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { depender };

                var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);
                CollectionAssert.AreEquivalent(relationship_resolver.ModList(), new List<CkanModule>
                {
                    dependant,
                    depender
                });
            }
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

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(depender),
                                                      CkanModule.ToJson(olderDependency),
                                                      CkanModule.ToJson(newerDependency)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);

                // Act
                RelationshipResolver rr = new RelationshipResolver(
                    new CkanModule[] { depender }, null,
                    options, registry, null);

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

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(depender),
                                                      CkanModule.ToJson(olderDependency),
                                                      CkanModule.ToJson(newerDependency)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);

                // Act
                RelationshipResolver rr = new RelationshipResolver(
                    new CkanModule[] { depender }, null,
                    options, registry, null);

                // Assert
                CollectionAssert.Contains(      rr.ModList(), olderDependency);
                CollectionAssert.DoesNotContain(rr.ModList(), newerDependency);
            }
        }

        [Test]
        public void ReasonFor_WithModsNotInList_Empty()
        {
            var mod = generator.GeneratorRandomModule();

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(mod)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod };
                var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);

                var mod_not_in_resolver_list = generator.GeneratorRandomModule();
                CollectionAssert.DoesNotContain(relationship_resolver.ModList(), mod_not_in_resolver_list);
                Assert.IsEmpty(relationship_resolver.ReasonsFor(mod_not_in_resolver_list));
            }
        }

        [Test]
        public void ReasonFor_WithUserAddedMods_GivesReasonUserAdded()
        {
            var mod = generator.GeneratorRandomModule();

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(mod)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod };

                var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);
                var reasons = relationship_resolver.ReasonsFor(mod);
                Assert.That(reasons[0], Is.AssignableTo<SelectionReason.UserRequested>());
            }
        }

        [Test]
        public void ReasonFor_WithSuggestedMods_GivesCorrectParent()
        {
            var suggested = generator.GeneratorRandomModule();
            var mod =
                generator.GeneratorRandomModule(suggests:
                    new List<RelationshipDescriptor> {new ModuleRelationshipDescriptor {name = suggested.identifier}});

            var user = new NullUser();
            using (var repo = new TemporaryRepository(CkanModule.ToJson(mod),
                                                      CkanModule.ToJson(suggested)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod };

                options.with_all_suggests = true;
                var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);
                var reasons = relationship_resolver.ReasonsFor(suggested);

                Assert.That(reasons[0], Is.AssignableTo<SelectionReason.Suggested>());
                Assert.That(reasons[0].Parent, Is.EqualTo(mod));
            }
        }

        [Test]
        public void ReasonFor_WithTreeOfMods_GivesCorrectParents()
        {
            var suggested = generator.GeneratorRandomModule();
            var recommendedA = generator.GeneratorRandomModule();
            var recommendedB = generator.GeneratorRandomModule();
            var mod = generator.GeneratorRandomModule(
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
            using (var repo = new TemporaryRepository(CkanModule.ToJson(mod),
                                                      CkanModule.ToJson(suggested),
                                                      CkanModule.ToJson(recommendedA),
                                                      CkanModule.ToJson(recommendedB)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var list = new List<CkanModule> { mod };

                options.with_all_suggests = true;
                options.with_recommends = true;
                var relationship_resolver = new RelationshipResolver(list, null, options, registry, null);
                var reasons = relationship_resolver.ReasonsFor(recommendedA);
                Assert.That(reasons[0], Is.AssignableTo<SelectionReason.Recommended>());
                Assert.That(reasons[0].Parent, Is.EqualTo(suggested));

                reasons = relationship_resolver.ReasonsFor(recommendedB);
                Assert.That(reasons[0], Is.AssignableTo<SelectionReason.Recommended>());
                Assert.That(reasons[0].Parent, Is.EqualTo(suggested));
            }
        }

        // The whole point of autodetected mods is they can participate in relationships.
        // This makes sure they can (at least for dependencies). It may overlap with other
        // tests, but that's cool, beacuse it's a test. :D
        [Test]
        public void AutodetectedCanSatisfyRelationships()
        {
            using (var ksp = new DisposableKSP())
            {
                var registry = CKAN.Registry.Empty();
                registry.SetDlls(new Dictionary<string, string>()
                {
                    {
                        "ModuleManager",
                        ksp.KSP.ToRelativeGameDir(Path.Combine(ksp.KSP.game.PrimaryModDirectory(ksp.KSP),
                                                               "ModuleManager.dll"))
                    }
                });

                var depends = new List<RelationshipDescriptor>
                {
                    new ModuleRelationshipDescriptor { name = "ModuleManager" }
                };

                CkanModule mod = generator.GeneratorRandomModule(depends: depends);

                new RelationshipResolver(
                    new CkanModule[] { mod }, null, RelationshipResolverOptions.DefaultOpts(),
                    registry, new GameVersionCriteria(GameVersion.Parse("1.0.0")));
            }
        }

        // Models the EVE - EVE-Config - AVP - AVP-Textures relationship
        [Test]
        public void UninstallingConflictingModule_InstallingRecursiveDependencies_ResolvesSuccessfully()
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
            var user = new NullUser();
            using (var ksp = new DisposableKSP())
            using (var repo = new TemporaryRepository(CkanModule.ToJson(eve),
                                                      CkanModule.ToJson(eveDefaultConfig),
                                                      CkanModule.ToJson(avp),
                                                      CkanModule.ToJson(avp2kTextures)))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);

                // Start with eve and eveDefaultConfig installed
                registry.RegisterModule(eve, new List<string>(), ksp.KSP, false);
                registry.RegisterModule(eveDefaultConfig, new List<string>(), ksp.KSP, false);

                Assert.DoesNotThrow(() => registry.CheckSanity());

                List<CkanModule> modulesToInstall;
                List<CkanModule> modulesToRemove;
                RelationshipResolver resolver;

                // Act and assert: play through different possible user interactions
                // Scenario 1 - Try installing AVP, expect an exception for proceed_with_inconsistencies=false

                modulesToInstall = new List<CkanModule> { avp };
                modulesToRemove = new List<CkanModule>();

                options.proceed_with_inconsistencies = false;
                var exception = Assert.Throws<InconsistentKraken>(() =>
                {
                    resolver = new RelationshipResolver(modulesToInstall, modulesToRemove, options, registry, null);
                });
                Assert.AreEqual($"{avp} conflicts with {eveDefaultConfig}",
                                exception.ShortDescription);

                // Scenario 2 - Try installing AVP, expect no exception for proceed_with_inconsistencies=true, but a conflict list

                resolver = null;
                options.proceed_with_inconsistencies = true;
                Assert.DoesNotThrow(() =>
                {
                    resolver = new RelationshipResolver(modulesToInstall, modulesToRemove, options, registry, null);
                });
                CollectionAssert.AreEquivalent(modulesToInstall,
                                               resolver.ConflictList.Keys);
                CollectionAssert.AreEquivalent(new List<string> {$"{avp} conflicts with {eveDefaultConfig}"},
                                               resolver.ConflictList.Values);

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
    }
}
