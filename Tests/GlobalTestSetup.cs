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
            Logging.Initialize();
        }
    }
}
