using NUnit.Framework;
using Tests;
using System.Collections.Generic;

namespace CKANTests
{
    [TestFixture()]
    public class ModuleInstallDescriptor
    {
        [Test()]
        // NOTE: I've *never* got these to fail. The problem I'm trying to reproduce
        // seems to involve saving to the registry and back. It's now fixed in
        // JsonSingleOrArrayConverter.cs, but these tests remain, because tests are good.
        public void Null_Filters()
        {
            // We had a bug whereby we could end up with a filter list of a single null.
            // Make sure that doesn't happen ever again.

            // We want a module that doesn't specify filters.
            CKAN.CkanModule mod = TestData.kOS_014_module();

            test_filter(mod.install[0].filter, "kOS/filter");
            test_filter(mod.install[0].filter_regexp, "kOS/filter_regexp");

            // And Firespitter seems to trigger it.

            CKAN.CkanModule firespitter = TestData.FireSpitterModule();

            foreach (var stanza in firespitter.install)
            {
                test_filter(stanza.filter, "Firespitter/filter");
                test_filter(stanza.filter_regexp, "Firespitter/filter_regexp");
            }
        }

        private void test_filter(List<string> filter, string message)
        {
            if (filter != null)
            {
                Assert.IsFalse(filter.Contains(null), message);
            }
        }
    }
}

