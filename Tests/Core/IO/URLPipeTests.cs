using System;
using System.IO.Pipes;
using System.Threading;

using NUnit.Framework;

using CKAN.IO;

namespace Tests.Core.IO
{
    [TestFixture]
    public class URLPipeTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Parallel test processes and any running CKAN each need their own pipe.
            #if NET5_0_OR_GREATER
            URLPipe.Name = $"CKAN_URL_PIPE_TEST_{Environment.ProcessId}";
            #else
            URLPipe.Name = $"CKAN_URL_PIPE_TEST_{System.Diagnostics.Process.GetCurrentProcess().Id}";
            #endif
        }

        [TearDown]
        public void TearDown()
        {
            // A platform (apparently Unix) that might ignore the cancel token is picked up here.
            Assert.IsTrue(URLPipe.StopServer().Wait(5000), "StopServer did not finish");
        }

        [Test]
        public void TrySend_NoServer_ReturnsFalse()
        {
            Assert.IsFalse(URLPipe.TrySend("ckan://focus?mod=JNSQ"));
        }

        [Test]
        public void StartServer_CalledTwice_ReturnsTrue()
        {
            Assert.IsTrue(URLPipe.StartServer());
            Assert.IsTrue(URLPipe.StartServer());
        }

        [Test]
        public void StartServer_PipeAlreadyOwned_ReturnsFalse()
        {
            using var blocker = new NamedPipeServerStream(URLPipe.Name, PipeDirection.In, 1,
                                                          PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

            Assert.IsFalse(URLPipe.StartServer());
        }

        [Test]
        public void TrySend_ServerRunning_DispatchesUrl()
        {
            using var received = new ManualResetEventSlim();
            string? mod = null;
            Action<string> handler = v => { mod = v; received.Set(); };
            ProtocolRouter.OnFocus += handler;
            try
            {
                Assert.IsTrue(URLPipe.StartServer());

                // 500 ms because on Unix the server may not be accepting yet.
                Assert.IsTrue(URLPipe.TrySend("ckan://focus?mod=JNSQ", 500));
                Assert.IsTrue(received.Wait(2000));
                Assert.AreEqual("JNSQ", mod);

                // The server disconnects and re-connects after each URL. Send again to cover that.
                received.Reset();
                Assert.IsTrue(URLPipe.TrySend("ckan://focus?mod=Astrogator", 500));
                Assert.IsTrue(received.Wait(2000));
                Assert.AreEqual("Astrogator", mod);
            }
            finally
            {
                ProtocolRouter.OnFocus -= handler;
            }
        }

        [Test]
        public void StopServer_ThenStart_OwnsPipeAgain()
        {
            Assert.IsTrue(URLPipe.StartServer());
            URLPipe.StopServer().Wait();

            Assert.IsFalse(URLPipe.TrySend("ckan://focus?mod=JNSQ"));
            Assert.IsTrue(URLPipe.StartServer());
        }
    }
}
