using CKAN.Versioning;
using NUnit.Framework;

#pragma warning disable 414

namespace Tests.Core.Versioning
{
    public sealed class KspVersionRangeTests
    {
        private static readonly object[] EqualityCases =
        {
            new object[]
            {
                new KspVersionRange(new KspVersionBound(), new KspVersionBound()),
                new KspVersionRange(new KspVersionBound(), new KspVersionBound()),
                true
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 2, 3, 4), false),
                    new KspVersionBound(new KspVersion(1, 2, 3, 4), false)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 2, 3, 4), false),
                    new KspVersionBound(new KspVersion(1, 2, 3, 4), false)
                ),
                true
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 2, 3, 4), false),
                    new KspVersionBound(new KspVersion(5, 6, 7, 8), true)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 2, 3, 4), false),
                    new KspVersionBound(new KspVersion(5, 6, 7, 8), true)
                ),
                true
            }
        };

        private static readonly object[] ToStringCases =
        {
            new object[]
            {
                new KspVersionRange(new KspVersionBound(), new KspVersionBound()),
                "[,]"
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 2, 3, 4), false),
                    new KspVersionBound()),
                "(1.2.3.4,]"
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 2, 3, 4), false),
                    new KspVersionBound(new KspVersion(5, 6, 7, 8), false)),
                "(1.2.3.4,5.6.7.8)"
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 2, 3, 4), true),
                    new KspVersionBound(new KspVersion(5, 6, 7, 8), true)),
                "[1.2.3.4,5.6.7.8]"
            }
        };

        private static readonly object[] IntersectWithCases =
        {
            new object[]
            {
                new KspVersionRange(new KspVersionBound(), new KspVersionBound()),
                new KspVersionRange(new KspVersionBound(), new KspVersionBound()),
                new KspVersionRange(new KspVersionBound(), new KspVersionBound()),
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 2, 3, 4), true),
                    new KspVersionBound(new KspVersion(1, 2, 3, 4), true)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 2, 3, 4), true),
                    new KspVersionBound(new KspVersion(1, 2, 3, 4), true)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 2, 3, 4), true),
                    new KspVersionBound(new KspVersion(1, 2, 3, 4), true)
                ),
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 2, 3, 4), false),
                    new KspVersionBound(new KspVersion(1, 2, 3, 4), false)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 2, 3, 4), false),
                    new KspVersionBound(new KspVersion(1, 2, 3, 4), false)
                ),
                null
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 2, 3, 4), false),
                    new KspVersionBound(new KspVersion(1, 2, 3, 4), false)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 2, 3, 4), true),
                    new KspVersionBound(new KspVersion(1, 2, 3, 4), true)
                ),
                null
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 0, 0), true),
                    new KspVersionBound(new KspVersion(1, 1, 0, 0), false)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 4, 0), true),
                    new KspVersionBound(new KspVersion(1, 0, 5, 0), false)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 4, 0), true),
                    new KspVersionBound(new KspVersion(1, 0, 5, 0), false)
                ),
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 1, 0, 0), true),
                    new KspVersionBound(new KspVersion(1, 2, 0, 0), false)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 4, 0), true),
                    new KspVersionBound(new KspVersion(1, 0, 5, 0), false)
                ),
                null
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 4, 1234), true),
                    new KspVersionBound(new KspVersion(1, 0, 4, 1234), true)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 4, 1234), true),
                    new KspVersionBound(new KspVersion(1, 0, 4, 1234), true)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 4, 1234), true),
                    new KspVersionBound(new KspVersion(1, 0, 4, 1234), true)
                ),
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 4, 1235), true),
                    new KspVersionBound(new KspVersion(1, 0, 4, 1235), true)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 4, 1234), true),
                    new KspVersionBound(new KspVersion(1, 0, 4, 1234), true)
                ),
                null
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 4, 0), true),
                    new KspVersionBound(new KspVersion(1, 0, 5, 0), false)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 4, 1234), true),
                    new KspVersionBound(new KspVersion(1, 0, 4, 1234), true)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 4, 1234), true),
                    new KspVersionBound(new KspVersion(1, 0, 4, 1234), true)
                ),
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 4, 1234), true),
                    new KspVersionBound(new KspVersion(1, 0, 4, 1234), true)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 4, 0), true),
                    new KspVersionBound(new KspVersion(1, 0, 5, 0), false)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 4, 1234), true),
                    new KspVersionBound(new KspVersion(1, 0, 4, 1234), true)
                ),
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 4, 0), true),
                    new KspVersionBound(new KspVersion(1, 0, 4, 0), true)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 4, 0), true),
                    new KspVersionBound(new KspVersion(1, 0, 5, 0), false)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 4, 0), true),
                    new KspVersionBound(new KspVersion(1, 0, 4, 0), true)
                ),
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 0, 0), true),
                    new KspVersionBound(new KspVersion(1, 1, 0, 0), false)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 0, 0), true),
                    new KspVersionBound(new KspVersion(1, 1, 0, 0), false)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 0, 0), true),
                    new KspVersionBound(new KspVersion(1, 1, 0, 0), false)
                ),
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 1, 0, 0), true),
                    new KspVersionBound(new KspVersion(1, 2, 0, 0), false)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 1, 0, 0), true),
                    new KspVersionBound(new KspVersion(1, 2, 0, 0), false)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 1, 0, 0), true),
                    new KspVersionBound(new KspVersion(1, 2, 0, 0), false)
                ),
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 1, 0, 0), true),
                    new KspVersionBound(new KspVersion(1, 2, 0, 0), false)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 0, 0), true),
                    new KspVersionBound(new KspVersion(1, 1, 0, 0), false)
                ),
                null
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 0, 0), true),
                    new KspVersionBound(new KspVersion(1, 1, 0, 0), false)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 1, 0, 0), true),
                    new KspVersionBound(new KspVersion(1, 2, 0, 0), false)
                ),
                null
            },
        };

        private static readonly object[] IsSupersetOfCases =
        {
            new object[]
            {
                new KspVersionRange(new KspVersionBound(), new KspVersionBound()),
                new KspVersionRange(new KspVersionBound(), new KspVersionBound()),
                true
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 0, 0), true),
                    new KspVersionBound(new KspVersion(2, 0, 0, 0), true)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 0, 0), true),
                    new KspVersionBound(new KspVersion(2, 0, 0, 0), true)
                ),
                true
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 0, 0), true),
                    new KspVersionBound(new KspVersion(2, 0, 0, 0), true)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 0, 0), false),
                    new KspVersionBound(new KspVersion(2, 0, 0, 0), false)
                ),
                true
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 0, 0), false),
                    new KspVersionBound(new KspVersion(2, 0, 0, 0), false)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 0, 0), false),
                    new KspVersionBound(new KspVersion(2, 0, 0, 0), false)
                ),
                true
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 0, 0), false),
                    new KspVersionBound(new KspVersion(2, 0, 0, 0), false)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 0, 0), true),
                    new KspVersionBound(new KspVersion(2, 0, 0, 0), true)
                ),
                false
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 0, 0), true),
                    new KspVersionBound(new KspVersion(3, 0, 0, 0), true)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(2, 0, 0, 0), false),
                    new KspVersionBound(new KspVersion(4, 0, 0, 0), false)
                ),
                false
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(),
                    new KspVersionBound(new KspVersion(3, 0, 0, 0), true)
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 0, 0), true),
                    new KspVersionBound(new KspVersion(2, 0, 0, 0), true)
                ),
                true
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 0, 0), true),
                    new KspVersionBound()
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 0, 0), true),
                    new KspVersionBound(new KspVersion(2, 0, 0, 0), true)
                ),
                true
            },
            new object[]
            {
                new KspVersionRange(
                    new KspVersionBound(),
                    new KspVersionBound()
                ),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 0, 0), true),
                    new KspVersionBound(new KspVersion(2, 0, 0, 0), true)
                ),
                true
            }
        };

        [Test]
        public void CtorWorksCorrectly()
        {
            // Arrange
            var lower = new KspVersionBound(new KspVersion(1, 2, 3, 4), false);
            var upper = new KspVersionBound(new KspVersion(5, 6, 7, 8), true);

            // Act
            var result = new KspVersionRange(lower, upper);

            // Assert
            Assert.That(result.Lower, Is.EqualTo(lower));
            Assert.That(result.Upper, Is.EqualTo(upper));
        }

        [Test]
        public void CtorThrowsOnNullLowerParameter()
        {
            // Act
            // ReSharper disable once ObjectCreationAsStatement
            TestDelegate act =
                () => new KspVersionRange(null, new KspVersionBound(new KspVersion(1, 2, 3, 4), false));

            // Assert
            Assert.That(act, Throws.Exception);
        }

        [Test]
        public void CtorThrowsOnNullUpperParameter()
        {
            // Act
            // ReSharper disable once ObjectCreationAsStatement
            TestDelegate act =
                () => new KspVersionRange(new KspVersionBound(new KspVersion(1, 2, 3, 4), false), null);

            // Assert
            Assert.That(act, Throws.Exception);
        }

        [TestCaseSource("ToStringCases")]
        public void ToStringWorksCorrectly(KspVersionRange vr, string expected)
        {
            // Act
            var result = vr.ToString();

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCaseSource("IntersectWithCases")]
        public void IntersectWithWorksCorrectly(KspVersionRange left, KspVersionRange right, KspVersionRange expected)
        {
            // Act
            var result = left.IntersectWith(right);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCaseSource("IsSupersetOfCases")]
        public void IsSupersetOfWorksCorrectly(KspVersionRange left, KspVersionRange right, bool expected)
        {
            // Act
            var result = left.IsSupersetOf(right);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void IsSupersetOfThrowsOnNullParameter()
        {
            // Arrange
            var sut = new KspVersionRange(new KspVersionBound(), new KspVersionBound());

            // Act
            TestDelegate act = () => sut.IsSupersetOf(null);

            // Assert
            Assert.That(act, Throws.Exception);
        }

        [TestCaseSource("EqualityCases")]
        public void EqualityWorksCorrectly(KspVersionRange vr1, KspVersionRange vr2, bool areEqual)
        {
            // Act
            var genericEquals = vr1.Equals(vr2);
            var nonGenericEquals = vr1.Equals((object)vr2);
            var equalsOperator = vr1 == vr2;
            var notEqualsOperator = vr1 != vr2;
            var reverseEqualsOperator = vr2 == vr1;
            var reverseNotEqualsOperator = vr2 != vr1;

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
            // Arrange
            var sut = new KspVersionRange(new KspVersionBound(), new KspVersionBound());

            // Act
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            var genericEquals = sut.Equals(null);
            var nonGenericEquals = sut.Equals((object)null);
            var equalsOperatorNullLeft = null == sut;
            var equalsOperatorNullRight = sut == null;
            var notEqualsOperatorNullLeft = null != sut;
            var notEqualsOperatorNullRight = sut != null;
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
            var sut = new KspVersionRange(new KspVersionBound(), new KspVersionBound());

            // Act
            var genericEquals = sut.Equals(sut);
            var nonGenericEquals = sut.Equals((object)sut);

            // Assert
            Assert.IsTrue(genericEquals);
            Assert.IsTrue(nonGenericEquals);
        }

        [Test]
        public void GetHashCodeDoesNotThrow(
            [Random(0, int.MaxValue, 1)]int lowerMajor,
            [Random(0, int.MaxValue, 1)]int lowerMinor,
            [Random(0, int.MaxValue, 1)]int lowerPatch,
            [Random(0, int.MaxValue, 1)]int lowerBuilder,
            [Values(false, true)]bool lowerInclusive,
            [Random(0, int.MaxValue, 1)]int upperMajor,
            [Random(0, int.MaxValue, 1)]int upperMinor,
            [Random(0, int.MaxValue, 1)]int upperPatch,
            [Random(0, int.MaxValue, 1)]int upperBuilder,
            [Values(false, true)]bool upperInclusive
        )
        {
            // Arrange
            var lower = new KspVersionBound(
                new KspVersion(lowerMajor, lowerMinor, lowerPatch, lowerBuilder),
                lowerInclusive
            );

            var upper = new KspVersionBound(
                new KspVersion(upperMajor, upperMinor, upperPatch, upperBuilder),
                upperInclusive
            );

            // Act
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            TestDelegate act = () => new KspVersionRange(lower, upper).GetHashCode();

            // Assert
            Assert.That(act, Throws.Nothing);
        }
    }
}
