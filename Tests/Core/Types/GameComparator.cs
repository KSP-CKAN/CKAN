using System;
using CKAN.Versioning;
using NUnit.Framework;
using Tests.Data;
using log4net;

namespace Tests.Core.Types
{
    [TestFixture]
    public class GameComparator
    {
        static readonly KspVersion gameVersion = KspVersion.Parse("1.0.4");
        CKAN.CkanModule gameMod;

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
                = KspVersion.Parse("1.0.4");

            // Now test!
            Assert.AreEqual(expected, comparator.Compatible(new KspVersionCriteria (gameVersion), gameMod));
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
                = KspVersion.Parse("1.0.3");

            // Now test!
            Assert.AreEqual(expected, comparator.Compatible(new KspVersionCriteria (gameVersion), gameMod));
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
                = KspVersion.Parse("1.0.3");

            gameMod.ksp_version_strict = true;

            // Now test!
            Assert.AreEqual(expected, comparator.Compatible(new KspVersionCriteria (gameVersion), gameMod));
        }

        [Test]
        [TestCase(typeof(CKAN.StrictGameComparator), true)]
        [TestCase(typeof(CKAN.GrasGameComparator), true)]
        [TestCase(typeof(CKAN.YoyoGameComparator), true)]
        public void CompatibleWithManyKspVersions(Type type, bool expected)
        {
            var comparator = (CKAN.IGameComparator)Activator.CreateInstance(type);

            // We're going to tweak compatibly to mark the mod as being for 1.0.*
            gameMod.ksp_version = KspVersion.Parse("1.0");         

            // Now test!
            Assert.AreEqual(expected, comparator.Compatible(new KspVersionCriteria(gameVersion), gameMod));
        }

        [Test]
        [TestCase(typeof(CKAN.StrictGameComparator), false)]
        [TestCase(typeof(CKAN.GrasGameComparator), false)]
        [TestCase(typeof(CKAN.YoyoGameComparator), true)]
        public void IncompatibleWhenMarkedForManyKspVersions(Type type, bool expected)
        {
            var comparator = (CKAN.IGameComparator)Activator.CreateInstance(type);

            // We're going to tweak compatibly to mark the mod as being for 1.1.*
            gameMod.ksp_version = KspVersion.Parse("1.1");

            // Now test!
            Assert.AreEqual(expected, comparator.Compatible(new KspVersionCriteria(gameVersion), gameMod));
        }

        [Test]
        [TestCase(typeof(CKAN.StrictGameComparator), true)]
        [TestCase(typeof(CKAN.GrasGameComparator), true)]
        [TestCase(typeof(CKAN.YoyoGameComparator), true)]
        public void CompatibleBothBuildVersionsSpecified(Type type, bool expected)
        {
            var comparator = (CKAN.IGameComparator)Activator.CreateInstance(type);
            
            gameMod.ksp_version = KspVersion.Parse("1.0.4.1234");

            // Now test!
            Assert.AreEqual(expected, comparator.Compatible(new KspVersionCriteria(KspVersion.Parse("1.0.4.1234")), gameMod));
        }

        [Test]
        [TestCase(typeof(CKAN.StrictGameComparator), false)]
        [TestCase(typeof(CKAN.GrasGameComparator), false)]
        [TestCase(typeof(CKAN.YoyoGameComparator), true)]
        public void IncompatibleBothBuildVersionsSpecified(Type type, bool expected)
        {
            var comparator = (CKAN.IGameComparator)Activator.CreateInstance(type);
            
            gameMod.ksp_version = KspVersion.Parse("1.0.4.1235");

            // Now test!
            Assert.AreEqual(expected, comparator.Compatible(new KspVersionCriteria(KspVersion.Parse("1.0.4.1234")), gameMod));
        }

        [Test]
        [TestCase(typeof(CKAN.StrictGameComparator), true)]
        [TestCase(typeof(CKAN.GrasGameComparator), true)]
        [TestCase(typeof(CKAN.YoyoGameComparator), true)]
        public void CompatibleGameBuildVersionSpecified(Type type, bool expected)
        {
            var comparator = (CKAN.IGameComparator)Activator.CreateInstance(type);

            gameMod.ksp_version = KspVersion.Parse("1.0.4");

            // Now test!
            Assert.AreEqual(expected, comparator.Compatible(new KspVersionCriteria(KspVersion.Parse("1.0.4.1234")), gameMod));
        }       

        [Test]
        [TestCase(typeof(CKAN.StrictGameComparator), true)]
        [TestCase(typeof(CKAN.GrasGameComparator), true)]
        [TestCase(typeof(CKAN.YoyoGameComparator), true)]
        public void IncompatibleModBuildVersionSpecifiedButGameIsNot(Type type, bool expected)
        {
            var comparator = (CKAN.IGameComparator)Activator.CreateInstance(type);

            gameMod.ksp_version = KspVersion.Parse("1.0.4.1234");

            // Now test!
            Assert.AreEqual(expected, comparator.Compatible(new KspVersionCriteria(KspVersion.Parse("1.0.4")), gameMod));
        }

