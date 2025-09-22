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
                    if (exitCode != Exit.OK)
                    {
                        return;
                    }

                    switch (option)
                    {
                        case "list":
                            exitCode = List(MainClass.GetGameInstance(manager))
                                           ? Exit.OK
                                           : Exit.ERROR;
                            break;

                        case "clear":
                            exitCode = Clear(MainClass.GetGameInstance(manager))
                                           ? Exit.OK
                                           : Exit.ERROR;
                            break;

                        case "add":
                            exitCode = Add((CompatAddOptions)suboptions,
                                           MainClass.GetGameInstance(manager))
                                           ? Exit.OK
                                           : Exit.ERROR;
                            break;

                        case "forget":
                            exitCode = Forget((CompatForgetOptions)suboptions,
                                              MainClass.GetGameInstance(manager))
                                           ? Exit.OK
                                           : Exit.ERROR;
                            break;

                        case "set":
                            exitCode = Set((CompatSetOptions)suboptions,
                                           MainClass.GetGameInstance(manager))
                                           ? Exit.OK
                                           : Exit.ERROR;
                            break;

                        default:
                            exitCode = Exit.BADOPT;
                            break;
                    }
                }
            }, () => { exitCode = MainClass.AfterHelp(user); });
            return exitCode;
        }

        private bool List(CKAN.GameInstance instance)
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
            return true;
        }

        private bool Clear(CKAN.GameInstance instance)
        {
            instance.SetCompatibleVersions(new List<GameVersion>());
            List(instance);
            return true;
        }

        private bool Add(CompatAddOptions  addOptions,
                         CKAN.GameInstance instance)
        {
            if (addOptions.Versions?.Count < 1)
            {
                user.RaiseError(Properties.Resources.CompatMissing);
                PrintUsage("add");
                return false;
            }
            if (!TryParseVersions(addOptions.Versions,
                                  out GameVersion[] goodVers,
                                  out string[]      badVers))
            {
                user.RaiseError(Properties.Resources.CompatInvalid,
                                string.Join(", ", badVers));
                return false;
            }
            instance.SetCompatibleVersions(instance.CompatibleVersions
                                                   .Concat(goodVers)
                                                   .Distinct()
                                                   .ToList());
            List(instance);
            return true;
        }

        private bool Forget(CompatForgetOptions forgetOptions,
                            CKAN.GameInstance   instance)
        {
            if (forgetOptions.Versions?.Count < 1)
            {
                user.RaiseError(Properties.Resources.CompatMissing);
                PrintUsage("forget");
                return false;
            }
            if (!TryParseVersions(forgetOptions.Versions,
                                  out GameVersion[] goodVers,
                                  out string[]      badVers))
            {
                user.RaiseError(Properties.Resources.CompatInvalid,
                                string.Join(", ", badVers));
                return false;
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
                return false;
            }
            instance.SetCompatibleVersions(instance.CompatibleVersions
                                                   .Except(goodVers)
                                                   .ToList());
            List(instance);
            return true;
        }

        private bool Set(CompatSetOptions  setOptions,
                         CKAN.GameInstance instance)
        {
            if (setOptions.Versions?.Count < 1)
            {
                user.RaiseError(Properties.Resources.CompatMissing);
                PrintUsage("set");
                return false;
            }
            if (!TryParseVersions(setOptions.Versions,
                                  out GameVersion[] goodVers,
                                  out string[]      badVers))
            {
                user.RaiseError(Properties.Resources.CompatInvalid,
                                string.Join(", ", badVers));
                return false;
            }
            instance.SetCompatibleVersions(goodVers.Distinct().ToList());
            List(instance);
            return true;
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
