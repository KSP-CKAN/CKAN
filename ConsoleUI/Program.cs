using System;
using System.Diagnostics;
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

            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionEventHandler;

            new ConsoleCKAN(debug);

            return 0;
        }

        /// <summary>
        /// Handle unhandled exceptions
        /// </summary>
        /// <param name="sender">Source of the exceptions</param>
        /// <param name="e">Exception exception</param>
        public static void UnhandledExceptionEventHandler(Object sender, UnhandledExceptionEventArgs e)
        {
            // Provide a stack backtrace, so our users and non-debugging devs can
            // see what's gone wrong.
            log.ErrorFormat("Unhandled exception:\r\n{0} ", e.ExceptionObject.ToString());
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(ConsoleUI));
    }
}
