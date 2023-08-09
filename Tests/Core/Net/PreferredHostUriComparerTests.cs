using System;
using System.Linq;

using NUnit.Framework;

using CKAN;

namespace Tests.Core.Net
{
    [TestFixture]
    public sealed class PreferredHostUriComparerTests
    {
        private static readonly Uri[] uris = new Uri[]
        {
            new Uri("https://taniwha.org/"),
            new Uri("https://spacedock.info/"),
            new Uri("https://github.com/"),
            new Uri("https://archive.org/"),
        };

        // Reminder: null means "all other hosts"

        [Test,
            // Null settings
            TestCase(null,
                     new string[]
                     {
                        "https://taniwha.org/",
                        "https://spacedock.info/",
                        "https://github.com/",
                        "https://archive.org/",
                     }),
            // Empty settings
            TestCase(new string[] { },
                     new string[]
                     {
                        "https://taniwha.org/",
                        "https://spacedock.info/",
                        "https://github.com/",
                        "https://archive.org/",
                     }),
            // Irrelevant settings
            TestCase(new string[] { "api.github.com", "curseforge.com", null, "www.dropbox.com", "drive.google.com" },
                     new string[]
                     {
                        "https://taniwha.org/",
                        "https://spacedock.info/",
                        "https://github.com/",
                        "https://archive.org/",
                     }),
            // Prioritize one
            TestCase(new string[] { "github.com", null },
                     new string[]
                     {
                        "https://github.com/",
                        "https://taniwha.org/",
                        "https://spacedock.info/",
                        "https://archive.org/",
                     }),
            // De-prioritize one
            TestCase(new string[] { null, "spacedock.info" },
                     new string[]
                     {
                        "https://taniwha.org/",
                        "https://github.com/",
                        "https://archive.org/",
                        "https://spacedock.info/",
                     }),
            // Prioritize one, de-prioritize another
            TestCase(new string[] { "github.com", null, "spacedock.info" },
                     new string[]
                     {
                        "https://github.com/",
                        "https://taniwha.org/",
                        "https://archive.org/",
                        "https://spacedock.info/",
                     }),
        ]
        public void OrderBy_WithPreferences_SortsCorrectly(string[] preferredHosts,
                                                           string[] correctAnswer)
        {
            // Arrange
            var comparer = new PreferredHostUriComparer(preferredHosts);

            // Act
            var result = uris.OrderBy(u => u, comparer)
                             .Select(u => u.ToString())
                             .ToArray();

            // Assert
            Assert.AreEqual(correctAnswer, result);
        }
    }
}
