using System.IO;
using CKAN;
using NUnit.Framework;

namespace CKANTests
{
    [TestFixture()]
    public class KSPPathUtils
    {
        [Test()]
        public void NormalizePath()
        {
            Assert.AreEqual("/a/b/c".Replace('/', Path.DirectorySeparatorChar), CKAN.KSPPathUtils.NormalizePath("/a/b/c"), "Identity function failed");
            Assert.AreEqual("/a/b/c".Replace('/', Path.DirectorySeparatorChar), CKAN.KSPPathUtils.NormalizePath("\\a\\b\\c"), "Actual replace");
            Assert.AreEqual("/a/b/c".Replace('/', Path.DirectorySeparatorChar), CKAN.KSPPathUtils.NormalizePath("\\a/b\\c"), "Mixed slashes");
            Assert.AreEqual("a/b/c".Replace('/', Path.DirectorySeparatorChar), CKAN.KSPPathUtils.NormalizePath("a/b\\c"), "No starting slash");
            Assert.AreEqual("/a/b/c".Replace('/', Path.DirectorySeparatorChar), CKAN.KSPPathUtils.NormalizePath("\\a/b\\c\\"), "Trailing slash");
            Assert.AreEqual("SPACE", CKAN.KSPPathUtils.NormalizePath("SPACE"), "All upper-case, no slashes");
        }

        [Test()]
        public void GetLastPathElement()
        {
            Assert.AreEqual("c", CKAN.KSPPathUtils.GetLastPathElement("/a/b/c"), "Simple case");
            Assert.AreEqual("c", CKAN.KSPPathUtils.GetLastPathElement("\\a\\b\\c"), "With other slashes");
            Assert.AreEqual("c", CKAN.KSPPathUtils.GetLastPathElement("\\a/b\\c"), "With mixed slashes");
            Assert.AreEqual("c", CKAN.KSPPathUtils.GetLastPathElement("a/b\\c"), "No starting slash");
            Assert.AreEqual("c", CKAN.KSPPathUtils.GetLastPathElement("\\a/b\\c\\"), "Trailing slash");
            Assert.AreEqual("kOS", CKAN.KSPPathUtils.GetLastPathElement("GameData/kOS"), "Real world test");
            Assert.AreEqual("buckethead", CKAN.KSPPathUtils.GetLastPathElement("buckethead"), "No slashes at all");
        }

        [Test()]
        public void GetLeadingPathElements()
        {
            Assert.AreEqual("/a/b".Replace('/', Path.DirectorySeparatorChar), CKAN.KSPPathUtils.GetLeadingPathElements("/a/b/c"), "Simple case");
            Assert.AreEqual("/a/b".Replace('/', Path.DirectorySeparatorChar), CKAN.KSPPathUtils.GetLeadingPathElements("\\a\\b\\c"), "With other slashes");
            Assert.AreEqual("/a/b".Replace('/', Path.DirectorySeparatorChar), CKAN.KSPPathUtils.GetLeadingPathElements("\\a/b\\c"), "With mixed slashes");
            Assert.AreEqual("a/b".Replace('/', Path.DirectorySeparatorChar), CKAN.KSPPathUtils.GetLeadingPathElements("a/b\\c"), "No starting slash");
            Assert.AreEqual("/a/b".Replace('/', Path.DirectorySeparatorChar), CKAN.KSPPathUtils.GetLeadingPathElements("\\a/b\\c\\"), "Trailing slash");

            Assert.IsEmpty(CKAN.KSPPathUtils.GetLeadingPathElements("ModuleManager.2.5.1.dll"));
        }

