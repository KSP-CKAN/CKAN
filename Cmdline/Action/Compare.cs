using CKAN.Versioning;
using CommandLine;

namespace CKAN.CmdLine.Action
{
    /// <summary>
    /// Class for comparing version strings.
    /// </summary>
    public class Compare : ICommand
    {
        private readonly IUser _user;

        /// <summary>
        /// Initializes a new instance of the <see cref="CKAN.CmdLine.Action.Compare"/> class.
        /// </summary>
        /// <param name="user">The current <see cref="CKAN.IUser"/> to raise messages to the user.</param>
        public Compare(IUser user)
        {
            _user = user;
        }

        /// <summary>
        /// Run the 'compare' command.
        /// </summary>
        /// <inheritdoc cref="ICommand.RunCommand"/>
        public int RunCommand(CKAN.GameInstance inst, object args)
        {
            var opts = (CompareOptions)args;
            if (string.IsNullOrWhiteSpace(opts.Left) || string.IsNullOrWhiteSpace(opts.Right))
            {
                _user.RaiseMessage("compare <version1> <version2> - argument(s) missing, perhaps you forgot it?");
                return Exit.BadOpt;
            }

            var leftVersion = new ModuleVersion(opts.Left);
            var rightVersion = new ModuleVersion(opts.Right);

            var compareResult = leftVersion.CompareTo(rightVersion);

            if (opts.MachineReadable)
            {
                _user.RaiseMessage(compareResult.ToString());
            }
            else if (compareResult == 0)
            {
                _user.RaiseMessage("\"{0}\" and \"{1}\" are the same versions.", leftVersion, rightVersion);
            }
            else if (compareResult < 0)
            {
                _user.RaiseMessage("\"{0}\" is lower than \"{1}\".", leftVersion, rightVersion);
            }
            else
            {
                _user.RaiseMessage("\"{0}\" is higher than \"{1}\".", leftVersion, rightVersion);
            }

            return Exit.Ok;
        }
    }

    [Verb("compare", HelpText = "Compare version strings")]
    internal class CompareOptions : CommonOptions
    {
        [Option("machine-readable", HelpText = "Output in a machine readable format: -1, 0 or 1")]
        public bool MachineReadable { get; set; }

        [Value(0, MetaName = "version1", HelpText = "The first version to compare")]
        public string Left { get; set; }

        [Value(1, MetaName = "version2", HelpText = "The second version to compare")]
        public string Right { get; set; }
    }
}
