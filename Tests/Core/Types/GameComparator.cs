using System;

using NUnit.Framework;

using CKAN;
using CKAN.Versioning;
using Tests.Data;

namespace Tests.Core.Types
{
    [TestFixture]
    public class GameComparator
    {
        private static readonly GameVersion gameVersion = GameVersion.Parse("1.0.4");
        private CkanModule gameMod;

        [SetUp]
        public void Setup()
        {
            // Refresh our mod every time since our tests will hack its version and things.
            gameMod = TestData.kOS_014_module();
        }

        [Test, TestCase(typeof(StrictGameComparator), true)]
        public void TotallyCompatible(Type type, bool expected)
        {
            var comparator = (IGameComparator) Activator.CreateInstance(type);

            // Mark the mod as being for 1.0.4
            gameMod.ksp_version = gameMod.ksp_version_min = gameMod.ksp_version_max
                = GameVersion.Parse("1.0.4");

            // Now test!
            Assert.AreEqual(expected, comparator.Compatible(new GameVersionCriteria (gameVersion), gameMod));
        }

        [Test, TestCase(typeof(StrictGameComparator), false)]
        public void GenerallySafeLax(Type type, bool expected)
        {
            var comparator = (IGameComparator) Activator.CreateInstance(type);

            // We're going to tweak compatibly to mark the mod as being for 1.0.3
            gameMod.ksp_version = gameMod.ksp_version_min = gameMod.ksp_version_max
                = GameVersion.Parse("1.0.3");

            // Now test!
            Assert.AreEqual(expected, comparator.Compatible(new GameVersionCriteria (gameVersion), gameMod));
        }

        [Test, TestCase(typeof(StrictGameComparator), false)]
        public void GenerallySafeStrict(Type type, bool expected)
        {
            var comparator = (IGameComparator) Activator.CreateInstance(type);

            // We're going to tweak compatibly to mark the mod as being for 1.0.3 ONLY
            gameMod.ksp_version = gameMod.ksp_version_min = gameMod.ksp_version_max
                = GameVersion.Parse("1.0.3");

            gameMod.ksp_version_strict = true;

            // Now test!
            Assert.AreEqual(expected, comparator.Compatible(new GameVersionCriteria (gameVersion), gameMod));
        }

        [Test, TestCase(typeof(StrictGameComparator), false)]
        public void Incompatible(Type type, bool expected)
        {
            var comparator = (IGameComparator) Activator.CreateInstance(type);

            // The mod already starts off being incompatible, so just do the test. :)
            Assert.AreEqual(expected, comparator.Compatible(new GameVersionCriteria (gameVersion), gameMod));
        }

        public static readonly object[] TestStrictGameComparatorCases =
        {
            //             Mod compat.      KSP             expected
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
        public void TestStrictGameComparator(string modVersion, string gameVersion, bool expectedResult)
        {
            var comparator = new StrictGameComparator();

            // We're going to tweak compatibly of the mod
            gameMod.ksp_version = GameVersion.Parse(modVersion);

            // Now test!
            Assert.AreEqual(expectedResult, comparator.Compatible(new GameVersionCriteria(GameVersion.Parse(gameVersion)), gameMod));
        }

        public static readonly object[] TestStrictGameComparatorMinMaxCases =
        {
            //             Min comapat      Max comapat     KSP           expected
            new object[] { "1.0.4",         null,           "1.0.3",      false },
            new object[] { "1.0.4",         null,           "1.0.4",      true },
            new object[] { "1.0.4",         null,           "1.0.5",      true },
            new object[] { "1.0.4",         null,           "1.1",        true },

            new object[] { "1.0",           null,           "0.9",        false },
            new object[] { "1.0",           null,           "1.0",        true },
            new object[] { "1.0",           null,           "1.0.4",      true },
            new object[] { "1.0",           null,           "1.1",        true },

            new object[] { "1.1",           null,           "1.0.4",      false },
            new object[] { "1.1",           null,           "1.1",        true },
            new object[] { "1.1",           null,           "1.1.1",      true },
            new object[] { "1.1",           null,           "1.2",        true },

            new object[] { null,            "1.0.4",        "1.0.5",      false },
            new object[] { null,            "1.0.4",        "1.0.4",      true },
            new object[] { null,            "1.0.4",        "1.0.3",      true },
            new object[] { null,            "1.0.4",        "1.0",        true },

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
        public void TestStrictGameComparatorMinMax(string modMinVersion, string modMaxVersion, string gameVersion, bool expectedResult)
        {
            var comparator = new StrictGameComparator();

            gameMod.ksp_version = null;
            gameMod.ksp_version_min = modMinVersion == null ? null : GameVersion.Parse(modMinVersion);
            gameMod.ksp_version_max = modMaxVersion == null ? null : GameVersion.Parse(modMaxVersion);

            // Now test!
            Assert.AreEqual(expectedResult, comparator.Compatible(new GameVersionCriteria(GameVersion.Parse(gameVersion)), gameMod));
        }
    }
}
