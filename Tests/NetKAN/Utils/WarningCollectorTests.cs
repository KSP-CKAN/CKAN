using NUnit.Framework;
using log4net;
using log4net.Config;

using CKAN.NetKAN.Utils;

namespace Tests.NetKAN.Utils
{
    [TestFixture]
    public sealed class WarningCollectorTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            BasicConfigurator.Configure(LogManager.GetRepository());
        }

        [Test]
        public void Collects_OnlyWarningsInScope_NestedScopesSeeInner()
        {
            log.Warn("before");
            using (var outer = new WarningCollector())
            {
                log.Info("info");
                log.Warn("outer");
                using (var inner = new WarningCollector())
                {
                    log.Error("error");
                    log.Warn("inner");
                    CollectionAssert.AreEqual(new[] { "inner" }, inner.Warnings);
                }
                log.Warn("after inner");
                CollectionAssert.AreEqual(new[] { "outer", "inner", "after inner" },
                                          outer.Warnings);
            }
        }

        [Test]
        public void Collects_IgnoresOtherThreads()
        {
            using (var collector = new WarningCollector())
            {
                var t = new System.Threading.Thread(() => log.Warn("other thread"));
                // A thread inherits the async context, so start it with flow suppressed
                using (System.Threading.ExecutionContext.SuppressFlow())
                {
                    t.Start();
                }
                t.Join();
                log.Warn("mine");
                CollectionAssert.AreEqual(new[] { "mine" }, collector.Warnings);
            }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(WarningCollectorTests));
    }
}
