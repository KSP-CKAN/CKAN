using System;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics.CodeAnalysis;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    [ExcludeFromCodeCoverage]
    public static class GUI
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            Main_(args, null);
        }

        public static void Main_(string[]             args,
                                 string?              userAgent,
                                 GameInstanceManager? manager = null,
                                 bool                 showConsole = false)
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
                URLHandlers.RegisterURLHandler(null, null, null);
            }
            else
            {
                var main = new Main(args, manager, userAgent);
                if (!showConsole)
                {
                    Util.HideConsoleWindow();
                }
                Application.Run(main);
            }
        }

        public static void UnhandledExceptionEventHandler(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                // Provide a stack backtrace, so our users and non-debugging devs can
                // see what's gone wrong.
                CKAN.GUI.Main.Instance?.ErrorDialog("Unhandled exception:\r\n{0} ",
                                                    exception.ToString());
            }
        }
    }
}
