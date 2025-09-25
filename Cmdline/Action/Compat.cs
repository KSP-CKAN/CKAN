using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using CommandLine;
using CommandLine.Text;

using CKAN.Versioning;

namespace CKAN.CmdLine
{
    [ExcludeFromCodeCoverage]
    public class CompatSubOptions : VerbCommandOptions
    {
        [VerbOption("list", HelpText = "List compatible game versions")]
        public CompatListOptions? List { get; set; }

        [VerbOption("clear", HelpText = "Forget all compatible game versions")]
        public CompatClearOptions? Clear { get; set; }

        [VerbOption("add", HelpText = "Add versions to compatible game versions list")]
        public CompatAddOptions? Add { get; set; }

        [VerbOption("forget", HelpText = "Forget compatible game versions")]
        public CompatForgetOptions? Forget { get; set; }

        [VerbOption("set", HelpText = "Set the compatible game versions list")]
        public CompatSetOptions? Set { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            var ht = HelpText.AutoBuild(this, verb);
            foreach (var h in GetHelp(verb))
            {
                ht.AddPreOptionsLine(h);
            }
            return ht;
        }

        [ExcludeFromCodeCoverage]
        public static IEnumerable<string> GetHelp(string verb)
        {
            // Add a usage prefix line
            yield return " ";
            if (string.IsNullOrEmpty(verb))
            {
                yield return $"ckan compat - {Properties.Resources.CompatHelpSummary}";
                yield return $"{Properties.Resources.Usage}: ckan compat <{Properties.Resources.Command}> [{Properties.Resources.Options}]";
            }
            else
            {
                yield return "compat " + verb + " - " + GetDescription(typeof(CompatSubOptions), verb);
                switch (verb)
                {
                    // First the commands with one string argument
                    case "add":
                    case "set":
                    case "forget":
                        yield return $"{Properties.Resources.Usage}: ckan compat {verb} [{Properties.Resources.Options}] version [version2 ...]";
                        break;

                    // Now the commands with only --flag type options
                    case "list":
                    case "clear":
                    default:
                        yield return $"{Properties.Resources.Usage}: ckan compat {verb} [{Properties.Resources.Options}]";
                        break;
                }
            }
        }
    }

    public class CompatListOptions : InstanceSpecificOptions { }

    public class CompatClearOptions : InstanceSpecificOptions { }

    public class CompatAddOptions : InstanceSpecificOptions
    {
        [ValueList(typeof(List<string>))]
        public List<string>? Versions { get; set; }
    }

    public class CompatForgetOptions : InstanceSpecificOptions
    {
        [ValueList(typeof(List<string>))]
        public List<string>? Versions { get; set; }
    }

    public class CompatSetOptions : InstanceSpecificOptions
    {
        [ValueList(typeof(List<string>))]
        public List<string>? Versions { get; set; }
    }

    public class Compat : ISubCommand
    {
        public Compat(GameInstanceManager mgr,
                      IUser               user)
        {
            manager   = mgr;
            this.user = user;
        }

        public int RunSubCommand(CommonOptions?    opts,
                                 SubCommandOptions options)
        {
            var exitCode = Exit.OK;

            Parser.Default.ParseArgumentsStrict(options.options.ToArray(), new CompatSubOptions(), (option, suboptions) =>
            {
                // ParseArgumentsStrict calls us unconditionally, even with bad arguments
                if (!string.IsNullOrEmpty(option) && suboptions != null)
                {
                    CommonOptions comOpts = (CommonOptions)suboptions;
                    comOpts.Merge(opts);
                    exitCode = comOpts.Handle(manager, user);
                    if (exitCode == Exit.OK)
                    {
                        exitCode = suboptions switch
                        {
                            CompatListOptions        => List(MainClass.GetGameInstance(manager)),
                            CompatClearOptions       => Clear(MainClass.GetGameInstance(manager)),
                            CompatAddOptions    opts => Add(opts, MainClass.GetGameInstance(manager)),
                            CompatForgetOptions opts => Forget(opts, MainClass.GetGameInstance(manager)),
                            CompatSetOptions    opts => Set(opts, MainClass.GetGameInstance(manager)),
                            _                        => Exit.ERROR,
                        };
                    }
                }
            }, () => { exitCode = MainClass.AfterHelp(user); });
            return exitCode;
        }

