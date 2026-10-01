using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

using log4net;

namespace CKAN.IO
{
    // Passes ckan:// URLs to the running CKAN instance over a named pipe.
    // The receiver dispatches them via ProtocolRouter on a pipe thread.
    public static class URLPipe
    {
        // The user name suffix is needed to stop two accounts on one machine fighting over the same pipe.
        // Passed to the URL handler stub at registration time.
        public static string Name { get; internal set; } = $"CKAN_URL_PIPE_{Environment.UserName}";

        private static readonly ILog log = LogManager.GetLogger(typeof(URLPipe));
        private static CancellationTokenSource? cancelTokenSrc;
        private static Task? serverTask;

        // Returns false if another instance already owns the pipe.
        public static bool StartServer()
        {
            if (serverTask != null)
            {
                return true;
            }

            NamedPipeServerStream boundPipe;
            try
            {
                boundPipe = NewPipeServer();
            }
            catch (IOException ex)
            {
                log.WarnFormat("Could not bind URL pipe '{0}': {1}. This instance will not receive ckan:// URLs.", Name, ex.Message);
                return false;
            }

            cancelTokenSrc = new CancellationTokenSource();
            var token = cancelTokenSrc.Token;

            log.Debug("Starting URL pipe server task");
            serverTask = Task.Run(async () =>
            {
                using var server = boundPipe;
                while (true)
                {
                    try
                    {
                        await server.WaitForConnectionAsync(token);

                        using var reader = new StreamReader(server, System.Text.Encoding.UTF8, true, 1024, leaveOpen: true);
                        var url = await reader.ReadLineAsync();

                        log.DebugFormat("Pipe received URL: {0}", url);
                        ProtocolRouter.Handle(url);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    // URL could come from anywhere so a failure here must not kill the server.
                    catch (Exception ex)
                    {
                        log.WarnFormat("URL pipe error: {0}", ex.Message);
                    }
                    finally
                    {
                        if (server.IsConnected)
                        {
                            server.Disconnect();
                        }
                    }

                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                }
            });

            return true;
        }

        // PipeTransmissionMode.Byte is required for Unix compatibility.
        private static NamedPipeServerStream NewPipeServer()
            => new NamedPipeServerStream(Name, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

        public static async Task StopServer()
        {
            if (cancelTokenSrc == null)
            {
                return;
            }

            cancelTokenSrc.Cancel();

            // Mono on Linux ignores the Cancel token once WaitForConnectionAsync is waiting.
            // Therefore connect to complete the wait instead.
            try
            {
                using var client = new NamedPipeClientStream(".", Name, PipeDirection.Out);
                await client.ConnectAsync(50);
            }
            // The server is already gone if this times out, which is what we wanted anyway.
            catch
            { }

            try
            {
                if (serverTask != null)
                {
                    await serverTask;
                }
            }
            finally
            {
                cancelTokenSrc.Dispose();
                cancelTokenSrc = null;
                serverTask = null;
            }
        }

        // Forward a URL to the running instance. Returns true if delivered.
        // Duplicated in URLHandler/Program.cs (Windows only URL handler exe).
        public static bool TrySend(string url, int timeoutMs = 50)
        {
            using var client = new NamedPipeClientStream(".", Name, PipeDirection.Out);

            try
            {
                client.Connect(timeoutMs);
                using var writer = new StreamWriter(client) { AutoFlush = true };
                writer.WriteLine(url);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
