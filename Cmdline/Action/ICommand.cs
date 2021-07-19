namespace CKAN.CmdLine.Action
{
    /// <summary>
    /// Interface for regular commands.
    /// </summary>
    internal interface ICommand
    {
        /// <summary>
        /// Run the command.
        /// </summary>
        /// <param name="inst">The game instance which to handle with mods.</param>
        /// <param name="args">The command line arguments handled by the parser.</param>
        /// <returns>An <see cref="CKAN.Exit"/> code.</returns>
        int RunCommand(CKAN.GameInstance inst, object args);
    }
}
