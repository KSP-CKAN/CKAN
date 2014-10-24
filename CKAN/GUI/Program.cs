using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace CKAN
{
    public static class GUI
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionEventHandler;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }

        public static void UnhandledExceptionEventHandler(Object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject;

            // Provide a stack backtrace, so our users and non-debugging devs can
            // see what's gone wrong.
            Console.WriteLine(exception.ToString());
            Debugger.Break();
        }
    }
}
