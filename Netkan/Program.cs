using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using CommandLine;
using log4net;
using log4net.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CKAN.Versioning;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Processors;
using CKAN.NetKAN.Transformers;

namespace CKAN.NetKAN
{
    public static class Program
    {
        private const int ExitOk = 0;
        private const int ExitBadOpt = 1;
        private const int ExitError = 2;

        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        private static CmdLineOptions Options { get; set; }

        public static int Main(string[] args)
        {
            try
            {
                ProcessArgs(args);

                // Force-allow TLS 1.2 for HTTPS URLs, because GitHub requires it.
                // This is on by default in .NET 4.6, but not in 4.5.
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                // If we see the --version flag, then display our build info
                // and exit.
                if (Options.Version)
                {
                    Console.WriteLine(Meta.GetVersion(VersionFormat.Full));
                    return ExitOk;
                }

                if (!string.IsNullOrEmpty(Options.ValidateCkan))
                {
                    var ckan = new Metadata(JObject.Parse(File.ReadAllText(Options.ValidateCkan)));
                    var inf = new Inflator(
                        Options.CacheDir,
                        Options.OverwriteCache,
                        Options.GitHubToken,
                        Options.PreRelease
                    );
                    inf.ValidateCkan(ckan);
                    Console.WriteLine(QueueHandler.serializeCkan(
                        new PropertySortTransformer().Transform(ckan, null).First()
                    ));
                    return ExitOk;
                }

                if (!string.IsNullOrEmpty(Options.Queues))
                {
                    var queues = Options.Queues.Split(new char[] { ',' }, 2);
                    var qh = new QueueHandler(
                        queues[0],
                        queues[1],
                        Options.CacheDir,
                        Options.OverwriteCache,
                        Options.GitHubToken,
                        Options.PreRelease
                    );
                    qh.Process();
                    return ExitOk;
                }

                if (Options.File != null)
                {
                    Log.InfoFormat("Transforming {0}", Options.File);

                    var netkan = ReadNetkan();
                    Log.Info("Finished reading input");

                    var inf = new Inflator(
                        Options.CacheDir,
                        Options.OverwriteCache,
                        Options.GitHubToken,
                        Options.PreRelease
                    );
                    var ckans = inf.Inflate(
                        Options.File,
                        netkan,
                        new TransformOptions(
                            ParseReleases(Options.Releases),
                            ParseSkipReleases(Options.SkipReleases),
                            ParseHighestVersion(Options.HighestVersion)
                        )
                    );
                    foreach (Metadata ckan in ckans)
                    {
                        WriteCkan(ckan);
                    }
                }
                else
                {
                    Log.Fatal(
                        "Usage: netkan [--verbose|--debug] [--debugger] [--prerelease] [--outputdir=...] <filename>"
                    );
                    return ExitBadOpt;
                }
            }
            catch (Exception e)
            {
                e = e.GetBaseException();

                Log.Fatal(e.Message);

                if (Options == null || Options.Debug)
                {
                    Log.Fatal(e.StackTrace);
                }

                return ExitError;
            }

            return ExitOk;
        }

        private static int? ParseReleases(string val)
        {
            return val == "all" ? (int?)null : int.Parse(val);
        }

        private static int? ParseSkipReleases(string val)
        {
            return string.IsNullOrWhiteSpace(val) ? (int?)null : int.Parse(val);
        }

        private static ModuleVersion ParseHighestVersion(string val)
        {
            return val == null ? null : new ModuleVersion(val);
        }

        private static void ProcessArgs(string[] args)
        {
            if (args.Any(i => i == "--debugger"))
            {
                Debugger.Launch();
            }

            Options = new CmdLineOptions();
            Parser.Default.ParseArgumentsStrict(args, Options);

            Logging.Initialize();
            LogManager.GetRepository().Threshold = Level.Warn;

            if (Options.Verbose)
            {
                LogManager.GetRepository().Threshold = Level.Info;
            }
            else if (Options.Debug)
            {
                LogManager.GetRepository().Threshold = Level.Debug;
            }

            if (Options.NetUserAgent != null)
            {
                Net.UserAgentString = Options.NetUserAgent;
            }
        }

        private static Metadata ReadNetkan()
        {
            if (!Options.File.EndsWith(".netkan"))
            {
                Log.WarnFormat("Input is not a .netkan file");
            }

            return new Metadata(JObject.Parse(File.ReadAllText(Options.File)));
        }

        internal static string CkanFileName(Metadata metadata)
        {
            return Path.Combine(
                Options.OutputDir,
                string.Format(
                    "{0}-{1}.ckan",
                    metadata.Identifier,
                    metadata.Version.ToString().Replace(':', '-')
                )
            );
        }

        private static void WriteCkan(Metadata metadata)
        {
            var finalPath = CkanFileName(metadata);

            var sb = new StringBuilder();
            var sw = new StringWriter(sb);

            using (var writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;
                writer.IndentChar = ' ';

                var serializer = new JsonSerializer();
                serializer.Serialize(writer, metadata.Json());
            }

            File.WriteAllText(finalPath, sw + Environment.NewLine);

            Log.InfoFormat("Transformation written to {0}", finalPath);
        }

    }
}
