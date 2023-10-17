using CKAN.Versioning;

namespace CKAN.CmdLine
{
    // Does not need an instance, so this is not an ICommand
    public class Compare
    {
        private readonly IUser user;

        public Compare(IUser user)
        {
            this.user = user;
        }

        public int RunCommand(object rawOptions)
        {
            var options = (CompareOptions)rawOptions;

            if (options.Left != null && options.Right != null)
            {
                var leftVersion  = new ModuleVersion(options.Left);
                var rightVersion = new ModuleVersion(options.Right);

                int compareResult = leftVersion.CompareTo(rightVersion);

                if (options.machine_readable)
                {
                    user.RaiseMessage(compareResult.ToString());
                }
                else if (compareResult == 0)
                {
                    user.RaiseMessage(Properties.Resources.CompareSame, leftVersion, rightVersion);
                }
                else if (compareResult < 0)
                {
                    user.RaiseMessage(Properties.Resources.CompareLower, leftVersion, rightVersion);
                }
                else if (compareResult > 0)
                {
                    user.RaiseMessage(Properties.Resources.CompareHigher, leftVersion, rightVersion);
                }
                else
                {
                    user.RaiseMessage("{0}: ckan compare version1 version2", Properties.Resources.Usage);
                }
            }
            else
            {
                user.RaiseMessage("{0}: ckan compare version1 version2", Properties.Resources.Usage);
                return Exit.BADOPT;
            }

            return Exit.OK;
        }
    }
}
