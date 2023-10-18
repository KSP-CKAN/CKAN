namespace CKAN.CmdLine
{
    internal interface ISubCommand
    {
        int RunSubCommand(GameInstanceManager manager, CommonOptions opts, SubCommandOptions options);
    }
}
