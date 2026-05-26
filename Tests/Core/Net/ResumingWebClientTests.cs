using System;
using System.IO;
using System.Net;
using System.Threading;

using NUnit.Framework;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

using CKAN;
using Tests.Data;

namespace Tests.Core.Net
{
    [TestFixture]
    public class ResumingWebClientTests
    {
        [Test]
        public void DownloadFileAsyncWithResume_RequestedRangeNotSatisfiable_FileUnchanged()
        {
            // Arrange
            using (var tempDir = new TemporaryDirectory())
            using (var server  = WireMockServer.Start())
            {
                server.Given(Request.Create()
                                    .WithPath("/")
                                    .UsingGet())
                      .RespondWith(Response.Create()
                                           .WithStatusCode(HttpStatusCode.RequestedRangeNotSatisfiable)
                                           .WithHeader("Content-Type", "text/plain")
                                           .WithBody("Error page from server"));
                var path = Path.Combine(tempDir, "target.zip");
                File.Copy(TestData.DogeCoinFlagZip(), path);
                bool done = false;
                var sut = new ResumingWebClient();
                sut.DownloadFileCompleted += (_, _) => done = true;

                // Act
                sut.DownloadFileAsyncWithResume(new Uri(server.Url!), path,
                                                new FileInfo(path).Length, null);
                while (!done)
                {
                    Thread.Sleep(100);
                }

                // Assert
                FileAssert.AreEqual(TestData.DogeCoinFlagZip(), path);
            }
        }
    }
}
