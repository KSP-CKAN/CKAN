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
        public void RunCommand_AGExt_Works()
        {
            // Ensure
            CultureInfo.DefaultThreadCurrentUICulture =
                Thread.CurrentThread.CurrentUICulture =
                    new CultureInfo("en-GB");

            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            var repo = new Repository("test", "https://github.com/");
            using (var inst     = new DisposableKSP())
            using (var repoData = new TemporaryRepositoryData(user, new Dictionary<Repository, RepositoryData>
            {
                { repo, RepositoryData.FromJson(TestData.TestRepository(), null)! },
            }))
            using (var regMgr = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                         new Repository[] { repo }))
            {
                ICommand sut  = new Show(repoData.Manager, user);
                var      opts = new ShowOptions()
                                {
                                    with_versions = true,
                                    modules       = new List<string> { "AGExt" },
                                };

                // Act
                sut.RunCommand(inst.KSP, opts);

                // Assert
                CollectionAssert.AreEqual(
                    new string[]
                    {
                        "Action Groups Extended: Increases the number of action groups to 250 and allows in-flight editing.",
                        "",
                        "Module info:",
                        "  Version:	1.24a",
                        "  Authors:	Diazo",
                        "  Status:	stable",
                        "  Licence:	GPL-3.0",
                        "",
                        "Recommends:",
                        "  - Toolbar",
                        "",
                        "Resources:",
                        "  Home page:	http://forum.kerbalspaceprogram.com/threads/74195",
                        "  Repository:	https://github.com/SirDiazo/AGExt",
                        "",
                        "Filename: F3862938-AGExt-1.24a.zip",
                        "",
                        "Version  Game Versions",
                        "-------  -------------",
                        "1.24a    KSP 0.25     ",
                        "1.24     KSP 0.25     ",
                        "1.23c    KSP 0.25     ",
                        "1.23a    KSP 0.25     ",
                        "1.23     KSP 0.25     ",
                        "1.22b    KSP 0.25     ",
                        "1.22a    KSP 0.25     ",
                        "1.22     KSP 0.25     ",
                        "1.21a    KSP 0.25     ",
                        "1.20     KSP 0.25     ",
                        ""
                    },
                    user.RaisedMessages);
            }
        }
    }
}
