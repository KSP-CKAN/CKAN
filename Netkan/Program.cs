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
using YamlDotNet.RepresentationModel;

using CKAN.Games;
using CKAN.Versioning;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Processors;
using CKAN.NetKAN.Transformers;
using CKAN.NetKAN.Extensions;

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

                var game = KnownGames.GameByShortName(Options.Game);

                if (!string.IsNullOrEmpty(Options.ValidateCkan))
                {
                    var ckan = new Metadata(JObject.Parse(File.ReadAllText(Options.ValidateCkan)));
                    var inf = new Inflator(
                        Options.CacheDir,
                        Options.OverwriteCache,
                        Options.GitHubToken,
                        Options.GitLabToken,
                        Options.PreRelease,
                        game
                    );
                    inf.ValidateCkan(ckan);
                    Console.WriteLine(QueueHandler.serializeCkan(
                        PropertySortTransformer.SortProperties(ckan)));
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
                        Options.GitLabToken,
                        Options.PreRelease,
                        game
                    );
                    qh.Process();
                    return ExitOk;
                }

                if (Options.File != null)
                {
                    Log.InfoFormat("Transforming {0}", Options.File);

                    var netkans = ReadNetkans();
                    Log.Info("Finished reading input");

                    var inf = new Inflator(
                        Options.CacheDir,
                        Options.OverwriteCache,
                        Options.GitHubToken,
                        Options.GitLabToken,
                        Options.PreRelease,
                        game
                    );
                    var ckans = inf.Inflate(
                            Options.File,
                            netkans,
                            new TransformOptions(
                                ParseReleases(Options.Releases),
                                ParseSkipReleases(Options.SkipReleases),
                                ParseHighestVersion(Options.HighestVersion),
                                netkans[0].Staged,
                                netkans[0].StagingReason))
                        .ToArray();
                    foreach (Metadata ckan in ckans)
                    {
                        WriteCkan(ckan.Json());
                    }
                }
                else
                {
                    Log.Fatal(
                        "Usage: netkan [--verbose|--debug] [--debugger] [--prerelease] [--outputdir=...] <filename|URL>"
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

            LogManager.GetRepository().Threshold =
                  Options.Verbose ? Level.Info
                : Options.Debug   ? Level.Debug
                :                   Level.Warn;

            if (Options.NetUserAgent != null)
            {
                Net.UserAgentString = Options.NetUserAgent;
            }
        }

        private static Metadata[] ReadNetkans()
        {
            if (!Options.File.EndsWith(".netkan"))
            {
                Log.WarnFormat("Input is not a .netkan file");
            }

            return ArgContents(Options.File).Select(ymap => new Metadata(ymap))
                                            .ToArray();
        }

        private static YamlMappingNode[] ArgContents(string arg)
            => Uri.IsWellFormedUriString(arg, UriKind.Absolute)
                ? YamlExtensions.Parse(Net.DownloadText(new Uri(arg)))
                : YamlExtensions.Parse(File.OpenText(arg));

        internal static string CkanFileName(JObject json)
            => Path.Combine(
                Options.OutputDir,
                string.Format(
                    "{0}-{1}.ckan",
                    (string)json["identifier"],
                    ((string)json["version"]).Replace(':', '-')));

        private static void WriteCkan(JObject json)
        {
            var finalPath = CkanFileName(json);

            var sb = new StringBuilder();
            var sw = new StringWriter(sb);

            using (var writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;
                writer.IndentChar = ' ';

                var serializer = new JsonSerializer();
                serializer.Serialize(writer, json);
            }

            File.WriteAllText(finalPath, sw + Environment.NewLine);

            Log.InfoFormat("Transformation written to {0}", finalPath);
        }

    }
}
