using System;
using log4net;

namespace CKAN.ConsoleUI
{

    /// <summary>
    /// Class containing main driver functionality for console UI
    /// </summary>
    public static class ConsoleUI
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        [STAThread]
        public static void Main(string[] args)
        {
            Main_(args);
        }

        /// <summary>
        /// Shared entry point for the application, used by real command line
        /// and by other parts of CKAN that want to launch the console UI.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <param name="debug">True if debug options should be available, false otherwise</param>
        /// <returns>
        /// Process exit status
        /// </returns>
        public static int Main_(string[] args, bool debug = false)
        {
            Logging.Initialize();

            new ConsoleCKAN(debug);

            // Tell RegistryManager not to throw Dispose-related exceptions at exit
            RegistryManager.DisposeAll();

            return 0;
        }
    }
}
