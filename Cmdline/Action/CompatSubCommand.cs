using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CKAN.Versioning;
using CommandLine;

namespace CKAN.CmdLine.Action
{
    public class CompatSubCommand : ISubCommand
    {
        private readonly KSPManager _kspManager;
        private readonly IUser _user;

        public CompatSubCommand(KSPManager kspManager, IUser user)
        {
            _kspManager = kspManager;
            _user = user;
        }

        public int RunSubCommand(SubCommandOptions options)
        {
            var exitCode = 0;

            Parser.Default.ParseArgumentsStrict(options.options.ToArray(), new CompatOptions(), (option, suboptions) =>
            {
                switch (option)
                {
                    case "list":
                        {
                            var ksp = _kspManager.CurrentInstance;

                            const string versionHeader = "Version";
                            const string actualHeader = "Actual";

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
                            var ksp = _kspManager.CurrentInstance;
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
                            var ksp = _kspManager.CurrentInstance;
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
            });

            return exitCode;
        }

        private class CompatOptions : CommonOptions
        {
            [VerbOption("list", HelpText = "List compatible KSP versions")]
            public CompatListOptions List { get; set; }

            [VerbOption("add", HelpText = "Add version to KSP compatibility list")]
            public CompatAddOptions Add { get; set; }

            [VerbOption("forget", HelpText = "Forget version on KSP compatibility list")]
            public CompatForgetOptions Forget { get; set; }
        }

        private class CompatListOptions : CommonOptions { }

        private class CompatAddOptions : CommonOptions
        {
            [ValueOption(0)]
            public string Version { get; set; }
        }

        private class CompatForgetOptions : CommonOptions
        {
            [ValueOption(0)]
            public string Version { get; set; }
        }
    }
}
