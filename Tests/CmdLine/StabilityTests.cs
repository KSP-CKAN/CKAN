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
                ISubCommand sut     = new Stability(manager, repoData.Manager, user);
                var         args    = new string[] { "stability", "list" };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.AreEqual(new string[] { "Overall stability tolerance: stable" },
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
                ISubCommand sut = new Stability(manager, repoData.Manager, user);
                var         args    = new string[] { "stability", "set", "testing" };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                Assert.AreEqual(ReleaseStatus.testing,
                                inst.KSP.StabilityToleranceConfig.OverallStabilityTolerance);
            }
        }
    }
}
