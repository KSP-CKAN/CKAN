using NUnit.Framework;

using CKAN.Versioning;

namespace Tests.Core.Versioning
{
    [TestFixture]
    public sealed class CkanModuleVersionTests
    {
        [TestCase("v1.36.1.25100", "v1.36.1.25100", ExpectedResult = true,
                  Description = "Four piece dev build")]
        [TestCase("v1.36.0",       "v1.36.0",       ExpectedResult = true,
                  Description = "Three piece stable build")]
        [TestCase("v1.36.0.25098", "v1.36.0",       ExpectedResult = true,
                  Description = "Three piece remote build matches four piece local build")]
        [TestCase("v1.36.0.25098", "v1.36.2",       ExpectedResult = false,
                  Description = "Stable release upgrading to next stable release")]
        [TestCase("v1.36.0.25098", "v1.36.1.25100", ExpectedResult = false,
                  Description = "Stable release upgrading to dev build")]
        [TestCase("v1.36.1.25100", "v1.36.0",       ExpectedResult = false,
                  Description = "Dev build downgrading to stable release")]
        public bool SameClientVersion_TwoVersions_CorrectResult(string a, string b)
            => new CkanModuleVersion(b, "test").SameClientVersion(new ModuleVersion(a));
    }
}
