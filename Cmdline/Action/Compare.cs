namespace CKAN.CmdLine
{
    public class Compare : ICommand
    {
        public int RunCommand(CKAN.KSP ksp, object rawOptions)
        {
            var options = (CompareOptions)rawOptions;

            var leftVersion = new Version(options.Left);
            var rightVersion = new Version(options.Right);

            ksp.User.RaiseMessage(leftVersion.CompareTo(rightVersion).ToString());

            return 0;
        }
    }
}
