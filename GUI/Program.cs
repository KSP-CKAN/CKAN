﻿using System;
using System.Linq;
using System.Windows.Forms;

namespace CKAN.GUI
{
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
                new Main(args, manager, showConsole);
            }
        }

        public static void UnhandledExceptionEventHandler(Object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject;

            // Provide a stack backtrace, so our users and non-debugging devs can
            // see what's gone wrong.
            CKAN.GUI.Main.Instance.ErrorDialog("Unhandled exception:\r\n{0} ", exception.ToString());
        }
    }
}
