namespace CKAN.CmdLine
{
    public interface ICommand
    {
        int RunCommand(CKAN.KSP ksp, object options);
    }
}

