using NUnit.Framework;

using CKAN;
using CKAN.CmdLine;
using Tests.Data;
using Tests.Core.Configuration;

namespace Tests.CmdLine
{
    [TestFixture]
    public class StabilityTests
    {
        [Test]
        public void RunSubCommand_List_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst     = new DisposableKSP())
            using (var repoData = new TemporaryRepositoryData(user))
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager  = new GameInstanceManager(user, config))
            {
                manager.SetCurrentInstance(inst.KSP);
                ISubCommand sut     = new Stability(manager, repoData.Manager, user);
                var         args    = new string[] { "stability", "list" };
                var         subOpts = new SubCommandOptions(args);
                var         stabTol = inst.KSP.StabilityToleranceConfig;

                // Act
                stabTol.SetModStabilityTolerance("mod1", ReleaseStatus.stable);
                stabTol.SetModStabilityTolerance("mod2", ReleaseStatus.testing);
                stabTol.SetModStabilityTolerance("mod3", ReleaseStatus.development);
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                              "Overall stability tolerance: stable",
                                              "",
                                              "Mod   Override   ",
                                              "----  -----------",
                                              "mod1  stable     ",
                                              "mod2  testing    ",
                                              "mod3  development",
                                          },
                                          user.RaisedMessages);
            }
        }

        [Test]
        public void RunSubCommand_Set_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst     = new DisposableKSP())
            using (var repoData = new TemporaryRepositoryData(user))
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager  = new GameInstanceManager(user, config))
            {
                manager.SetCurrentInstance(inst.KSP);
                var stabTol = inst.KSP.StabilityToleranceConfig;
                ISubCommand sut      = new Stability(manager, repoData.Manager, user);
                var         args1    = new string[] { "stability", "set", "testing" };
                var         subOpts1 = new SubCommandOptions(args1);
                var         args2    = new string[] { "stability", "set", "--mod", "mod1", "stable" };
                var         subOpts2 = new SubCommandOptions(args2);
                var         args3    = new string[] { "stability", "set", "--mod", "mod2", "testing" };
                var         subOpts3 = new SubCommandOptions(args3);
                var         args4    = new string[] { "stability", "set", "--mod", "mod3", "development" };
                var         subOpts4 = new SubCommandOptions(args4);

                // Act
                sut.RunSubCommand(null, subOpts1);
                sut.RunSubCommand(null, subOpts2);
                sut.RunSubCommand(null, subOpts3);
                sut.RunSubCommand(null, subOpts4);

                // Assert
                CollectionAssert.IsEmpty(user.RaisedErrors);
                Assert.AreEqual(ReleaseStatus.testing,     stabTol.OverallStabilityTolerance);
                Assert.AreEqual(ReleaseStatus.stable,      stabTol.ModStabilityTolerance("mod1"));
                Assert.AreEqual(ReleaseStatus.testing,     stabTol.ModStabilityTolerance("mod2"));
                Assert.AreEqual(ReleaseStatus.development, stabTol.ModStabilityTolerance("mod3"));
            }
        }
    }
}
