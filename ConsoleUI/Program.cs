using System;

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
        #pragma warning disable IDE0060
        public static void Main(string[] args)
        #pragma warning restore IDE0060
        {
            Main_(null, null);
        }

        /// <summary>
        /// Shared entry point for the application, used by real command line
        /// and by other parts of CKAN that want to launch the console UI.
        /// </summary>
        /// <param name="manager">Game instance manager object potentially initialized by command line flags</param>
        /// <param name="themeName">'default' to use default theme, 'dark' to use dark theme</param>
        /// <param name="debug">True if debug options should be available, false otherwise</param>
        /// <returns>
        /// Process exit status
        /// </returns>
        public static int Main_(GameInstanceManager manager, string themeName, bool debug = false)
        {
            Logging.Initialize();

            new ConsoleCKAN(manager, themeName, debug);

            // Tell RegistryManager not to throw Dispose-related exceptions at exit
            RegistryManager.DisposeAll();

            return 0;
        }
    }
}
