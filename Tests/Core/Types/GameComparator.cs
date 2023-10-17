using System;
using CKAN;
using CKAN.Versioning;
using NUnit.Framework;
using Tests.Data;

namespace Tests.Core.Types
{
    [TestFixture]
    public class GameComparator
    {
        static readonly GameVersion gameVersion = GameVersion.Parse("1.0.4");
        CkanModule gameMod;

        [SetUp]
        public void Setup()
        {
            // Refresh our mod every time since our tests will hack its version and things.
            gameMod = TestData.kOS_014_module();
        }

        [Test]
        [TestCase(typeof(CKAN.StrictGameComparator), true)]
        [TestCase(typeof(CKAN.GrasGameComparator), true)]
        [TestCase(typeof(CKAN.YoyoGameComparator), true)]
        public void TotallyCompatible(Type type, bool expected)
        {
            var comparator = (CKAN.IGameComparator) Activator.CreateInstance(type);

            // Mark the mod as being for 1.0.4
            gameMod.ksp_version = gameMod.ksp_version_min = gameMod.ksp_version_max
                = GameVersion.Parse("1.0.4");

            // Now test!
            Assert.AreEqual(expected, comparator.Compatible(new GameVersionCriteria (gameVersion), gameMod));
        }

        [Test]
        [TestCase(typeof(CKAN.StrictGameComparator), false)]
        [TestCase(typeof(CKAN.GrasGameComparator), true)]
        [TestCase(typeof(CKAN.YoyoGameComparator), true)]
        public void GenerallySafeLax(Type type, bool expected)
        {
            var comparator = (CKAN.IGameComparator) Activator.CreateInstance(type);

            // We're going to tweak compatibly to mark the mod as being for 1.0.3
            gameMod.ksp_version = gameMod.ksp_version_min = gameMod.ksp_version_max
                = GameVersion.Parse("1.0.3");

            // Now test!
            Assert.AreEqual(expected, comparator.Compatible(new GameVersionCriteria (gameVersion), gameMod));
        }

        [Test]
        [TestCase(typeof(CKAN.StrictGameComparator), false)]
        [TestCase(typeof(CKAN.GrasGameComparator), false)]
        [TestCase(typeof(CKAN.YoyoGameComparator), true)]
        public void GenerallySafeStrict(Type type, bool expected)
        {
            var comparator = (CKAN.IGameComparator) Activator.CreateInstance(type);

            // We're going to tweak compatibly to mark the mod as being for 1.0.3 ONLY
            gameMod.ksp_version = gameMod.ksp_version_min = gameMod.ksp_version_max
                = GameVersion.Parse("1.0.3");

            gameMod.ksp_version_strict = true;

            // Now test!
            Assert.AreEqual(expected, comparator.Compatible(new GameVersionCriteria (gameVersion), gameMod));
        }

        [Test]
        [TestCase(typeof(CKAN.StrictGameComparator), false)]
        [TestCase(typeof(CKAN.GrasGameComparator), false)]
        [TestCase(typeof(CKAN.YoyoGameComparator), true)]
        public void Incompatible(Type type, bool expected)
        {
            var comparator = (CKAN.IGameComparator) Activator.CreateInstance(type);

            // The mod already starts off being incompatible, so just do the test. :)
            Assert.AreEqual(expected, comparator.Compatible(new GameVersionCriteria (gameVersion), gameMod));
        }

