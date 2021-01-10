using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;
using log4net;

namespace CKAN.CmdLine
{
    /// <summary>
    /// Subcommand for setting flags on modules,
    /// currently the auto-installed flag
    /// </summary>
    public class Mark : ISubCommand
    {
        /// <summary>
        /// Initialize the subcommand
        /// </summary>
        public Mark() { }

        /// <summary>
        /// Run the subcommand
        /// </summary>
        /// <param name="mgr">Manager to provide game instances</param>
        /// <param name="opts">Command line parameters paritally handled by parser</param>
        /// <param name="unparsed">Command line parameters not yet handled by parser</param>
        /// <returns>
        /// Exit code
        /// </returns>
        public int RunSubCommand(GameInstanceManager mgr, CommonOptions opts, SubCommandOptions unparsed)
        {
            string[] args = unparsed.options.ToArray();
            int exitCode = Exit.OK;
            // Parse and process our sub-verbs
            Parser.Default.ParseArgumentsStrict(args, new MarkSubOptions(), (string option, object suboptions) =>
            {
                // ParseArgumentsStrict calls us unconditionally, even with bad arguments
                if (!string.IsNullOrEmpty(option) && suboptions != null)
                {
                    CommonOptions options = (CommonOptions)suboptions;
                    options.Merge(opts);
                    user     = new ConsoleUser(options.Headless);
                    manager  = mgr ?? new GameInstanceManager(user);
                    exitCode = options.Handle(manager, user);
                    if (exitCode != Exit.OK)
                        return;

                    switch (option)
                    {
                        case "auto":
                            exitCode = MarkAuto((MarkAutoOptions)suboptions, true, option, "auto-installed");
                            break;

                        case "user":
                            exitCode = MarkAuto((MarkAutoOptions)suboptions, false, option, "user-selected");
                            break;

                        default:
                            user.RaiseMessage("Unknown command: mark {0}", option);
                            exitCode = Exit.BADOPT;
                            break;
                    }
                }
            }, () => { exitCode = MainClass.AfterHelp(); });
            return exitCode;
        }

        private int MarkAuto(MarkAutoOptions opts, bool value, string verb, string descrip)
        {
            if (opts.modules.Count < 1)
            {
                user.RaiseMessage("Usage: ckan mark {0} Mod [Mod2 ...]", verb);
                return Exit.BADOPT;
            }

            int exitCode = opts.Handle(manager, user);
            if (exitCode != Exit.OK)
            {
                return exitCode;
            }

            var  ksp      = MainClass.GetGameInstance(manager);
            var  regMgr   = RegistryManager.Instance(ksp);
            bool needSave = false;
            Search.AdjustModulesCase(ksp, opts.modules);
            foreach (string id in opts.modules)
            {
                InstalledModule im = regMgr.registry.InstalledModule(id);
                if (im == null)
                {
                    user.RaiseError("{0} is not installed.", id);
                }
                else if (im.AutoInstalled == value)
                {
                    user.RaiseError("{0} is already marked as {1}.", id, descrip);
                }
                else
                {
                    user.RaiseMessage("Marking {0} as {1}...", id, descrip);
                    try
                    {
                        im.AutoInstalled = value;
                        needSave = true;
                    }
                    catch (ModuleIsDLCKraken kraken)
                    {
                        user.RaiseMessage($"Can't mark expansion '{kraken.module.name}' as auto-installed.");
                        return Exit.BADOPT;
                    }
                }
            }
            if (needSave)
            {
                regMgr.Save(false);
                user.RaiseMessage("Changes made!");
            }
            return Exit.OK;
        }

        private GameInstanceManager manager { get; set; }
        private IUser      user    { get; set; }

        private static readonly ILog log = LogManager.GetLogger(typeof(Mark));
    }

    internal class MarkSubOptions : VerbCommandOptions
    {
        [VerbOption("auto", HelpText = "Mark modules as auto installed")]
        public MarkAutoOptions MarkAutoOptions { get; set; }

        [VerbOption("user", HelpText = "Mark modules as user selected (opposite of auto installed)")]
        public MarkAutoOptions MarkUserOptions { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            HelpText ht = HelpText.AutoBuild(this, verb);
            // Add a usage prefix line
            ht.AddPreOptionsLine(" ");
            if (string.IsNullOrEmpty(verb))
            {
                ht.AddPreOptionsLine("ckan mark - Edit flags on modules");
                ht.AddPreOptionsLine($"Usage: ckan mark <command> [options]");
            }
            else
            {
                ht.AddPreOptionsLine("ksp " + verb + " - " + GetDescription(verb));
                switch (verb)
                {
                    case "auto":
                        ht.AddPreOptionsLine($"Usage: ckan mark {verb} [options] Mod [Mod2 ...]");
                        break;

                    case "user":
                        ht.AddPreOptionsLine($"Usage: ckan mark {verb} [options] Mod [Mod2 ...]");
                        break;
                }
            }
            return ht;
        }
    }

    internal class MarkAutoOptions : InstanceSpecificOptions
    {
        [ValueList(typeof(List<string>))]
        public List<string> modules { get; set; }
    }
}
