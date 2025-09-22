using System;
using System.Linq;
using System.Collections.Generic;

using NUnit.Framework;
using Moq;

using CKAN;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Sources.Github;

using Tests.Data;

namespace Tests.NetKAN.Sources.Github
{
    [TestFixture]
    public sealed class GithubApiTests
    {
        [Test]
        public void GetRepoLatestReleaseAndOrgMembers_CKANTest_Works()
        {
            // Arrange
            using (var dir   = new TemporaryDirectory())
            using (var cache = new NetFileCache(dir.Directory.FullName))
            {
                var http      = new Mock<IHttpService>();
                http.Setup(h => h.DownloadText(It.Is<Uri>(u => u.AbsolutePath.StartsWith("/repos/")
                                                               && !u.AbsolutePath.EndsWith("/releases")),
                                               It.IsAny<string?>(), It.IsAny<string?>()))
                    .Returns(@"{
                                 ""name"": ""Test"",
                                 ""full_name"": ""KSP-CKAN/Test"",
                                 ""owner"": {
                                   ""login"": ""KSP-CKAN"",
                                   ""url"": ""https://api.github.com/users/KSP-CKAN"",
                                   ""html_url"": ""https://github.com/KSP-CKAN"",
                                   ""type"": ""Organization""
                                 },
                                 ""html_url"": ""https://github.com/KSP-CKAN/Test"",
                                 ""description"": ""This repo is used for testing."",
                                 ""homepage"": null,
                                 ""has_issues"": true,
                                 ""has_discussions"": false,
                                 ""archived"": false,
                                 ""license"": null
                             }");
                http.Setup(h => h.DownloadText(It.Is<Uri>(u => u.AbsolutePath.StartsWith("/repos/")
                                                               && u.AbsolutePath.EndsWith("/releases")),
                                               It.IsAny<string?>(), It.IsAny<string?>()))
                    .Returns(@"[
                                  {
                                    ""author"": {
                                      ""login"": ""pjf"",
                                      ""type"": ""User""
                                    },
                                    ""tag_name"": ""v1.00"",
                                    ""prerelease"": false,
                                    ""published_at"": ""2014-10-21T11:14:12Z"",
                                    ""assets"": [
                                      {
                                        ""name"": ""dogecoin.png"",
                                        ""uploader"": {
                                          ""login"": ""pjf"",
                                          ""type"": ""User""
                                        },
                                        ""updated_at"": ""2014-10-21T11:14:07Z"",
                                        ""browser_download_url"": ""https://github.com/KSP-CKAN/Test/releases/download/v1.00/dogecoin.png""
                                      },
                                      {
                                        ""name"": ""ModuleManager-2.5.1.zip"",
                                        ""uploader"": {
                                          ""login"": ""pjf"",
                                          ""type"": ""User""
                                        },
                                        ""updated_at"": ""2014-11-09T03:05:22Z"",
                                        ""browser_download_url"": ""https://github.com/KSP-CKAN/Test/releases/download/v1.00/ModuleManager-2.5.1.zip""
                                      }
                                    ],
                                    ""zipball_url"": ""https://api.github.com/repos/KSP-CKAN/Test/zipball/v1.00""
                                  },
                                  {
                                    ""author"": {
                                      ""login"": ""pjf"",
                                      ""type"": ""User""
                                    },
                                    ""tag_name"": ""v1.01"",
                                    ""prerelease"": true,
                                    ""published_at"": ""2014-11-07T01:47:15Z"",
                                    ""assets"": [],
                                    ""zipball_url"": ""https://api.github.com/repos/KSP-CKAN/Test/zipball/v1.01""
                                  }
                                ]");
                http.Setup(h => h.DownloadText(It.Is<Uri>(u => u.AbsolutePath.StartsWith("/orgs/")
                                                               && u.AbsolutePath.EndsWith("/public_members")),
                                               It.IsAny<string?>(), It.IsAny<string?>()))
                    .Returns(@"[
                                  {
                                    ""login"": ""AlexanderDzhoganov"",
                                    ""type"": ""User""
                                  },
                                  {
                                    ""login"": ""averageksp"",
                                    ""type"": ""User""
                                  },
                                  {
                                    ""login"": ""DasSkelett"",
                                    ""type"": ""User""
                                  },
                                  {
                                    ""login"": ""Dazpoet"",
                                    ""type"": ""User""
                                  },
                                  {
                                    ""login"": ""dbent"",
                                    ""type"": ""User""
                                  },
                                  {
                                    ""login"": ""DinCahill"",
                                    ""type"": ""User""
                                  },
                                  {
                                    ""login"": ""hakan42"",
                                    ""type"": ""User""
                                  },
                                  {
                                    ""login"": ""HebaruSan"",
                                    ""type"": ""User""
                                  },
                                  {
                                    ""login"": ""netkan-bot"",
                                    ""type"": ""User""
                                  },
                                  {
                                    ""login"": ""Olympic1"",
                                    ""type"": ""User""
                                  },
                                  {
                                    ""login"": ""parisba"",
                                    ""type"": ""User""
                                  },
                                  {
                                    ""login"": ""pjf"",
                                    ""type"": ""User""
                                  },
                                  {
                                    ""login"": ""politas"",
                                    ""type"": ""User""
                                  },
                                  {
                                    ""login"": ""ProfFan"",
                                    ""type"": ""User""
                                  },
                                  {
                                    ""login"": ""rafl"",
                                    ""type"": ""User""
                                  },
                                  {
                                    ""login"": ""techman83"",
                                    ""type"": ""User""
                                  }
                                ]");
                var sut       = new GithubApi(http.Object,
                                              Environment.GetEnvironmentVariable("GITHUB_TOKEN"));
                var githubRef = new GithubRef("KSP-CKAN", "Test");

                // Act
                var repo    = sut.GetRepo(githubRef)!;
                var release = sut.GetLatestRelease(githubRef, false)!;
                var members = sut.getOrgMembers(repo.Owner!);

                // Assert
                Assert.IsNotNull(repo.Name);
                Assert.IsNotNull(repo.FullName);
                Assert.IsNotNull(repo.Description);
                Assert.IsNotNull(repo.Owner);

                Assert.IsNotNull(release.Author);
                Assert.IsNotNull(release.Tag);
                Assert.IsNotNull(release.Assets?.FirstOrDefault());
                Assert.IsNotNull(release.Assets?.FirstOrDefault()?.Download);
                Assert.IsNotNull(release.SourceArchiveAsset);

                Assert.That(members.Count, Is.GreaterThanOrEqualTo(3));
                Assert.Contains("HebaruSan", members.Select(u => u.Login).ToArray());
            }
        }

        [TestCase("https://github.com/awesomemod/example/blob/master/AwesomeMod.netkan"),
         TestCase("https://github.com/awesomemod/example/tree/master/AwesomeMod.netkan"),
         TestCase("https://github.com/awesomemod/example/raw/master/AwesomeMod.netkan"),
         TestCase("https://raw.githubusercontent.com/awesomemod/example/master/AwesomeMod.netkan"),
        ]
        public void DownloadText_WithGithubUrl_UsesApiUrl(string url)
        {
            // Arrange
            var urls  = new HashSet<Uri>();
            var mHttp = new Mock<IHttpService>();
            mHttp.Setup(i => i.DownloadText(It.IsAny<Uri>(),
                                            It.IsAny<string?>(), It.IsAny<string?>()))
                 .Callback((Uri u, string _, string _) => urls.Add(u))
                 .Returns("Dummy API output");
            var sut = new GithubApi(mHttp.Object);

            // Act
            var result = sut.DownloadText(new Uri(url));

            // Assert
            Assert.AreEqual("Dummy API output", result);
            Assert.AreEqual("https://api.github.com/repos/awesomemod/example/contents/AwesomeMod.netkan?ref=master",
                            urls.Single().OriginalString);
        }
    }
}
