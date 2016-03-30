using System;
using CKAN.Versioning;
using NUnit.Framework;

#pragma warning disable 414

namespace Tests.Core.Versioning
{
    [TestFixture]
    public sealed class KspVersionBoundTests
    {
        private static readonly object[] EqualityCases =
        {
            new object[]
            {
                new KspVersionBound(),
                new KspVersionBound(),
                true
            },
            new object[]
            {
                new KspVersionBound(new KspVersion(), false),
                new KspVersionBound(new KspVersion(), true),
                false
            },
            new object[]
            {
                new KspVersionBound(new KspVersion(1, 2, 3, 4), false),
                new KspVersionBound(new KspVersion(1, 2, 3, 4), false),
                true
            },
            new object[]
            {
                new KspVersionBound(new KspVersion(1, 2, 3, 4), false),
                new KspVersionBound(new KspVersion(1, 3, 4, 4), true),
                false
            }
        };

        [Test]
        public void ParameterlessCtorWorksCorrectly()
        {
            // Act
            var result = new KspVersionBound();

            // Assert
            Assert.AreEqual(KspVersion.Any, result.Value);
            Assert.IsTrue(result.Inclusive);
        }

        [Test]
        public void ParameterfulCtorWorksCorrectly()
        {
            // Act
            var result = new KspVersionBound(new KspVersion(1, 2, 3, 4), true);

            // Assert
            Assert.AreEqual(new KspVersion(1, 2, 3, 4), result.Value);
            Assert.IsTrue(result.Inclusive);
        }

        [Test]
        public void ParameterfulCtorThrowsOnNullParameter()
        {
            // Act
            // ReSharper disable once ObjectCreationAsStatement
            TestDelegate act = () => new KspVersionBound(null, false);

            // Assert
            Assert.That(act, Throws.Exception.InstanceOf<ArgumentNullException>());
        }

        [Test]
        public void ParameterfulCtorThrowsOnPartiallyDefinedParameter()
        {
            // Act
            // ReSharper disable once ObjectCreationAsStatement
            TestDelegate act = () => new KspVersionBound(new KspVersion(1, 2, 3), false);

            // Assert
            Assert.That(act, Throws.Exception);
        }

        [TestCaseSource("EqualityCases")]
        public void EqualityWorksCorrectly(KspVersionBound vb1, KspVersionBound vb2, bool areEqual)
        {
            // Act
            var genericEquals = vb1.Equals(vb2);
            var nonGenericEquals = vb1.Equals((object)vb2);
            var equalsOperator = vb1 == vb2;
            var notEqualsOperator = vb1 != vb2;
            var reverseEqualsOperator = vb2 == vb1;
            var reverseNotEqualsOperator = vb2 != vb1;

            // Assert
            Assert.AreEqual(areEqual, genericEquals);
            Assert.AreEqual(areEqual, nonGenericEquals);
            Assert.AreEqual(areEqual, equalsOperator);
            Assert.AreNotEqual(areEqual, notEqualsOperator);
            Assert.AreEqual(areEqual, reverseEqualsOperator);
            Assert.AreNotEqual(areEqual, reverseNotEqualsOperator);
        }

        [Test]
        public void NullEqualityWorksCorrectly()
        {
            // Act
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            var genericEquals = new KspVersionBound().Equals(null);
            var nonGenericEquals = new KspVersionBound().Equals((object)null);
            var equalsOperatorNullLeft = null == new KspVersionBound();
            var equalsOperatorNullRight = new KspVersionBound() == null;
            var notEqualsOperatorNullLeft = null != new KspVersionBound();
            var notEqualsOperatorNullRight = new KspVersionBound() != null;
            // ReSharper restore ConditionIsAlwaysTrueOrFalse

            // ASsert
            Assert.IsFalse(genericEquals);
            Assert.IsFalse(nonGenericEquals);
            Assert.IsFalse(equalsOperatorNullLeft);
            Assert.IsFalse(equalsOperatorNullRight);
            Assert.IsTrue(notEqualsOperatorNullLeft);
            Assert.IsTrue(notEqualsOperatorNullRight);
        }

        [Test]
        public void ReferenceEqualityWorksCorrectly()
        {
            // Arrange
            var sut = new KspVersionBound();

            // Act
            var genericEquals = sut.Equals(sut);
            var nonGenericEquals = sut.Equals((object)sut);

            // Assert
            Assert.IsTrue(genericEquals);
            Assert.IsTrue(nonGenericEquals);
        }

        [Test]
        public void GetHashCodeDoesNotThrow(
            [Random(0, int.MaxValue, 1)]int major,
            [Random(0, int.MaxValue, 1)]int minor,
            [Random(0, int.MaxValue, 1)]int patch,
            [Random(0, int.MaxValue, 1)]int build,
            [Values(false, true)]bool inclusive
        )
        {
            // Act
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            TestDelegate act =
                () => new KspVersionBound(new KspVersion(major, minor, patch, build), inclusive).GetHashCode();

            // Assert
            Assert.That(act, Throws.Nothing);
        }
    }
}
