using System;
using CKAN.Versioning;
using NUnit.Framework;

#pragma warning disable 219, 414

namespace Tests.Core.Versioning
{
    [TestFixture]
    public class KspVersionTests
    {
        private static readonly object[] ParseCases =
        {
            new object[] { "1", new KspVersion(1) },
            new object[] { "1.2", new KspVersion(1, 2) },
            new object[] { "1.2.3", new KspVersion(1, 2, 3) },
            new object[] { "1.2.3.4", new KspVersion(1, 2, 3, 4) }
        };

        private static readonly object[] ParseFailureCases =
        {
            new object[] { null },
            new object[] { "" },
            new object[] { "1.2.3.4.5" },
            new object[] { "1.2.3.A" },
            new object[] { "1.2.3.-1" },
            new object[] { "9876543210.1.2.3" },
            new object[] { "1.9876543210.2.3" },
            new object[] { "1.2.9876543210.3" },
            new object[] { "1.2.3.9876543210" }
        };

        private static readonly object[] EqualityCases =
        {
            new object[] { new KspVersion(), null, false },
            new object[] { new KspVersion(), new KspVersion(), true },
            new object[] { new KspVersion(1), new KspVersion(1), true },
            new object[] { new KspVersion(1, 2), new KspVersion(1, 2), true},
            new object[] { new KspVersion(1, 2, 3), new KspVersion(1, 2, 3), true},
            new object[] { new KspVersion(1, 2, 3, 4), new KspVersion(1, 2, 3, 4), true},
            new object[] { new KspVersion(), new KspVersion(1), false },
            new object[] { new KspVersion(1), new KspVersion(1, 2), false },
            new object[] { new KspVersion(1, 2), new KspVersion(1, 2, 3), false},
            new object[] { new KspVersion(1, 2, 3), new KspVersion(1, 2, 3, 4), false},
            new object[] { new KspVersion(1, 2, 3, 4), new KspVersion(1, 2, 3, 5), false}
        };

        private static readonly object[] CompareToCases =
        {
            new object[] { new KspVersion(), new KspVersion(), 0 },
            new object[] { new KspVersion(1), new KspVersion(1), 0 },
            new object[] { new KspVersion(1, 2), new KspVersion(1, 2), 0 },
            new object[] { new KspVersion(1, 2, 3), new KspVersion(1, 2, 3), 0 },
            new object[] { new KspVersion(1, 2, 3, 4), new KspVersion(1, 2, 3, 4), 0 },
            new object[] { new KspVersion(), new KspVersion(1), -1 },
            new object[] { new KspVersion(1), new KspVersion(1, 2), -1 },
            new object[] { new KspVersion(1, 2), new KspVersion(1, 2, 3), -1 },
            new object[] { new KspVersion(1, 2, 3), new KspVersion(1, 2, 3, 4), -1 },
            new object[] { new KspVersion(1, 2, 3, 4), new KspVersion(1, 2, 3, 5), -1 },
            new object[] { new KspVersion(1), new KspVersion(), 1 },
            new object[] { new KspVersion(1, 2), new KspVersion(1), 1 },
            new object[] { new KspVersion(1, 2, 3), new KspVersion(1, 2), 1 },
            new object[] { new KspVersion(1, 2, 3, 5), new KspVersion(1, 2, 3, 4), 1}
        };