        [Test]
        [TestCase(typeof(CKAN.StrictGameComparator), true)]
        [TestCase(typeof(CKAN.GrasGameComparator), true)]
        [TestCase(typeof(CKAN.YoyoGameComparator), true)]
        public void IncompatibleModBuildVersionSpecifiedButGameIsNot2(Type type, bool expected)
        {
            var comparator = (CKAN.IGameComparator)Activator.CreateInstance(type);

            gameMod.ksp_version = KspVersion.Parse("1.0.4.0000");

            // Now test!
            Assert.AreEqual(expected, comparator.Compatible(new KspVersionCriteria(KspVersion.Parse("1.0.4")), gameMod));
        }

        [Test]
        [TestCase(typeof(CKAN.StrictGameComparator), true)]
        [TestCase(typeof(CKAN.GrasGameComparator), true)]
        [TestCase(typeof(CKAN.YoyoGameComparator), true)]
        public void CompatibleBecauseGameVersionIsRange(Type type, bool expected)
        {
            var comparator = (CKAN.IGameComparator)Activator.CreateInstance(type);

            gameMod.ksp_version = KspVersion.Parse("1.0.4");

            // Now test!
            Assert.AreEqual(expected, comparator.Compatible(new KspVersionCriteria(KspVersion.Parse("1.0")), gameMod));
        }

        [Test]
        [TestCase(typeof(CKAN.StrictGameComparator), false)]
        [TestCase(typeof(CKAN.GrasGameComparator), false)]
        [TestCase(typeof(CKAN.YoyoGameComparator), true)]
        public void IncompatibleWhenGameVersionIsRange(Type type, bool expected)
        {
            var comparator = (CKAN.IGameComparator)Activator.CreateInstance(type);

            gameMod.ksp_version = KspVersion.Parse("1.1");

            // Now test!
            Assert.AreEqual(expected, comparator.Compatible(new KspVersionCriteria(KspVersion.Parse("1.0")), gameMod));
        }

        [Test]
        [TestCase(typeof(CKAN.StrictGameComparator), true)]
        [TestCase(typeof(CKAN.GrasGameComparator), true)]
        [TestCase(typeof(CKAN.YoyoGameComparator), true)]
        public void CompatibleWhenApperIsUnbounded(Type type, bool expected)
        {
            var comparator = (CKAN.IGameComparator)Activator.CreateInstance(type);

            gameMod.ksp_version = null;
            gameMod.ksp_version_min = KspVersion.Parse("1.0");
            gameMod.ksp_version_max = null;

            // Now test!
            Assert.AreEqual(expected, comparator.Compatible(new KspVersionCriteria(gameVersion), gameMod));
        }

        [Test]
        [TestCase(typeof(CKAN.StrictGameComparator), false)]
        [TestCase(typeof(CKAN.GrasGameComparator), false)]
        [TestCase(typeof(CKAN.YoyoGameComparator), true)]
        public void IncompatibleWhenApperIsUnbounded(Type type, bool expected)
        {
            var comparator = (CKAN.IGameComparator)Activator.CreateInstance(type);

            gameMod.ksp_version = null;
            gameMod.ksp_version_min = KspVersion.Parse("1.1");
            gameMod.ksp_version_max = null;

            // Now test!
            Assert.AreEqual(expected, comparator.Compatible(new KspVersionCriteria(gameVersion), gameMod));
        }

        [Test]
        [TestCase(typeof(CKAN.StrictGameComparator), true)]
        [TestCase(typeof(CKAN.GrasGameComparator), true)]
        [TestCase(typeof(CKAN.YoyoGameComparator), true)]
        public void CompatibleWhenLowerIsUnbounded(Type type, bool expected)
        {
            var comparator = (CKAN.IGameComparator)Activator.CreateInstance(type);

            gameMod.ksp_version = null;
            gameMod.ksp_version_min = null;
            gameMod.ksp_version_max = KspVersion.Parse("1.1");

            // Now test!
            Assert.AreEqual(expected, comparator.Compatible(new KspVersionCriteria(gameVersion), gameMod));
        }

        [Test]
        [TestCase(typeof(CKAN.StrictGameComparator), false)]
        [TestCase(typeof(CKAN.GrasGameComparator), false)]
        [TestCase(typeof(CKAN.YoyoGameComparator), true)]
        public void IncompatibleWhenLowerIsUnbounded(Type type, bool expected)
        {
            var comparator = (CKAN.IGameComparator)Activator.CreateInstance(type);

            gameMod.ksp_version = null;
            gameMod.ksp_version_min = null; 
            gameMod.ksp_version_max = KspVersion.Parse("1.0");

            // Now test!
            Assert.AreEqual(expected, comparator.Compatible(new KspVersionCriteria(gameVersion), gameMod));
        }

        [Test]
        [TestCase(typeof(CKAN.StrictGameComparator), false)]
        [TestCase(typeof(CKAN.GrasGameComparator), false)]
        [TestCase(typeof(CKAN.YoyoGameComparator), true)]
        public void Incompatible(Type type, bool expected)
        {
            var comparator = (CKAN.IGameComparator) Activator.CreateInstance(type);

            // The mod already starts off being incompatible, so just do the test. :)
            Assert.AreEqual(expected, comparator.Compatible(new KspVersionCriteria (gameVersion), gameMod));
        }
    }
}

