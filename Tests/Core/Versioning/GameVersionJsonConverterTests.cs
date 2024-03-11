using System;

using Newtonsoft.Json;
using NUnit.Framework;

using CKAN.Versioning;

#pragma warning disable 414

namespace Tests.Core.Versioning
{
    public sealed class GameVersionJsonConverterTests
    {
        private static readonly object[] WriteJsonCases =
        {
            new object[] { new GameVersion(1), "1" },
            new object[] { new GameVersion(1, 2), "1.2" },
            new object[] { new GameVersion(1, 2, 3), "1.2.3" },
            new object[] { new GameVersion(1, 2, 3, 4), "1.2.3.4" },
        };

        private static readonly object[] ReadJsonCases =
        {
            new object[] { @"{ ""GameVersion"": null }", null },
            new object[] { @"{ ""GameVersion"": ""any"" }", GameVersion.Any },
            new object[] { @"{ ""GameVersion"": ""1""} ", new GameVersion(1) },
            new object[] { @"{ ""GameVersion"": ""1.2""} ", new GameVersion(1, 2) },
            new object[] { @"{ ""GameVersion"": ""1.2.3""} ", new GameVersion(1, 2, 3) },
            new object[] { @"{ ""GameVersion"": ""1.2.3.4""} ", new GameVersion(1, 2, 3, 4) },
            new object[] { @"{ ""GameVersion"": ""1.1.""} ", new GameVersion(1,1) }, // #1780
        };

        private static readonly object[] ReadJsonFailureCases =
        {
            new object[] { @"{ ""GameVersion"": ""     "" }" },
            new object[] { @"{ ""GameVersion"": ""a.b.c"" }" }
        };

        [TestCaseSource("WriteJsonCases")]
        public void WriteJsonWorksCorrectly(GameVersion version, string expected)
        {
            // Arrange
            var poco = new TestPoco { GameVersion = version };

            // Act
            dynamic result = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(poco));

            // Assert
            Assert.That((string)result.GameVersion, Is.EqualTo(expected));
        }

        [TestCaseSource("ReadJsonCases")]
        public void ReadJsonWorksCorrectly(string json, GameVersion expected)
        {
            // Act
            var result = JsonConvert.DeserializeObject<TestPoco>(json);

            // Assert
            Assert.That(result.GameVersion, Is.EqualTo(expected));
        }

        [TestCaseSource("ReadJsonFailureCases")]
        public void ReadJsonThrowsOnInvalidValue(string json)
        {
            // Act
            TestDelegate act = () => JsonConvert.DeserializeObject<TestPoco>(json);

            // Assert
            Assert.That(act, Throws.Exception);
        }

        [TestCase(typeof(GameVersion), true)]
        [TestCase(typeof(GameVersionBound), false)]
        [TestCase(typeof(GameVersionRange), false)]
        [TestCase(typeof(string), false)]
        [TestCase(typeof(int), false)]
        [TestCase(typeof(object), false)]
        public void CanConvertWorksCorrectly(Type objectType, bool expected)
        {
            // Arrange
            var sut = new GameVersionJsonConverter();

            // Act
            var result = sut.CanConvert(objectType);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        private sealed class TestPoco
        {
            public GameVersion GameVersion { get; set; }
        }
    }
}
