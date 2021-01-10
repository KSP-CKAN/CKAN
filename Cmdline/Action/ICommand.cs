namespace CKAN.CmdLine
{
    public interface ICommand
    {
        int RunCommand(CKAN.GameInstance ksp, object options);
    }
}
