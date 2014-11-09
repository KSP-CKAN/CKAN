using NUnit.Framework;
using System;
using CKAN;

namespace Tests
{
    [TestFixture()]
    public class KSPPathUtils
    {
        [Test()]
        public void NormalizePath()
        {
            Assert.AreEqual("/a/b/c", CKAN.KSPPathUtils.NormalizePath("/a/b/c"), "Identity function failed");
            Assert.AreEqual("/a/b/c", CKAN.KSPPathUtils.NormalizePath("\\a\\b\\c"), "Actual replace");
            Assert.AreEqual("/a/b/c", CKAN.KSPPathUtils.NormalizePath("\\a/b\\c"), "Mixed slashes");
            Assert.AreEqual("a/b/c", CKAN.KSPPathUtils.NormalizePath("a/b\\c"), "No starting slash");
            Assert.AreEqual("/a/b/c", CKAN.KSPPathUtils.NormalizePath("\\a/b\\c\\"), "Trailing slash");
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
            Assert.AreEqual("/a/b", CKAN.KSPPathUtils.GetLeadingPathElements("/a/b/c"), "Simple case");
            Assert.AreEqual("/a/b", CKAN.KSPPathUtils.GetLeadingPathElements("\\a\\b\\c"), "With other slashes");
            Assert.AreEqual("/a/b", CKAN.KSPPathUtils.GetLeadingPathElements("\\a/b\\c"), "With mixed slashes");
            Assert.AreEqual("a/b", CKAN.KSPPathUtils.GetLeadingPathElements("a/b\\c"), "No starting slash");
            Assert.AreEqual("/a/b", CKAN.KSPPathUtils.GetLeadingPathElements("\\a/b\\c\\"), "Trailing slash");

            Assert.IsEmpty(CKAN.KSPPathUtils.GetLeadingPathElements("ModuleManager.2.5.1.dll"));
        }

    }
}

