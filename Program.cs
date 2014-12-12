using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace CKAN
{
    public static class GUI
    {
        internal static GUIUser user = new GUIUser();
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionEventHandler;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            new Main(user);
        }

        public static void UnhandledExceptionEventHandler(Object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject;

            // Provide a stack backtrace, so our users and non-debugging devs can
            // see what's gone wrong.
            user.DisplayError("Unhandled exception:\n{0} ", exception.ToString());
            Debugger.Break();
        }
    }
}
