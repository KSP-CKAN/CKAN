using NUnit.Framework;

using CKAN;
using CKAN.CmdLine;
using Tests.Data;

namespace Tests.CmdLine
{
    [TestFixture]
    public class AvailableTests
    {
        [Test]
        public void RunCommand_WithMods_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst     = new DisposableKSP())
            using (var repo     = new TemporaryRepository(
                                      @"{
                                          ""spec_version"": 1,
                                          ""identifier"":   ""Mod1"",
                                          ""name"":         ""Mod One"",
                                          ""abstract"":     ""The first mod"",
                                          ""version"":      ""1.0"",
                                          ""download"":     ""https://github.com/""
                                      }",
                                      @"{
                                          ""spec_version"": 1,
                                          ""identifier"":   ""Mod2"",
                                          ""name"":         ""Mod Two"",
                                          ""abstract"":     ""The second mod"",
                                          ""version"":      ""2.0"",
                                          ""download"":     ""https://github.com/""
                                      }",
                                      @"{
                                          ""spec_version"": 1,
                                          ""identifier"":   ""Mod3"",
                                          ""name"":         ""Mod Three"",
                                          ""abstract"":     ""The third mod"",
                                          ""version"":      ""3.0"",
                                          ""download"":     ""https://github.com/""
                                      }",
                                      @"{
                                          ""spec_version"": 1,
                                          ""identifier"":   ""Mod4"",
                                          ""name"":         ""Mod Four"",
                                          ""abstract"":     ""The fourth mod"",
                                          ""version"":      ""4.0"",
                                          ""download"":     ""https://github.com/""
                                      }"))
            using (var repoData = new TemporaryRepositoryData(new NullUser(), repo.repo))
            using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                           new Repository[] { repo.repo }))
            {
                ICommand sut  = new Available(repoData.Manager, user);
                var      opts = new AvailableOptions() { detail = true };

                // Act
                sut.RunCommand(inst.KSP, opts);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                              "Modules compatible with KSP 0.25.0.642",
                                              "",
                                              "* Mod1 (1.0) - Mod One - The first mod",
                                              "* Mod2 (2.0) - Mod Two - The second mod",
                                              "* Mod3 (3.0) - Mod Three - The third mod",
                                              "* Mod4 (4.0) - Mod Four - The fourth mod",
                                          },
                                          user.RaisedMessages);
            }
        }
    }
}
