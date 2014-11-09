using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace LocalRepo
{
    public class Program
    {

        private static string repoPath = "";
        private static uint port = 8081;

        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("LocalRepo.exe <repository path> <port=8081>");
                return;
            }

            if (args.Length >= 2)
            {
                port = uint.Parse(args[1]);
            }

            repoPath = args[0];

            if (!Directory.Exists(repoPath))
            {
                Console.WriteLine("Error: {0} doesn't exist or isn't a directory", repoPath);
                return;
            }

            Console.WriteLine("Running async server on port {0}", port);
            Console.WriteLine("Serving metadata from {0}", repoPath);
            new AsyncServer(port);
        }

        public static void CreateZipFromRepo(string filename)
        {
            ZipOutputStream stream = new ZipOutputStream(File.Create(filename));

            foreach(var file in Directory.GetFiles(repoPath))
            {
                if (Path.GetExtension(file) == ".ckan")
                {
                    var fileInfo = new FileInfo(file);
                    ZipEntry entry = new ZipEntry(Path.GetFileName(file));
                    entry.Size = fileInfo.Length;
                    stream.PutNextEntry(entry);

                    byte[] buffer = new byte[4096];
                    using (FileStream streamReader = File.OpenRead(file))
                    {
                        StreamUtils.Copy(streamReader, stream, buffer);
                    }

                    stream.CloseEntry();
                }
            }

            stream.Close();
        }

    }

    public class AsyncServer
    {
        public AsyncServer(uint port)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add(String.Format("http://127.0.0.1:{0}/", port));

            listener.Start();

            while (true)
            {
                try
                {
                    HttpListenerContext context = listener.GetContext();
                    ThreadPool.QueueUserWorkItem(o => HandleRequest(context));
                }
                catch (Exception)
                {
                    // Ignored for this example
                }
            }
        }

        private void HandleRequest(object state)
        {
            try
            {
                var context = (HttpListenerContext) state;

                context.Response.StatusCode = 200;
                context.Response.SendChunked = true;
                context.Response.ContentType = "application/zip";

                var filename = Path.GetTempFileName();
                Program.CreateZipFromRepo(filename);
               
                var bytes = File.ReadAllBytes(filename);
                context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                context.Response.OutputStream.Close();

                File.Delete(filename);
            }
            catch (Exception)
            {
                // Client disconnected or some other error - ignored for this example
            }
        }
    }
}