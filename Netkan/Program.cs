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
using CKAN.Extensions;

namespace CKAN.NetKAN
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var Options = ProcessArgs(args);
            try
            {
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
                if (game == null)
                {
                    return ExitBadOpt;
                }

                if (!string.IsNullOrEmpty(Options.ValidateCkan))
                {
                    var ckan = new Metadata(JObject.Parse(File.ReadAllText(Options.ValidateCkan)));
                    var inf = new Inflator(Options.CacheDir,
                                           Options.OverwriteCache,
                                           Options.GitHubToken,
                                           Options.GitLabToken,
                                           Options.NetUserAgent,
                                           Options.PreRelease,
                                           game);
                    inf.ValidateCkan(ckan);
                    Console.WriteLine(QueueHandler.serializeCkan(
                        PropertySortTransformer.SortProperties(ckan)));
                    return ExitOk;
                }

                if (Options.Queues is string { Length: > 0 }
                    && Options.Queues.Split(new char[] { ',' }, 2)
                       //is [var input, var output]
                       is string[] array
                    && array.Length == 2
                    && array[0] is var input
                    && array[1] is var output)
                {
                    var qh = new QueueHandler(input,
                                              output,
                                              Options.CacheDir,
                                              Options.OutputDir,
                                              Options.OverwriteCache,
                                              Options.GitHubToken,
                                              Options.GitLabToken,
                                              Options.NetUserAgent,
                                              Options.PreRelease,
                                              game);
                    qh.Process();
                    return ExitOk;
                }

                if (Options.File != null)
                {
                    Log.InfoFormat("Transforming {0}", Options.File);

                    var netkans = ReadNetkans(Options);
                    Log.Info("Finished reading input");

                    var inf = new Inflator(Options.CacheDir,
                                           Options.OverwriteCache,
                                           Options.GitHubToken,
                                           Options.GitLabToken,
                                           Options.NetUserAgent,
                                           Options.PreRelease,
                                           game);
                    var ckans = inf.Inflate(
                            Options.File,
                            netkans,
                            new TransformOptions(
                                ParseReleases(Options.Releases),
                                ParseSkipReleases(Options.SkipReleases),
                                ParseHighestVersion(Options.HighestVersion),
                                ParseHighestVersion(Options.HighestVersionPrerelease),
                                netkans.First().Staged,
                                netkans.First().StagingReason))
                        .ToArray();
                    foreach (Metadata ckan in ckans)
                    {
                        WriteCkan(Options.OutputDir, ckan.AllJson);
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

        private static int? ParseReleases(string? val)
            => val == null  ? 1
             : val == "all" ? null
             : int.Parse(val);

        private static int? ParseSkipReleases(string? val)
            => string.IsNullOrWhiteSpace(val) ? null : int.Parse(val);

        private static ModuleVersion? ParseHighestVersion(string? val)
            => val == null ? null
                           : new ModuleVersion(val);

        private static CmdLineOptions ProcessArgs(string[] args)
        {
            if (args.Contains("--debugger"))
            {
                Debugger.Launch();
            }

            var Options = new CmdLineOptions();
            Parser.Default.ParseArgumentsStrict(args, Options);

            Logging.Initialize();

            LogManager.GetRepository().Threshold =
                  Options.Verbose ? Level.Info
                : Options.Debug   ? Level.Debug
                :                   Level.Warn;

            return Options;
        }

        private static Metadata[] ReadNetkans(CmdLineOptions Options)
        {
            if (!Options.File?.EndsWith(".netkan", StringComparison.OrdinalIgnoreCase)
                             ?? false)
            {
                Log.WarnFormat("Input is not a .netkan file");
            }

            return ArgContents(Options.NetUserAgent ?? Net.UserAgentString, Options.File)
                       .Select(ymap => new Metadata(ymap))
                       .ToArray();
        }

        private static YamlMappingNode[] ArgContents(string userAgent, string? arg)
            => arg == null
                ? Array.Empty<YamlMappingNode>()
                : Uri.IsWellFormedUriString(arg, UriKind.Absolute)
                   && Net.DownloadText(new Uri(arg), userAgent) is string s
                    ? YamlExtensions.Parse(s)
                    : YamlExtensions.Parse(File.OpenText(arg));

        internal static string CkanFileName(string? dirPath, JObject json)
            => Path.Combine(
                dirPath ?? ".",
                string.Format(
                    "{0}-{1}.ckan",
                    (string?)json["identifier"] ?? "",
                    ((string?)json["version"])?.Replace(':', '-') ?? ""));

        private static void WriteCkan(string? outputDir, JObject json)
        {
            var finalPath = CkanFileName(outputDir, json);

            using (var swriter = new StringWriter(new StringBuilder()))
            using (var jwriter = new JsonTextWriter(swriter))
            {
                jwriter.Formatting = Formatting.Indented;
                jwriter.Indentation = 4;
                jwriter.IndentChar = ' ';

                var serializer = new JsonSerializer();
                serializer.Serialize(jwriter, json);

                File.WriteAllText(finalPath, swriter + Environment.NewLine);
            }
            Log.InfoFormat("Transformation written to {0}", finalPath);
        }

        private const int ExitOk = 0;
        private const int ExitBadOpt = 1;
        private const int ExitError = 2;

        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));
    }
}
