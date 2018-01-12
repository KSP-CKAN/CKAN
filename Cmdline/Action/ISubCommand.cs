namespace CKAN.CmdLine
{
    internal interface ISubCommand
    {
        int RunSubCommand(KSPManager manager, CommonOptions opts, SubCommandOptions options);
    }
}
