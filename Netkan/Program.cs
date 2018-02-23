using System;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Transformers;
using CKAN.NetKAN.Validators;
using CommandLine;
using log4net;
using log4net.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

                if (Options.File != null)
                {
                    Log.InfoFormat("Transforming {0}", Options.File);

                    var moduleService = new ModuleService();
                    var fileService = new FileService();
                    var http = new CachingHttpService(FindCache(new KSPManager(new ConsoleUser(false))));

                    var netkan = ReadNetkan();
                    Log.Info("Finished reading input");

                    new NetkanValidator().Validate(netkan);
                    Log.Info("Input successfully passed pre-validation");

                    var transformer = new NetkanTransformer(
                        http,
                        fileService,
                        moduleService,
                        Options.GitHubToken,
                        Options.PreRelease
                    );

                    var ckan = transformer.Transform(netkan);
                    Log.Info("Finished transformation");

                    new CkanValidator(netkan, http, moduleService).Validate(ckan);
                    Log.Info("Output successfully passed post-validation");

                    WriteCkan(ckan);
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
                Log.Fatal(e.Message);

                if (Options == null || Options.Debug)
                {
                    Log.Fatal(e.StackTrace);
                }

                return ExitError;
            }

            return ExitOk;
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

        private static NetFileCache FindCache(KSPManager kspManager)
        {
            if (Options.CacheDir != null)
            {
                Log.InfoFormat("Using user-supplied cache at {0}", Options.CacheDir);
                return new NetFileCache(Options.CacheDir);
            }

            try
            {
                var ksp = kspManager.GetPreferredInstance();
                Log.InfoFormat("Using CKAN cache at {0}", ksp.Cache.GetCachePath());
                /// Create a new file cache in the same location so NetKAN can download pure URLs not sourced from CkanModules
                return new NetFileCache(ksp.Cache.GetCachePath());
            }
            catch
            {
                // Meh, can't find KSP. 'Scool, bro.
            }

            var tempdir = Path.GetTempPath();
            Log.InfoFormat("Using tempdir for cache: {0}", tempdir);

            return new NetFileCache(tempdir);
        }

        private static Metadata ReadNetkan()
        {
            if (!Options.File.EndsWith(".netkan"))
            {
                Log.WarnFormat("Input is not a .netkan file");
            }

            return new Metadata(JObject.Parse(File.ReadAllText(Options.File)));
        }

        private static void WriteCkan(Metadata metadata)
        {
            var versionFilename = metadata.Version.ToString().Replace(':', '-');
            var finalPath = Path.Combine(
                Options.OutputDir,
                string.Format("{0}-{1}.ckan", metadata.Identifier, versionFilename)
            );

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
