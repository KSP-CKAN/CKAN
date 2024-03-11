using NUnit.Framework;

using CKAN.Versioning;
using CKAN.Games;
using CKAN.Games.KerbalSpaceProgram;

#pragma warning disable 414

namespace Tests.Core.Versioning
{
    public sealed class GameVersionRangeTests
    {
        #pragma warning disable 0414, IDE0052
        private static readonly object[] EqualityCases =
        {
            new object[]
            {
                new GameVersionRange(new GameVersionBound(), new GameVersionBound()),
                new GameVersionRange(new GameVersionBound(), new GameVersionBound()),
                true
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 2, 3, 4), false),
                    new GameVersionBound(new GameVersion(1, 2, 3, 4), false)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 2, 3, 4), false),
                    new GameVersionBound(new GameVersion(1, 2, 3, 4), false)
                ),
                true
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 2, 3, 4), false),
                    new GameVersionBound(new GameVersion(5, 6, 7, 8), true)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 2, 3, 4), false),
                    new GameVersionBound(new GameVersion(5, 6, 7, 8), true)
                ),
                true
            }
        };

        private static readonly object[] ToStringCases =
        {
            new object[]
            {
                new GameVersionRange(new GameVersionBound(), new GameVersionBound()),
                "[,]"
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 2, 3, 4), false),
                    new GameVersionBound()),
                "(1.2.3.4,]"
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 2, 3, 4), false),
                    new GameVersionBound(new GameVersion(5, 6, 7, 8), false)),
                "(1.2.3.4,5.6.7.8)"
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 2, 3, 4), true),
                    new GameVersionBound(new GameVersion(5, 6, 7, 8), true)),
                "[1.2.3.4,5.6.7.8]"
            }
        };

        private static readonly object[] IntersectWithCases =
        {
            new object[]
            {
                new GameVersionRange(new GameVersionBound(), new GameVersionBound()),
                new GameVersionRange(new GameVersionBound(), new GameVersionBound()),
                new GameVersionRange(new GameVersionBound(), new GameVersionBound()),
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 2, 3, 4), true),
                    new GameVersionBound(new GameVersion(1, 2, 3, 4), true)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 2, 3, 4), true),
                    new GameVersionBound(new GameVersion(1, 2, 3, 4), true)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 2, 3, 4), true),
                    new GameVersionBound(new GameVersion(1, 2, 3, 4), true)
                ),
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 2, 3, 4), false),
                    new GameVersionBound(new GameVersion(1, 2, 3, 4), false)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 2, 3, 4), false),
                    new GameVersionBound(new GameVersion(1, 2, 3, 4), false)
                ),
                null
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 2, 3, 4), false),
                    new GameVersionBound(new GameVersion(1, 2, 3, 4), false)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 2, 3, 4), true),
                    new GameVersionBound(new GameVersion(1, 2, 3, 4), true)
                ),
                null
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 0, 0), true),
                    new GameVersionBound(new GameVersion(1, 1, 0, 0), false)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 4, 0), true),
                    new GameVersionBound(new GameVersion(1, 0, 5, 0), false)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 4, 0), true),
                    new GameVersionBound(new GameVersion(1, 0, 5, 0), false)
                ),
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 1, 0, 0), true),
                    new GameVersionBound(new GameVersion(1, 2, 0, 0), false)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 4, 0), true),
                    new GameVersionBound(new GameVersion(1, 0, 5, 0), false)
                ),
                null
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 4, 1234), true),
                    new GameVersionBound(new GameVersion(1, 0, 4, 1234), true)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 4, 1234), true),
                    new GameVersionBound(new GameVersion(1, 0, 4, 1234), true)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 4, 1234), true),
                    new GameVersionBound(new GameVersion(1, 0, 4, 1234), true)
                ),
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 4, 1235), true),
                    new GameVersionBound(new GameVersion(1, 0, 4, 1235), true)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 4, 1234), true),
                    new GameVersionBound(new GameVersion(1, 0, 4, 1234), true)
                ),
                null
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 4, 0), true),
                    new GameVersionBound(new GameVersion(1, 0, 5, 0), false)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 4, 1234), true),
                    new GameVersionBound(new GameVersion(1, 0, 4, 1234), true)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 4, 1234), true),
                    new GameVersionBound(new GameVersion(1, 0, 4, 1234), true)
                ),
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 4, 1234), true),
                    new GameVersionBound(new GameVersion(1, 0, 4, 1234), true)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 4, 0), true),
                    new GameVersionBound(new GameVersion(1, 0, 5, 0), false)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 4, 1234), true),
                    new GameVersionBound(new GameVersion(1, 0, 4, 1234), true)
                ),
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 4, 0), true),
                    new GameVersionBound(new GameVersion(1, 0, 4, 0), true)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 4, 0), true),
                    new GameVersionBound(new GameVersion(1, 0, 5, 0), false)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 4, 0), true),
                    new GameVersionBound(new GameVersion(1, 0, 4, 0), true)
                ),
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 0, 0), true),
                    new GameVersionBound(new GameVersion(1, 1, 0, 0), false)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 0, 0), true),
                    new GameVersionBound(new GameVersion(1, 1, 0, 0), false)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 0, 0), true),
                    new GameVersionBound(new GameVersion(1, 1, 0, 0), false)
                ),
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 1, 0, 0), true),
                    new GameVersionBound(new GameVersion(1, 2, 0, 0), false)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 1, 0, 0), true),
                    new GameVersionBound(new GameVersion(1, 2, 0, 0), false)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 1, 0, 0), true),
                    new GameVersionBound(new GameVersion(1, 2, 0, 0), false)
                ),
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 1, 0, 0), true),
                    new GameVersionBound(new GameVersion(1, 2, 0, 0), false)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 0, 0), true),
                    new GameVersionBound(new GameVersion(1, 1, 0, 0), false)
                ),
                null
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 0, 0), true),
                    new GameVersionBound(new GameVersion(1, 1, 0, 0), false)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 1, 0, 0), true),
                    new GameVersionBound(new GameVersion(1, 2, 0, 0), false)
                ),
                null
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 4, 0), true),
                    new GameVersionBound()
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 4, 0), true),
                    new GameVersionBound(new GameVersion(1, 0, 5, 0), false)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 4, 0), true),
                    new GameVersionBound(new GameVersion(1, 0, 5, 0), false)
                )
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(),
                    new GameVersionBound(new GameVersion(1, 0, 4, 0), true)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 4, 0), true),
                    new GameVersionBound(new GameVersion(1, 0, 5, 0), false)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 4, 0), true),
                    new GameVersionBound(new GameVersion(1, 0, 4, 0), true)
                )
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(),
                    new GameVersionBound(new GameVersion(1, 0, 4, 0), false)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 3, 0), true),
                    new GameVersionBound(new GameVersion(1, 0, 4, 0), false)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 3, 0), true),
                    new GameVersionBound(new GameVersion(1, 0, 4, 0), false)
                )
            }
        };

        private static readonly object[] IsSupersetOfCases =
        {
            new object[]
            {
                new GameVersionRange(new GameVersionBound(), new GameVersionBound()),
                new GameVersionRange(new GameVersionBound(), new GameVersionBound()),
                true
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 0, 0), true),
                    new GameVersionBound(new GameVersion(2, 0, 0, 0), true)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 0, 0), true),
                    new GameVersionBound(new GameVersion(2, 0, 0, 0), true)
                ),
                true
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 0, 0), true),
                    new GameVersionBound(new GameVersion(2, 0, 0, 0), true)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 0, 0), false),
                    new GameVersionBound(new GameVersion(2, 0, 0, 0), false)
                ),
                true
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 0, 0), false),
                    new GameVersionBound(new GameVersion(2, 0, 0, 0), false)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 0, 0), false),
                    new GameVersionBound(new GameVersion(2, 0, 0, 0), false)
                ),
                true
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 0, 0), false),
                    new GameVersionBound(new GameVersion(2, 0, 0, 0), false)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 0, 0), true),
                    new GameVersionBound(new GameVersion(2, 0, 0, 0), true)
                ),
                false
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 0, 0), true),
                    new GameVersionBound(new GameVersion(3, 0, 0, 0), true)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(2, 0, 0, 0), false),
                    new GameVersionBound(new GameVersion(4, 0, 0, 0), false)
                ),
                false
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(),
                    new GameVersionBound(new GameVersion(3, 0, 0, 0), true)
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 0, 0), true),
                    new GameVersionBound(new GameVersion(2, 0, 0, 0), true)
                ),
                true
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 0, 0), true),
                    new GameVersionBound()
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 0, 0), true),
                    new GameVersionBound(new GameVersion(2, 0, 0, 0), true)
                ),
                true
            },
            new object[]
            {
                new GameVersionRange(
                    new GameVersionBound(),
                    new GameVersionBound()
                ),
                new GameVersionRange(
                    new GameVersionBound(new GameVersion(1, 0, 0, 0), true),
                    new GameVersionBound(new GameVersion(2, 0, 0, 0), true)
                ),
                true
            },
        };
        #pragma warning restore 0414, IDE0052

        [Test]
        public void CtorWorksCorrectly()
        {
            // Arrange
            var lower = new GameVersionBound(new GameVersion(1, 2, 3, 4), false);
            var upper = new GameVersionBound(new GameVersion(5, 6, 7, 8), true);

            // Act
            var result = new GameVersionRange(lower, upper);

            // Assert
            Assert.That(result.Lower, Is.EqualTo(lower));
            Assert.That(result.Upper, Is.EqualTo(upper));
        }

        [Test]
        public void RangeFromVersionsEqualsRangeFromBounds()
        {
            // Arrange
            var lowerBound = new GameVersionBound(new GameVersion(1, 2, 0, 0), true);
            var upperBound = new GameVersionBound(new GameVersion(2, 4, 7, 0), false);
            var lowerVersion = new GameVersion(1, 2);
            var upperVersion = new GameVersion(2, 4, 6);

            // Act
            var resultFromBounds = new GameVersionRange(lowerBound, upperBound);
            var resultFromVersions = new GameVersionRange(lowerVersion, upperVersion);

            // Assert
            Assert.That(resultFromBounds.Lower, Is.EqualTo(resultFromVersions.Lower));
            Assert.That(resultFromBounds.Upper, Is.EqualTo(resultFromVersions.Upper));
        }

        [Test]
        public void Ctor_NullLowerParameter_Unbounded()
        {
            // Act
            // ReSharper disable once ObjectCreationAsStatement
            var range = new GameVersionRange(null, new GameVersionBound(new GameVersion(1, 2, 3, 4), false));

            // Assert
            Assert.AreEqual(GameVersionBound.Unbounded, range.Lower);
        }

        [Test]
        public void Ctor_NullUpperParameter_Unbounded()
        {
            // Act
            // ReSharper disable once ObjectCreationAsStatement
            var range = new GameVersionRange(new GameVersionBound(new GameVersion(1, 2, 3, 4), false), null);

            // Assert
            Assert.AreEqual(GameVersionBound.Unbounded, range.Upper);
        }

        [TestCaseSource("ToStringCases")]
        public void ToStringWorksCorrectly(GameVersionRange vr, string expected)
        {
            // Act
            var result = vr.ToString();

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCaseSource("IntersectWithCases")]
        public void IntersectWithWorksCorrectly(GameVersionRange left, GameVersionRange right, GameVersionRange expected)
        {
            // Act
            var result = left.IntersectWith(right);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCaseSource("IsSupersetOfCases")]
        public void IsSupersetOfWorksCorrectly(GameVersionRange left, GameVersionRange right, bool expected)
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
            var sut = new GameVersionRange(new GameVersionBound(), new GameVersionBound());

            // Act
            TestDelegate act = () => sut.IsSupersetOf(null);

            // Assert
            Assert.That(act, Throws.Exception);
        }

        [TestCaseSource("EqualityCases")]
        public void EqualityWorksCorrectly(GameVersionRange vr1, GameVersionRange vr2, bool areEqual)
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
            var sut = new GameVersionRange(new GameVersionBound(), new GameVersionBound());

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
            var sut = new GameVersionRange(new GameVersionBound(), new GameVersionBound());

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
            var lower = new GameVersionBound(
                new GameVersion(lowerMajor, lowerMinor, lowerPatch, lowerBuilder),
                lowerInclusive
            );

            var upper = new GameVersionBound(
                new GameVersion(upperMajor, upperMinor, upperPatch, upperBuilder),
                upperInclusive
            );

            // Act
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            TestDelegate act = () => new GameVersionRange(lower, upper).GetHashCode();

            // Assert
            Assert.That(act, Throws.Nothing);
        }

        [Test]
        public void VersionSpan_AllVersions_CorrectString()
        {
            // Arrange
            IGame game = new KerbalSpaceProgram();
            GameVersion min = GameVersion.Any;
            GameVersion max = GameVersion.Any;
            // Act
            string s = GameVersionRange.VersionSpan(game, min, max);
            // Assert
            Assert.AreEqual("KSP All versions", s);
        }

        [Test]
        public void VersionSpan_MinOnly_CorrectString()
        {
            // Arrange
            IGame game = new KerbalSpaceProgram();
            GameVersion min = new GameVersion(1, 0, 0);
            GameVersion max = GameVersion.Any;
            // Act
            string s = GameVersionRange.VersionSpan(game, min, max);
            // Assert
            Assert.AreEqual("KSP 1.0.0 and later", s);
        }

        [Test]
        public void VersionSpan_MaxOnly_CorrectString()
        {
            // Arrange
            IGame game = new KerbalSpaceProgram();
            GameVersion min = GameVersion.Any;
            GameVersion max = new GameVersion(1, 0, 0);
            // Act
            string s = GameVersionRange.VersionSpan(game, min, max);
            // Assert
            Assert.AreEqual("KSP 1.0.0 and earlier", s);
        }

        [Test]
        public void VersionSpan_OneOnly_CorrectString()
        {
            // Arrange
            IGame game = new KerbalSpaceProgram();
            GameVersion min = new GameVersion(1, 0, 0);
            GameVersion max = new GameVersion(1, 0, 0);
            // Act
            string s = GameVersionRange.VersionSpan(game, min, max);
            // Assert
            Assert.AreEqual("KSP 1.0.0", s);
        }

        [Test]
        public void VersionSpan_FiniteRange_CorrectString()
        {
            // Arrange
            IGame game = new KerbalSpaceProgram();
            GameVersion min = new GameVersion(1, 0, 0);
            GameVersion max = new GameVersion(1, 1, 1);
            // Act
            string s = GameVersionRange.VersionSpan(game, min, max);
            // Assert
            Assert.AreEqual("KSP 1.0.0–1.1.1", s);
        }

        [Test,
            // Less than min
            TestCase("1.0", "2.0", "0.5", false),
            // Equal to min
            TestCase("1.0", "2.0", "1.0", true),
            // Equal to max
            TestCase("1.0", "2.0", "2.0", true),
            // In between
            TestCase("1.0", "2.0", "1.5", true),
            // Greater than max
            TestCase("1.0", "2.0", "3.0", false),
            // Single version range, equal
            TestCase("1.0", "1.0", "1.0", true),
            // Single version range, not equal
            TestCase("1.0", "1.0", "2.0", false),
            // Single version range, Any version
            TestCase("1.0", "1.0", "any", true),
            // Less or equal range, in range
            TestCase("any", "1.0", "0.5", true),
            // Less or equal range, out of range
            TestCase("any", "1.0", "2.0", false),
            // Greater or equal range, in range
            TestCase("1.0", "any", "2.0", true),
            // Greater or equal range, out of range
            TestCase("1.0", "any", "0.5", false),
            // Unbounded range, a version
            TestCase("any", "any", "1.0", true),
            // Unbounded range, Any version
            TestCase("any", "any", "any", true),
        ]
        public void Contains_GameVersion_Works(string min, string max, string ver, bool contained)
        {
            // Arrange
            var range   = new GameVersionRange(GameVersion.Parse(min),
                                               GameVersion.Parse(max));
            var gameVer = GameVersion.Parse(ver);

            // Act / Assert
            Assert.AreEqual(contained, range.Contains(gameVer));
        }

    }
}