        private static readonly object[] ToVersionRangeWorksCorrectlyCases =
        {
            new object[] { new KspVersion(), KspVersionRange.Any },
            new object[]
            {
                new KspVersion(1),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 0, 0, 0), inclusive: true),
                    new KspVersionBound(new KspVersion(2, 0, 0, 0), inclusive: false)
                )
            },
            new object[]
            {
                new KspVersion(1, 2),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 2, 0, 0), inclusive: true),
                    new KspVersionBound(new KspVersion(1, 3, 0, 0), inclusive: false)
                )
            },
            new object[]
            {
                new KspVersion(1, 2, 3),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 2, 3, 0), inclusive: true),
                    new KspVersionBound(new KspVersion(1, 2, 4, 0), inclusive: false)
                )
            },
            new object[]
            {
                new KspVersion(1, 2, 3, 4),
                new KspVersionRange(
                    new KspVersionBound(new KspVersion(1, 2, 3, 4), inclusive: true),
                    new KspVersionBound(new KspVersion(1, 2, 3, 4), inclusive: true)
                )
            }
        };

        [Test]
        public void ParameterlessCtorWorksCorrectly()
        {
            // Act
            var result = new KspVersion();

            // Assert
            Assert.AreEqual(-1, result.Major);
            Assert.AreEqual(-1, result.Minor);
            Assert.AreEqual(-1, result.Patch);
            Assert.AreEqual(-1, result.Build);

            Assert.IsFalse(result.IsMajorDefined);
            Assert.IsFalse(result.IsMinorDefined);
            Assert.IsFalse(result.IsPatchDefined);
            Assert.IsFalse(result.IsBuildDefined);

            Assert.IsFalse(result.IsFullyDefined);
            Assert.IsTrue(result.IsAny);

            Assert.AreEqual(null, result.ToString());
        }

        [Test]
        public void SingleParameterCtorWorksCorrectly()
        {
            // Act
            var result = new KspVersion(1);

            // Assert
            Assert.AreEqual(1, result.Major);
            Assert.AreEqual(-1, result.Minor);
            Assert.AreEqual(-1, result.Patch);
            Assert.AreEqual(-1, result.Build);

            Assert.IsTrue(result.IsMajorDefined);
            Assert.IsFalse(result.IsMinorDefined);
            Assert.IsFalse(result.IsPatchDefined);
            Assert.IsFalse(result.IsBuildDefined);

            Assert.IsFalse(result.IsFullyDefined);
            Assert.IsFalse(result.IsAny);

            Assert.AreEqual("1", result.ToString());
        }

        [TestCase(-1)]
        public void SingleParameterCtorThrowsOnInvalidParameters(int major)
        {
            // Act
            // ReSharper disable once ObjectCreationAsStatement
            TestDelegate act = () => new KspVersion(major);

            // Assert
            Assert.That(act, Throws.Exception.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void DoubleParameterCtorWorksCorrectly()
        {
            // Act
            var result = new KspVersion(1, 2);

            // Assert
            Assert.AreEqual(1, result.Major);
            Assert.AreEqual(2, result.Minor);
            Assert.AreEqual(-1, result.Patch);
            Assert.AreEqual(-1, result.Build);

            Assert.IsTrue(result.IsMajorDefined);
            Assert.IsTrue(result.IsMinorDefined);
            Assert.IsFalse(result.IsPatchDefined);
            Assert.IsFalse(result.IsBuildDefined);

            Assert.IsFalse(result.IsFullyDefined);
            Assert.IsFalse(result.IsAny);

            Assert.AreEqual("1.2", result.ToString());
        }

        [TestCase(-1, 0)]
        [TestCase(0, -1)]
        public void DoubleParameterCtorThrowsOnInvalidParameters(int major, int minor)
        {
            // Act
            // ReSharper disable once ObjectCreationAsStatement
            TestDelegate act = () => new KspVersion(major, minor);

            // Assert
            Assert.That(act, Throws.Exception.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void TripleParameterCtorWorksCorrectly()
        {
            // Act
            var result = new KspVersion(1, 2, 3);

            // Assert
            Assert.AreEqual(1, result.Major);
            Assert.AreEqual(2, result.Minor);
            Assert.AreEqual(3, result.Patch);
            Assert.AreEqual(-1, result.Build);

            Assert.IsTrue(result.IsMajorDefined);
            Assert.IsTrue(result.IsMinorDefined);
            Assert.IsTrue(result.IsPatchDefined);
            Assert.IsFalse(result.IsBuildDefined);

            Assert.IsFalse(result.IsFullyDefined);
            Assert.IsFalse(result.IsAny);

            Assert.AreEqual("1.2.3", result.ToString());
        }

        [TestCase(-1, 0, 0)]
        [TestCase(0, -1, 0)]
        [TestCase(0, 0, -1)]
        public void TripleParameterCtorThrowsOnInvalidParameters(int major, int minor, int patch)
        {
            // Act
            // ReSharper disable once ObjectCreationAsStatement
            TestDelegate act = () => new KspVersion(major, minor, patch);

            // Assert
            Assert.That(act, Throws.Exception.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void QuadrupleParameterCtorWorksCorrectly()
        {
            // Act
            var result = new KspVersion(1, 2, 3, 4);

            // Assert
            Assert.AreEqual(1, result.Major);
            Assert.AreEqual(2, result.Minor);
            Assert.AreEqual(3, result.Patch);
            Assert.AreEqual(4, result.Build);

            Assert.IsTrue(result.IsMajorDefined);
            Assert.IsTrue(result.IsMinorDefined);
            Assert.IsTrue(result.IsPatchDefined);
            Assert.IsTrue(result.IsBuildDefined);

            Assert.IsTrue(result.IsFullyDefined);
            Assert.IsFalse(result.IsAny);

            Assert.AreEqual("1.2.3.4", result.ToString());
        }

        [TestCase(-1, 0, 0, 0)]
        [TestCase(0, -1, 0, 0)]
        [TestCase(0, 0, -1, 0)]
        [TestCase(0, 0, 0, -1)]
        public void QuadrupleParameterCtorThrowsOnInvalidParameters(int major, int minor, int patch, int build)
        {
            // Act
            // ReSharper disable once ObjectCreationAsStatement
            TestDelegate act = () => new KspVersion(major, minor, patch, build);

            // Assert
            Assert.That(act, Throws.Exception.InstanceOf<ArgumentOutOfRangeException>());
        }

        [TestCaseSource("ParseCases")]
        public void ParseWorksCorrectly(string s, KspVersion version)
        {
            // Act
            var result = KspVersion.Parse(s);

            // Assert
            Assert.AreEqual(version, result);
            Assert.AreEqual(s, result.ToString());
        }

        [TestCaseSource("ParseFailureCases")]
        public void ParseThrowsExceptionOnInvalidParameter(string s)
        {
            // Act
            // ReSharper disable once ObjectCreationAsStatement
            TestDelegate act = () => KspVersion.Parse(s);

            // Assert
            Assert.That(act, Throws.Exception);
        }

        [TestCaseSource("ToVersionRangeWorksCorrectlyCases")]
        public void ToVersionRangeWorksCorrectly(KspVersion version, KspVersionRange expectedRange)
        {
            // Act
            var result = version.ToVersionRange();

            // Assert
            Assert.AreEqual(expectedRange, result);
        }

        [TestCaseSource("ParseCases")]
        public void TryParseWorksCorrectly(string s, KspVersion version)
        {
            // Act
            KspVersion result;
            var success = KspVersion.TryParse(s, out result);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual(version, result);
            Assert.AreEqual(s, result.ToString());
        }

        [TestCaseSource("ParseFailureCases")]
        public void TryParseReturnsFalseOnInvalidParameter(string s)
        {
            // Act
            KspVersion result;
            var success = KspVersion.TryParse(s, out result);

            // Assert
            Assert.IsFalse(success);
        }

        [TestCaseSource("EqualityCases")]
        public void EqualityWorksCorrectly(KspVersion a, KspVersion b, bool areEqual)
        {
            // Act
            var genericEquality = a.Equals(b);
            var nonGenericEquality = a.Equals((object)b);
            var operatorEquality = a == b;
            var operatorInequality = a != b;
            var genericReferenceEquality = a.Equals(a);
            var nonGenericRefereneEquality = a.Equals((object)a);

            // Assert
            Assert.AreEqual(areEqual, genericEquality);
            Assert.AreEqual(areEqual, nonGenericEquality);
            Assert.AreEqual(areEqual, operatorEquality);
            Assert.AreNotEqual(areEqual, operatorInequality);
            Assert.IsTrue(genericReferenceEquality);
            Assert.IsTrue(nonGenericRefereneEquality);
        }

        [TestCaseSource("CompareToCases")]
        public void CompareToWorksCorrectly(KspVersion v1, KspVersion v2, int comparison)
        {
            // Act
            var genericCompareTo = v1.CompareTo(v2);
            var nonGenericCompareTo = v1.CompareTo((object)v2);
            var lessThanOperator = v1 < v2;
            var lessThanOrEqualOperator = v1 <= v2;
            var greaterThanOperator = v1 > v2;
            var greaterThanOrEqualOperator = v1 >= v2;

            var reverseGenericCompareTo = v2.CompareTo(v1);
            var reverseNonGenericCompareTo = v2.CompareTo((object)v1);
            var reverseLessThanOperator = v2 < v1;
            var reverseLessThanOrEqualOperator = v2 <= v1;
            var reverseGreaterThanOperator = v2 > v1;
            var reverseGreaterThanOrEqualOperator = v2 >= v1;

            // Assert
            Assert.AreEqual(Math.Sign(comparison), Math.Sign(genericCompareTo));
            Assert.AreEqual(Math.Sign(comparison), Math.Sign(nonGenericCompareTo));
            Assert.AreEqual(comparison < 0, lessThanOperator);
            Assert.AreEqual(comparison <= 0, lessThanOrEqualOperator);
            Assert.AreEqual(comparison > 0, greaterThanOperator);
            Assert.AreEqual(comparison >= 0, greaterThanOrEqualOperator);
            Assert.AreEqual(-Math.Sign(comparison), Math.Sign(reverseGenericCompareTo));
            Assert.AreEqual(-Math.Sign(comparison), Math.Sign(reverseNonGenericCompareTo));
            Assert.AreEqual(comparison > 0, reverseLessThanOperator);
            Assert.AreEqual(comparison >= 0, reverseLessThanOrEqualOperator);
            Assert.AreEqual(comparison < 0, reverseGreaterThanOperator);
            Assert.AreEqual(comparison <= 0, reverseGreaterThanOrEqualOperator);
        }

        [Test]
        public void CompareToThrowsOnNullParameters()
        {
            // Act
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            // ReSharper disable UnusedVariable
            TestDelegate actGenericCompareTo = () => new KspVersion().CompareTo(null);
            TestDelegate actNonGenericCompareTo = () => new KspVersion().CompareTo((object)null);
            TestDelegate lessThanOperatorNullLeft = () => { var _ = null < new KspVersion(); };
            TestDelegate lessThanOperatorNullRight = () => { var _ = new KspVersion() < null; };
            TestDelegate lessThanOrEqualOperatorNullLeft = () => { var _ = null <= new KspVersion(); };
            TestDelegate lessThanOrEqualOperatorNullRight = () => { var _ = new KspVersion() <= null; };
            TestDelegate greaterThanOperatorNullLeft = () => { var _ = null > new KspVersion(); };
            TestDelegate greaterThanOperatorNullRight = () => { var _ = new KspVersion() > null; };
            TestDelegate greaterThanOrEqualOperatorNullLeft = () => { var _ = null >= new KspVersion(); };
            TestDelegate greaterThanOrEqualOperatorNullRight = () => { var _ = new KspVersion() >= null; };
            // ReSharper restore UnusedVariable
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed

            // Assert
            Assert.That(actGenericCompareTo, Throws.Exception);
            Assert.That(actNonGenericCompareTo, Throws.Exception);
            Assert.That(lessThanOperatorNullLeft, Throws.Exception);
            Assert.That(lessThanOperatorNullRight, Throws.Exception);
            Assert.That(lessThanOrEqualOperatorNullLeft, Throws.Exception);
            Assert.That(lessThanOrEqualOperatorNullRight, Throws.Exception);
            Assert.That(greaterThanOperatorNullLeft, Throws.Exception);
            Assert.That(greaterThanOperatorNullRight, Throws.Exception);
            Assert.That(greaterThanOrEqualOperatorNullLeft, Throws.Exception);
            Assert.That(greaterThanOrEqualOperatorNullRight, Throws.Exception);
        }

        [Test]
        public void NonGenericCompareToThrowsOnInvalidType()
        {
            // Act
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            TestDelegate act = () => new KspVersion().CompareTo(new object());

            // Assert
            Assert.That(act, Throws.ArgumentException);
        }

        [Test]
        public void GetHashCodeDoesNotThrow(
            [Random(0, int.MaxValue, 1)]int major,
            [Random(0, int.MaxValue, 1)]int minor,
            [Random(0, int.MaxValue, 1)]int patch,
            [Random(0, int.MaxValue, 1)]int build
        )
        {
            // Act
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            TestDelegate act = () => new KspVersion(major, minor, patch, build).GetHashCode();

            // Assert
            Assert.That(act, Throws.Nothing);
        }
    }
}
