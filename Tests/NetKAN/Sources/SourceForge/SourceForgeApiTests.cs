using System;
using System.Linq;

using NUnit.Framework;
using Moq;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Sources.SourceForge;
using CKAN.NetKAN.Services;

namespace Tests.NetKAN.Sources.SourceForge
{
    [TestFixture]
    public sealed class SourceForgeApiTests
    {
        [Test]
        public void GetMod_KSRe_Works()
        {
            // Arrange
            var http = new Mock<IHttpService>();
            http.Setup(h => h.DownloadText(It.IsAny<Uri>(),
                                           It.IsAny<string?>(),
                                           It.IsAny<string?>()))
                .Returns(@"<?xml version=""1.0"" encoding=""utf-8""?>
                    <rss xmlns:content=""http://purl.org/rss/1.0/modules/content/"" xmlns:files=""https://sourceforge.net/api/files.rdf#"" xmlns:media=""http://video.search.yahoo.com/mrss/"" xmlns:doap=""http://usefulinc.com/ns/doap#"" xmlns:sf=""https://sourceforge.net/api/sfelements.rdf#"" version=""2.0"">
                      <channel xmlns:files=""https://sourceforge.net/api/files.rdf#"" xmlns:media=""http://video.search.yahoo.com/mrss/"" xmlns:doap=""http://usefulinc.com/ns/doap#"" xmlns:sf=""https://sourceforge.net/api/sfelements.rdf#"">
                        <title>KSRe (KSP1)</title>
                        <link>https://sourceforge.net</link>
                        <description><![CDATA[Files from KSRe (KSP1) The KSP1 version of KSRe is a similar overhaul mod that contains earlier iterations of design ideas. See the Files tab for a full list of features and dependencies]]></description>
                        <pubDate>Tue, 07 Oct 2025 14:07:54 UT</pubDate>
                        <managingEditor>noreply@sourceforge.net (SourceForge.net)</managingEditor>
                        <docs>http://blogs.law.harvard.edu/tech/rss</docs>
                        <item>
                          <title><![CDATA[/README.md]]></title>
                          <link>https://sourceforge.net/projects/ksp1.ksre.p/files/README.md/download</link>
                          <guid>https://sourceforge.net/projects/ksp1.ksre.p/files/README.md/download</guid>
                          <pubDate>Fri, 16 Aug 2024 10:27:33 UT</pubDate>
                          <files:sf-file-id xmlns:files=""https://sourceforge.net/api/files.rdf#"">53294502</files:sf-file-id>
                          <files:extra-info xmlns:files=""https://sourceforge.net/api/files.rdf#"">text</files:extra-info>
                          <media:content xmlns:media=""http://video.search.yahoo.com/mrss/"" type=""text/plain; charset=us-ascii"" url=""https://sourceforge.net/projects/ksp1.ksre.p/files/README.md/download"" filesize=""5808""><media:hash algo=""md5"">d9d509a29952cf113a804f532b9b0576</media:hash></media:content>
                        </item>
                        <item>
                          <title><![CDATA[/KSRe_beta_0_13.zip]]></title>
                          <link>https://sourceforge.net/projects/ksp1.ksre.p/files/KSRe_beta_0_13.zip/download</link>
                          <guid>https://sourceforge.net/projects/ksp1.ksre.p/files/KSRe_beta_0_13.zip/download</guid>
                          <pubDate>Fri, 16 Aug 2024 09:51:47 UT</pubDate>
                          <files:sf-file-id xmlns:files=""https://sourceforge.net/api/files.rdf#"">53291185</files:sf-file-id>
                          <files:extra-info xmlns:files=""https://sourceforge.net/api/files.rdf#"">empty (Zip archive data)</files:extra-info>
                          <media:content xmlns:media=""http://video.search.yahoo.com/mrss/"" type=""application/zip; charset=binary"" url=""https://sourceforge.net/projects/ksp1.ksre.p/files/KSRe_beta_0_13.zip/download"" filesize=""1940144""><media:hash algo=""md5"">fdbcb01e70a9a2bdc4f5808c8873de58</media:hash></media:content>
                        </item>
                      </channel>
                    </rss>");
            var sut            = new SourceForgeApi(http.Object);
            var sourceForgeRef = new SourceForgeRef(new RemoteRef("#/ckan/sourceforge/ksp1.ksre.p"));

            // Act
            var mod = sut.GetMod(sourceForgeRef);

            // Assert
            Assert.IsNotNull(mod.Title);
            Assert.IsNotNull(mod.Description);
            Assert.IsNotNull(mod.HomepageLink);
            Assert.IsNotNull(mod.RepositoryLink);
            Assert.That(mod.Versions.Length, Is.GreaterThanOrEqualTo(1));
            var first = mod.Versions.First();
            Assert.IsNotNull(first.Title);
            Assert.IsNotNull(first.Timestamp);
            Assert.IsNotNull(first.Link);
        }
    }
}
