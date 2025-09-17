using System.Collections.Generic;
using System.Threading;
using System.Globalization;

using NUnit.Framework;

using CKAN;
using CKAN.CmdLine;
using Tests.Data;

namespace Tests.CmdLine
{
    [TestFixture]
    public class ShowTests
    {
        [Test]
        public void RunCommand_WithTestMods_Works()
        {
            // Ensure the default locale is used
            CultureInfo.DefaultThreadCurrentUICulture =
                Thread.CurrentThread.CurrentUICulture =
                    new CultureInfo("en-GB");

            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst     = new DisposableKSP())
            using (var repo     = new TemporaryRepository(
                                      @"{
                                          ""spec_version"": 1,
                                          ""identifier"": ""TestMod"",
                                          ""name"": ""Test Mod"",
                                          ""abstract"": ""A mod with lots of metadata to be shown"",
                                          ""description"": ""We have to fill in a lot of fields to cover the show command"",
                                          ""author"": [ ""User1"", ""User2"" ],
                                          ""version"": ""1.0"",
                                          ""license"": ""MIT"",
                                          ""release_status"": ""stable"",
                                          ""tags"": [ ""plugin"" ],
                                          ""localizations"": [ ""en-US"" ],
                                          ""resources"": {
                                              ""homepage"": ""https://testmod.com""
                                          },
                                          ""provides"":   [ ""provided"" ],
                                          ""depends"":    [ { ""name"": ""Dependency""  } ],
                                          ""recommends"": [ { ""name"": ""Recommended"" } ],
                                          ""suggests"":   [ { ""name"": ""Suggested""   } ],
                                          ""supports"":   [ { ""name"": ""Supported""   } ],
                                          ""download"": ""https://github.com/""
                                      }",
                                      @"{
                                          ""spec_version"": 1,
                                          ""identifier"": ""InstalledMod"",
                                          ""name"": ""Installed Mod"",
                                          ""abstract"": ""A mod with lots of metadata to be shown"",
                                          ""description"": ""We have to fill in a lot of fields to cover the show command"",
                                          ""author"": [ ""User1"", ""User2"" ],
                                          ""version"": ""1.0"",
                                          ""license"": ""MIT"",
                                          ""release_status"": ""stable"",
                                          ""tags"": [ ""plugin"" ],
                                          ""localizations"": [ ""en-US"" ],
                                          ""resources"": {
                                              ""homepage"": ""https://testmod.com""
                                          },
                                          ""provides"":   [ ""provided"" ],
                                          ""depends"":    [ { ""name"": ""Dependency""  } ],
                                          ""recommends"": [ { ""name"": ""Recommended"" } ],
                                          ""suggests"":   [ { ""name"": ""Suggested""   } ],
                                          ""supports"":   [ { ""name"": ""Supported""   } ],
                                          ""download"": ""https://github.com/""
                                      }",
                                      @"{
                                          ""spec_version"": 1,
                                          ""identifier"": ""Dependency"",
                                          ""name"": ""Dependency Mod"",
                                          ""abstract"": ""A mod that we need for TestMod to be compatible"",
                                          ""version"": ""1.0"",
                                          ""download"": ""https://github.com/""
                                      }"))
            using (var repoData = new TemporaryRepositoryData(new NullUser(), repo.repo))
            using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                           new Repository[] { repo.repo }))
            {
                ICommand sut  = new Show(repoData.Manager, user);
                var      opts = new ShowOptions()
                                {
                                    with_versions = true,
                                    modules       = new List<string> { "InstalledMod", "TestMod" },
                                };

                // Act
                regMgr.registry.RegisterModule(regMgr.registry.LatestAvailable("InstalledMod",
                                                                               inst.KSP.StabilityToleranceConfig,
                                                                               inst.KSP.VersionCriteria())!,
                                               new string[] { inst.KSP.ToAbsoluteGameDir("GameData/InstalledMod.dll") },
                                               inst.KSP,
                                               false);
                sut.RunCommand(inst.KSP, opts);

                // Assert
                CollectionAssert.AreEqual(
                    new string[]
                    {
                        "Installed Mod: A mod with lots of metadata to be shown",
                        "",
                        "We have to fill in a lot of fields to cover the show command",
                        "",
                        "Module info:",
                        "  Version:	1.0",
                        "  Authors:	User1, User2",
                        "  Status:	stable",
                        "  Licence:	MIT",
                        "  Tags: 	plugin",
                        "  Languages:	en-US",
                        "",
                        "Depends:",
                        "  - Dependency",
                        "",
                        "Recommends:",
                        "  - Recommended",
                        "",
                        "Suggests:",
                        "  - Suggested",
                        "",
                        "Supports:",
                        "  - Supported",
                        "",
                        "Provides:",
                        "  - provided",
                        "",
                        "Resources:",
                        "  Home page:	https://testmod.com/",
                        "",
                        "Filename: D7B3438D-InstalledMod-1.0.zip",
                        "",
                        "Showing 1 installed files:",
                        "  - GameData/InstalledMod.dll",
                        "",
                        "Version  Game Versions   ",
                        "-------  ----------------",
                        "1.0      KSP All versions",
                        "",
                        "Test Mod: A mod with lots of metadata to be shown",
                        "",
                        "We have to fill in a lot of fields to cover the show command",
                        "",
                        "Module info:",
                        "  Version:	1.0",
                        "  Authors:	User1, User2",
                        "  Status:	stable",
                        "  Licence:	MIT",
                        "  Tags: 	plugin",
                        "  Languages:	en-US",
                        "",
                        "Depends:",
                        "  - Dependency",
                        "",
                        "Recommends:",
                        "  - Recommended",
                        "",
                        "Suggests:",
                        "  - Suggested",
                        "",
                        "Supports:",
                        "  - Supported",
                        "",
                        "Provides:",
                        "  - provided",
                        "",
                        "Resources:",
                        "  Home page:	https://testmod.com/",
                        "",
                        "Filename: D7B3438D-TestMod-1.0.zip",
                        "",
                        "Version  Game Versions   ",
                        "-------  ----------------",
                        "1.0      KSP All versions",
                        "",
                    },
                    user.RaisedMessages);
            }
        }
    }
}
