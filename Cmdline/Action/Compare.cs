using CKAN.Versioning;

namespace CKAN.CmdLine
{
    // Does not need an instance, so this is not an ICommand
    public class Compare
    {
        private IUser user;

        public Compare(IUser user)
        {
            this.user = user;
        }

        public int RunCommand(object rawOptions)
        {
            var options = (CompareOptions)rawOptions;

            if (options.Left != null && options.Right != null)
            {
                var leftVersion = new ModuleVersion(options.Left);
                var rightVersion = new ModuleVersion(options.Right);

                int compareResult = leftVersion.CompareTo(rightVersion);

                if (options.machine_readable)
                {
                    user.RaiseMessage(compareResult.ToString());
                }
                else if (compareResult == 0)
                {
                    user.RaiseMessage(
                        "\"{0}\" and \"{1}\" are the same versions.", leftVersion, rightVersion);
                }
                else if (compareResult < 0)
                {
                    user.RaiseMessage(
                        "\"{0}\" is lower than \"{1}\".", leftVersion, rightVersion);
                }
                else if (compareResult > 0)
                {
                    user.RaiseMessage(
                        "\"{0}\" is higher than \"{1}\".", leftVersion, rightVersion);
                }
                else
                {
                    user.RaiseMessage(
                        "Usage: ckan compare version1 version2");
                }
            }
            else
            {
                user.RaiseMessage(
                    "Usage: ckan compare version1 version2");
                return Exit.BADOPT;
            }

            return Exit.OK;
        }
    }
}
