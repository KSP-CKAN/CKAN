using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using CommandLine;
using CommandLine.Text;
using log4net;
using log4net.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using CKAN.Versioning;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Processors;
using CKAN.NetKAN.Transformers;
using CKAN.NetKAN.Extensions;

namespace CKAN.NetKAN
{
    public static class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        private static CmdLineOptions Options { get; set; }

        public static int Main(string[] args)
        {
            var parser = new Parser(c => c.HelpWriter = null).ParseArguments<CmdLineOptions>(args);
            return parser.MapResult(opts => Run(opts), errs =>
            {
                if (errs.IsVersion())
                {
                    Console.WriteLine(Meta.GetVersion(VersionFormat.Full));
                }
                else
                {
                    var ht = HelpText.AutoBuild(parser, h =>
                    {
                        h.AddDashesToOption = true;                                     // Add dashes to options
                        h.AddNewLineBetweenHelpSections = true;                         // Add blank line between heading and usage
                        h.AutoHelp = false;                                             // Hide built-in help option
                        h.AutoVersion = false;                                          // Hide built-in version option
                        h.Heading = $"NetKAN {Meta.GetVersion(VersionFormat.Full)}";    // Create custom heading
                        h.Copyright = $"Copyright © 2014-{DateTime.Now.Year}";          // Create custom copyright
                        h.AddPreOptionsLine("USAGE:\n  netkan <filename> [options]"); // Show usage
                        return HelpText.DefaultParsingErrorsHandler(parser, h);
                    }, e => e, true);
                    Console.WriteLine(ht);
                }

                return Exit.Ok;
            });
        }

        private static int Run(CmdLineOptions options)
        {
            Options = options;
            try
            {
                if (Options.Debugger)
                {
                    Debugger.Launch();
                }

                Logging.Initialize();

                LogManager.GetRepository().Threshold =
                    Options.Verbose ? Level.Info
                    : Options.Debug ? Level.Debug
                    : Level.Warn;

                if (Options.NetUserAgent != null)
                {
                    Net.UserAgentString = Options.NetUserAgent;
                }

                // Force-allow TLS 1.2 for HTTPS URLs, because GitHub requires it.
                // This is on by default in .NET 4.6, but not in 4.5.
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

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
                    Console.WriteLine(QueueHandler.serializeCkan(new PropertySortTransformer().Transform(ckan, null).First()));
                    return Exit.Ok;
                }

                if (!string.IsNullOrEmpty(Options.Queues))
                {
                    var queues = Options.Queues.Split(new[] { ',' }, 2);
                    var qh = new QueueHandler(
                        queues[0],
                        queues[1],
                        Options.CacheDir,
                        Options.OverwriteCache,
                        Options.GitHubToken,
                        Options.PreRelease
                    );
                    qh.Process();
                    return Exit.Ok;
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
                    Console.WriteLine("There was no file provided, maybe you forgot it?\n\nUSAGE:\n  netkan <filename> [options]");
                    return Exit.BadOpt;
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

                return Exit.Error;
            }

            return Exit.Ok;
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

        private static Metadata ReadNetkan()
        {
            if (!Options.File.EndsWith(".netkan"))
            {
                Log.WarnFormat("Input is not a .netkan file");
            }

            return new Metadata(YamlExtensions.Parse(File.OpenText(Options.File)));
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
