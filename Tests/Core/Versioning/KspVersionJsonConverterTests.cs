using System;
using CKAN.Versioning;
using Newtonsoft.Json;
using NUnit.Framework;

#pragma warning disable 414

namespace Tests.Core.Versioning
{
    public sealed class KspVersionJsonConverterTests
    {
        private static readonly object[] WriteJsonCases =
        {
            new object[] { new KspVersion(1), "1" },
            new object[] { new KspVersion(1, 2), "1.2" },
            new object[] { new KspVersion(1, 2, 3), "1.2.3" },
            new object[] { new KspVersion(1, 2, 3, 4), "1.2.3.4" },
        };

        private static readonly object[] ReadJsonCases =
        {
            new object[] { @"{ ""KspVersion"": null }", null },
            new object[] { @"{ ""KspVersion"": ""any"" }", KspVersion.Any },
            new object[] { @"{ ""KspVersion"": ""1""} ", new KspVersion(1) },
            new object[] { @"{ ""KspVersion"": ""1.2""} ", new KspVersion(1, 2) },
            new object[] { @"{ ""KspVersion"": ""1.2.3""} ", new KspVersion(1, 2, 3) },
            new object[] { @"{ ""KspVersion"": ""1.2.3.4""} ", new KspVersion(1, 2, 3, 4) },
        };

        private static readonly object[] ReadJsonFailureCases =
        {
            new object[] { @"{ ""KspVersion"": ""     "" }" },
            new object[] { @"{ ""KspVersion"": ""a.b.c"" }" }
        };

        [TestCaseSource("WriteJsonCases")]
        public void WriteJsonWorksCorrectly(KspVersion version, string expected)
        {
            // Arrange
            var poco = new TestPoco { KspVersion = version };

            // Act
            dynamic result = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(poco));

            // Assert
            Assert.That((string)result.KspVersion, Is.EqualTo(expected));
        }

        [TestCaseSource("ReadJsonCases")]
        public void ReadJsonWorksCorrectly(string json, KspVersion expected)
        {
            // Act
            var result = JsonConvert.DeserializeObject<TestPoco>(json);

            // Assert
            Assert.That(result.KspVersion, Is.EqualTo(expected));
        }

        [TestCaseSource("ReadJsonFailureCases")]
        public void ReadJsonThrowsOnInvalidValue(string json)
        {
            // Act
            TestDelegate act = () => JsonConvert.DeserializeObject<TestPoco>(json);

            // Assert
            Assert.That(act, Throws.Exception);
        }

        [TestCase(typeof(KspVersion), true)]
        [TestCase(typeof(KspVersionBound), false)]
        [TestCase(typeof(KspVersionRange), false)]
        [TestCase(typeof(string), false)]
        [TestCase(typeof(int), false)]
        [TestCase(typeof(object), false)]
        public void CanConvertWorksCorrectly(Type objectType, bool expected)
        {
            // Arrange
            var sut = new KspVersionJsonConverter();

            // Act
            var result = sut.CanConvert(objectType);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        private sealed class TestPoco
        {
            public KspVersion KspVersion { get; set; }
        }
    }
}
