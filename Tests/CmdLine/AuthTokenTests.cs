using System.Linq;

using NUnit.Framework;

using CKAN;
using CKAN.CmdLine;
using Tests.Data;
using Tests.Core.Configuration;

namespace Tests.CmdLine
{
    [TestFixture]
    public class AuthTokenTests
    {
        [Test]
        public void RunSubCommand_List_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                config.SetAuthToken("testhost.com", "abcdefg");
                ISubCommand sut     = new AuthToken(manager, user);
                var         args    = new string[] { "authtoken", "list" };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                              "Host          Token  ",
                                              "------------  -------",
                                              "testhost.com  abcdefg",
                                          },
                                          user.RaisedMessages);
            }
        }

        [Test]
        public void RunSubCommand_Add_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                ISubCommand sut     = new AuthToken(manager, user);
                var         args    = new string[] { "authtoken", "add", "testhost.com", "abcdefg" };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.AreEqual(Enumerable.Repeat("testhost.com", 1),
                                          config.GetAuthTokenHosts());
                Assert.IsTrue(config.TryGetAuthToken("testhost.com",
                                                     out string? actualToken));
                Assert.AreEqual("abcdefg", actualToken);
            }
        }

        [Test]
        public void RunSubCommand_Remove_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                config.SetAuthToken("testhost.com", "abcdefg");
                ISubCommand sut     = new AuthToken(manager, user);
                var         args    = new string[] { "authtoken", "remove", "testhost.com" };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.IsEmpty(config.GetAuthTokenHosts());
                Assert.IsFalse(config.TryGetAuthToken("testhost.com",
                                                      out string? actualToken));
            }
        }
    }
}