        private int List(CKAN.GameInstance instance)
        {
            var versionHeader = Properties.Resources.CompatVersionHeader;
            var actualHeader  = Properties.Resources.CompatActualHeader;
            var output = (instance.Version() is GameVersion v
                              ? Enumerable.Repeat((Version: v,
                                                   Actual:  true),
                                           1)
                              : Enumerable.Empty<(GameVersion Version, bool Actual)>())
                .Concat(instance.CompatibleVersions
                                .OrderDescending()
                                .Select(v => (Version: v,
                                              Actual:  false)))
                .ToList();

            var versionWidth = Enumerable.Repeat(versionHeader, 1)
                .Concat(output.Select(i => i.Version.ToString()))
                .Max(i => i?.Length ?? 0);

            var actualWidth = Enumerable.Repeat(actualHeader, 1)
                .Concat(output.Select(i => i.Actual.ToString()))
                .Max(i => i?.Length ?? 0);

            const string columnFormat = "{0}  {1}";

            user.RaiseMessage(columnFormat,
                              versionHeader.PadRight(versionWidth),
                              actualHeader.PadRight(actualWidth));

            user.RaiseMessage(columnFormat,
                              new string('-', versionWidth),
                              new string('-', actualWidth));

            foreach (var line in output)
            {
                user.RaiseMessage(columnFormat,
                                  (line.Version.ToString() ?? "")
                                               .PadRight(versionWidth),
                                  line.Actual.ToString()
                                             .PadRight(actualWidth));
            }
            return Exit.OK;
        }

        private int Clear(CKAN.GameInstance instance)
        {
            instance.SetCompatibleVersions(new List<GameVersion>());
            List(instance);
            return Exit.OK;
        }

        private int Add(CompatAddOptions  addOptions,
                        CKAN.GameInstance instance)
        {
            if (addOptions.Versions?.Count < 1)
            {
                user.RaiseError(Properties.Resources.CompatMissing);
                PrintUsage("add");
                return Exit.BADOPT;
            }
            if (!TryParseVersions(addOptions.Versions,
                                  out GameVersion[] goodVers,
                                  out string[]      badVers))
            {
                user.RaiseError(Properties.Resources.CompatInvalid,
                                string.Join(", ", badVers));
                return Exit.BADOPT;
            }
            instance.SetCompatibleVersions(instance.CompatibleVersions
                                                   .Concat(goodVers)
                                                   .Distinct()
                                                   .ToList());
            List(instance);
            return Exit.OK;
        }

        private int Forget(CompatForgetOptions forgetOptions,
                           CKAN.GameInstance   instance)
        {
            if (forgetOptions.Versions?.Count < 1)
            {
                user.RaiseError(Properties.Resources.CompatMissing);
                PrintUsage("forget");
                return Exit.BADOPT;
            }
            if (!TryParseVersions(forgetOptions.Versions,
                                  out GameVersion[] goodVers,
                                  out string[]      badVers))
            {
                user.RaiseError(Properties.Resources.CompatInvalid,
                                string.Join(", ", badVers));
                return Exit.BADOPT;
            }
            var rmActualVers = goodVers.Intersect(instance.Version() is GameVersion gv
                                                      ? new GameVersion[] { gv, gv.WithoutBuild }
                                                      : Array.Empty<GameVersion>())
                                       .Select(gv => gv.ToString())
                                       .ToArray();
            if (rmActualVers.Length > 0)
            {
                user.RaiseError(Properties.Resources.CompatCantForget,
                                string.Join(", ", rmActualVers));
                return Exit.ERROR;
            }
            instance.SetCompatibleVersions(instance.CompatibleVersions
                                                   .Except(goodVers)
                                                   .ToList());
            List(instance);
            return Exit.OK;
        }

        private int Set(CompatSetOptions  setOptions,
                        CKAN.GameInstance instance)
        {
            if (setOptions.Versions?.Count < 1)
            {
                user.RaiseError(Properties.Resources.CompatMissing);
                PrintUsage("set");
                return Exit.BADOPT;
            }
            if (!TryParseVersions(setOptions.Versions,
                                  out GameVersion[] goodVers,
                                  out string[]      badVers))
            {
                user.RaiseError(Properties.Resources.CompatInvalid,
                                string.Join(", ", badVers));
                return Exit.BADOPT;
            }
            instance.SetCompatibleVersions(goodVers.Distinct().ToList());
            List(instance);
            return Exit.OK;
        }

        [ExcludeFromCodeCoverage]
        private void PrintUsage(string verb)
        {
            foreach (var h in CompatSubOptions.GetHelp(verb))
            {
                user.RaiseError("{0}", h);
            }
        }

        private static bool TryParseVersions(IEnumerable<string>? versions,
                                             out GameVersion[]    goodVers,
                                             out string[]         badVers)
        {
            var gameVersions = (versions ?? Enumerable.Empty<string>())
                .Select(v => GameVersion.TryParse(v, out GameVersion? gv)
                                 ? new Tuple<string, GameVersion?>(v, gv)
                                 : new Tuple<string, GameVersion?>(v, null))
                .ToArray();
            goodVers = gameVersions.Select(tuple => tuple.Item2)
                                   .OfType<GameVersion>()
                                   .ToArray();
            badVers = gameVersions.Where(tuple => tuple.Item2 == null)
                                  .Select(tuple => tuple.Item1)
                                  .ToArray();
            return badVers.Length < 1;
        }

        private readonly IUser               user;
        private readonly GameInstanceManager manager;
    }
}
