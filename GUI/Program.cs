using System;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
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
            if (Platform.IsWindows)
            {
                // By default, Windows will stretch the window and make it blurry; tell it not to.
                SetProcessDPIAware();
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Contains(URLHandlers.UrlRegistrationArgument))
            {
                // Passing in null will cause a NullReferenceException if it tries to show the dialog
                // asking for elevation permission, but we want that to happen. Doing that keeps us
                // from getting in to a infinite loop of trying to register.
                URLHandlers.RegisterURLHandler(null, null, null);
            }
            else
            {
                #if NET10_0_OR_GREATER
                if (Platform.IsWindows && Util.DarkMode)
                {
                    Application.SetColorMode(SystemColorMode.System);
                }
                #endif
                var main = new Main(args, manager, userAgent);
                if (Platform.IsWindows && Util.DarkMode)
                {
                    int val = 1;
                    DwmSetWindowAttribute(main.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1,
                                          ref val, sizeof(int));
                    DwmSetWindowAttribute(main.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE,
                                          ref val, sizeof(int));
                }
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

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE             = 20;
    }
}
