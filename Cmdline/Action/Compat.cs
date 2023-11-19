using System.Linq;

using CommandLine;
using CommandLine.Text;

using CKAN.Versioning;

namespace CKAN.CmdLine
{
    public class CompatOptions : VerbCommandOptions
    {
        [VerbOption("list", HelpText = "List compatible KSP versions")]
        public CompatListOptions List { get; set; }

        [VerbOption("add", HelpText = "Add version to KSP compatibility list")]
        public CompatAddOptions Add { get; set; }

        [VerbOption("forget", HelpText = "Forget version on KSP compatibility list")]
        public CompatForgetOptions Forget { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            HelpText ht = HelpText.AutoBuild(this, verb);
            // Add a usage prefix line
            ht.AddPreOptionsLine(" ");
            if (string.IsNullOrEmpty(verb))
            {
                ht.AddPreOptionsLine($"ckan compat - {Properties.Resources.CompatHelpSummary}");
                ht.AddPreOptionsLine($"{Properties.Resources.Usage}: ckan compat <{Properties.Resources.Command}> [{Properties.Resources.Options}]");
            }
            else
            {
                ht.AddPreOptionsLine("compat " + verb + " - " + GetDescription(verb));
                switch (verb)
                {
                    // First the commands with one string argument
                    case "add":
                    case "forget":
                        ht.AddPreOptionsLine($"{Properties.Resources.Usage}: ckan compat {verb} [{Properties.Resources.Options}] version");
                        break;

                    // Now the commands with only --flag type options
                    case "list":
                    default:
                        ht.AddPreOptionsLine($"{Properties.Resources.Usage}: ckan compat {verb} [{Properties.Resources.Options}]");
                        break;
                }
            }
            return ht;
        }
    }

    public class CompatListOptions : InstanceSpecificOptions { }

    public class CompatAddOptions : InstanceSpecificOptions
    {
        [ValueOption(0)] public string Version { get; set; }
    }

    public class CompatForgetOptions : InstanceSpecificOptions
    {
        [ValueOption(0)] public string Version { get; set; }
    }

    public class Compat : ISubCommand
    {
        public Compat() { }

        public int RunSubCommand(GameInstanceManager manager, CommonOptions opts, SubCommandOptions options)
        {
            var exitCode = Exit.OK;

            Parser.Default.ParseArgumentsStrict(options.options.ToArray(), new CompatOptions(), (string option, object suboptions) =>
            {
                // ParseArgumentsStrict calls us unconditionally, even with bad arguments
                if (!string.IsNullOrEmpty(option) && suboptions != null)
                {
                    CommonOptions comOpts = (CommonOptions)suboptions;
                    comOpts.Merge(opts);
                    _user       = new ConsoleUser(comOpts.Headless);
                    _kspManager = manager ?? new GameInstanceManager(_user);
                    exitCode    = comOpts.Handle(_kspManager, _user);
                    if (exitCode != Exit.OK)
                    {
                        return;
                    }

                    switch (option)
                    {
                        case "list":
                            {
                                var instance = MainClass.GetGameInstance(_kspManager);

                                string versionHeader = Properties.Resources.CompatVersionHeader;
                                string actualHeader  = Properties.Resources.CompatActualHeader;

                                var output = instance
                                    .GetCompatibleVersions()
                                    .Select(i => new
                                    {
                                        Version = i,
                                        Actual = false
                                    })
                                    .ToList();

                                output.Add(new
                                {
                                    Version = instance.Version(),
                                    Actual = true
                                });

                                output = output
                                    .OrderByDescending(i => i.Actual)
                                    .ThenByDescending(i => i.Version)
                                    .ToList();

                                var versionWidth = Enumerable
                                    .Repeat(versionHeader, 1)
                                    .Concat(output.Select(i => i.Version.ToString()))
                                    .Max(i => i.Length);

                                var actualWidth = Enumerable
                                    .Repeat(actualHeader, 1)
                                    .Concat(output.Select(i => i.Actual.ToString()))
                                    .Max(i => i.Length);

                                const string columnFormat = "{0}  {1}";

                                _user.RaiseMessage(string.Format(columnFormat,
                                    versionHeader.PadRight(versionWidth),
                                    actualHeader.PadRight(actualWidth)
                                ));

                                _user.RaiseMessage(string.Format(columnFormat,
                                    new string('-', versionWidth),
                                    new string('-', actualWidth)
                                ));

                                foreach (var line in output)
                                {
                                    _user.RaiseMessage(columnFormat,
                                       line.Version.ToString().PadRight(versionWidth),
                                       line.Actual.ToString().PadRight(actualWidth)
                                    );
                                }
                            }
                            break;

                        case "add":
                            {
                                var instance = MainClass.GetGameInstance(_kspManager);
                                var addOptions = (CompatAddOptions)suboptions;

                                if (GameVersion.TryParse(addOptions.Version, out GameVersion gv))
                                {
                                    var newCompatibleVersion = instance.GetCompatibleVersions();
                                    newCompatibleVersion.Add(gv);
                                    instance.SetCompatibleVersions(newCompatibleVersion);
                                }
                                else
                                {
                                    _user.RaiseError(Properties.Resources.CompatInvalid);
                                    exitCode = Exit.ERROR;
                                }
                            }
                            break;

                        case "forget":
                            {
                                var instance = MainClass.GetGameInstance(_kspManager);
                                var addOptions = (CompatForgetOptions)suboptions;

                                if (GameVersion.TryParse(addOptions.Version, out GameVersion gv))
                                {
                                    if (gv != instance.Version())
                                    {
                                        var newCompatibleVersion = instance.GetCompatibleVersions();
                                        newCompatibleVersion.RemoveAll(i => i == gv);
                                        instance.SetCompatibleVersions(newCompatibleVersion);
                                    }
                                    else
                                    {
                                        _user.RaiseError(Properties.Resources.CompatCantForget);
                                        exitCode = Exit.ERROR;
                                    }
                                }
                                else
                                {
                                    _user.RaiseError(Properties.Resources.CompatInvalid);
                                    exitCode = Exit.ERROR;
                                }
                            }
                            break;

                        default:
                            exitCode = Exit.BADOPT;
                            break;
                    }
                }
            }, () => { exitCode = MainClass.AfterHelp(); });
            return exitCode;
        }

        private IUser               _user;
        private GameInstanceManager _kspManager;
    }
}