        [Test]
        public void ToRelative()
        {
            Assert.AreEqual(
                "GameData/Cake".Replace('/', Path.DirectorySeparatorChar),
                CKAN.KSPPathUtils.ToRelative("/home/fionna/KSP/GameData/Cake", "/home/fionna/KSP"),
                "Basic operation"
            );

            Assert.AreEqual(
                "GameData/Cake".Replace('/', Path.DirectorySeparatorChar),
                CKAN.KSPPathUtils.ToRelative(@"\home\fionna\KSP\GameData\Cake", "/home/fionna/KSP"),
                "Swapped slashes"
            );

            Assert.AreEqual(
                "GameData/Cake".Replace('/', Path.DirectorySeparatorChar),
                CKAN.KSPPathUtils.ToRelative("/home/fionna/KSP/GameData/Cake/", "/home/fionna/KSP"),
                "Trailing slash in path"
            );

            Assert.AreEqual(
                "GameData/Cake".Replace('/', Path.DirectorySeparatorChar),
                CKAN.KSPPathUtils.ToRelative("/home/fionna/KSP/GameData/Cake", "/home/fionna/KSP/"),
                "Trailing slash in root"
            );

            Assert.AreEqual(
                "GameData/Cake".Replace('/', Path.DirectorySeparatorChar),
                CKAN.KSPPathUtils.ToRelative("/home/fionna/KSP/GameData/Cake/", "/home/fionna/KSP/"),
                "Trailing slashes for everyone!"
            );

            Assert.Throws<PathErrorKraken>(delegate
            {
                CKAN.KSPPathUtils.ToRelative("/home/fionna/KSP/GameData/Cake", "/home/finn/KSP");
            }, "Not a sub-path");

            Assert.Throws<PathErrorKraken>(delegate
            {
                CKAN.KSPPathUtils.ToRelative("KSP/GameData/Cake", "/KSP/GameData");
            }, "Path not absolute");

            Assert.Throws<PathErrorKraken>(delegate
            {
                CKAN.KSPPathUtils.ToRelative("/home/fionna/KSP/GameData/Cake", "home/fionna/KSP");
            }, "Root not absolute");

            Assert.Throws<PathErrorKraken>(delegate
            {
                CKAN.KSPPathUtils.ToRelative(null, "/home/fionna/KSP");
            }, "null path");

            Assert.Throws<PathErrorKraken>(delegate
            {
                CKAN.KSPPathUtils.ToRelative("/home/fionna/KSP/GameData/Cake", null);
            }, "null root");
        }

        [Test]
        public void ToAbsolute()
        {
            Assert.AreEqual(
                "/home/fionna/KSP/GameData/Cake".Replace('/', Path.DirectorySeparatorChar),
                CKAN.KSPPathUtils.ToAbsolute("GameData/Cake", "/home/fionna/KSP"),
                "Basic functionality"
            );

            Assert.AreEqual(
                "/home/fionna/KSP/GameData/Cake".Replace('/', Path.DirectorySeparatorChar),
                CKAN.KSPPathUtils.ToAbsolute("GameData/Cake/", "/home/fionna/KSP"),
                "Trailing slashes path"
            );

            Assert.AreEqual(
                "/home/fionna/KSP/GameData/Cake".Replace('/', Path.DirectorySeparatorChar),
                CKAN.KSPPathUtils.ToAbsolute("GameData/Cake", "/home/fionna/KSP/"),
                "Trailing slashes root"
            );

            Assert.AreEqual(
                "/home/fionna/KSP/GameData/Cake".Replace('/', Path.DirectorySeparatorChar),
                CKAN.KSPPathUtils.ToAbsolute("GameData/Cake/", "/home/fionna/KSP/"),
                "Trailing slashes for all"
            );

            Assert.AreEqual(
                "/home/fionna/KSP/GameData/Cake".Replace('/', Path.DirectorySeparatorChar),
                CKAN.KSPPathUtils.ToAbsolute(@"GameData\Cake\", "/home/fionna/KSP"),
                "Swapped slashes"
            );

            Assert.Throws<PathErrorKraken>(delegate
            {
                CKAN.KSPPathUtils.ToAbsolute("/GameData/Cake", "/home/fionna/KSP");
            }, "Rooted path");

            Assert.Throws<PathErrorKraken>(delegate
            {
                CKAN.KSPPathUtils.ToAbsolute("GameData/Cake", "home/fionna/KSP");
            }, "Unrooted root");

            Assert.Throws<PathErrorKraken>(delegate
            {
                CKAN.KSPPathUtils.ToAbsolute(null, "/home/fionna/KSP");
            }, "null path");

            Assert.Throws<PathErrorKraken>(delegate
            {
                CKAN.KSPPathUtils.ToAbsolute("/home/fionna/KSP/GameData/Cake", null);
            }, "null root");
        }
    }
}