        public static readonly object[] TestStrictGameComparatorCases =
        {
                        //MOD comapat.      //KSP           //expected
            new object[] { "1.0",           "1.0.4",        true },
            new object[] { "1.1",           "1.0.4",        false },

            new object[] { "1.0.4.1234",    "1.0.4.1234",   true },
            new object[] { "1.0.4.1235",    "1.0.4.1234",   false },
            new object[] { "1.0.4",         "1.0.4.1234",   true },
            new object[] { "1.0.4.1234",    "1.0.4",        true },

            new object[] { "1.0.4.0000",    "1.0.4",        true },
            new object[] { "1.0",           "1.0",          true },
            new object[] { "1.1",           "1.1",          true },
            new object[] { "1.1",           "1.0",          false },
            new object[] { "1.0",           "1.1",          false },

            new object[] { "1.0.4",         "1",            true },
            new object[] { "1.0.4",         "1.0",          true },
            new object[] { "1.0.4",         "1.0.4",        true },
            new object[] { "1.0.4",         "1.0.4.1234",   true },
            new object[] { "1.0.4",         "1.0.5",        false },
            new object[] { "1.0.4",         "1.0.3",        false },
            new object[] { "1.0.4",         "1.1",          false },
            new object[] { "1.0.4",         "0.9",          false },

            new object[] { "1",             "1",            true },
            new object[] { "1",             "1.0",          true },
            new object[] { "1",             "1.0.4",        true },
            new object[] { "1",             "1.0.4.1234",   true },
            new object[] { "1",             "2",            false },
            new object[] { "1",             "2.1",          false },
            new object[] { "1",             "0",            false },
            new object[] { "1",             "0.9",          false },
        };

        [TestCaseSource("TestStrictGameComparatorCases")]
        public void TestStrictGameComparator(String modVersion, String gameVersion, bool expectedResult)
        {
            var comparator = new CKAN.StrictGameComparator();

            // We're going to tweak compatibly of the mod
            gameMod.ksp_version = GameVersion.Parse(modVersion);

            // Now test!
            Assert.AreEqual(expectedResult, comparator.Compatible(new GameVersionCriteria(GameVersion.Parse(gameVersion)), gameMod));
        }

        public static readonly object[] TestStrictGameComparatorMinMaxCases =
        {
                        //Min comapat     //Max comapat     //KSP           //expected
            new object[] { "1.0.4",           null,           "1.0.3",       false },
            new object[] { "1.0.4",           null,           "1.0.4",       true },
            new object[] { "1.0.4",           null,           "1.0.5",       true },
            new object[] { "1.0.4",           null,           "1.1",         true },

            new object[] { "1.0",           null,           "0.9",          false },
            new object[] { "1.0",           null,           "1.0",          true },
            new object[] { "1.0",           null,           "1.0.4",        true },
            new object[] { "1.0",           null,           "1.1",          true },

            new object[] { "1.1",           null,           "1.0.4",        false },
            new object[] { "1.1",           null,           "1.1",          true },
            new object[] { "1.1",           null,           "1.1.1",        true },
            new object[] { "1.1",           null,           "1.2",          true },

            new object[] { null,            "1.0.4",        "1.0.5",        false },
            new object[] { null,            "1.0.4",        "1.0.4",        true },
            new object[] { null,            "1.0.4",        "1.0.3",        true },
            new object[] { null,            "1.0.4",        "1.0",          true },

            new object[] { null,            "1.0",          "0.9",        true },
            new object[] { null,            "1.0",          "1.0",        true },
            new object[] { null,            "1.0",          "1.0.4",      true },
            new object[] { null,            "1.0",          "1.1",        false },

            new object[] { null,            "1.1",          "1.0",        true },
            new object[] { null,            "1.1",          "1.1",        true },
            new object[] { null,            "1.1",          "1.1.1",      true },
            new object[] { null,            "1.1",          "1.2",        false },
        };

        [TestCaseSource("TestStrictGameComparatorMinMaxCases")]
        public void TestStrictGameComparatorMinMax(String modMinVersion, String modMaxVersion, String gameVersion, bool expectedResult)
        {
            var comparator = new CKAN.StrictGameComparator();

            gameMod.ksp_version = null;
            gameMod.ksp_version_min = modMinVersion == null ? null : GameVersion.Parse(modMinVersion);
            gameMod.ksp_version_max = modMaxVersion == null ? null : GameVersion.Parse(modMaxVersion);

            // Now test!
            Assert.AreEqual(expectedResult, comparator.Compatible(new GameVersionCriteria(GameVersion.Parse(gameVersion)), gameMod));
        }
    }
}
