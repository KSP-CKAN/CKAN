using System.Globalization;

using NUnit.Framework;

using CKAN;

namespace Tests
{
    [SetUpFixture]
    public class GlobalTestSetup
    {
        [OneTimeSetUp]
        public void GlobalSetup()
        {
            CultureInfo.CurrentUICulture = new CultureInfo("en-GB");
            Logging.Initialize();
        }
    }
}
