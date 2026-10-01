using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Tests.Data;

using CKAN;
using CKAN.IO;
using CKAN.Configuration;
using CKAN.Versioning;

namespace Tests.Core.IO
{
    [TestFixture]
    public class InstallResolverTests
    {
        private readonly RandomModuleGenerator gen = new RandomModuleGenerator(new Random(0451));
        private readonly StabilityToleranceConfig stability = new StabilityToleranceConfig("");
        private readonly GameVersionCriteria crit = new GameVersionCriteria(GameVersion.Parse("1.12.5"));
        private static readonly GameVersion tooOld = GameVersion.Parse("1.4.1");

        [Test]
        public void Resolve_Compatible_Ready()
        {
            var result = Resolve(new[] { Mod("JNSQ", "0.11.0") }, ("JNSQ", null));

            Assert.AreEqual(InstallOutcome.Ready, result.Outcome);
            Assert.AreEqual("0.11.0", result.Module?.version.ToString());
        }

        [Test]
        public void Resolve_ResolvedVersionInstalled_AlreadyInstalled()
        {
            var jnsq = Mod("JNSQ", "0.11.0");

            Assert.AreEqual(InstallOutcome.AlreadyInstalled,
                            Resolve(new[] { jnsq }, ("JNSQ", null), jnsq).Outcome);
        }

        [Test]
        public void Resolve_DifferentVersionInstalled_Ready()
        {
            Assert.AreEqual(InstallOutcome.Ready,
                            Resolve(new[] { Mod("JNSQ", "0.11.0") }, ("JNSQ", null),
                                    Mod("JNSQ", "0.10.0")).Outcome);
        }

        [Test]
        public void Resolve_OnlyIncompatible_Incompatible()
        {
            Assert.AreEqual(InstallOutcome.Incompatible,
                            Resolve(new[] { Mod("JNSQ", "0.11.0", tooOld) }, ("JNSQ", null)).Outcome);
        }

        [Test]
        public void Resolve_NotInRegistry_Unknown()
        {
            var result = Resolve(new[] { Mod("Astrogator", "1.0.0") }, ("JNSQ", null));

            Assert.AreEqual(InstallOutcome.Unknown, result.Outcome);
            Assert.IsNull(result.Module);
            Assert.AreEqual("JNSQ", result.Query);
        }

        [TestCase("jnsq")]
        [TestCase("JnSq")]
        public void Resolve_QueryCaseInsensitive_Resolves(string query)
        {
            var result = Resolve(new[] { Mod("JNSQ", "0.11.0") }, (query, null));

            Assert.AreEqual(InstallOutcome.Ready, result.Outcome);
            Assert.AreEqual("JNSQ", result.Module?.identifier);
        }

        [Test]
        public void Resolve_PinnedVersionAvailable_Ready()
        {
            var result = Resolve(new[] { Mod("JNSQ", "0.10.0") }, ("JNSQ", "0.10.0"));

            Assert.AreEqual(InstallOutcome.Ready, result.Outcome);
            Assert.AreEqual("0.10.0", result.Module?.version.ToString());
        }

        [Test]
        public void Resolve_PinnedVersionMissing_PinMissedOffersLatest()
        {
            var result = Resolve(new[] { Mod("JNSQ", "0.11.0") }, ("JNSQ", "0.10.0"));

            Assert.AreEqual(InstallOutcome.PinMissed, result.Outcome);
            Assert.AreEqual("0.11.0", result.Module?.version.ToString());
        }

        // A version copied from the UI has no epoch or leading v.
        [TestCase("1.0", "1.00")]
        [TestCase("1.0", "1:1.0")]
        [TestCase("1.0", "v1.0")]
        [TestCase("v1.0", "V1.0")]
        [TestCase("1.0", "1:v1.0")]
        public void Resolve_PinDiffersOnlyByEpochOrV_Ready(string pin, string inRegistry)
        {
            Assert.AreEqual(InstallOutcome.Ready,
                            Resolve(new[] { Mod("JNSQ", inRegistry) }, ("JNSQ", pin)).Outcome);
        }

        [TestCase("0.10", "0.10.0")]
        [TestCase("1.0", "2.0")]
        public void Resolve_PinDiffersByNumber_PinMissed(string pin, string inRegistry)
        {
            Assert.AreEqual(InstallOutcome.PinMissed,
                            Resolve(new[] { Mod("JNSQ", inRegistry) }, ("JNSQ", pin)).Outcome);
        }

        [Test]
        public void Resolve_PinMatchesSeveralEpochs_TakesHighest()
        {
            var result = Resolve(new[] { Mod("JNSQ", "2:1.0"), Mod("JNSQ", "1:1.0") }, ("JNSQ", "1.0"));

            Assert.AreEqual(InstallOutcome.Ready, result.Outcome);
            Assert.AreEqual("2:1.0", result.Module?.version.ToString());
        }

        // Make sure a pinned version doesn't bypass compatibility, even when the mod's latest version is compatible.
        [Test]
        public void Resolve_PinnedVersionIncompatible_Incompatible()
        {
            var result = Resolve(new[] { ("JNSQ", (string?)"0.9.0") },
                                 new[] { Mod("JNSQ", "0.11.0"), Mod("JNSQ", "0.9.0", tooOld) })
                         .Single();

            Assert.AreEqual(InstallOutcome.Incompatible, result.Outcome);
            Assert.AreEqual("0.9.0", result.Module?.version.ToString());
        }

        [Test]
        public void Resolve_MixedBatch_KeepsUrlOrderAndClassifiesEach()
        {
            var results = Resolve(
                new[] { ("Astrogator", null), ("RealChute", null), ("NoSuchMod", null), ("JNSQ", "9.9") },
                new[] { Mod("JNSQ", "0.11.0"), Mod("Astrogator", "1.0.0"), Mod("RealChute", "1.4.8", tooOld) });

            CollectionAssert.AreEqual(
                new[] { "Astrogator", "RealChute", "NoSuchMod", "JNSQ" },
                results.Select(r => r.Query));
            CollectionAssert.AreEqual(
                new[] { InstallOutcome.Ready, InstallOutcome.Incompatible, InstallOutcome.Unknown, InstallOutcome.PinMissed },
                results.Select(r => r.Outcome));
        }

        [Test]
        public void Resolve_DuplicateIdentifiers_ResolvedOnce()
        {
            var results = Resolve(new[] { ("JNSQ", (string?)null), ("JNSQ", null) },
                                  new[] { Mod("JNSQ", "0.11.0") });

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(InstallOutcome.Ready, results[0].Outcome);
        }

        private List<ResolvedMod> Resolve((string, string?)[] mods, CkanModule[] available, params CkanModule[] installed)
        {
            var user = new NullUser();
            using (var ksp      = new DisposableKSP())
            using (var repo     = new TemporaryRepository(available.Select(m => m.ToJson()).ToArray()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                foreach (var m in installed)
                {
                    registry.RegisterModule(m, Array.Empty<string>(), ksp.KSP, false);
                }

                return InstallResolver.Resolve(mods, registry, stability, crit);
            }
        }

        private ResolvedMod Resolve(CkanModule[] available, (string, string?) mod, params CkanModule[] installed)
            => Resolve(new[] { mod }, available, installed).Single();

        private CkanModule Mod(string identifier, string version, GameVersion? gameVersion = null)
            => gen.GenerateRandomModule(identifier: identifier,
                                        version: new ModuleVersion(version),
                                        ksp_version: gameVersion);
    }
}
