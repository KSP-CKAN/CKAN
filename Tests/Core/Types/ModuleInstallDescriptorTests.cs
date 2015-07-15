using System.Collections.Generic;
using CKAN;
using NUnit.Framework;
using Tests.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ICSharpCode.SharpZipLib.Zip;

namespace Tests.Core.Types
{
    [TestFixture]
    public class ModuleInstallDescriptorTests
    {
        [Test]
        // NOTE: I've *never* got these to fail. The problem I'm trying to reproduce
        // seems to involve saving to the registry and back. It's now fixed in
        // JsonSingleOrArrayConverter.cs, but these tests remain, because tests are good.
        public void Null_Filters()
        {
            // We had a bug whereby we could end up with a filter list of a single null.
            // Make sure that doesn't happen ever again.

            // We want a module that doesn't specify filters.
            CkanModule mod = TestData.kOS_014_module();

            test_filter(mod.install[0].filter, "kOS/filter");
            test_filter(mod.install[0].filter_regexp, "kOS/filter_regexp");

            // And Firespitter seems to trigger it.

            CkanModule firespitter = TestData.FireSpitterModule();

            foreach (var stanza in firespitter.install)
            {
                test_filter(stanza.filter, "Firespitter/filter");
                test_filter(stanza.filter_regexp, "Firespitter/filter_regexp");
            }
        }

        [Test]
        [TestCase("/DogeCoinFlag", "DogeCoinFlag-1.01/GameData/DogeCoinFlag","Anchored")]
        [TestCase("DogeCoinFlag", "DogeCoinFlag-1.01","Missing anchors")]
        public void regexp_filter_1089(string regexp, string expected, string comment)
        {
            string json = @"{
                ""install_to""  : ""GameData"",
                ""find_regexp"" : """ + regexp + @"""
            }"; 

            var mid = JsonConvert.DeserializeObject<CKAN.ModuleInstallDescriptor>(json);
            var zip = new ZipFile(TestData.DogeCoinFlagZipWithExtras());

            mid = mid.ConvertFindToFile(zip);

            Assert.AreEqual(expected, mid.file, comment);
        }

        private static void test_filter(List<string> filter, string message)
        {
            if (filter != null)
            {
                Assert.IsFalse(filter.Contains(null), message);
            }
        }
    }
}

