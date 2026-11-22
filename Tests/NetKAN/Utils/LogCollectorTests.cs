using System;
using System.Linq;

using NUnit.Framework;
using log4net;
using log4net.Config;
using log4net.Repository.Hierarchy;
using log4net.Core;

using CKAN.NetKAN.Utils;

namespace Tests.NetKAN.Utils
{
    [TestFixture]
    public sealed class LogCollectorTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            BasicConfigurator.Configure(LogManager.GetRepository());
        }

        // The tests' log4net.xml drops everything below WARN
        [SetUp]
        public void LowerLogLevel()
        {
            if (LogManager.GetRepository() is Hierarchy hierarchy)
            {
                previousLevel        = hierarchy.Root.Level;
                hierarchy.Root.Level  = Level.All;
                hierarchy.RaiseConfigurationChanged(EventArgs.Empty);
            }
        }

        [TearDown]
        public void RestoreLogLevel()
        {
            if (LogManager.GetRepository() is Hierarchy hierarchy)
            {
                hierarchy.Root.Level = previousLevel;
                hierarchy.RaiseConfigurationChanged(EventArgs.Empty);
            }
        }

        private Level? previousLevel;

        [Test]
        public void Collects_InfoAndAboveInScope_NestedScopesSeeInner()
        {
            log.Warn("before");
            using (var outer = new LogCollector())
            {
                log.Debug("debug");
                log.Info("outer");
                using (var inner = new LogCollector())
                {
                    log.Error("inner");
                    CollectionAssert.AreEqual(new[] { "inner" },
                                              inner.Messages.Select(m => m.Message));
                }
                log.Warn("after inner");
                CollectionAssert.AreEqual(new[] { ("INFO",  "outer"),
                                                  ("ERROR", "inner"),
                                                  ("WARN",  "after inner") },
                                          outer.Messages.Select(m => (m.Level, m.Message)));
            }
        }

        [Test]
        public void Collects_IgnoresOtherThreads()
        {
            using (var collector = new LogCollector())
            {
                var t = new System.Threading.Thread(() => log.Warn("other thread"));
                // A thread inherits the async context, so start it with flow suppressed
                using (System.Threading.ExecutionContext.SuppressFlow())
                {
                    t.Start();
                }
                t.Join();
                log.Warn("mine");
                CollectionAssert.AreEqual(new[] { "mine" },
                                          collector.Messages.Select(m => m.Message));
            }
        }

        [Test]
        public void Replay_KeepsLoggerAndLevel()
        {
            LogMessage original;
            using (var collector = new LogCollector())
            {
                log.Warn("replay me");
                original = collector.Messages.Single();
            }

            using (var collector = new LogCollector())
            {
                original.Replay(typeof(LogCollectorTests));
                var replayed = collector.Messages.Single();
                Assert.AreEqual(typeof(LogCollectorTests).FullName, replayed.Logger);
                Assert.AreEqual("WARN",      replayed.Level);
                Assert.AreEqual("replay me", replayed.Message);
            }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(LogCollectorTests));
    }
}
