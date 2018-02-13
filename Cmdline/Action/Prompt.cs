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
                    MainClass.Execute(manager, opts, command.Split(' '));
                }
            }
            return Exit.OK;
        }

        private const string exitCommand = "exit";
    }

}
