using System;
using NUnit.Framework;
using Tests.Data;
using log4net;

namespace Tests.Core.Types
{
    [TestFixture]
    public class GameComparator
    {
        static readonly CKAN.KSPVersion gameVersion = new CKAN.KSPVersion("1.0.5");
        CKAN.CkanModule gameMod;

        [SetUp]
        public void Setup()
        {
            // Refresh our mod every time since our tests will hack its version and things.
            gameMod = TestData.kOS_014_module();
        }

        [Test]
        [TestCase(typeof(CKAN.GameComparatorStrict), true)]
        [TestCase(typeof(CKAN.GameComparatorGRAS), true)]
        [TestCase(typeof(CKAN.GameComparatorYOYO), true)]
        public void TotallyCompatible(Type type, bool expected)
        {
            var comparator = (CKAN.IGameComparator) Activator.CreateInstance(type);

            // Mark the mod as being for 1.0.5
            gameMod.ksp_version = gameMod.ksp_version_min = gameMod.ksp_version_max
                = new CKAN.KSPVersion("1.0.5");

            // Now test!
            Assert.AreEqual(expected, comparator.Compatible(gameVersion, gameMod));
        }

        [Test]
        [TestCase(typeof(CKAN.GameComparatorStrict), false)]
        [TestCase(typeof(CKAN.GameComparatorGRAS), true)]
        [TestCase(typeof(CKAN.GameComparatorYOYO), true)]
        public void GenerallySafeLax(Type type, bool expected)
        {
            var comparator = (CKAN.IGameComparator) Activator.CreateInstance(type);

            // We're going to tweak compatibly to mark the mod as being for 1.0.4
            gameMod.ksp_version = gameMod.ksp_version_min = gameMod.ksp_version_max
                = new CKAN.KSPVersion("1.0.4");

            // Now test!
            Assert.AreEqual(expected, comparator.Compatible(gameVersion, gameMod));
        }

        [Test]
        [TestCase(typeof(CKAN.GameComparatorStrict), false)]
        [TestCase(typeof(CKAN.GameComparatorGRAS), false)]
        [TestCase(typeof(CKAN.GameComparatorYOYO), true)]
        public void GenerallySafeStrict(Type type, bool expected)
        {
            var comparator = (CKAN.IGameComparator) Activator.CreateInstance(type);

            // We're going to tweak compatibly to mark the mod as being for 1.0.4 ONLY
            gameMod.ksp_version = gameMod.ksp_version_min = gameMod.ksp_version_max
                = new CKAN.KSPVersion("1.0.4");

            gameMod.ksp_version_strict = true;

            // Now test!
            Assert.AreEqual(expected, comparator.Compatible(gameVersion, gameMod));
        }

        [Test]
        [TestCase(typeof(CKAN.GameComparatorStrict), false)]
        [TestCase(typeof(CKAN.GameComparatorGRAS), false)]
        [TestCase(typeof(CKAN.GameComparatorYOYO), true)]
        public void Incompatible(Type type, bool expected)
        {
            var comparator = (CKAN.IGameComparator) Activator.CreateInstance(type);

            // The mod already starts off being incompatible, so just do the test. :)
            Assert.AreEqual(expected, comparator.Compatible(gameVersion, gameMod));
        }
    }
}

