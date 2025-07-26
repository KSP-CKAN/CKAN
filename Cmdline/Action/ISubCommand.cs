namespace CKAN.CmdLine
{
    internal interface ISubCommand
    {
        int RunSubCommand(CommonOptions?    opts,
                          SubCommandOptions options);
    }
}
