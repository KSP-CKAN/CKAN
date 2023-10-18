namespace CKAN.CmdLine
{
    public interface ICommand
    {
        int RunCommand(CKAN.GameInstance instance, object options);
    }
}
