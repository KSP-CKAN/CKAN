using System;
using CommandLine;
using CommandLine.Text;
using log4net;

namespace CKAN.CmdLine
{

    public class Prompt
    {
        public Prompt() { }

        public int RunCommand(KSPManager manager, object raw_options)
        {
            CommonOptions opts = raw_options as CommonOptions;
            // Print an intro if not in headless mode
            if (!(opts?.Headless ?? false))
            {
                Console.WriteLine("Welcome to CKAN!");
                Console.WriteLine("");
                if (DateTime.Now.Month == 4 && DateTime.Now.Day == 1)
                {
                    Console.WriteLine("Happy April Fools' Day! You may want to try the consoleui command.");
                    Console.WriteLine("");
                }
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
                            ? $"CKAN {Meta.GetVersion()}: KSP {manager.CurrentInstance.Version()} ({manager.CurrentInstance.Name})> "
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
                    // but with a persistent KSPManager object.
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
