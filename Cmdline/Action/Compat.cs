using System.Linq;
using CKAN.Versioning;
using CommandLine;
using CommandLine.Text;

namespace CKAN.CmdLine.Action
{
    public class Compat : ISubCommand
    {
        public Compat() { }

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
                    ht.AddPreOptionsLine("ckan compat - Manage KSP version compatibility");
                    ht.AddPreOptionsLine($"Usage: ckan compat <command> [options]");
                }
                else
                {
                    ht.AddPreOptionsLine("compat " + verb + " - " + GetDescription(verb));
                    switch (verb)
                    {
                        // First the commands with one string argument
                        case "add":
                        case "forget":
                            ht.AddPreOptionsLine($"Usage: ckan compat {verb} [options] version");
                            break;

                        // Now the commands with only --flag type options
                        case "list":
                        default:
                            ht.AddPreOptionsLine($"Usage: ckan compat {verb} [options]");
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

        public int RunSubCommand(KSPManager manager, CommonOptions opts, SubCommandOptions options)
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
                    _kspManager = manager ?? new KSPManager(_user);
                    exitCode    = comOpts.Handle(_kspManager, _user);
                    if (exitCode != Exit.OK)
                        return;

                    switch (option)
                    {
                        case "list":
                            {
                                var ksp = MainClass.GetGameInstance(_kspManager);

                                const string versionHeader = "Version";
                                const string actualHeader  = "Actual";

                                var output = ksp
                                    .GetCompatibleVersions()
                                    .Select(i => new
                                    {
                                        Version = i,
                                        Actual = false
                                    })
                                    .ToList();

                                output.Add(new
                                {
                                    Version = ksp.Version(),
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
                                    _user.RaiseMessage(string.Format(columnFormat,
                                       line.Version.ToString().PadRight(versionWidth),
                                       line.Actual.ToString().PadRight(actualWidth)
                                   ));
                                }
                            }
                            break;

                        case "add":
                            {
                                var ksp = MainClass.GetGameInstance(_kspManager);
                                var addOptions = (CompatAddOptions)suboptions;

                                KspVersion kspVersion;
                                if (KspVersion.TryParse(addOptions.Version, out kspVersion))
                                {
                                    var newCompatibleVersion = ksp.GetCompatibleVersions();
                                    newCompatibleVersion.Add(kspVersion);
                                    ksp.SetCompatibleVersions(newCompatibleVersion);
                                }
                                else
                                {
                                    _user.RaiseError("ERROR: Invalid KSP version.");
                                    exitCode = Exit.ERROR;
                                }
                            }
                            break;

                        case "forget":
                            {
                                var ksp = MainClass.GetGameInstance(_kspManager);
                                var addOptions = (CompatForgetOptions)suboptions;

                                KspVersion kspVersion;
                                if (KspVersion.TryParse(addOptions.Version, out kspVersion))
                                {
                                    if (kspVersion != ksp.Version())
                                    {
                                        var newCompatibleVersion = ksp.GetCompatibleVersions();
                                        newCompatibleVersion.RemoveAll(i => i == kspVersion);
                                        ksp.SetCompatibleVersions(newCompatibleVersion);
                                    }
                                    else
                                    {
                                        _user.RaiseError("ERROR: Cannot forget actual KSP version.");
                                        exitCode = Exit.ERROR;
                                    }
                                }
                                else
                                {
                                    _user.RaiseError("ERROR: Invalid KSP version.");
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

        private KSPManager _kspManager;
        private IUser      _user;
    }
}
