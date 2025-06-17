using NUnit.Framework;

using CKAN;
using CKAN.IO;

namespace Tests.Core.IO
{
    [TestFixture]
    public class CKANPathUtilsTests
    {
        [Test]
        public void NormalizePath()
        {
            Assert.AreEqual("/a/b/c", CKANPathUtils.NormalizePath("/a/b/c"), "Identity function failed");
            Assert.AreEqual("/a/b/c", CKANPathUtils.NormalizePath("\\a\\b\\c"), "Actual replace");
            Assert.AreEqual("/a/b/c", CKANPathUtils.NormalizePath("\\a/b\\c"), "Mixed slashes");
            Assert.AreEqual("a/b/c", CKANPathUtils.NormalizePath("a/b\\c"), "No starting slash");
            Assert.AreEqual("/a/b/c", CKANPathUtils.NormalizePath("\\a/b\\c\\"), "Trailing slash");
            Assert.AreEqual("SPACE", CKANPathUtils.NormalizePath("SPACE"), "All upper-case, no slashes");
        }

        [Test]
        public void ToRelative()
        {
            Assert.AreEqual(
                "GameData/Cake",
                CKANPathUtils.ToRelative("/home/fionna/KSP/GameData/Cake", "/home/fionna/KSP"),
                "Basic operation"
            );

            Assert.AreEqual(
                "GameData/Cake",
                CKANPathUtils.ToRelative(@"\home\fionna\KSP\GameData\Cake", "/home/fionna/KSP"),
                "Swapped slashes"
            );

            Assert.AreEqual(
                "GameData/Cake",
                CKANPathUtils.ToRelative("/home/fionna/KSP/GameData/Cake/", "/home/fionna/KSP"),
                "Trailing slash in path"
            );

            Assert.AreEqual(
                "GameData/Cake",
                CKANPathUtils.ToRelative("/home/fionna/KSP/GameData/Cake", "/home/fionna/KSP/"),
                "Trailing slash in root"
            );

            Assert.AreEqual(
                "GameData/Cake",
                CKANPathUtils.ToRelative("/home/fionna/KSP/GameData/Cake/", "/home/fionna/KSP/"),
                "Trailing slashes for everyone!"
            );

            // Mono can't handle these tests
            if (Platform.IsWindows)
            {
                Assert.AreEqual(
                    "GameData/Cake",
                    CKANPathUtils.ToRelative("K:GameData/Cake", "K:"),
                    "Root of a Windows drive"
                );

                Assert.AreEqual(
                    "GameData/Cake",
                    CKANPathUtils.ToRelative("K:GameData/Cake", "K:/"),
                    "Root of a Windows drive, slash in root"
                );

                Assert.AreEqual(
                    "GameData/Cake",
                    CKANPathUtils.ToRelative("K:/GameData/Cake", "K:"),
                    "Root of a Windows drive, slash in path"
                );

                Assert.AreEqual(
                    "GameData/Cake",
                    CKANPathUtils.ToRelative("K:/GameData/Cake", "K:/"),
                    "Root of a Windows drive, slash in both"
                );
            }

            Assert.Throws<PathErrorKraken>(delegate
            {
                CKANPathUtils.ToRelative("/home/fionna/KSP/GameData/Cake", "/home/finn/KSP");
            }, "Not a sub-path");

            Assert.Throws<PathErrorKraken>(delegate
            {
                CKANPathUtils.ToRelative("KSP/GameData/Cake", "/KSP/GameData");
            }, "Path not absolute");

            Assert.Throws<PathErrorKraken>(delegate
            {
                CKANPathUtils.ToRelative("/home/fionna/KSP/GameData/Cake", "home/fionna/KSP");
            }, "Root not absolute");

        }

        [Test]
        public void ToRelative_PathEqualsRoot_DontCrash()
        {
            // Arrange
            string path = "/home/fionna/KSP";
            string root = "/home/fionna/KSP";
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                string s = CKANPathUtils.ToRelative(path, root);
                Assert.IsEmpty(s);
            });
        }

        [Test]
        public void ToAbsolute()
        {
            Assert.AreEqual(
                "/home/fionna/KSP/GameData/Cake",
                CKANPathUtils.ToAbsolute("GameData/Cake","/home/fionna/KSP"),
                "Basic functionality"
            );

            Assert.AreEqual(
                "/home/fionna/KSP/GameData/Cake",
                CKANPathUtils.ToAbsolute("GameData/Cake/","/home/fionna/KSP"),
                "Trailing slashes path"
            );

            Assert.AreEqual(
                "/home/fionna/KSP/GameData/Cake",
                CKANPathUtils.ToAbsolute("GameData/Cake","/home/fionna/KSP/"),
                "Trailing slashes root"
            );

            Assert.AreEqual(
                "/home/fionna/KSP/GameData/Cake",
                CKANPathUtils.ToAbsolute("GameData/Cake/","/home/fionna/KSP/"),
                "Trailing slashes for all"
            );

            Assert.AreEqual(
                "/home/fionna/KSP/GameData/Cake",
                CKANPathUtils.ToAbsolute(@"GameData\Cake\","/home/fionna/KSP"),
                "Swapped slashes"
            );

            Assert.Throws<PathErrorKraken>(delegate
            {
                CKANPathUtils.ToAbsolute("/GameData/Cake", "/home/fionna/KSP");
            }, "Rooted path");

            Assert.Throws<PathErrorKraken>(delegate
            {
                CKANPathUtils.ToAbsolute("GameData/Cake", "home/fionna/KSP");
            }, "Unrooted root");

        }

    }
}
