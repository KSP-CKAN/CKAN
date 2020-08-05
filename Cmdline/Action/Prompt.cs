using System;
using CommandLine;

namespace CKAN.CmdLine.Action
{
    /// <summary>
    /// Class for managing multiple commands.
    /// </summary>
    public class Prompt
    {
        private const string ExitCommand = "exit";

        /// <summary>
        /// Run the 'prompt' command.
        /// </summary>
        /// <param name="manager">The manager to provide game instances.</param>
        /// <param name="args">The command line arguments handled by the parser.</param>
        /// <returns>An <see cref="CKAN.Exit"/> code.</returns>
        public int RunCommand(GameInstanceManager manager, object args, string[] argStrings)
        {
            var opts = (PromptOptions)args;
            // Print an intro if not in headless mode
            if (!(opts?.Headless ?? false))
            {
                Console.WriteLine("Welcome to CKAN!");
                Console.WriteLine("");
                Console.WriteLine("To get help, type help and press enter.");
                Console.WriteLine("");
            }
            bool done = false;
            while (!done)
            {
                // Prompt if not in headless mode
                if (!(opts?.Headless ?? false))
                {
                    Console.Write(
                        manager.CurrentInstance != null
                            ? string.Format("CKAN {0}: {1} {2} ({3})> ",
                                Meta.GetVersion(),
                                manager.CurrentInstance.game.ShortName,
                                manager.CurrentInstance.Version(),
                                manager.CurrentInstance.Name)
                            : string.Format("CKAN {0}> ", Meta.GetVersion())
                    );
                }

                // Get input
                var command = Console.ReadLine();
                if (command == null || command == ExitCommand)
                {
                    done = true;
                }
                else if (command != string.Empty)
                {
                    // Parse input as if it was a normal command line,
                    // but with a persistent GameInstanceManager object
                    var cmdExitCode = MainClass.Execute(manager, command.Split(' '), argStrings);
                    if ((opts?.Headless ?? false) && cmdExitCode != Exit.Ok)
                    {
                        // Pass failure codes to calling process in headless mode
                        // (in interactive mode the user can see the error and try again)
                        return cmdExitCode;
                    }
                }
            }

            return Exit.Ok;
        }
    }

    [Verb("prompt", HelpText = "Run CKAN prompt for executing multiple commands in a row")]
    internal class PromptOptions : CommonOptions { }
}
