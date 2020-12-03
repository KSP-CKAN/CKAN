using System;
using CommandLine;
using CommandLine.Text;
using log4net;

namespace CKAN.CmdLine
{

    public class Prompt
    {
        public Prompt() { }

        public int RunCommand(GameInstanceManager manager, object raw_options)
        {
            CommonOptions opts = raw_options as CommonOptions;
            bool done = false;
            while (!done)
            {
                // Prompt if not in headless mode
                if (!(opts?.Headless ?? false))
                {
                    Console.Write(
                        manager.CurrentInstance != null
                            ? $"CKAN {Meta.GetVersion()}: {manager.CurrentInstance.game.ShortName} {manager.CurrentInstance.Version()} ({manager.CurrentInstance.Name})> "
                            : $"CKAN {Meta.GetVersion()}> "
                    );
                }
                // Get input
                string command = Console.ReadLine();
                if (command == null || command == exitCommand)
                {
                    done = true;
                }
                else if (command != "")
                {
                    // Parse input as if it was a normal command line,
                    // but with a persistent GameInstanceManager object.
                    int cmdExitCode = MainClass.Execute(manager, opts, command.Split(' '));
                    if ((opts?.Headless ?? false) && cmdExitCode != Exit.OK)
                    {
                        // Pass failure codes to calling process in headless mode
                        // (in interactive mode the user can see the error and try again)
                        return cmdExitCode;
                    }
                }
            }
            return Exit.OK;
        }

        private const string exitCommand = "exit";
    }

}
