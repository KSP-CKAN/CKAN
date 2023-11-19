using System;
using System.Linq;
using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public static class GUI
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            Main_(args);
        }

        public static void Main_(string[] args, GameInstanceManager manager = null, bool showConsole = false)
        {
            Logging.Initialize();

            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionEventHandler;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Contains(URLHandlers.UrlRegistrationArgument))
            {
                //Passing in null will cause a NullReferenceException if it tries to show the dialog
                //asking for elevation permission, but we want that to happen. Doing that keeps us
                //from getting in to a infinite loop of trying to register.
                URLHandlers.RegisterURLHandler(null, null);
            }
            else
            {
                var main = new Main(args, manager);
                if (!showConsole)
                {
                    Util.HideConsoleWindow();
                }
                Application.Run(main);
            }
        }

        public static void UnhandledExceptionEventHandler(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject;

            // Provide a stack backtrace, so our users and non-debugging devs can
            // see what's gone wrong.
            CKAN.GUI.Main.Instance.ErrorDialog("Unhandled exception:\r\n{0} ", exception.ToString());
        }
    }
}
