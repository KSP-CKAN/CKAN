namespace CKAN.CmdLine.Action
{
    /// <summary>
    /// Interface for commands that use nested commands.
    /// </summary>
    internal interface ISubCommand
    {
        /// <summary>
        /// Run the command with nested commands.
        /// </summary>
        /// <param name="manager">The manager to provide game instances.</param>
        /// <param name="args">The command line arguments handled by the parser.</param>
        /// <returns>An <see cref="CKAN.Exit"/> code.</returns>
        int RunCommand(GameInstanceManager manager, object args);

        /// <summary>
        /// Displays an <c>USAGE</c> prefix line in the help screen.
        /// </summary>
        /// <param name="prefix">The <c>USAGE</c> prefix with the app name.</param>
        /// <param name="args">The command line arguments handled by the parser.</param>
        /// <returns>An <c>USAGE</c>: <see cref="string"/> which contains information on how to use the command.</returns>
        string GetUsage(string prefix, string[] args);
    }
}
